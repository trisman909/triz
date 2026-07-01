using System;

namespace Lanternfall.Gameplay.Combat
{
    public enum DamageElement
    {
        Physical,
        Ember,
        Storm,
        Frost,
        Gloam,
        Radiance
    }

    public readonly struct DamageRequest
    {
        public DamageRequest(
            float baseDamage,
            float additiveBonus,
            float criticalChance,
            float criticalMultiplier,
            float armor,
            DamageElement element,
            float criticalRoll)
        {
            if (baseDamage < 0f) throw new ArgumentOutOfRangeException(nameof(baseDamage));
            BaseDamage = baseDamage;
            AdditiveBonus = additiveBonus;
            CriticalChance = Math.Max(0f, Math.Min(1f, criticalChance));
            CriticalMultiplier = Math.Max(1f, criticalMultiplier);
            Armor = Math.Max(0f, armor);
            Element = element;
            CriticalRoll = criticalRoll;
        }

        public float BaseDamage { get; }
        public float AdditiveBonus { get; }
        public float CriticalChance { get; }
        public float CriticalMultiplier { get; }
        public float Armor { get; }
        public DamageElement Element { get; }
        public float CriticalRoll { get; }
    }

    public readonly struct DamageResult
    {
        public DamageResult(float amount, bool critical, DamageElement element)
        {
            Amount = amount;
            Critical = critical;
            Element = element;
        }

        public float Amount { get; }
        public bool Critical { get; }
        public DamageElement Element { get; }
    }

    public static class DamageResolver
    {
        public static DamageResult Resolve(DamageRequest request)
        {
            bool critical = request.CriticalRoll < request.CriticalChance;
            float raw = request.BaseDamage * Math.Max(0f, 1f + request.AdditiveBonus);
            if (critical) raw *= request.CriticalMultiplier;
            // Smooth armor curve avoids hard immunity and behaves well at scale.
            float mitigated = raw * (100f / (100f + request.Armor));
            return new DamageResult(mitigated, critical, request.Element);
        }
    }
}
