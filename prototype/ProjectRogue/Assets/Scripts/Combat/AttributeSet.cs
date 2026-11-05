using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum AttributeType
{
    Vigor,
    Strength,
    Dexterity,
    Mind,
    Constitution
}

public class AttributeSet : MonoBehaviour
{
    [Header("Base Attributes")]

    [FormerlySerializedAs("vigor")]
    [SerializeField, Min(0)]
    private int baseVigor = 5;

    [FormerlySerializedAs("strength")]
    [SerializeField, Min(0)]
    private int baseStrength = 5;

    [FormerlySerializedAs("dexterity")]
    [SerializeField, Min(0)]
    private int baseDexterity = 5;

    [FormerlySerializedAs("mind")]
    [SerializeField, Min(0)]
    private int baseMind = 5;

    [FormerlySerializedAs("constitution")]
    [SerializeField, Min(0)]
    private int baseConstitution = 5;


    [Header("Run-Local Attribute Investments")]

    [SerializeField, Min(0)]
    private int investedVigor;

    [SerializeField, Min(0)]
    private int investedStrength;

    [SerializeField, Min(0)]
    private int investedDexterity;

    [SerializeField, Min(0)]
    private int investedMind;

    [SerializeField, Min(0)]
    private int investedConstitution;


    public event Action<AttributeType, int> AttributeChanged;


    // Keep this signature stable.
    // Existing combat and weapon scaling already depend on it.
    public int Get(AttributeType type)
    {
        return GetBase(type) + GetInvested(type);
    }


    public int GetBase(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.Vigor:
                return baseVigor;

            case AttributeType.Strength:
                return baseStrength;

            case AttributeType.Dexterity:
                return baseDexterity;

            case AttributeType.Mind:
                return baseMind;

            case AttributeType.Constitution:
                return baseConstitution;

            default:
                return 0;
        }
    }


    public int GetInvested(AttributeType type)
    {
        switch (type)
        {
            case AttributeType.Vigor:
                return investedVigor;

            case AttributeType.Strength:
                return investedStrength;

            case AttributeType.Dexterity:
                return investedDexterity;

            case AttributeType.Mind:
                return investedMind;

            case AttributeType.Constitution:
                return investedConstitution;

            default:
                return 0;
        }
    }


    public int TotalInvested =>
        investedVigor +
        investedStrength +
        investedDexterity +
        investedMind +
        investedConstitution;


    // Intended for PlayerProgression.
    // AttributeSet itself does not decide whether the player
    // has enough Attribute Points to spend.
    public bool AddInvestment(AttributeType type, int amount = 1)
    {
        if (amount <= 0)
            return false;

        switch (type)
        {
            case AttributeType.Vigor:
                investedVigor += amount;
                break;

            case AttributeType.Strength:
                investedStrength += amount;
                break;

            case AttributeType.Dexterity:
                investedDexterity += amount;
                break;

            case AttributeType.Mind:
                investedMind += amount;
                break;

            case AttributeType.Constitution:
                investedConstitution += amount;
                break;

            default:
                return false;
        }

        AttributeChanged?.Invoke(type, Get(type));
        return true;
    }


    // Returns how many Attribute Points were refunded.
    public int ResetInvestments()
    {
        int refunded = TotalInvested;

        if (refunded == 0)
            return 0;

        investedVigor = 0;
        investedStrength = 0;
        investedDexterity = 0;
        investedMind = 0;
        investedConstitution = 0;

        NotifyAllChanged();

        return refunded;
    }


    // Used later by ClassDefinition in Objective 1.1.
    // Starting classes configure BASE values, not run-local investments.
    public void SetBase(AttributeType type, int value)
    {
        value = Mathf.Max(0, value);

        switch (type)
        {
            case AttributeType.Vigor:
                baseVigor = value;
                break;

            case AttributeType.Strength:
                baseStrength = value;
                break;

            case AttributeType.Dexterity:
                baseDexterity = value;
                break;

            case AttributeType.Mind:
                baseMind = value;
                break;

            case AttributeType.Constitution:
                baseConstitution = value;
                break;

            default:
                return;
        }

        AttributeChanged?.Invoke(type, Get(type));
    }


    public void SetBaseProfile(
        int vigor,
        int strength,
        int dexterity,
        int mind,
        int constitution)
    {
        baseVigor = Mathf.Max(0, vigor);
        baseStrength = Mathf.Max(0, strength);
        baseDexterity = Mathf.Max(0, dexterity);
        baseMind = Mathf.Max(0, mind);
        baseConstitution = Mathf.Max(0, constitution);

        NotifyAllChanged();
    }


    private void NotifyAllChanged()
    {
        AttributeChanged?.Invoke(
            AttributeType.Vigor,
            Get(AttributeType.Vigor));

        AttributeChanged?.Invoke(
            AttributeType.Strength,
            Get(AttributeType.Strength));

        AttributeChanged?.Invoke(
            AttributeType.Dexterity,
            Get(AttributeType.Dexterity));

        AttributeChanged?.Invoke(
            AttributeType.Mind,
            Get(AttributeType.Mind));

        AttributeChanged?.Invoke(
            AttributeType.Constitution,
            Get(AttributeType.Constitution));
    }
}