using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Progression;
using UnityEngine;
using NUnit.Framework;

namespace Lanternfall.Tests
{
    public sealed class DodgeTimingModelTests
    {
        [Test]
        public void DodgeHasBoundedInvulnerabilityAndCooldown()
        {
            var dodge = new DodgeTimingModel(0.3f, 0.2f, 0.7f);
            Assert.That(dodge.TryStart(), Is.True);
            Assert.That(dodge.IsInvulnerable, Is.True);
            dodge.Tick(0.21f);
            Assert.That(dodge.IsInvulnerable, Is.False);
            Assert.That(dodge.IsDodging, Is.True);
            dodge.Tick(0.1f);
            Assert.That(dodge.IsDodging, Is.False);
            Assert.That(dodge.TryStart(), Is.False);
            dodge.Tick(0.4f);
            Assert.That(dodge.TryStart(), Is.True);
        }

        [Test]
        public void DamagePipelineAppliesCriticalHitThenArmorCurve()
        {
            DamageResult result = DamageResolver.Resolve(new DamageRequest(
                100f, 0.2f, 0.5f, 2f, 100f, DamageElement.Storm, 0.1f));

            Assert.That(result.Critical, Is.True);
            Assert.That(result.Amount, Is.EqualTo(120f).Within(0.001f));
            Assert.That(result.Element, Is.EqualTo(DamageElement.Storm));
        }

        [Test]
        public void WalletAndShopTransactionAreAtomic()
        {
            var wallet = new RunWallet();
            wallet.Add(CurrencyKind.Gold, 30);
            GameObject owner = new GameObject("Inventory");
            RunInventory inventory = owner.AddComponent<RunInventory>();
            RelicDefinition relic = CreateRelic(
                "relic.test", MemoryAspect.Ember, 1f);

            Assert.That(
                ShopTransaction.TryBuy(
                    wallet, inventory, relic, CurrencyKind.Gold, 40),
                Is.False);
            Assert.That(wallet.Get(CurrencyKind.Gold), Is.EqualTo(30));
            Assert.That(
                ShopTransaction.TryBuy(
                    wallet, inventory, relic, CurrencyKind.Gold, 20),
                Is.True);
            Assert.That(wallet.Get(CurrencyKind.Gold), Is.EqualTo(10));
            Assert.That(inventory.Contains("relic.test"), Is.True);

            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(relic);
        }

        [Test]
        public void ResonanceChainDetectsHarmonyClashAndAwakening()
        {
            RelicDefinition emberA = CreateRelic("a", MemoryAspect.Ember, 1f);
            RelicDefinition emberB = CreateRelic("b", MemoryAspect.Ember, 1f);
            RelicDefinition tide = CreateRelic("c", MemoryAspect.Tide, 1f);
            var chain = new ResonanceChain();
            chain.Equip(0, emberA);
            chain.Equip(1, emberB);
            chain.Equip(2, tide);

            ResonanceSummary summary = chain.Evaluate();
            Assert.That(summary.Harmonies, Is.EqualTo(1));
            Assert.That(summary.Clashes, Is.EqualTo(1));
            Assert.That(summary.Awakened, Is.False);

            Object.DestroyImmediate(emberA);
            Object.DestroyImmediate(emberB);
            Object.DestroyImmediate(tide);
        }

        [Test]
        public void RewardSelectionIsSeedReproducible()
        {
            RelicDefinition a = CreateRelic("a", MemoryAspect.Ember, 1f);
            RelicDefinition b = CreateRelic("b", MemoryAspect.Storm, 2f);
            RelicDefinition c = CreateRelic("c", MemoryAspect.Gloam, 3f);
            RelicDefinition[] catalog = { a, b, c };

            Assert.That(
                RewardSelector.Select(catalog, 991UL),
                Is.SameAs(RewardSelector.Select(catalog, 991UL)));

            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
            Object.DestroyImmediate(c);
        }

        private static RelicDefinition CreateRelic(
            string id,
            MemoryAspect aspect,
            float weight)
        {
            RelicDefinition relic = ScriptableObject.CreateInstance<RelicDefinition>();
            relic.Configure(
                id, id, aspect, RelicRarity.Common, weight, "test", 0.1f);
            return relic;
        }
    }
}
