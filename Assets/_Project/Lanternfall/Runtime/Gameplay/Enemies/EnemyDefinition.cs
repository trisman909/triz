using UnityEngine;

namespace Lanternfall.Gameplay.Enemies
{
    public enum EnemyArchetype
    {
        Melee,
        Archer,
        Tank,
        Flying,
        Burrowing,
        Explosive,
        Summoner,
        Assassin
    }

    public enum EliteModifier
    {
        None,
        Frenzied,
        Bulwark,
        Volatile
    }

    [CreateAssetMenu(menuName = "Lanternfall/Enemies/Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "enemy.unset";
        [SerializeField] private EnemyArchetype archetype;
        [SerializeField, Min(1f)] private float health = 50f;
        [SerializeField, Min(0f)] private float armor;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.5f;
        [SerializeField, Min(0.1f)] private float attackDamage = 10f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
        [SerializeField, Min(0.05f)] private float windup = 0.5f;
        [SerializeField, Min(0.05f)] private float recovery = 0.7f;

        public string StableId => stableId;
        public EnemyArchetype Archetype => archetype;
        public float Health => health;
        public float Armor => armor;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float Windup => windup;
        public float Recovery => recovery;

#if UNITY_EDITOR
        public void Configure(
            string id,
            EnemyArchetype role,
            float maximumHealth,
            float armorValue,
            float speed,
            float damage,
            float range,
            float telegraph,
            float recoverySeconds)
        {
            stableId = id;
            archetype = role;
            health = maximumHealth;
            armor = armorValue;
            moveSpeed = speed;
            attackDamage = damage;
            attackRange = range;
            windup = telegraph;
            recovery = recoverySeconds;
        }
#endif
    }
}

