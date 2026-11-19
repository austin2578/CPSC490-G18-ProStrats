using System;
using UnityEngine;
using System.Reflection;

namespace ProjectRogue.Run
{
    // How a run begins; owns RunSession and applies its effects onto the player allocated Attribute Points -> AttributeSet, Heal -> Health
    [DefaultExecutionOrder(-50)]
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] private RunTuning tuning = new RunTuning();
        [SerializeField, Tooltip("0 = new random seed every run. Set a number to replay the same rolls.")]
        private int seed;
        [SerializeField] private AttributeSet playerAttributes;
        [SerializeField] private Health playerHealth;
        [SerializeField] private bool startRunOnAwake = true;

        private static readonly AttributeType[] AllAttributes =
            (AttributeType[])Enum.GetValues(typeof(AttributeType));
        private readonly int[] classBaseline = new int[AllAttributes.Length];
        private bool baselineCaptured;

        public static RunManager Instance { get; private set; }
        public RunSession Session { get; private set; }
        public event Action<RunSession> RunStarted;

        public GameObject Player
        {
            get
            {
                if (playerAttributes != null) return playerAttributes.gameObject;
                return playerHealth != null ? playerHealth.gameObject : null;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("A second RunManager was found and disabled.", this);
                enabled = false;
                return;
            }
            Instance = this;

            if (playerAttributes == null)
            {
                var combat = FindFirstObjectByType<PlayerCombat>();
                playerAttributes = combat != null ? combat.GetComponent<AttributeSet>() : FindFirstObjectByType<AttributeSet>();
            }
            if (playerHealth == null && playerAttributes != null) playerHealth = playerAttributes.GetComponent<Health>();
            if (playerAttributes == null)
                Debug.LogWarning("RunManager: no AttributeSet on the player, so Attribute Points will be tracked but not applied.", this);

            if (startRunOnAwake) StartNewRun();
        }

        private void OnDestroy()
        {
            Unhook();
            if (Instance == this) Instance = null;
        }

        [ContextMenu("Start New Run")]
        public void StartNewRun()
        {
            Unhook();
            if (playerAttributes != null)
            {
                if (!baselineCaptured)
                {
                    // Whatever the AttributeSet holds at the start is baseline
                    foreach (AttributeType t in AllAttributes) classBaseline[(int)t] = playerAttributes.Get(t);
                    baselineCaptured = true;
                }
                else
                {
                    // attribute are run-local: return player to baseline
                    foreach (AttributeType t in AllAttributes) Write(playerAttributes, t, classBaseline[(int)t]);
                }
            }

            int runSeed = seed != 0 ? seed : Environment.TickCount;
            Session = new RunSession(tuning, runSeed, t => baselineCaptured ? classBaseline[(int)t] : 0);
            Session.AttributesChanged += ApplyAttributes;
            Session.HealRequested += OnHeal;
            Session.RewardGranted += OnReward;
            Session.Progress.LeveledUp += OnLevelUp;
            Debug.Log($"Run started (seed {runSeed}).", this);
            RunStarted?.Invoke(Session);
        }

        public bool IsPlayer(GameObject go)
        {
            GameObject player = Player;
            return go != null && player != null && (go == player || go.transform.IsChildOf(player.transform));
        }

        private void Unhook()
        {
            if (Session == null) return;
            Session.AttributesChanged -= ApplyAttributes;
            Session.HealRequested -= OnHeal;
            Session.RewardGranted -= OnReward;
            Session.Progress.LeveledUp -= OnLevelUp;
        }

        private void ApplyAttributes()
        {
            if (playerAttributes == null || Session == null) return;
            foreach (AttributeType t in AllAttributes) Write(playerAttributes, t, Session.GetAttribute(t));
        }

        private void OnHeal(int amount)
        {
            if (playerHealth == null) return;
            if (!PlayerStatBridge.TryHeal(playerHealth, amount) && !warnedHeal)
            {
                warnedHeal = true;
                Debug.LogWarning("RunManager: Health has no Heal(float) or Heal(int) method, so Heal rewards are tracked but not applied.", this);
            }
        }

        private void OnReward(GrantedReward reward) => Debug.Log($"[Run] {reward}", this);

        private void OnLevelUp(int level, int points) =>
            Debug.Log($"[Run] Level {level}! +{points} Attribute Points", this);

        private bool warnedHeal;
        private bool warnedAttributes;

        private void Write(AttributeSet set, AttributeType type, int value)
        {
            if (PlayerStatBridge.TrySetAttribute(set, type, value) || warnedAttributes) return;
            warnedAttributes = true;
            Debug.LogWarning($"RunManager: couldn't find a way to set {type} on {set.GetType().Name}. " +
                             "Add a public Set(AttributeType, int) method (or a public int field/property per attribute) " +
                             "so allocated Attribute Points affect combat.", this);
        }
    }

    // AttributeSet
    internal static class PlayerStatBridge
    {
        private const BindingFlags Members =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
        private static readonly string[] SetterNames = { "Set", "SetAttribute", "SetValue", "SetBase" };

        public static bool TrySetAttribute(AttributeSet set, AttributeType type, int value)
        {
            if (set == null) return false;
            Type t = set.GetType();

            foreach (string methodName in SetterNames)
            {
                MethodInfo m = t.GetMethod(methodName, Members, null, new[] { typeof(AttributeType), typeof(int) }, null);
                if (m == null) continue;
                m.Invoke(set, new object[] { type, value });
                return true;
            }

            string name = type.ToString();
            FieldInfo field = t.GetField(name, Members);
            if (field != null && field.FieldType == typeof(int) && !field.IsInitOnly)
            {
                field.SetValue(set, value);
                return true;
            }

            PropertyInfo property = t.GetProperty(name, Members);
            if (property != null && property.PropertyType == typeof(int) && property.GetSetMethod(true) != null)
            {
                property.SetValue(set, value);
                return true;
            }
            return false;
        }

        public static bool TryHeal(Health health, int amount)
        {
            if (health == null) return false;
            Type t = health.GetType();
            MethodInfo m = t.GetMethod("Heal", Members, null, new[] { typeof(float) }, null);
            if (m != null) { m.Invoke(health, new object[] { (float)amount }); return true; }
            m = t.GetMethod("Heal", Members, null, new[] { typeof(int) }, null);
            if (m != null) { m.Invoke(health, new object[] { amount }); return true; }
            return false;
        }
    }
}
