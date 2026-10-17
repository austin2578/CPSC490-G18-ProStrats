using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace ProjectRogue.Movement
{
    [RequireComponent(typeof(WeaponTraversal), typeof(ElementPassives))]
    public sealed class PrototypeLoadoutPanel : MonoBehaviour
    {
        [SerializeField] private Sprite[] weaponIcons = new Sprite[3];
        [SerializeField] private Sprite[] elementIcons = new Sprite[3];
        private WeaponTraversal traversal;
        private ElementPassives elements;
        private UnityEngine.UI.Text status;
        private GameObject canvasObject;
        private GameObject ownedEventSystem;
        private readonly UnityEngine.UI.Image[] buttons = new UnityEngine.UI.Image[6];
        private Font font;

        private void Start()
        {
            traversal = GetComponent<WeaponTraversal>();
            elements = GetComponent<ElementPassives>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvasObject = new GameObject("Prototype Loadout", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var panel = Rect("Panel", canvasObject.transform, new Vector2(20, -20), new Vector2(650, 205));
            panel.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
            Label("Weapon", panel, new Vector2(12, -12), new Vector2(80, 42), 20);
            Label("Element", panel, new Vector2(12, -62), new Vector2(80, 42), 20);
            string[] weapons = { "Sword", "Axe", "Bow" };
            string[] elementNames = { "Fire", "Water", "Wind" };
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                MakeButton(weapons[i], weaponIcons[i], panel, new Vector2(100 + i * 175, -10), i,
                    () => traversal.SelectWeapon(index));
                MakeButton(elementNames[i], elementIcons[i], panel, new Vector2(100 + i * 175, -60), i + 3,
                    () => elements.SelectElement(index));
            }
            status = Label("", panel, new Vector2(14, -114), new Vector2(620, 76), 18);
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                ownedEventSystem = new GameObject("Prototype EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                ownedEventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private void Update()
        {
            if (status == null) return;
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].color = (i < 3 ? i == (int)traversal.SelectedWeapon : i - 3 == (int)elements.SelectedElement)
                    ? new Color(0.18f, 0.4f, 0.55f) : new Color(0.17f, 0.2f, 0.25f);
            string effect = elements.SelectedElement == ElementKind.Fire
                ? $"Fire placeholder: {elements.BurnTickDamage:g} damage/tick, every {elements.BurnTickInterval:g}s for {elements.BurnDuration:g}s"
                : elements.SelectedElement == ElementKind.Wind
                    ? $"Wind placeholder: x{elements.AttackSpeedMultiplier:g} attack speed"
                    : "Water: passive undecided (neutral placeholder)";
            string ready = traversal.IsUsingUtility ? "active" : traversal.CooldownRemaining > 0
                ? $"cooldown {traversal.CooldownRemaining:0.0}s" : "ready";
            string actionHint = traversal.SelectedWeapon == WeaponKind.Axe ? "Jump, then Shift: slam"
                : traversal.SelectedWeapon == WeaponKind.Bow ? "Shift on ground: backward hop" : "Shift: forward dash";
            status.text = $"{actionHint} ({ready}) | W / Space: jump\n{effect}\nCombat passives are data only; no damage or attack changes yet.";
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        private UnityEngine.UI.Text Label(string text, Transform parent, Vector2 pos, Vector2 size, int fontSize)
        {
            var label = Rect("Label", parent, pos, size).gameObject.AddComponent<UnityEngine.UI.Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.text = text;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }
        private void MakeButton(string name, Sprite sprite, Transform parent, Vector2 pos, int index, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(name, parent, pos, new Vector2(165, 42));
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            buttons[index] = image;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            button.onClick.AddListener(action);
            var icon = Rect("Icon", rect, new Vector2(7, -5), new Vector2(32, 32)).gameObject.AddComponent<UnityEngine.UI.Image>();
            icon.sprite = sprite;
            icon.color = index == 3 ? new Color(1f, 0.35f, 0.08f)
                : index == 4 ? new Color(0.15f, 0.6f, 1f)
                : index == 5 ? new Color(0.55f, 0.95f, 0.75f)
                : index == 2 ? new Color(0.75f, 0.55f, 0.32f) : new Color(0.85f, 0.87f, 0.9f);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            Label(name, rect, new Vector2(48, -8), new Vector2(110, 28), 20);
        }
        private void OnDestroy()
        {
            if (canvasObject != null) Destroy(canvasObject);
            if (ownedEventSystem != null) Destroy(ownedEventSystem);
        }
    }
}
