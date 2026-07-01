using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    public enum BossAttackPattern
    {
        BellShockwave,
        PrismCharge,
        RootSummon
    }

    [CreateAssetMenu(menuName = "Lanternfall/Bosses/Guardian")]
    public sealed class BossDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "boss.unset";
        [SerializeField] private string displayName = "Unnamed Guardian";
        [SerializeField] private BossAttackPattern pattern;
        [SerializeField, Min(1f)] private float health = 500f;
        [SerializeField, Min(0f)] private float armor = 10f;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0.1f)] private float attackDamage = 22f;
        [SerializeField, Min(0.05f)] private float telegraph = 0.9f;
        [SerializeField, Min(0.05f)] private float recovery = 1.1f;
        [SerializeField, Min(0f)] private float introDuration = 1.5f;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public BossAttackPattern Pattern => pattern;
        public float Health => health;
        public float Armor => armor;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float Telegraph => telegraph;
        public float Recovery => recovery;
        public float IntroDuration => introDuration;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string title,
            BossAttackPattern attackPattern,
            float maximumHealth,
            float armorValue,
            float speed,
            float damage,
            float windup,
            float recoverySeconds,
            float introSeconds)
        {
            stableId = id;
            displayName = title;
            pattern = attackPattern;
            health = maximumHealth;
            armor = armorValue;
            moveSpeed = speed;
            attackDamage = damage;
            telegraph = windup;
            recovery = recoverySeconds;
            introDuration = introSeconds;
        }
#endif
    }
}

