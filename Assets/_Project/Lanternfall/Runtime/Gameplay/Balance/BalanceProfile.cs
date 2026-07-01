using System.Collections.Generic;
using UnityEngine;

namespace Lanternfall.Gameplay.Balance
{
    public enum DifficultyTier { Story, Standard, Veteran, Eclipse }

    [CreateAssetMenu(menuName = "Lanternfall/Balance Profile")]
    public sealed class BalanceProfile : ScriptableObject
    {
        [SerializeField] private float[] enemyHealth = { .8f, 1f, 1.18f, 1.35f };
        [SerializeField] private float[] enemyDamage = { .7f, 1f, 1.15f, 1.3f };
        [SerializeField] private float[] rewardYield = { 1.15f, 1f, 1f, 1.1f };
        [SerializeField] private float targetRunMinutes = 35f;

        public float TargetRunMinutes => targetRunMinutes;

        public float HealthMultiplier(DifficultyTier tier, int biome) =>
            Tier(enemyHealth, tier) * BiomeGrowth(biome, .12f);

        public float DamageMultiplier(DifficultyTier tier, int biome) =>
            Tier(enemyDamage, tier) * BiomeGrowth(biome, .08f);

        public float RewardMultiplier(DifficultyTier tier, int biome) =>
            Tier(rewardYield, tier) * BiomeGrowth(biome, .04f);

        public List<string> Validate()
        {
            var errors = new List<string>();
            ValidateCurve(enemyHealth, "health", errors);
            ValidateCurve(enemyDamage, "damage", errors);
            ValidateCurve(rewardYield, "rewards", errors);
            if (targetRunMinutes < 25f || targetRunMinutes > 45f)
                errors.Add("Target run duration must stay within 25–45 minutes.");
            return errors;
        }

        private static float Tier(float[] values, DifficultyTier tier)
        {
            int index = Mathf.Clamp((int)tier, 0, values.Length - 1);
            return values[index];
        }

        private static float BiomeGrowth(int biome, float step) =>
            1f + Mathf.Clamp(biome, 0, 4) * step;

        private static void ValidateCurve(
            float[] curve, string label, List<string> errors)
        {
            if (curve == null || curve.Length != 4)
            {
                errors.Add($"Balance {label} curve must contain four tiers.");
                return;
            }
            for (int index = 0; index < curve.Length; index++)
                if (curve[index] <= 0f || curve[index] > 3f)
                    errors.Add($"Balance {label}[{index}] is outside (0, 3].");
        }
    }
}
