using UnityEngine;

public enum ResourceType { None, Stamina, Mana }

// Stamina and Mana. Health lives on the Health component.
public class ResourcePool : MonoBehaviour
{
    public float maxStamina = 100f, maxMana = 50f;
    public float staminaRegenPerSec = 15f, manaRegenPerSec = 3f;

    public float Stamina { get; private set; }
    public float Mana { get; private set; }

    void Awake() { Stamina = maxStamina; Mana = maxMana; }

    void Update()
    {
        Stamina = Mathf.Min(maxStamina, Stamina + staminaRegenPerSec * Time.deltaTime);
        Mana = Mathf.Min(maxMana, Mana + manaRegenPerSec * Time.deltaTime);
    }

    public bool TrySpend(ResourceType type, float cost)
    {
        if (type == ResourceType.None || cost <= 0f) return true;

        if (type == ResourceType.Stamina)
        {
            if (Stamina < cost) return false;
            Stamina -= cost;
            return true;
        }

        if (Mana < cost) return false;
        Mana -= cost;
        return true;
    }
    void OnGUI() { GUI.Label(new Rect(10, 10, 300, 20), $"Stamina {Stamina:0} Mana {Mana:0}"); }
}