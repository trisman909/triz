using System.Collections;
using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Enemies;
using Lanternfall.Gameplay.World;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Audio;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.UI;
using Lanternfall.Gameplay.Performance;
using UnityEngine.SceneManagement;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Run;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Radiance;
using Lanternfall.Gameplay.Presentation;
using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lanternfall.Tests
{
    public sealed class PlayerMotorPlayModeTests
    {
        [UnityTest]
        public IEnumerator MotorMovesAndDodgeGrantsTemporaryInvulnerability()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(20f, 1f, 20f);

            GameObject player = new GameObject("Test Bearer");
            player.transform.position = Vector3.up;
            player.AddComponent<CharacterController>();
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            yield return null;

            Vector3 start = player.transform.position;
            for (int index = 0; index < 10; index++)
            {
                motor.Step(Vector2.up, false, index == 0, 0.02f);
            }

            Assert.That(player.transform.position.z, Is.GreaterThan(start.z + 1f));
            Assert.That(motor.IsInvulnerable, Is.True);
            motor.Step(Vector2.zero, false, false, 0.25f);
            Assert.That(motor.IsInvulnerable, Is.False);

            Object.Destroy(player);
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator ProjectileDeliversResolvedDamageAndReturns()
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = new Vector3(0f, 1f, 3f);
            Health health = target.AddComponent<Health>();
            health.Configure(100f, 0f, false);

            WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.Configure(
                "test.weapon", "Test Weapon", 25f, 2f, 20f, 0f, 0f,
                DamageElement.Ember);

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.GetComponent<SphereCollider>().enabled = false;
            Projectile projectile = projectileObject.AddComponent<Projectile>();
            bool returned = false;
            projectile.Launch(
                weapon, new Vector3(0f, 1f, 0f), Vector3.forward, projectileObject.transform,
                item =>
                {
                    returned = true;
                    item.gameObject.SetActive(false);
                });

            float timeout = 1f;
            while (!returned && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(returned, Is.True);
            Assert.That(health.Current, Is.EqualTo(75f).Within(0.01f));
            Object.Destroy(target);
            Object.Destroy(projectileObject);
            Object.Destroy(weapon);
        }

        [UnityTest]
        public IEnumerator EnemyTelegraphsThenDamagesTarget()
        {
            GameObject target = new GameObject("Target");
            Health targetHealth = target.AddComponent<Health>();
            targetHealth.Configure(100f, 0f, false);

            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.Configure(
                "test.melee", EnemyArchetype.Melee, 30f, 0f, 2f,
                10f, 2f, 0.05f, 0.1f);

            GameObject enemy = new GameObject("Enemy");
            enemy.transform.position = new Vector3(0f, 0f, 1f);
            enemy.AddComponent<CharacterController>();
            enemy.AddComponent<Health>();
            EnemyBrain brain = enemy.AddComponent<EnemyBrain>();
            brain.Configure(definition, target.transform, EliteModifier.None);

            float timeout = 1f;
            while (targetHealth.Current >= 100f && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(targetHealth.Current, Is.LessThan(100f));
            Object.Destroy(enemy);
            Object.Destroy(target);
            Object.Destroy(definition);
        }

        [UnityTest]
        public IEnumerator SeededRunPresenterBuildsCriticalRoomViews()
        {
            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            template.SetActive(false);
            GameObject root = new GameObject("Run Presenter");
            RunLayoutPresenter presenter = root.AddComponent<RunLayoutPresenter>();
            presenter.Configure(template, 42UL, 12);
            presenter.Generate(42UL);
            yield return null;

            Assert.That(presenter.SpawnedRooms.Count, Is.GreaterThan(15));
            bool hasBoss = false;
            bool hasSecret = false;
            foreach (GameObject item in presenter.SpawnedRooms)
            {
                hasBoss |= item.name.Contains("Boss");
                hasSecret |= item.name.Contains("Secret");
            }
            Assert.That(hasBoss, Is.True);
            Assert.That(hasSecret, Is.True);
            Object.Destroy(root);
            Object.Destroy(template);
        }

        [UnityTest]
        public IEnumerator RewardPedestalTransfersEchoOnlyOnce()
        {
            GameObject player = new GameObject("Collector");
            RunInventory inventory = player.AddComponent<RunInventory>();
            RelicDefinition relic = ScriptableObject.CreateInstance<RelicDefinition>();
            relic.Configure(
                "relic.test_pickup", "Test Pickup", MemoryAspect.Radiance,
                RelicRarity.Rare, 1f, "test", .2f);

            GameObject pedestalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            RewardPedestal pedestal = pedestalObject.AddComponent<RewardPedestal>();
            pedestal.Configure(relic);

            Assert.That(pedestal.TryCollect(inventory), Is.True);
            Assert.That(inventory.Contains("relic.test_pickup"), Is.True);
            Assert.That(pedestal.TryCollect(inventory), Is.False);
            yield return null;

            Object.Destroy(player);
            Object.Destroy(pedestalObject);
            Object.Destroy(relic);
        }

        [UnityTest]
        public IEnumerator GuardianAdvancesPhaseFromAuthoritativeHealth()
        {
            GameObject target = new GameObject("Bearer");
            target.AddComponent<Health>().Configure(200f, 0f, false);
            BossDefinition definition = ScriptableObject.CreateInstance<BossDefinition>();
            definition.Configure(
                "boss.test", "Test Guardian", BossAttackPattern.BellShockwave,
                300f, 0f, 2f, 10f, .05f, .1f, .01f);

            GameObject guardian = new GameObject("Guardian");
            guardian.AddComponent<CharacterController>();
            Health bossHealth = guardian.AddComponent<Health>();
            BossBrain brain = guardian.AddComponent<BossBrain>();
            brain.Configure(definition, target.transform);
            bossHealth.ApplyDamage(new DamageRequest(
                150f, 0f, 0f, 1f, 0f, DamageElement.Physical, 1f));
            yield return null;

            Assert.That(brain.Phase, Is.EqualTo(2));
            Object.Destroy(guardian);
            Object.Destroy(target);
            Object.Destroy(definition);
        }

        [UnityTest]
        public IEnumerator HudBuildsResponsiveCanvasAndPauseSurface()
        {
            GameObject player = new GameObject("HUD Test Bearer");
            player.AddComponent<Health>().Configure(180f, 0f, false);
            player.AddComponent<PlayerInputReader>();
            GameHud hud = player.AddComponent<GameHud>();
            yield return null;

            Assert.That(player.transform.Find("Lanternfall HUD"), Is.Not.Null);
            hud.SetPaused(true);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            hud.SetPaused(false);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
            GameplayPresentationSignals.RaiseSubtitle(
                "TEST", "Readable subtitle content.", 1f);
            yield return null;
            Assert.That(hud.SubtitleVisible, Is.True);
            hud.ShowTitleCard("TEST CHAMBER", .5f);
            yield return null;
            Canvas canvas =
                player.GetComponentInChildren<Canvas>(true);
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.pixelPerfect, Is.True);
            hud.DisplayRunSummary(new RunSummaryData(
                true, 7UL, "class.test", 120f,
                40, 50, 5, 100, 3, 1, 0));
            Assert.That(hud.SummaryVisible, Is.True);

            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PresentationDirectorsUseFixedProceduralBanks()
        {
            GameObject systems = new GameObject("Presentation Test");
            GameplayAudioDirector audio =
                systems.AddComponent<GameplayAudioDirector>();
            CombatVfxDirector vfx =
                systems.AddComponent<CombatVfxDirector>();
            yield return null;

            Assert.That(audio.GeneratedClipCount, Is.EqualTo(12));
            Assert.That(vfx.PoolSize, Is.EqualTo(32));
            foreach (Renderer renderer in
                     systems.GetComponentsInChildren<Renderer>(true))
                Assert.That(
                    renderer.sharedMaterial.shader.name,
                    Does.StartWith("Universal Render Pipeline/"));
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.Ability, Vector3.zero);
            yield return null;
            Assert.That(audio.CueCount, Is.EqualTo(1));
            Assert.That(vfx.ActivePulseCount, Is.EqualTo(1));

            Object.Destroy(systems);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TelegraphAppliesHighContrastAccessibilityColor()
        {
            var settings = new SettingsData
            {
                highContrastTelegraphs = true,
                flashIntensity = .5f
            };
            AccessibilityRuntime.Apply(settings);
            GameObject actor = new GameObject("Telegraph Test");
            CombatTelegraph telegraph =
                actor.AddComponent<CombatTelegraph>();
            yield return null;
            telegraph.Show(3f, 1f);
            yield return null;

            Assert.That(telegraph.Visible, Is.True);
            Assert.That(telegraph.CurrentColor.r, Is.GreaterThan(.9f));
            Assert.That(telegraph.CurrentColor.g, Is.GreaterThan(.8f));
            Renderer telegraphRenderer =
                actor.GetComponentInChildren<Renderer>(true);
            Assert.That(
                telegraphRenderer.sharedMaterial.shader.name,
                Does.StartWith("Universal Render Pipeline/"));

            AccessibilityRuntime.Apply(new SettingsData());
            Object.Destroy(actor);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiveReviewChambersLoadWithUrpAccessiblePresentation()
        {
            string[] scenes =
            {
                "ArtReview_DrownedNarthex",
                "ArtReview_SiltglassObservatory",
                "ArtReview_EmberOssuary",
                "ArtReview_GloamOrchard",
                "ArtReview_StormvaultFoundry"
            };
            var accessibilitySettings = new SettingsData
            {
                subtitles = true,
                reducedMotion = true,
                highContrastTelegraphs = true,
                flashIntensity = .35f,
                cameraShake = 0f
            };
            foreach (string scene in scenes)
            {
                SceneManager.LoadScene(scene);
                yield return null;
                yield return null;
                AccessibilityRuntime.Apply(accessibilitySettings);

                Assert.That(
                    Object.FindObjectsByType<ProductionAssetMarker>(
                        FindObjectsSortMode.None).Length,
                    Is.GreaterThanOrEqualTo(11),
                    scene);
                BiomeAmbientAudio ambience =
                    Object.FindFirstObjectByType<BiomeAmbientAudio>();
                Assert.That(ambience, Is.Not.Null, scene);
                Assert.That(ambience.GeneratedClipCount, Is.EqualTo(2), scene);
                ActorPresentation[] actors =
                    Object.FindObjectsByType<ActorPresentation>(
                        FindObjectsSortMode.None);
                Assert.That(actors.Length, Is.GreaterThanOrEqualTo(2), scene);
                foreach (ActorPresentation actor in actors)
                    Assert.That(
                        actor.OrnamentCount,
                        Is.GreaterThan(0),
                        scene);
                foreach (Renderer renderer in
                         Object.FindObjectsByType<Renderer>(
                             FindObjectsSortMode.None))
                {
                    if (renderer.GetComponent<TextMesh>() != null) continue;
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        Assert.That(material, Is.Not.Null, scene);
                        Assert.That(
                            material.shader.name,
                            Does.StartWith("Universal Render Pipeline/"),
                            $"{scene}/{renderer.name}");
                    }
                }
            }
            Assert.That(AccessibilityRuntime.ReducedMotion, Is.True);
            Assert.That(
                AccessibilityRuntime.HighContrastTelegraphs,
                Is.True);
            Assert.That(AccessibilityRuntime.CameraShake, Is.Zero);
            AccessibilityRuntime.Apply(new SettingsData());
        }

        [UnityTest]
        public IEnumerator RepresentativeBiomeSwitcherLoadsRequestedScene()
        {
            SceneManager.LoadScene("ArtReview_DrownedNarthex");
            yield return null;
            yield return null;

            RepresentativeBiomeSwitcher switcher =
                Object.FindFirstObjectByType<RepresentativeBiomeSwitcher>();
            Assert.That(switcher, Is.Not.Null);
            switcher.SwitchToBiome(4);
            yield return null;
            yield return null;

            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo("ArtReview_StormvaultFoundry"));
            Assert.That(
                Object.FindFirstObjectByType<PlayerInputReader>(),
                Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator InputReaderExposesFullInteractiveRebindingSurface()
        {
            GameObject owner = new GameObject("Rebind Test");
            PlayerInputReader input =
                owner.AddComponent<PlayerInputReader>();
            yield return null;

            Assert.That(input.RebindSlotCount, Is.GreaterThanOrEqualTo(18));
            Assert.That(
                input.ApplyBindingOverride(
                    "Dodge", 0, "<Keyboard>/k"),
                Is.True);
            Assert.That(input.SaveBindingOverrides(), Is.Not.Empty);
            Assert.That(input.BindingDisplay(0), Is.Not.Empty);
            input.ResetBindingOverrides();

            Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AdaptiveScoreAcceptsRuntimeStateChanges()
        {
            GameObject audio = new GameObject("Adaptive Score Test");
            DynamicAudioDirector director =
                audio.AddComponent<DynamicAudioDirector>();
            yield return null;

            Assert.That(director.State, Is.EqualTo(MusicState.Exploration));
            director.SetState(MusicState.Combat);
            Assert.That(director.State, Is.EqualTo(MusicState.Combat));

            Object.Destroy(audio);
            yield return null;
        }

        [UnityTest]
        public IEnumerator VerticalSliceStaysInsideRuntimeFrameAndPopulationBudgets()
        {
            SceneManager.LoadScene("FirstBiomeVerticalSlice");
            yield return null;
            FrameBudgetMonitor monitor =
                Object.FindFirstObjectByType<FrameBudgetMonitor>();
            Assert.That(monitor, Is.Not.Null);

            for (int frame = 0; frame < 180; frame++)
                yield return null;

            FrameBudgetSnapshot snapshot = monitor.Snapshot;
            Assert.That(snapshot.Samples, Is.GreaterThanOrEqualTo(120));
            Assert.That(snapshot.Meets60FpsBudget, Is.True,
                $"Average {snapshot.AverageMilliseconds:0.00}ms; " +
                $"over-budget {snapshot.OverBudgetRatio:P1}");
            Assert.That(
                Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None).Length,
                Is.LessThanOrEqualTo(8));
            Assert.That(
                Object.FindObjectsByType<BossBrain>(FindObjectsSortMode.None).Length,
                Is.LessThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator HubPreparedRunLoadsIntegratedClassAppliedChamber()
        {
            SceneManager.LoadScene("LanternfallHub");
            yield return null;
            HubController hub = HubController.Instance;
            Assert.That(hub, Is.Not.Null);
            hub.PrepareRun(777UL, "class.gloamstep");

            SceneManager.LoadScene("RunChamber");
            yield return null;
            yield return null;

            Assert.That(hub.ActiveRun.Rooms.Count, Is.EqualTo(40));
            Assert.That(hub.ActiveRun.ClassId, Is.EqualTo("class.gloamstep"));
            Assert.That(
                Object.FindFirstObjectByType<RunChamberController>(),
                Is.Not.Null);
            Assert.That(
                Object.FindFirstObjectByType<RunExitGate>().Unlocked,
                Is.True);
            Assert.That(
                Object.FindFirstObjectByType<Health>().Maximum,
                Is.EqualTo(125f).Within(.01f));
            Assert.That(hub.ActiveRun.OutsideRewardMultiplier, Is.EqualTo(2f));
            Assert.That(
                Object.FindFirstObjectByType<ClassPassiveController>().ActivePassive,
                Is.EqualTo("boundary.crossing"));
            Assert.That(
                Object.FindFirstObjectByType<RadianceField>(),
                Is.Not.Null);
            BiomeChamberPresenter biome =
                Object.FindFirstObjectByType<BiomeChamberPresenter>();
            Assert.That(biome, Is.Not.Null);
            Assert.That(biome.BiomeIndex, Is.EqualTo(0));
            Assert.That(biome.GeneratedPropCount, Is.EqualTo(14));
            Assert.That(
                Object.FindObjectsByType<BiomeHazard>(
                    FindObjectsSortMode.None).Length,
                Is.EqualTo(2));
            Assert.That(
                Object.FindFirstObjectByType<GameplayAudioDirector>(),
                Is.Not.Null);
            Assert.That(
                Object.FindFirstObjectByType<CombatVfxDirector>(),
                Is.Not.Null);
            Assert.That(
                Object.FindFirstObjectByType<ActorPresentation>(),
                Is.Not.Null);
            Assert.That(
                Object.FindFirstObjectByType<GameHud>().RouteText,
                Does.Contain("ROOM 1/8"));
        }
    }
}
