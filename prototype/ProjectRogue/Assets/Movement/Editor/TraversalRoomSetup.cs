using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectRogue.Movement.Editor
{
    public static class TraversalRoomSetup
    {
        public const string RoomPath = "Assets/Scenes/TraversalRoom.unity";
        public const string ReportPath = "Logs/traversal-room-result.txt";

        [MenuItem("Project Rogue/Movement/Create Traversal Room")]
        public static void CreateRoom()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before creating the room.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new OperationCanceledException("Scene creation cancelled.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(RoomPath) != null)
            {
                EditorSceneManager.OpenScene(RoomPath);
                return;
            }
            Scene source = SceneManager.GetSceneByPath("Assets/Scenes/MovementFoundation.unity");
            if (!source.IsValid() || !source.isLoaded)
                source = EditorSceneManager.OpenScene("Assets/Scenes/MovementFoundation.unity", OpenSceneMode.Additive);
            PlayerMovement template = null;
            foreach (GameObject root in source.GetRootGameObjects())
                if (root.TryGetComponent(out PlayerMovement candidate)) template = candidate;
            if (template == null) throw new InvalidOperationException("Movement test player is missing.");
            Scene roomScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(roomScene);
            var player = UnityEngine.Object.Instantiate(template.gameObject);
            player.name = "Player";
            SceneManager.MoveGameObjectToScene(player, roomScene);
            player.transform.position = new Vector3(-11, 0.35f, 0);
            Sprite sprite = player.GetComponent<SpriteRenderer>().sprite;
            Block("Entrance Floor", new Vector2(-10, -1), new Vector2(6, 1), sprite, Color.gray);
            Block("Platform A", new Vector2(-5.5f, 0.5f), new Vector2(3, 0.5f), sprite, Color.gray);
            Block("Middle Landing", new Vector2(0, -1), new Vector2(4, 1), sprite, Color.gray);
            Block("Platform B", new Vector2(4.5f, 0.6f), new Vector2(2.5f, 0.5f), sprite, Color.gray);
            Block("Exit Floor", new Vector2(9.5f, -1), new Vector2(6, 1), sprite, Color.gray);
            Block("Left Boundary", new Vector2(-13.5f, 2), new Vector2(1, 14), sprite, Color.gray);
            Block("Right Boundary", new Vector2(13.5f, 2), new Vector2(1, 14), sprite, Color.gray);
            var entrance = Marker("Entrance", new Vector2(-11.8f, 0.4f), new Vector2(0.3f, 1.8f), sprite, Color.blue);
            var spawn = new GameObject("Entrance Spawn").transform;
            spawn.position = player.transform.position;
            var exit = Marker("Exit", new Vector2(11.2f, 0.5f), new Vector2(0.8f, 2), sprite, Color.yellow);
            var volume = exit.AddComponent<BoxCollider2D>();
            volume.size = new Vector2(0.8f, 2);
            volume.isTrigger = true;
            var room = exit.AddComponent<TraversalRoom>();
            var serialized = new SerializedObject(room);
            serialized.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerMovement>();
            serialized.FindProperty("entrance").objectReferenceValue = spawn;
            serialized.FindProperty("exitVolume").objectReferenceValue = volume;
            serialized.FindProperty("exitMarker").objectReferenceValue = exit.GetComponent<SpriteRenderer>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 9;
            camera.transform.position = new Vector3(0, 2, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            if (!EditorSceneManager.SaveScene(roomScene, RoomPath))
                throw new InvalidOperationException("Traversal scene save failed.");
            EditorSceneManager.OpenScene(RoomPath, OpenSceneMode.Single);
        }

        private static GameObject Marker(string name, Vector2 position, Vector2 size, Sprite sprite, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = -1;
            return obj;
        }

        private static void Block(string name, Vector2 position, Vector2 size, Sprite sprite, Color color)
        {
            var obj = Marker(name, position, size, sprite, color);
            obj.GetComponent<SpriteRenderer>().sortingOrder = 0;
            obj.AddComponent<BoxCollider2D>().size = size;
        }

        [MenuItem("Project Rogue/Movement/Verify Traversal Room")]
        public static void VerifyRoom()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before verification.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var previousMode = Physics2D.simulationMode;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(RoomPath, OpenSceneMode.Single);
                var player = GameObject.Find("Player").GetComponent<PlayerMovement>();
                var room = GameObject.Find("Exit").GetComponent<TraversalRoom>();
                var body = player.GetComponent<Rigidbody2D>();
                player.SendMessage("Awake");
                room.SendMessage("Awake");
                Physics2D.simulationMode = SimulationMode2D.Script;
                PhysicsScene2D physics = scene.GetPhysicsScene2D();
                Action step = () =>
                {
                    player.SendMessage("FixedUpdate");
                    physics.Simulate(Time.fixedDeltaTime);
                    room.SendMessage("FixedUpdate");
                };
                var utility = player.GetComponent<WeaponTraversal>();
                var passive = player.GetComponent<ElementPassives>();
                foreach (WeaponKind weapon in Enum.GetValues(typeof(WeaponKind)))
                foreach (ElementKind element in Enum.GetValues(typeof(ElementKind)))
                {
                    utility.SelectWeapon((int)weapon);
                    passive.SelectElement((int)element);
                    room.RestartRoom();
                    Physics2D.SyncTransforms();
                    for (int i = 0; i < 60; i++) step();
                    Require(!room.IsComplete && player.IsGrounded, "Room must start grounded and incomplete.");
                    float[] jumpAt = { -7.6f, -4.3f, 1.6f, 5.3f };
                    int jump = 0;
                    player.SetHorizontalInput(1);
                    for (int frame = 0; frame < 1000 && !room.IsComplete; frame++)
                    {
                        if (jump < jumpAt.Length && player.IsGrounded && body.position.x >= jumpAt[jump])
                        {
                            player.RequestJump();
                            jump++;
                        }
                        step();
                    }
                    Require(room.IsComplete && room.FallCount == 0,
                        "Mandatory route failed for " + weapon + "/" + element);
                }
                Require(GameObject.Find("Exit").GetComponent<SpriteRenderer>().color == Color.green,
                    "Exit must visibly turn green on completion.");
                body.position = new Vector2(0, -6);
                body.linearVelocity = new Vector2(6, -20);
                player.RequestJump();
                room.SendMessage("FixedUpdate");
                Require(room.FallCount == 1 && !room.IsComplete, "Falling must reset completion and count one fall.");
                Require(Vector2.Distance(body.position, new Vector2(-11, 0.35f)) < 0.01f,
                    "Falling must return the player to the entrance.");
                Require(body.linearVelocity == Vector2.zero, "Respawn must clear old velocity.");
                step();
                Require(body.linearVelocity.y <= 0, "A jump queued before respawn must not fire afterward.");
                for (int i = 0; i < 60; i++) step();
                Require(player.IsGrounded, "The player must be able to land and retry after respawn.");
                File.WriteAllText(ReportPath, "PASS: all 9 weapon/element combinations completed entrance-to-exit with grounded jumps and no falls; green completion; fall resets position, velocity, pending jump, and completion; retry landing works.");
                Debug.Log(File.ReadAllText(ReportPath));
            }
            finally
            {
                Physics2D.simulationMode = previousMode;
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
        private static void Require(bool success, string message)
        {
            if (!success) throw new InvalidOperationException(message);
        }
    }
}
