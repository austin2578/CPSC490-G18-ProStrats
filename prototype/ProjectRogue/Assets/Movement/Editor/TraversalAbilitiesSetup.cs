using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectRogue.Movement.Editor
{
    public static class TraversalAbilitiesSetup
    {
        [MenuItem("Project Rogue/Movement/Apply Tunable Abilities")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) throw new OperationCanceledException();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string path in new[] { "Assets/Scenes/MovementFoundation.unity", TraversalRoomSetup.RoomPath })
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    var player = GameObject.Find("Player");
                    var motor = player.GetComponent<PlayerMovement>();
                    var data = new SerializedObject(motor);
                    if (!data.FindProperty("jumpHeightMigrated").boolValue)
                    {
                        float speed = data.FindProperty("jumpSpeed").floatValue;
                        float gravity = Mathf.Abs(Physics2D.gravity.y) * data.FindProperty("gravityScale").floatValue;
                        data.FindProperty("jumpHeight").floatValue = speed * speed / (2f * gravity);
                        data.FindProperty("jumpHeightMigrated").boolValue = true;
                        data.ApplyModifiedPropertiesWithoutUndo();
                    }
                    if (!player.TryGetComponent<WeaponTraversal>(out _)) player.AddComponent<WeaponTraversal>();
                    if (!player.TryGetComponent<ElementPassives>(out _)) player.AddComponent<ElementPassives>();
                    if (!player.TryGetComponent<PrototypeLoadoutPanel>(out var panel)) panel = player.AddComponent<PrototypeLoadoutPanel>();
                    var panelData = new SerializedObject(panel);
                    string[] weapons = { "Sword", "Axe", "Bow" };
                    string[] elements = { "Fire", "Water", "Wind" };
                    for (int i = 0; i < 3; i++)
                    {
                        panelData.FindProperty("weaponIcons").GetArrayElementAtIndex(i).objectReferenceValue =
                            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Placeholders/Weapons/" + weapons[i] + ".png");
                        panelData.FindProperty("elementIcons").GetArrayElementAtIndex(i).objectReferenceValue =
                            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Placeholders/Elements/" + elements[i] + ".png");
                    }
                    panelData.ApplyModifiedPropertiesWithoutUndo();
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        [MenuItem("Project Rogue/Movement/Verify Abilities")]
        public static void Verify()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) throw new OperationCanceledException();
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var previous = Physics2D.simulationMode;
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                Physics2D.simulationMode = SimulationMode2D.Script;
                var floor = new GameObject("Floor");
                floor.transform.position = new Vector3(0, -0.5f, 0);
                floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
                var obj = new GameObject("Player");
                var capsule = obj.AddComponent<CapsuleCollider2D>();
                capsule.size = new Vector2(0.8f, 1.6f);
                capsule.sharedMaterial = MovementSetup.GetPlayerMaterial();
                var body = obj.AddComponent<Rigidbody2D>();
                var motor = obj.AddComponent<PlayerMovement>();
                var utility = obj.AddComponent<WeaponTraversal>();
                var passive = obj.AddComponent<ElementPassives>();
                motor.SendMessage("Awake");
                var physics = scene.GetPhysicsScene2D();
                Action<int> step = frames =>
                {
                    for (int i = 0; i < frames; i++)
                    {
                        motor.SendMessage("FixedUpdate");
                        physics.Simulate(Time.fixedDeltaTime);
                    }
                };
                Action reset = () =>
                {
                    motor.ResetMotion();
                    body.position = new Vector2(0, 2);
                    obj.transform.position = body.position;
                    Physics2D.SyncTransforms();
                    step(100);
                };
                Set(utility, "dashDistance", 4);
                Set(utility, "dashSpeed", 10);
                reset();
                utility.SelectWeapon(0);
                float start = body.position.x;
                utility.RequestUtility();
                step(22);
                Require(Mathf.Abs(body.position.x - start - 4) < 0.06f, "Dash distance must follow its configured value.");
                start = body.position.x;
                utility.RequestUtility();
                step(1);
                Require(!utility.IsUsingUtility && Mathf.Abs(body.position.x - start) < 0.01f,
                    "Cooldown must reject repeated dash activation.");
                var wall = new GameObject("Thin Wall");
                wall.transform.position = new Vector3(1.5f, 2, 0);
                wall.AddComponent<BoxCollider2D>().size = new Vector2(0.1f, 8);
                reset();
                Set(utility, "dashSpeed", 100);
                utility.RequestUtility();
                step(4);
                Require(body.position.x < 1.15f, "Fast dash must not pass through a thin wall.");
                UnityEngine.Object.DestroyImmediate(wall);
                reset();
                Set(motor, "jumpHeight", 3);
                float groundY = body.position.y;
                float apex = groundY;
                motor.RequestJump();
                for (int i = 0; i < 80; i++) { step(1); apex = Mathf.Max(apex, body.position.y); }
                Require(apex - groundY > 2.7f && apex - groundY < 3.1f, "Jump height must follow tuning.");
                Set(utility, "hopDistance", 2);
                Set(utility, "hopCooldown", 0);
                foreach (int facing in new[] { -1, 1 })
                {
                    reset();
                    utility.SelectWeapon(2);
                    motor.SetHorizontalInput(facing);
                    motor.SetHorizontalInput(0);
                    utility.RequestUtility();
                    step(14);
                    Require(Mathf.Abs(body.position.x + facing * 2) < 0.06f, "Hop must move the configured distance backward.");
                    utility.RequestUtility();
                    step(1);
                    Require(!utility.IsUsingUtility, "Bow must not grant another hop in midair.");
                }
                reset();
                utility.SelectWeapon(1);
                utility.RequestUtility();
                step(1);
                Require(!utility.IsUsingUtility, "Slam must require the player to be airborne.");
                motor.RequestJump(); step(6);
                Set(utility, "slamSpeed", 27);
                utility.RequestUtility(); step(1);
                Require(Mathf.Abs(body.linearVelocity.y + 27) < 0.01f, "Slam speed must follow tuning beyond normal fall limit.");
                step(80);
                Require(motor.IsGrounded && !utility.IsUsingUtility, "Slam must end on landing.");
                reset(); utility.SelectWeapon(0);
                utility.RequestUtility(); step(1);
                motor.RequestJump(); motor.ResetMotion(); step(1);
                Require(!utility.IsUsingUtility && utility.CooldownRemaining == 0 && body.gravityScale > 0 && body.linearVelocity.y <= 0,
                    "Respawn must clear utility, cooldown, pending jump, and gravity override.");
                Set(passive, "burnTickDamage", 5);
                passive.SelectElement(0);
                Require(passive.BurnTickDamage == 5, "Burn placeholder must expose configured tick damage.");
                Set(passive, "attackSpeedBonusPercent", 35);
                passive.SelectElement(2);
                Require(Mathf.Abs(passive.AttackSpeedMultiplier - 1.35f) < 0.001f, "Wind percentage must expose expected multiplier.");
                passive.SelectElement(1);
                Require(passive.BurnTickDamage == 0 && passive.AttackSpeedMultiplier == 1, "Water must remain neutral.");
                File.WriteAllText("Logs/abilities-verification.txt", "PASS: configurable jump height, dash distance/speed/cooldown/thin-wall collision, backward hop in both directions/no air hop, airborne-only slam speed/landing, respawn cleanup, configurable Fire/Wind placeholders and neutral Water.");
                Debug.Log(File.ReadAllText("Logs/abilities-verification.txt"));
            }
            finally
            {
                Physics2D.simulationMode = previous;
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
        private static void Set(UnityEngine.Object obj, string field, float value)
        {
            var data = new SerializedObject(obj);
            data.FindProperty(field).floatValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Require(bool success, string message)
        {
            if (!success) throw new InvalidOperationException(message);
        }
    }
}
