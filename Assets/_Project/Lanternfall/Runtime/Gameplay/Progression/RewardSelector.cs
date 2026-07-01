using System;
using System.Collections.Generic;
using Lanternfall.Core.Random;

namespace Lanternfall.Gameplay.Progression
{
    public static class RewardSelector
    {
        public static RelicDefinition Select(
            IReadOnlyList<RelicDefinition> catalog,
            ulong seed,
            Predicate<RelicDefinition> eligible = null)
        {
            if (catalog == null || catalog.Count == 0)
                throw new ArgumentException("Reward catalog is empty.", nameof(catalog));
            float total = 0f;
            for (int index = 0; index < catalog.Count; index++)
                if (catalog[index] != null && (eligible == null || eligible(catalog[index])))
                    total += catalog[index].RewardWeight;
            if (total <= 0f) throw new InvalidOperationException("No eligible rewards.");

            var random = new DeterministicRandom(
                SeedDerivation.Derive(seed, "relic-reward"));
            float roll = random.NextFloat() * total;
            RelicDefinition lastEligible = null;
            for (int index = 0; index < catalog.Count; index++)
            {
                RelicDefinition relic = catalog[index];
                if (relic == null || (eligible != null && !eligible(relic))) continue;
                lastEligible = relic;
                roll -= relic.RewardWeight;
                if (roll <= 0f) return relic;
            }
            return lastEligible;
        }
    }

    public static class ShopTransaction
    {
        public static bool TryBuy(
            RunWallet wallet,
            RunInventory inventory,
            RelicDefinition relic,
            CurrencyKind currency,
            int price)
        {
            if (wallet == null || inventory == null || relic == null) return false;
            if (inventory.Contains(relic.StableId)) return false;
            if (!wallet.TrySpend(currency, price)) return false;
            if (inventory.TryAdd(relic)) return true;
            wallet.Add(currency, price);
            return false;
        }
    }
}
