using System;
using System.IO;
using ProjectRogue.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StartingClassSetup
{
    private const string Folder =
        "Assets/ScriptableObjects/Classes";

    private const string KnightPath =
        Folder + "/Knight.asset";

    private const string MagePath =
        Folder + "/Mage.asset";

    private const string ThiefPath =
        Folder + "/Thief.asset";


    [MenuItem("Project Rogue/Classes/Create or Update Starting Classes")]
    public static void CreateStartingClasses()
    {
        EnsureFolder();

        ClassDefinition knight = CreateOrLoad(KnightPath);
        Configure(
            knight,
            "Knight",
            ClassKind.Knight,
            vigor: 7,
            strength: 8,
            dexterity: 4,
            mind: 3,
            constitution: 8,
            health: 140f,
            stamina: 120f,
            mana: 30f);

        ClassDefinition mage = CreateOrLoad(MagePath);
        Configure(
            mage,
            "Mage",
            ClassKind.Mage,
            vigor: 4,
            strength: 3,
            dexterity: 5,
            mind: 11,
            constitution: 7,
            health: 90f,
            stamina: 70f,
            mana: 130f);

        ClassDefinition thief = CreateOrLoad(ThiefPath);
        Configure(
            thief,
            "Thief",
            ClassKind.Thief,
            vigor: 5,
            strength: 4,
            dexterity: 10,
            mind: 4,
            constitution: 7,
            health: 105f,
            stamina: 140f,
            mana: 45f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Starting class assets created/updated: " +
            "Knight, Mage, Thief.");
    }


    [MenuItem("Project Rogue/Classes/Apply Starting Classes To Current Player")]
    public static void ApplyToCurrentPlayer()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException(
                "Stop Play Mode before applying starting classes.");

        CreateStartingClasses();

        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogError(
                "No active GameObject named 'Player' exists in the current scene.");
            return;
        }

        PlayerClassController controller =
            player.GetComponent<PlayerClassController>();

        if (controller == null)
            controller = Undo.AddComponent<PlayerClassController>(player);

        ClassDefinition knight =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(KnightPath);

        ClassDefinition mage =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(MagePath);

        ClassDefinition thief =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(ThiefPath);

        controller.ConfigureClasses(
            new[]
            {
                knight,
                mage,
                thief
            },
            startIndex: 0);

        EditorUtility.SetDirty(controller);

        EditorSceneManager.MarkSceneDirty(
            player.scene);

        Selection.activeGameObject = player;

        Debug.Log(
            "Player configured with Knight, Mage, and Thief. " +
            "Knight is the default starting class.");
    }


    private static void EnsureFolder()
    {
        if (!Directory.Exists(Folder))
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
        }
    }


    private static ClassDefinition CreateOrLoad(string path)
    {
        ClassDefinition definition =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(path);

        if (definition != null)
            return definition;

        definition =
            ScriptableObject.CreateInstance<ClassDefinition>();

        AssetDatabase.CreateAsset(
            definition,
            path);

        return definition;
    }


    private static void Configure(
        ClassDefinition definition,
        string displayName,
        ClassKind classKind,
        int vigor,
        int strength,
        int dexterity,
        int mind,
        int constitution,
        float health,
        float stamina,
        float mana)
    {
        definition.displayName = displayName;
        definition.classKind = classKind;

        definition.vigor = vigor;
        definition.strength = strength;
        definition.dexterity = dexterity;
        definition.mind = mind;
        definition.constitution = constitution;

        definition.maxHealth = health;
        definition.maxStamina = stamina;
        definition.maxMana = mana;

        EditorUtility.SetDirty(definition);
    }
}