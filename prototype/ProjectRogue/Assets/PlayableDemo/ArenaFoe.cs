using UnityEngine;

// Demo-only enemy adapter; damage goes through the existing shared resolver.
public sealed class ArenaFoe : MonoBehaviour
{
    public GameObject target;
    public AbilityData attack;
    public SpriteRenderer sprite;
    float nextAttack, warningUntil, flashUntil;
    Health health;
    Rigidbody2D body;
    public int AttackCount { get; private set; }

    void Awake()
    {
        health = GetComponent<Health>(); body = GetComponent<Rigidbody2D>();
        health.Damaged += OnDamage;
    }
    void OnDamage(DamageInfo info) { flashUntil = Time.time + .13f; }
    void FixedUpdate()
    {
        if (target == null || health.IsDead) { body.linearVelocity = new Vector2(0,body.linearVelocity.y); return; }
        float distance = Vector2.Distance(transform.position,target.transform.position);
        if (warningUntil > 0)
        {
            body.linearVelocity = new Vector2(0,body.linearVelocity.y);
            if (Time.time >= warningUntil)
            {
                warningUntil = 0; nextAttack = Time.time + 1.1f;
                var victim = target.GetComponent<Health>();
                if (distance < 1.8f && !victim.IsDead) { CombatResolver.Apply(gameObject,attack,9,victim); AttackCount++; }
            }
        }
        else if (distance < 1.5f && Time.time >= nextAttack) warningUntil = Time.time + .55f;
        else body.linearVelocity = new Vector2(distance < 9 && distance > 1.2f ? Mathf.Sign(target.transform.position.x-transform.position.x)*1.7f : 0, body.linearVelocity.y);
        sprite.color = Time.time < flashUntil ? Color.white : warningUntil > 0 ? new Color(1,.75f,.2f) : new Color(.9f,.25f,.35f);
    }
    void OnDestroy() { if (health != null) health.Damaged -= OnDamage; }
}
