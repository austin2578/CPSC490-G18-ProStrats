using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectRogue.Movement.Editor
{
    public static class PlaceholderSpriteSetup
    {
        private const string Root = "Assets/Game/Art/Placeholders";
        private static readonly string[] Paths =
        {
            "Weapons/Sword", "Weapons/Axe", "Weapons/Bow",
            "Elements/Fire", "Elements/Water", "Elements/Wind"
        };
        private static readonly Color[] Tints =
        {
            new Color(0.85f, 0.87f, 0.9f), new Color(0.85f, 0.87f, 0.9f),
            new Color(0.75f, 0.55f, 0.32f), new Color(1f, 0.35f, 0.08f),
            new Color(0.15f, 0.6f, 1f), new Color(0.55f, 0.95f, 0.75f)
        };

        [MenuItem("Project Rogue/Art/Prepare Placeholder Sprites")]
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before preparing assets.");
            string prefabFolder = Root + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder(Root, "Prefabs");
            Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                for (int i = 0; i < Paths.Length; i++)
                {
                    string assetPath = Root + "/" + Paths[i] + ".png";
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null) throw new InvalidOperationException("Missing texture: " + assetPath);
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 512;
                    importer.spritePivot = new Vector2(0.5f, 0.5f);
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.maxTextureSize = 512;
                    importer.SaveAndReimport();
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite == null || sprite.rect.size != new Vector2(512, 512))
                        throw new InvalidOperationException("Sprite import failed: " + assetPath);
                    string name = Path.GetFileName(Paths[i]);
                    var obj = new GameObject(name);
                    SceneManager.MoveGameObjectToScene(obj, preview);
                    var renderer = obj.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.color = Tints[i];
                    string prefabPath = prefabFolder + "/" + name + ".prefab";
                    if (PrefabUtility.SaveAsPrefabAsset(obj, prefabPath) == null)
                        throw new InvalidOperationException("Prefab creation failed: " + prefabPath);
                    UnityEngine.Object.DestroyImmediate(obj);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null || prefab.GetComponent<SpriteRenderer>().sprite != sprite)
                        throw new InvalidOperationException("Prefab verification failed: " + prefabPath);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("Six placeholder sprites and six colored visual prefabs imported and verified.");
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); }
        }
    }
}
