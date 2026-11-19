using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ProjectRogue.Movement;

// Local integration slice: all tuning/content below is demo-only, not a new game specification.
public sealed class PlayableArena : MonoBehaviour
{
    GameObject world, player, exit;
    PlayerMovement movement;
    ClassTraversal traversal;
    PlayerCombat combat;
    Health health;
    ResourcePool resources;
    Camera view;
    UnityEngine.UI.Text hud, message;
    readonly List<ArenaFoe> foes = new List<ArenaFoe>();
    readonly List<Object> transient = new List<Object>();
    Sprite square;
    AbilityData slash, burst, enemyAttack;
    bool won;
    float flashUntil;
    GameObject slashVisual;
    public GameObject Player => player;
    public int Remaining => foes.FindAll(f => f != null && !f.GetComponent<Health>().IsDead).Count;
    public bool Won => won;

    void Start()
    {
        // Other loaded scenes are left intact in Edit mode; this scene owns its runtime world.
        var texture = new Texture2D(1, 1); texture.SetPixel(0, 0, Color.white); texture.Apply();
        square = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1, 0, SpriteMeshType.FullRect);
        transient.Add(square); transient.Add(texture);
        BuildHud();
        ResetArena();
    }

    public void ResetArena()
    {
        if (world != null) { world.SetActive(false); Destroy(world); }
        foreach (var asset in transient.ToArray())
            if (asset is AbilityData || asset is PhysicsMaterial2D) { Destroy(asset); transient.Remove(asset); }
        foes.Clear(); won = false;
        world = new GameObject("Playable arena runtime");
        world.transform.SetParent(transform);
        var cameraObject = new GameObject("Arena Camera"); cameraObject.transform.SetParent(world.transform);
        view = cameraObject.AddComponent<Camera>(); view.orthographic = true; view.orthographicSize = 6.8f;
        view.backgroundColor = new Color(.035f, .045f, .085f); view.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0, 1, -10);
        Box("Floor", new Vector2(0, -2), new Vector2(36, 1), new Color(.12f,.19f,.27f), true);
        Box("Left wall", new Vector2(-18, 2), new Vector2(1, 9), new Color(.12f,.19f,.27f), true);
        Box("Right wall", new Vector2(18, 2), new Vector2(1, 9), new Color(.12f,.19f,.27f), true);
        Box("Platform left", new Vector2(-7, .2f), new Vector2(4, .45f), new Color(.2f,.35f,.43f), true);
        Box("Platform right", new Vector2(5, .6f), new Vector2(4, .45f), new Color(.2f,.35f,.43f), true);
        for (int i = -16; i <= 16; i += 4)
            Box("Backdrop pillar", new Vector2(i, 2), new Vector2(.25f, 7), new Color(.055f,.075f,.12f), false).GetComponent<SpriteRenderer>().sortingOrder = -5;
        slash = Attack("Sword slash", 1, .32f, 0, new Vector2(1.15f, 0), new Vector2(2.1f, 1.5f));
        burst = Attack("Arc strike", 2.2f, .8f, 12, new Vector2(1.5f, 0), new Vector2(3.2f, 2));
        burst.resource = ResourceType.Mana;
        enemyAttack = Attack("Enemy strike", 1, 1, 0, Vector2.zero, Vector2.one);
        player = Box("Player", new Vector2(-14, -.55f), new Vector2(.75f, 1.5f), new Color(.2f,.85f,.92f), false);
        // Keep transform scale at one: combat hitbox offsets and movement capsule use world units.
        player.GetComponent<SpriteRenderer>().enabled = false;
        var playerVisual = new GameObject("Player visual"); playerVisual.transform.SetParent(player.transform, false);
        playerVisual.transform.localScale = new Vector3(.75f, 1.5f, 1);
        var playerSprite = playerVisual.AddComponent<SpriteRenderer>(); playerSprite.sprite = square; playerSprite.color = new Color(.2f,.85f,.92f);
        player.transform.localScale = Vector3.one;
        player.SetActive(false); player.layer = 2;
        player.AddComponent<Rigidbody2D>();
        var capsule = player.AddComponent<CapsuleCollider2D>(); capsule.size = new Vector2(.75f, 1.5f);
        var material = new PhysicsMaterial2D("Demo frictionless") { friction = 0, bounciness = 0 };
        transient.Add(material); capsule.sharedMaterial = material;
        traversal = player.AddComponent<ClassTraversal>(); movement = player.GetComponent<PlayerMovement>();
        resources = player.AddComponent<ResourcePool>(); resources.manaRegenPerSec = 8;
        health = player.AddComponent<Health>();
        health.Died += OnPlayerDied;
        combat = player.AddComponent<PlayerCombat>(); combat.basicAttack = slash; combat.ability1 = burst;
        combat.hitMask = 1 << 8;
        player.SetActive(true);
        slashVisual = Box("Sword arc", Vector2.zero, new Vector2(1.8f, .16f), new Color(.8f,1,.95f), false);
        slashVisual.SetActive(false);
        SpawnEnemy(-5); SpawnEnemy(3); SpawnEnemy(11);
        exit = Box("Exit", new Vector2(16, -.2f), new Vector2(1, 2.6f), new Color(.3f,.25f,.4f), false);
        message.text = "CLEAR THE ROOM  /  Defeat three enemies, then reach the exit";
    }

    void SpawnEnemy(float x)
    {
        var enemy = Box("Arena enemy", new Vector2(x, -.7f), new Vector2(.9f,1.4f), new Color(.9f,.25f,.35f), false);
        enemy.layer = 8; enemy.SetActive(false);
        enemy.AddComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        enemy.AddComponent<BoxCollider2D>().size = Vector2.one;
        enemy.AddComponent<Health>();
        var foe = enemy.AddComponent<ArenaFoe>(); foe.target = player; foe.attack = enemyAttack; foe.sprite = enemy.GetComponent<SpriteRenderer>();
        foes.Add(foe); enemy.SetActive(true);
    }

    AbilityData Attack(string title, float multiplier, float cooldown, float cost, Vector2 offset, Vector2 size)
    {
        var a = ScriptableObject.CreateInstance<AbilityData>(); a.name = title; a.displayName = title;
        a.damageMultiplier = multiplier; a.cooldown = cooldown; a.cost = cost; a.hitboxOffset = offset; a.hitboxSize = size;
        transient.Add(a); return a;
    }

    GameObject Box(string title, Vector2 position, Vector2 size, Color color, bool solid)
    {
        var go = new GameObject(title); go.transform.SetParent(world.transform); go.transform.position = position;
        go.transform.localScale = size; var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = square; renderer.color = color;
        if (solid) go.AddComponent<BoxCollider2D>();
        return go;
    }

    void Update()
    {
        if (Time.timeScale <= 0 || (ProjectRogue.UI.MenuFlow.Instance != null && !ProjectRogue.UI.MenuFlow.Instance.GameplayInputAllowed)) return;
        if (player == null) return;
        var keys = Keyboard.current;
        if (keys == null) return;
        if (!won && !health.IsDead)
        {
            float input = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1 : 0) - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1 : 0);
            movement.SetHorizontalInput(input);
            if (input != 0) player.transform.localScale = new Vector3(Mathf.Sign(input), 1, 1);
            if (keys.spaceKey.wasPressedThisFrame) movement.RequestJump();
            if (keys.leftShiftKey.wasPressedThisFrame) traversal.RequestUtility();
            if (keys.jKey.wasPressedThisFrame || keys.kKey.wasPressedThisFrame)
            {
                // PlayerCombat handles J/K; this is only the visible swing feedback.
                flashUntil = Time.time + .12f;
            }
            if (Remaining == 0)
            {
                exit.GetComponent<SpriteRenderer>().color = new Color(.2f,1,.6f);
                message.text = "ROOM CLEAR  /  Reach the green exit on the right";
                if (player.transform.position.x > 15) { won = true; movement.SetHorizontalInput(0); ProjectRogue.UI.MenuFlow.Instance?.EndDemo(true); }
            }
            if (player.transform.position.y < -8) { player.transform.position = new Vector3(-14,-.5f,0); movement.ResetMotion(); }
        }
        slashVisual.SetActive(Time.time < flashUntil && !won);
        slashVisual.transform.position = player.transform.position + new Vector3(player.transform.localScale.x * 1.2f, .1f, 0);
        view.transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, -6, 6), 1, -10);
        hud.text = $"PROJECT ROGUE   /   PLAYABLE ARENA\nHP {health.Current:0} / {health.Max:0}     STAMINA {resources.Stamina:0}     MANA {resources.Mana:0}     ENEMIES {Remaining}";
    }

    void BuildHud()
    {
        var canvasGo = new GameObject("Arena HUD"); canvasGo.transform.SetParent(transform);
        canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        hud = Label(canvasGo.transform, new Vector2(20,-20), new Vector2(1240,80), 24);
        message = Label(canvasGo.transform, new Vector2(20,-105), new Vector2(1240,50), 22);
        var controls = Label(canvasGo.transform, new Vector2(20,-650), new Vector2(1240,60), 20);
        controls.text = "A/D or arrows  MOVE     SPACE  JUMP     SHIFT  DASH     J  SLASH     K  ARC STRIKE     ESC  PAUSE";
    }

    UnityEngine.UI.Text Label(Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform)); go.transform.SetParent(parent, false);
        var text = go.AddComponent<UnityEngine.UI.Text>(); text.font = Resources.Load<Font>("Fonts/Jersey10-Regular");
        text.fontSize = fontSize; text.color = Color.white; text.raycastTarget = false;
        var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1); rect.anchoredPosition = position; rect.sizeDelta = size;
        return text;
    }

    void OnDestroy() { foreach (var asset in transient) if (asset != null) Destroy(asset); }
    void OnPlayerDied(GameObject source) { ProjectRogue.UI.MenuFlow.Instance?.EndDemo(false); }
}
