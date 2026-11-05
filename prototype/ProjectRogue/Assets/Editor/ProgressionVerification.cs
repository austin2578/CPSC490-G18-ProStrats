using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ProgressionVerification
{
    private const string ReportPath = "Logs/progression-verification.txt";

    [MenuItem("Project Rogue/Progression/Verify Objective 1.2")]
    public static void VerifyObjective12()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException(
                "Stop Play Mode before running progression verification.");

        GameObject testObject = null;
        var report = new StringBuilder();

        report.AppendLine("Project Rogue - Objective 1.2 Verification");
        report.AppendLine("Attributes / Resources / Respec");
        report.AppendLine();

        try
        {
            testObject = new GameObject("Objective1.2_Verification");

            AttributeSet attributes =
                testObject.AddComponent<AttributeSet>();

            PlayerProgression progression =
                testObject.AddComponent<PlayerProgression>();

            ResourcePool resources =
                testObject.AddComponent<ResourcePool>();

            Health health =
                testObject.AddComponent<Health>();

            // Initialize components for this temporary editor test object.
            testObject.SendMessage(
                "Awake",
                SendMessageOptions.DontRequireReceiver);


            // ---------------------------------------------------------
            // 1. FIVE PRIMARY ATTRIBUTES
            // ---------------------------------------------------------

            attributes.SetBaseProfile(
                vigor: 11,
                strength: 12,
                dexterity: 13,
                mind: 14,
                constitution: 15);

            Require(
                attributes.Get(AttributeType.Vigor) == 11,
                "Vigor value incorrect.");

            Require(
                attributes.Get(AttributeType.Strength) == 12,
                "Strength value incorrect.");

            Require(
                attributes.Get(AttributeType.Dexterity) == 13,
                "Dexterity value incorrect.");

            Require(
                attributes.Get(AttributeType.Mind) == 14,
                "Mind value incorrect.");

            Require(
                attributes.Get(AttributeType.Constitution) == 15,
                "Constitution value incorrect.");

            report.AppendLine(
                "PASS - All five primary attributes are present and readable.");


            // ---------------------------------------------------------
            // 2. RUN-LOCAL LEVELING + 3 ATTRIBUTE POINTS
            // ---------------------------------------------------------

            Require(
                progression.Level == 1,
                "Player should begin at level 1.");

            Require(
                progression.UnspentAttributePoints == 0,
                "Player should begin with 0 unspent AP.");

            int xpNeeded = progression.XpToNextLevel;

            progression.GainXp(xpNeeded);

            Require(
                progression.Level == 2,
                "Gaining one level's XP should raise the player to level 2.");

            Require(
                progression.UnspentAttributePoints == 3,
                "Each level must award exactly 3 Attribute Points.");

            report.AppendLine(
                "PASS - Leveling awards exactly 3 Attribute Points per level.");


            // ---------------------------------------------------------
            // 3. ATTRIBUTE POINT INVESTMENT
            // ---------------------------------------------------------

            bool strengthSpent =
                progression.TryInvest(AttributeType.Strength);

            bool dexteritySpent =
                progression.TryInvest(AttributeType.Dexterity);

            bool mindSpent =
                progression.TryInvest(AttributeType.Mind);

            Require(
                strengthSpent && dexteritySpent && mindSpent,
                "The player should be able to invest available AP.");

            Require(
                progression.UnspentAttributePoints == 0,
                "Three investments should consume all 3 AP.");

            Require(
                attributes.Get(AttributeType.Strength) == 13,
                "Strength should increase after investment.");

            Require(
                attributes.Get(AttributeType.Dexterity) == 14,
                "Dexterity should increase after investment.");

            Require(
                attributes.Get(AttributeType.Mind) == 15,
                "Mind should increase after investment.");

            Require(
                attributes.TotalInvested == 3,
                "AttributeSet should track 3 invested points.");

            Require(
                !progression.TryInvest(AttributeType.Vigor),
                "Investment should fail when no AP remains.");

            report.AppendLine(
                "PASS - Attribute Points can be invested and cannot be overspent.");


            // ---------------------------------------------------------
            // 4. RESPEC + REFUND
            // ---------------------------------------------------------

            int refunded = progression.Respec();

            Require(
                refunded == 3,
                "Respec should refund all 3 invested AP.");

            Require(
                progression.UnspentAttributePoints == 3,
                "Refunded AP should return to the player.");

            Require(
                attributes.TotalInvested == 0,
                "Respec should clear all run-local investments.");

            Require(
                attributes.Get(AttributeType.Strength) == 12,
                "Strength should return to its base value after respec.");

            Require(
                attributes.Get(AttributeType.Dexterity) == 13,
                "Dexterity should return to its base value after respec.");

            Require(
                attributes.Get(AttributeType.Mind) == 14,
                "Mind should return to its base value after respec.");

            report.AppendLine(
                "PASS - Respec returns every invested Attribute Point.");


            // ---------------------------------------------------------
            // 5. REASSIGN REFUNDED POINTS
            // ---------------------------------------------------------

            Require(
                progression.TryInvest(AttributeType.Constitution),
                "Refunded AP should be reusable.");

            Require(
                progression.TryInvest(AttributeType.Constitution),
                "Second refunded AP should be reusable.");

            Require(
                progression.TryInvest(AttributeType.Vigor),
                "Third refunded AP should be reusable.");

            Require(
                progression.UnspentAttributePoints == 0,
                "All refunded AP should be spendable again.");

            Require(
                attributes.Get(AttributeType.Constitution) == 17,
                "Constitution should receive reassigned AP.");

            Require(
                attributes.Get(AttributeType.Vigor) == 12,
                "Vigor should receive reassigned AP.");

            report.AppendLine(
                "PASS - Refunded Attribute Points can be reassigned.");


            // ---------------------------------------------------------
            // 6. STAMINA + MANA
            // ---------------------------------------------------------

            resources.ConfigureMaxima(
                stamina: 120f,
                mana: 80f,
                refill: true);

            Require(
                Approximately(resources.MaxStamina, 120f),
                "Maximum Stamina configuration failed.");

            Require(
                Approximately(resources.MaxMana, 80f),
                "Maximum Mana configuration failed.");

            Require(
                Approximately(resources.Stamina, 120f),
                "Stamina should refill to its configured maximum.");

            Require(
                Approximately(resources.Mana, 80f),
                "Mana should refill to its configured maximum.");

            Require(
                resources.TrySpend(ResourceType.Stamina, 20f),
                "Spending available Stamina should succeed.");

            Require(
                resources.TrySpend(ResourceType.Mana, 30f),
                "Spending available Mana should succeed.");

            Require(
                Approximately(resources.Stamina, 100f),
                "Stamina was not reduced correctly.");

            Require(
                Approximately(resources.Mana, 50f),
                "Mana was not reduced correctly.");

            Require(
                !resources.TrySpend(ResourceType.Stamina, 101f),
                "Overspending Stamina should fail.");

            Require(
                !resources.TrySpend(ResourceType.Mana, 51f),
                "Overspending Mana should fail.");

            report.AppendLine(
                "PASS - Stamina and Mana configure, spend, and reject overspending.");


            // ---------------------------------------------------------
            // 7. HEALTH
            // ---------------------------------------------------------

            health.ConfigureMax(
                value: 150f,
                refill: true);

            Require(
                Approximately(health.Max, 150f),
                "Maximum Health configuration failed.");

            Require(
                Approximately(health.Current, 150f),
                "Health should refill to its configured maximum.");

            health.TakeDamage(new DamageInfo
            {
                source = null,
                amount = 25f
            });

            Require(
                Approximately(health.Current, 125f),
                "Health did not decrease correctly after damage.");

            health.RestoreFull();

            Require(
                Approximately(health.Current, 150f),
                "RestoreFull should restore maximum Health.");

            Require(
                !health.IsDead,
                "A restored player should not be dead.");

            report.AppendLine(
                "PASS - Health configures, takes damage, and restores correctly.");


            // ---------------------------------------------------------
            // SUCCESS
            // ---------------------------------------------------------

            report.AppendLine();
            report.AppendLine(
                "PASS - Objective 1.2 automated verification completed successfully.");

            WriteReport(report.ToString());

            Debug.Log(
                "Objective 1.2 verification PASSED.\n" +
                $"Report: {ReportPath}");
        }
        catch (Exception exception)
        {
            report.AppendLine();
            report.AppendLine("FAIL");
            report.AppendLine(exception.Message);

            WriteReport(report.ToString());

            Debug.LogError(
                "Objective 1.2 verification FAILED: " +
                exception.Message);

            throw;
        }
        finally
        {
            if (testObject != null)
                UnityEngine.Object.DestroyImmediate(testObject);
        }
    }


    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }


    private static bool Approximately(float a, float b)
    {
        return Mathf.Approximately(a, b);
    }


    private static void WriteReport(string contents)
    {
        Directory.CreateDirectory("Logs");
        File.WriteAllText(ReportPath, contents);
    }
}