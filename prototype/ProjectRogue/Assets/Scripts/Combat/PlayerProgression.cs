using System;
using UnityEngine;

[RequireComponent(typeof(AttributeSet))]
public class PlayerProgression : MonoBehaviour
{
    [Header("Run-Local Progression")]
    [SerializeField, Min(1)]
    private int level = 1;

    [SerializeField, Min(0)]
    private int currentXp = 0;

    [SerializeField, Min(0)]
    private int unspentAttributePoints = 0;


    [Header("Leveling")]
    [SerializeField, Min(1)]
    private int baseXpToNextLevel = 100;

    [SerializeField, Min(0)]
    private int xpGrowthPerLevel = 25;

    [SerializeField, Min(1)]
    private int attributePointsPerLevel = 3;


    private AttributeSet attributes;


    public int Level => level;

    public int CurrentXp => currentXp;

    public int UnspentAttributePoints => unspentAttributePoints;

    public int XpToNextLevel =>
        baseXpToNextLevel + ((level - 1) * xpGrowthPerLevel);


    public event Action<int> LevelChanged;

    public event Action<int> XpChanged;

    public event Action<int> AttributePointsChanged;


    private void Awake()
    {
        attributes = GetComponent<AttributeSet>();
    }


    public void GainXp(int amount)
    {
        if (amount <= 0)
            return;

        currentXp += amount;

        while (currentXp >= XpToNextLevel)
        {
            int requiredXp = XpToNextLevel;

            currentXp -= requiredXp;

            LevelUp();
        }

        XpChanged?.Invoke(currentXp);
    }


    private void LevelUp()
    {
        level++;

        unspentAttributePoints += attributePointsPerLevel;

        LevelChanged?.Invoke(level);
        AttributePointsChanged?.Invoke(unspentAttributePoints);

        Debug.Log(
            $"Level up! Level {level}. " +
            $"+{attributePointsPerLevel} Attribute Points.");
    }


    public bool TryInvest(AttributeType type)
    {
        if (unspentAttributePoints <= 0)
            return false;

        if (!attributes.AddInvestment(type))
            return false;

        unspentAttributePoints--;

        AttributePointsChanged?.Invoke(unspentAttributePoints);

        Debug.Log(
            $"Invested 1 AP into {type}. " +
            $"{unspentAttributePoints} AP remaining.");

        return true;
    }


    public int Respec()
    {
        int refunded = attributes.ResetInvestments();

        if (refunded <= 0)
            return 0;

        unspentAttributePoints += refunded;

        AttributePointsChanged?.Invoke(unspentAttributePoints);

        Debug.Log(
            $"Respec refunded {refunded} Attribute Points. " +
            $"{unspentAttributePoints} AP available.");

        return refunded;
    }


    public void ResetRunProgression()
    {
        attributes.ResetInvestments();

        level = 1;
        currentXp = 0;
        unspentAttributePoints = 0;

        LevelChanged?.Invoke(level);
        XpChanged?.Invoke(currentXp);
        AttributePointsChanged?.Invoke(unspentAttributePoints);

        Debug.Log("Run-local progression reset.");
    }


    // Temporary helpers for prototype verification.

    [ContextMenu("Test: Gain 100 XP")]
    private void TestGainXp()
    {
        GainXp(100);
    }


    [ContextMenu("Test: Invest Strength")]
    private void TestInvestStrength()
    {
        TryInvest(AttributeType.Strength);
    }


    [ContextMenu("Test: Respec")]
    private void TestRespec()
    {
        Respec();
    }
}