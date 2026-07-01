using System;
using UnityEngine;

namespace Lanternfall.Gameplay.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        DamageResult ApplyDamage(DamageRequest request);
    }

    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maximum = 100f;
        [SerializeField, Min(0f)] private float armor;
        [SerializeField] private bool destroyOnDeath;

        public event Action<float, float> Changed;
        public event Action<DamageResult> Damaged;
        public event Action Died;

        public float Current { get; private set; }
        public float Maximum => maximum;
        public bool IsAlive => Current > 0f;

        private void Awake() => Current = maximum;

        public DamageResult ApplyDamage(DamageRequest request)
        {
            if (!IsAlive) return new DamageResult(0f, false, request.Element);
            var armored = new DamageRequest(
                request.BaseDamage,
                request.AdditiveBonus,
                request.CriticalChance,
                request.CriticalMultiplier,
                request.Armor + armor,
                request.Element,
                request.CriticalRoll);
            DamageResult result = DamageResolver.Resolve(armored);
            Current = Mathf.Max(0f, Current - result.Amount);
            Damaged?.Invoke(result);
            Changed?.Invoke(Current, maximum);
            if (Current <= 0f)
            {
                Died?.Invoke();
                if (destroyOnDeath) Destroy(gameObject);
            }
            return result;
        }

        public void Configure(float maximumHealth, float armorValue, bool destroy)
        {
            maximum = Mathf.Max(1f, maximumHealth);
            armor = Mathf.Max(0f, armorValue);
            destroyOnDeath = destroy;
            Current = maximum;
        }

        public void Restore(float value)
        {
            Current = Mathf.Clamp(value, 0f, maximum);
            Changed?.Invoke(Current, maximum);
        }
    }
}
