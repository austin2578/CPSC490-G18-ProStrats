using UnityEngine;

namespace ProjectRogue.Movement
{
    public enum ElementKind { Fire, Water, Wind }

    public sealed class ElementPassives : MonoBehaviour
    {
        [SerializeField] private ElementKind selectedElement;
        [Header("Fire - Combat Placeholder Only")]
        [SerializeField, Min(0f), Tooltip("Proposed damage per burn tick. Does not apply damage yet.")]
        private float burnTickDamage = 1f;
        [SerializeField, Min(0.05f), Tooltip("Proposed seconds between ticks. No burn timer is running yet.")]
        private float burnTickInterval = 1f;
        [SerializeField, Min(0f)] private float burnDuration = 3f;
        [Header("Wind - Combat Placeholder Only")]
        [SerializeField, Min(0f), Tooltip("20 means +20%, or a 1.2 attack-speed multiplier. No attacks are changed yet.")]
        private float attackSpeedBonusPercent = 20f;

        public ElementKind SelectedElement => selectedElement;
        public float BurnTickDamage => selectedElement == ElementKind.Fire ? Mathf.Max(0, burnTickDamage) : 0;
        public float BurnTickInterval => Mathf.Max(0.05f, burnTickInterval);
        public float BurnDuration => Mathf.Max(0, burnDuration);
        public float AttackSpeedMultiplier => selectedElement == ElementKind.Wind ? 1f + Mathf.Max(0, attackSpeedBonusPercent) / 100f : 1f;
        public void SelectElement(int index)
        {
            if (index >= 0 && index <= 2) selectedElement = (ElementKind)index;
        }
        // Water is deliberately neutral until its passive is designed.
    }
}
