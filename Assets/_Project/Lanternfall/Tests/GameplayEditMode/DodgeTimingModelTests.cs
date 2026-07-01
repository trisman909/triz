using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Save;
using Lanternfall.Gameplay.Accessibility;
using UnityEngine;
using UnityEditor;
using Lanternfall.Gameplay.World;
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

        [Test]
        public void BossPhaseModelTransitionsAndEnragesAtThresholds()
        {
            var phases = new BossPhaseModel();
            Assert.That(phases.Update(.8f), Is.False);
            Assert.That(phases.Phase, Is.EqualTo(1));
            Assert.That(phases.Update(.6f), Is.True);
            Assert.That(phases.Phase, Is.EqualTo(2));
            Assert.That(phases.Update(.3f), Is.True);
            Assert.That(phases.Phase, Is.EqualTo(3));
            phases.Update(.1f);
            Assert.That(phases.Enraged, Is.True);
        }

        [Test]
        public void SaveRoundTripUsesBackupWhenPrimaryIsCorrupt()
        {
            var storage = new MemoryStorage();
            var service = new SaveService(storage);
            SaveData data = service.Load();
            data.statistics.runsStarted = 3;
            service.Save(data);
            data.statistics.runsStarted = 4;
            service.Save(data);
            storage.CorruptPrimary();

            SaveData recovered = service.Load();

            Assert.That(recovered.statistics.runsStarted, Is.EqualTo(3));
            Assert.That(recovered.version, Is.EqualTo(SaveData.CurrentVersion));
        }

        [Test]
        public void MetaUnlocksAndQuestProgressAreIdempotent()
        {
            var data = new SaveData();
            var progression = new MetaProgression(data);
            Assert.That(progression.Unlock("class.wayfinder"), Is.True);
            Assert.That(progression.Unlock("class.wayfinder"), Is.False);

            var quests = new QuestJournal(data);
            Assert.That(quests.Advance("quest.test", 2), Is.True);
            Assert.That(quests.Advance("quest.test", 2), Is.True);
            Assert.That(quests.Find("quest.test").completed, Is.True);
            Assert.That(quests.Advance("quest.test", 2), Is.False);
        }

        private sealed class MemoryStorage : ISaveStorage
        {
            private string _primary;
            private string _backup;
            public bool Exists => _primary != null;
            public string Read() => _primary;
            public string ReadBackup() => _backup;
            public void WriteAtomic(string contents)
            {
                _backup = _primary;
                _primary = contents;
            }
            public void CorruptPrimary() => _primary = "{\"payload\":\"bad\"}";
        }

        [Test]
        public void ReleaseContentCatalogMeetsCountsAndStableIdRules()
        {
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(
                "Assets/_Project/Lanternfall/Settings/LanternfallContentCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.ValidateReleaseCounts(), Is.Empty);
            Assert.That(catalog.Classes.Count, Is.EqualTo(5));
            Assert.That(catalog.Biomes.Count, Is.EqualTo(5));
            Assert.That(catalog.Enemies.Count, Is.EqualTo(40));
            Assert.That(catalog.Bosses.Count, Is.EqualTo(15));
        }

        [Test]
        public void AchievementCatalogHasOneHundredUniqueCrossCategoryEntries()
        {
            AchievementCatalog catalog =
                AssetDatabase.LoadAssetAtPath<AchievementCatalog>(
                    "Assets/_Project/Lanternfall/Settings/" +
                    "LanternfallAchievementCatalog.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.ValidateReleaseCatalog(), Is.Empty);
            Assert.That(catalog.Entries.Count, Is.EqualTo(100));
        }

        [Test]
        public void AchievementTrackerUnlocksOnceAtTarget()
        {
            var save = new SaveData();
            var tracker = new AchievementTracker(save);
            var achievement = new AchievementDefinition(
                "achievement.test", "Test", "Test progress",
                AchievementCategory.Mastery, 2, false);

            Assert.That(tracker.AddProgress(achievement), Is.False);
            Assert.That(tracker.AddProgress(achievement), Is.True);
            Assert.That(tracker.AddProgress(achievement), Is.False);
            Assert.That(save.achievements, Is.EqualTo(
                new[] { "achievement.test" }));
        }

        [Test]
        public void AccessibilityProfileClampsUnsafePresentationValues()
        {
            var settings = new SettingsData
            {
                cameraShake = 4f,
                flashIntensity = -2f,
                uiScale = 3f,
                reducedMotion = true
            };

            AccessibilityRuntime.Apply(settings);

            Assert.That(AccessibilityRuntime.CameraShake, Is.EqualTo(1f));
            Assert.That(AccessibilityRuntime.FlashIntensity, Is.EqualTo(0f));
            Assert.That(AccessibilityRuntime.UiScale, Is.EqualTo(1.5f));
            Assert.That(AccessibilityRuntime.ReducedMotion, Is.True);
        }
    }
}
