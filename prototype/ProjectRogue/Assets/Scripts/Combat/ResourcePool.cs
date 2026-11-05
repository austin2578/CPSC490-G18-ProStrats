using UnityEngine;

public enum ResourceType
{
    None,
    Stamina,
    Mana
}

// Stamina and Mana.
// Health remains on the Health component.
public class ResourcePool : MonoBehaviour
{
    [Header("Maximum Resources")]

    [Min(0f)]
    public float maxStamina = 100f;

    [Min(0f)]
    public float maxMana = 50f;


    [Header("Regeneration")]

    [Min(0f)]
    public float staminaRegenPerSec = 15f;

    [Min(0f)]
    public float manaRegenPerSec = 3f;


    public float Stamina { get; private set; }

    public float Mana { get; private set; }

    public float MaxStamina => maxStamina;

    public float MaxMana => maxMana;


    private void Awake()
    {
        Refill();
    }


    private void Update()
    {
        Stamina = Mathf.Min(
            maxStamina,
            Stamina + staminaRegenPerSec * Time.deltaTime);

        Mana = Mathf.Min(
            maxMana,
            Mana + manaRegenPerSec * Time.deltaTime);
    }


    public bool TrySpend(ResourceType type, float cost)
    {
        if (type == ResourceType.None || cost <= 0f)
            return true;

        switch (type)
        {
            case ResourceType.Stamina:

                if (Stamina < cost)
                    return false;

                Stamina -= cost;
                return true;


            case ResourceType.Mana:

                if (Mana < cost)
                    return false;

                Mana -= cost;
                return true;


            default:
                return false;
        }
    }


    // Used later by ClassDefinition to establish
    // starting Stamina and Mana profiles.
    public void ConfigureMaxima(
        float stamina,
        float mana,
        bool refill = true)
    {
        maxStamina = Mathf.Max(0f, stamina);
        maxMana = Mathf.Max(0f, mana);

        if (refill)
        {
            Refill();
        }
        else
        {
            Stamina = Mathf.Min(Stamina, maxStamina);
            Mana = Mathf.Min(Mana, maxMana);
        }
    }


    public void Refill()
    {
        Stamina = maxStamina;
        Mana = maxMana;
    }


    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22
        };

        GUI.Label(
            new Rect(10, 10, 600, 100),
            $"Stamina {Stamina:0} / {maxStamina:0}\n" +
            $"Mana {Mana:0} / {maxMana:0}",
            style);
    }
}