using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProjectRogue.Movement.Editor
{
    public static class MovementSetup
    {
        private const string ScenePath = "Assets/Scenes/MovementFoundation.unity";

        [MenuItem("Project Rogue/Movement/Create Test Scene")]
        public static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/Settings/InputSystem_Actions.inputactions");
            if (input == null) throw new InvalidOperationException("Team input asset is missing.");
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            if (sprite == null) throw new InvalidOperationException("Test block sprite is unavailable.");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            VisualBlock("Floor", new Vector2(0, -1), new Vector2(22, 1), sprite, Color.gray);
            VisualBlock("Left Wall", new Vector2(-11, 3), new Vector2(1, 9), sprite, Color.gray);
            VisualBlock("Right Wall", new Vector2(11, 3), new Vector2(1, 9), sprite, Color.gray);
            VisualBlock("Low Platform", new Vector2(-2, 0.8f), new Vector2(3, 0.5f), sprite, Color.gray);
            VisualBlock("High Platform", new Vector2(2, 2.3f), new Vector2(3, 0.5f), sprite, Color.gray);
            VisualBlock("Ceiling Check", new Vector2(6, 2.5f), new Vector2(3, 0.5f), sprite, Color.gray);
            var player = new GameObject("Player");
            player.SetActive(false);
            player.transform.position = new Vector3(-7, 0.6f, 0);
            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(0.8f, 1.6f);
            renderer.color = Color.cyan;
            var capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.8f, 1.6f);
            capsule.sharedMaterial = GetPlayerMaterial();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<PlayerMovement>();
            var reader = player.AddComponent<PlayerMovementInput>();
            var serialized = new SerializedObject(reader);
            serialized.FindProperty("actions").objectReferenceValue = input;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            player.SetActive(true);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 3, -10);
            camera.orthographic = true;
            camera.orthographicSize = 7;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = player;
            Debug.Log("Movement foundation scene created. Use the existing Move and Jump bindings. " +
                "This is a movement test scene, not the completed Objective 2.1 traversal room.");
        }

        public static PhysicsMaterial2D GetPlayerMaterial()
        {
            const string path = "Assets/Movement/PlayerNoFriction.physicsMaterial2D";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (material == null)
            {
                material = new PhysicsMaterial2D("PlayerNoFriction");
                AssetDatabase.CreateAsset(material, path);
            }
            material.friction = 0f;
            material.bounciness = 0f;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void VisualBlock(string name, Vector2 position, Vector2 size, Sprite sprite, Color color)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            block.AddComponent<BoxCollider2D>().size = size;
        }

        // Save and restore the Editor scene setup around isolated verification. Also callable with -executeMethod.
        [MenuItem("Project Rogue/Movement/Verify Physics")]
        public static void VerifyPhysics()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            SimulationMode2D previousMode = Physics2D.simulationMode;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Physics2D.simulationMode = SimulationMode2D.Script;
            try
            {
                var floor = new GameObject("Floor");
                SceneManager.MoveGameObjectToScene(floor, scene);
                floor.transform.position = new Vector3(0, -0.5f, 0);
                floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
                var player = new GameObject("Player");
                SceneManager.MoveGameObjectToScene(player, scene);
                player.transform.position = new Vector3(0, 2, 0);
                var capsule = player.AddComponent<CapsuleCollider2D>();
                capsule.size = new Vector2(0.8f, 1.6f);
                capsule.sharedMaterial = GetPlayerMaterial();
                var body = player.AddComponent<Rigidbody2D>();
                var motor = player.AddComponent<PlayerMovement>();
                // Edit-mode verification invokes the same lifecycle/physics step explicitly.
                motor.SendMessage("Awake");
                PhysicsScene2D physics = scene.GetPhysicsScene2D();
                Action<int> step = frames =>
                {
                    for (int i = 0; i < frames; i++)
                    {
                        motor.SendMessage("FixedUpdate");
                        physics.Simulate(Time.fixedDeltaTime);
                    }
                };
                step(100);
                Require(motor.IsGrounded, "Player must fall and land on the floor.");
                float start = body.position.x;
                motor.SetHorizontalInput(1);
                step(30);
                Require(body.position.x > start + 1, "Right movement must change position.");
                motor.SetHorizontalInput(-1);
                step(60);
                Require(body.position.x < start, "Left movement must change position.");
                motor.SetHorizontalInput(0);
                step(30);
                Require(Mathf.Abs(body.linearVelocity.x) < 0.01f, "Release must stop horizontal movement.");
                motor.RequestJump();
                step(1);
                Require(body.linearVelocity.y > 0 && !motor.IsGrounded, "Grounded jump must launch.");
                float upwardSpeed = body.linearVelocity.y;
                motor.RequestJump();
                step(1);
                Require(body.linearVelocity.y < upwardSpeed, "Airborne jump must not relaunch.");
                step(100);
                Require(motor.IsGrounded, "Player must land after jumping.");
                UnityEngine.Object.DestroyImmediate(floor);
                var wall = new GameObject("Wall");
                SceneManager.MoveGameObjectToScene(wall, scene);
                wall.transform.position = new Vector3(body.position.x + 0.7f, body.position.y, 0);
                wall.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 10);
                step(1);
                Require(!motor.IsGrounded, "Wall contact must not count as ground.");
                motor.RequestJump();
                step(1);
                Require(body.linearVelocity.y <= 0, "Wall contact must not grant a jump.");
                foreach (int direction in new[] { -1, 1 })
                {
                    body.position = new Vector2(0f, 6f);
                    body.linearVelocity = Vector2.zero;
                    wall.transform.position = new Vector3(direction * 0.65f, 6f, 0f);
                    wall.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 30f);
                    Physics2D.SyncTransforms();
                    motor.SetHorizontalInput(direction);
                    step(30);
                    Require(body.position.y < 5.5f,
                        "Holding movement into either wall must not suspend the player.");
                    Require(!motor.IsGrounded, "Pressing into a wall must not grant ground support.");
                }
                motor.SetHorizontalInput(0f);
                step(150);
                Require(body.linearVelocity.y >= -21f, "Falling speed must remain bounded.");
                Debug.Log("Movement physics verification passed: falling/landing, left/right, stopping, jump, air-jump rejection, wall rejection, left/right wall sliding under held input, fall-speed limit.");
            }
            finally
            {
                Physics2D.simulationMode = previousMode;
                bool canRestore = previousSetup.Length > 0;
                foreach (SceneSetup entry in previousSetup)
                    if (string.IsNullOrEmpty(entry.path)) canRestore = false;
                if (canRestore) EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
