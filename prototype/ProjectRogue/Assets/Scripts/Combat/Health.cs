using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool destroyOnDeath = true;

    public float Current { get; private set; }
    public float Max => maxHealth;
    public bool IsDead { get; private set; }

    public event Action<DamageInfo> Damaged;
    public event Action<GameObject> Died; // passes the killer (info.source)

    void Awake() { Current = maxHealth; }

    public void TakeDamage(DamageInfo info)
    {
        if (IsDead) return;

        Current = Mathf.Max(0f, Current - info.amount);
        Debug.Log($"{name} took {info.amount:0.#} damage ({Current:0.#}/{maxHealth})");
        Damaged?.Invoke(info);

        if (Current <= 0f)
        {
            IsDead = true;
            Died?.Invoke(info.source);
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}