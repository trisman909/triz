using UnityEngine;

namespace Lanternfall.Gameplay.Combat
{
    [CreateAssetMenu(menuName = "Lanternfall/Combat/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "weapon.unset";
        [SerializeField] private string displayName = "Unnamed Weapon";
        [SerializeField, Min(0.1f)] private float damage = 12f;
        [SerializeField, Min(0.1f)] private float attacksPerSecond = 2f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 16f;
        [SerializeField, Min(0f)] private float knockback = 2f;
        [SerializeField, Range(0f, 1f)] private float criticalChance = 0.08f;
        [SerializeField] private DamageElement element;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public float Damage => damage;
        public float AttacksPerSecond => attacksPerSecond;
        public float ProjectileSpeed => projectileSpeed;
        public float Knockback => knockback;
        public float CriticalChance => criticalChance;
        public DamageElement Element => element;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string title,
            float baseDamage,
            float rate,
            float speed,
            float force,
            float crit,
            DamageElement damageElement)
        {
            stableId = id;
            displayName = title;
            damage = baseDamage;
            attacksPerSecond = rate;
            projectileSpeed = speed;
            knockback = force;
            criticalChance = crit;
            element = damageElement;
        }
#endif
    }
}

