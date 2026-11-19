using UnityEngine;

namespace ProjectRogue.Run
{
    // Grant XP and gold to nhe player after killing an enemy
    [RequireComponent(typeof(Health))]
    public sealed class EnemyRunRewards : MonoBehaviour
    {
        [SerializeField, Min(0)] private int experience = 15;
        [SerializeField, Min(0)] private int goldMin = 3;
        [SerializeField, Min(0)] private int goldMax = 8;

        private Health health;

        private void Awake() => health = GetComponent<Health>();
        private void OnEnable() => health.Died += OnDied;
        private void OnDisable() => health.Died -= OnDied;

        private void OnDied(GameObject killer)
        {
            RunManager run = RunManager.Instance;
            if (run == null || run.Session == null || !run.IsPlayer(killer)) return;
            run.Session.GrantKillRewards(experience, goldMin, goldMax, name);
        }
    }
}
