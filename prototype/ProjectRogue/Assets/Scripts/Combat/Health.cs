using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)]
    private float maxHealth = 100f;

    [SerializeField]
    private bool destroyOnDeath = true;


    public float Current { get; private set; }

    public float Max => maxHealth;

    public bool IsDead { get; private set; }


    public event Action<DamageInfo> Damaged;

    public event Action<GameObject> Died;


    private void Awake()
    {
        Current = maxHealth;
    }


    public void TakeDamage(DamageInfo info)
    {
        if (IsDead)
            return;

        Current = Mathf.Max(0f, Current - info.amount);

        Debug.Log(
            $"{name} took {info.amount:0.#} damage " +
            $"({Current:0.#}/{maxHealth:0.#})");

        Damaged?.Invoke(info);

        if (Current <= 0f)
        {
            IsDead = true;

            Died?.Invoke(info.source);

            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }


    // Used later by ClassDefinition to establish
    // a starting Health profile.
    public void ConfigureMax(float value, bool refill = true)
    {
        maxHealth = Mathf.Max(1f, value);

        if (refill)
        {
            Current = maxHealth;
            IsDead = false;
        }
        else
        {
            Current = Mathf.Min(Current, maxHealth);
        }
    }


    public void RestoreFull()
    {
        Current = maxHealth;
        IsDead = false;
    }
}