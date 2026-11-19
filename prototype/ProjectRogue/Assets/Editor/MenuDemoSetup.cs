using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;

public static class MenuDemoSetup
{
    const string Title = "Assets/Scenes/TitleScreen.unity";
    const string Arena = "Assets/PlayableDemo/PlayableArena.unity";

    [MenuItem("Project Rogue/UI/Create menu demo scenes")]
    public static void Create()
    {
        CreateScene(Title, true);
        CreateScene(Arena, false);
        var others = EditorBuildSettings.scenes.Where(s => s.path != Title && s.path != Arena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Title, true),
            new EditorBuildSettingsScene(Arena, true) }.Concat(others).ToArray();
        AssetDatabase.SaveAssets();
    }

    static void CreateScene(string path, bool title)
    {
        if (File.Exists(path)) return; // Never overwrite scene edits on a subsequent setup run.
        var previous = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var root = new GameObject(title ? "Menu Flow" : "Playable Arena");
        if (title) root.AddComponent<ProjectRogue.UI.MenuFlow>();
        else root.AddComponent<PlayableArena>();
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.CloseScene(scene, true);
        if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
    }

    // For -executeMethod; output path stays outside the repository and comes from the caller.
    public static void BuildWindows()
    {
        string output = System.Environment.GetEnvironmentVariable("PROJECT_ROGUE_UI_BUILD");
        if (string.IsNullOrWhiteSpace(output)) throw new System.InvalidOperationException("Set PROJECT_ROGUE_UI_BUILD to the output .exe path.");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { Title, Arena }, locationPathName = output,
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
        });
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(output), "build-result.txt"),
            report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("UI demo build failed: " + report.summary.result);
    }
}
