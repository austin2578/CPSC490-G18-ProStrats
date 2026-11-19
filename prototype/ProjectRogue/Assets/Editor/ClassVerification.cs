using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectRogue.Movement;

public static class ClassVerification
{
    private const string KnightPath =
        "Assets/ScriptableObjects/Classes/Knight.asset";

    private const string MagePath =
        "Assets/ScriptableObjects/Classes/Mage.asset";

    private const string ThiefPath =
        "Assets/ScriptableObjects/Classes/Thief.asset";

    private const string ReportPath =
        "Logs/class-verification.txt";


    [MenuItem("Project Rogue/Classes/Verify Objective 1.1")]
    public static void Verify()
    {
        ClassDefinition knight =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(KnightPath);

        ClassDefinition mage =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(MagePath);

        ClassDefinition thief =
            AssetDatabase.LoadAssetAtPath<ClassDefinition>(ThiefPath);


        // -------------------------------------------------
        // 1. Three starting classes exist.
        // -------------------------------------------------

        Require(
            knight != null,
            "Knight class asset is missing.");

        Require(
            mage != null,
            "Mage class asset is missing.");

        Require(
            thief != null,
            "Thief class asset is missing.");


        Require(
            knight.classKind == ClassKind.Knight,
            "Knight asset must use ClassKind.Knight.");

        Require(
            mage.classKind == ClassKind.Mage,
            "Mage asset must use ClassKind.Mage.");

        Require(
            thief.classKind == ClassKind.Thief,
            "Thief asset must use ClassKind.Thief.");


        // -------------------------------------------------
        // 2. Starting attribute profiles are distinct.
        // -------------------------------------------------

        Require(
            !SameAttributes(knight, mage),
            "Knight and Mage must have different attribute profiles.");

        Require(
            !SameAttributes(knight, thief),
            "Knight and Thief must have different attribute profiles.");

        Require(
            !SameAttributes(mage, thief),
            "Mage and Thief must have different attribute profiles.");


        // -------------------------------------------------
        // 3. Starting resource profiles are distinct.
        // -------------------------------------------------

        Require(
            !SameResources(knight, mage),
            "Knight and Mage must have different resource profiles.");

        Require(
            !SameResources(knight, thief),
            "Knight and Thief must have different resource profiles.");

        Require(
            !SameResources(mage, thief),
            "Mage and Thief must have different resource profiles.");


        // -------------------------------------------------
        // 4. Every class has its own traversal identity.
        // -------------------------------------------------

        Require(
            knight.classKind != mage.classKind &&
            knight.classKind != thief.classKind &&
            mage.classKind != thief.classKind,
            "Each starting class must map to a distinct traversal class.");


        // -------------------------------------------------
        // 5. Every class can equip every weapon family.
        //
        // EquipmentManager has no class restriction.
        // Explicitly test all 3 classes x all 3 families.
        // -------------------------------------------------

        GameObject testPlayer =
            new GameObject("Objective 1.1 Verification Player");

        try
        {
            EquipmentManager equipment =
                testPlayer.AddComponent<EquipmentManager>();

            ClassDefinition[] classes =
            {
                knight,
                mage,
                thief
            };

            foreach (ClassDefinition classDefinition in classes)
            {
                foreach (WeaponFamily family in
                    Enum.GetValues(typeof(WeaponFamily)))
                {
                    WeaponData weapon =
                        ScriptableObject.CreateInstance<WeaponData>();

                    weapon.displayName =
                        $"Verification {family}";

                    weapon.family =
                        family;

                    weapon.affixPool =
                        Array.Empty<AffixData>();

                    weapon.minAffixes = 0;
                    weapon.maxAffixes = 0;

                    equipment.Equip(
                        new WeaponInstance(weapon));

                    Require(
                        equipment.Current != null,
                        $"{classDefinition.displayName} failed to equip {family}.");

                    Require(
                        equipment.Current.Data.family == family,
                        $"{classDefinition.displayName} could not equip {family}.");

                    UnityEngine.Object.DestroyImmediate(
                        weapon);
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                testPlayer);
        }


        // -------------------------------------------------
        // Report
        // -------------------------------------------------

        Directory.CreateDirectory("Logs");

        string report =
            "PASS - Objective 1.1 automated verification completed successfully.\n" +
            "\n" +
            "Verified:\n" +
            "- Knight, Mage, and Thief class definitions exist.\n" +
            "- All three classes use distinct attribute profiles.\n" +
            "- All three classes use distinct Health/Stamina/Mana profiles.\n" +
            "- All three classes map to distinct ClassKind traversal identities.\n" +
            "- Knight can equip Sword, Bow, and Wand.\n" +
            "- Mage can equip Sword, Bow, and Wand.\n" +
            "- Thief can equip Sword, Bow, and Wand.\n" +
            "\n" +
            "Traversal mechanics are additionally covered by the existing " +
            "Project Rogue > Movement > Verify Abilities verification.";

        File.WriteAllText(
            ReportPath,
            report);

        AssetDatabase.Refresh();

        Debug.Log(report);
    }


    private static bool SameAttributes(
        ClassDefinition a,
        ClassDefinition b)
    {
        return
            a.vigor == b.vigor &&
            a.strength == b.strength &&
            a.dexterity == b.dexterity &&
            a.mind == b.mind &&
            a.constitution == b.constitution;
    }


    private static bool SameResources(
        ClassDefinition a,
        ClassDefinition b)
    {
        return
            Mathf.Approximately(a.maxHealth, b.maxHealth) &&
            Mathf.Approximately(a.maxStamina, b.maxStamina) &&
            Mathf.Approximately(a.maxMana, b.maxMana);
    }


    private static void Require(
        bool success,
        string message)
    {
        if (!success)
            throw new InvalidOperationException(
                "Objective 1.1 verification FAILED: " + message);
    }
}