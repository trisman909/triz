using System;
using Lanternfall.Core.Random;
using Lanternfall.Core.Run;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Enemies;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.World;
using UnityEngine;

namespace Lanternfall.Gameplay.Run
{
    public sealed class RunChamberController : MonoBehaviour
    {
        [SerializeField] private ContentCatalog contentCatalog;
        [SerializeField] private EnemyBrain enemyPrefab;
        [SerializeField] private BossBrain bossPrefab;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private RunExitGate exitGate;
        [SerializeField] private BiomeAtmosphere atmosphere;

        private EncounterDirector _encounter;

        public void Configure(
            ContentCatalog catalog,
            EnemyBrain standardEnemy,
            BossBrain guardian,
            Transform target,
            RunExitGate exit,
            BiomeAtmosphere biomeAtmosphere)
        {
            contentCatalog = catalog;
            enemyPrefab = standardEnemy;
            bossPrefab = guardian;
            playerTarget = target;
            exitGate = exit;
            atmosphere = biomeAtmosphere;
        }

        private void OnEnable() => BossEncounterSignals.Defeated += OnBossDefeated;
        private void OnDisable() => BossEncounterSignals.Defeated -= OnBossDefeated;

        private void Start()
        {
            RunSession session = HubController.Instance?.ActiveRun;
            if (session == null || contentCatalog == null)
            {
                exitGate?.SetUnlocked(true);
                return;
            }
            RunRoomPlan room = session.Current;
            session.BeginRoom();
            atmosphere?.Configure(contentCatalog.Biomes[room.BiomeIndex]);
            ApplyAtmosphereNow(contentCatalog.Biomes[room.BiomeIndex]);
            exitGate?.SetUnlocked(false);

            switch (room.Kind)
            {
                case RoomKind.Boss:
                    BeginBoss(room);
                    break;
                case RoomKind.Combat:
                case RoomKind.Elite:
                case RoomKind.Challenge:
                case RoomKind.MiniBoss:
                    BeginCombat(room);
                    break;
                case RoomKind.Healing:
                    HealPlayer();
                    exitGate?.SetUnlocked(true);
                    break;
                case RoomKind.Treasure:
                    SpawnEchoChoices(room.EncounterSeed);
                    exitGate?.SetUnlocked(true);
                    break;
                case RoomKind.Shrine:
                case RoomKind.Start:
                    SpawnVowChoices();
                    exitGate?.SetUnlocked(true);
                    break;
                default:
                    exitGate?.SetUnlocked(true);
                    break;
            }
        }

        private void BeginCombat(RunRoomPlan room)
        {
            int start = room.BiomeIndex * 8;
            var roster = new EnemyDefinition[8];
            for (int index = 0; index < roster.Length; index++)
                roster[index] = contentCatalog.Enemies[start + index];
            int count = room.Kind == RoomKind.MiniBoss ? 8 :
                room.Kind == RoomKind.Elite ? 7 : 5 + room.BiomeIndex;
            count = Mathf.Max(3, count +
                (HubController.Instance?.ActiveRun?.ConsumeEncounterConsequence() ?? 0));
            _encounter = gameObject.AddComponent<EncounterDirector>();
            _encounter.Configure(enemyPrefab, roster, playerTarget, count);
            _encounter.EnemyDefeated += OnEnemyDefeated;
            _encounter.EncounterCleared += OnEncounterCleared;
        }

        private void BeginBoss(RunRoomPlan room)
        {
            int start = room.BiomeIndex * 3;
            var choices = new BossDefinition[3];
            for (int index = 0; index < choices.Length; index++)
                choices[index] = contentCatalog.Bosses[start + index];
            var random = new DeterministicRandom(room.EncounterSeed);
            BossDefinition selected = choices[random.NextInt(choices.Length)];
            BossBrain boss = Instantiate(
                bossPrefab, Vector3.forward * 7f + Vector3.up,
                Quaternion.identity, transform);
            boss.gameObject.SetActive(true);
            boss.Configure(selected, playerTarget);
        }

        private void OnEnemyDefeated(EnemyBrain enemy)
        {
            if (HubController.Instance?.ActiveRun is RunSession session)
            {
                session.EnemiesDefeated++;
                session.RecordKill(enemy != null && enemy.InRadiance);
                int gold = enemy != null && !enemy.InRadiance
                    ? Mathf.RoundToInt(2f * session.OutsideRewardMultiplier)
                    : 1;
                AddGold(gold);
            }
        }

        private void OnEncounterCleared()
        {
            RunSession session = HubController.Instance?.ActiveRun;
            session?.ResolveVowForCombatRoom();
            if (session != null)
            {
                AddGold(Mathf.RoundToInt(10f * session.CurrentRewardMultiplier));
                SpawnEchoChoices(session.Current.EncounterSeed);
            }
            exitGate?.SetUnlocked(true);
        }

        private void OnBossDefeated(string _)
        {
            if (HubController.Instance?.ActiveRun is RunSession session)
            {
                session.BossesDefeated++;
                session.ResolveVowForCombatRoom();
                AddGold(Mathf.RoundToInt(25f * session.CurrentRewardMultiplier));
                SpawnEchoChoices(session.Current.EncounterSeed);
            }
            exitGate?.SetUnlocked(true);
        }

        private void AddGold(int amount)
        {
            if (amount <= 0) return;
            RunInventory inventory =
                playerTarget?.GetComponentInParent<RunInventory>();
            inventory?.Wallet.Add(CurrencyKind.Gold, amount);
        }

        private void SpawnVowChoices()
        {
            VowKind[] vows =
            {
                VowKind.UnbrokenFlame,
                VowKind.SwiftPassage,
                VowKind.BeyondLantern
            };
            for (int index = 0; index < vows.Length; index++)
            {
                GameObject choice =
                    GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                choice.transform.position =
                    new Vector3((index - 1) * 3.5f, .4f, 2f);
                choice.transform.localScale = new Vector3(1.2f, .4f, 1.2f);
                choice.GetComponent<Collider>().isTrigger = true;
                choice.AddComponent<VowPedestal>().Configure(vows[index]);
                AddWorldLabel(choice.transform, VowPedestal.Title(vows[index]));
            }
        }

        private void SpawnEchoChoices(ulong seed)
        {
            int count = Mathf.Min(3, contentCatalog.Relics.Count);
            var random = new DeterministicRandom(
                SeedDerivation.Derive(seed, "echo-choices"));
            var selected = new System.Collections.Generic.HashSet<int>();
            while (selected.Count < count)
                selected.Add(random.NextInt(contentCatalog.Relics.Count));
            int slot = 0;
            foreach (int index in selected)
            {
                RelicDefinition relic = contentCatalog.Relics[index];
                GameObject choice =
                    GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                choice.name = $"Echo — {relic.DisplayName}";
                choice.transform.position =
                    new Vector3((slot++ - 1) * 3f, .45f, 4f);
                choice.transform.localScale = new Vector3(1f, .45f, 1f);
                choice.GetComponent<Collider>().isTrigger = true;
                choice.AddComponent<RewardPedestal>().Configure(relic);
                AddWorldLabel(choice.transform, relic.DisplayName);
            }
        }

        private static void AddWorldLabel(Transform parent, string label)
        {
            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            textObject.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
            TextMesh text = textObject.AddComponent<TextMesh>();
            text.text = label;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 42;
            text.characterSize = .07f;
            text.color = Color.white;
        }

        private void HealPlayer()
        {
            if (playerTarget != null &&
                playerTarget.GetComponentInParent<Combat.Health>() is Combat.Health health)
                health.Restore(health.Maximum);
        }

        private static void ApplyAtmosphereNow(BiomeDefinition biome)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = biome.FogColor;
            RenderSettings.fogDensity = biome.FogDensity;
            RenderSettings.ambientLight = biome.AmbientColor;
        }
    }
}
