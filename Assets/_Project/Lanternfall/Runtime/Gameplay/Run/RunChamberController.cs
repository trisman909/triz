using System;
using Lanternfall.Core.Random;
using Lanternfall.Core.Run;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Enemies;
using Lanternfall.Gameplay.Hub;
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

        private void OnEnemyDefeated()
        {
            if (HubController.Instance?.ActiveRun is RunSession session)
                session.EnemiesDefeated++;
        }

        private void OnEncounterCleared() => exitGate?.SetUnlocked(true);

        private void OnBossDefeated(string _)
        {
            if (HubController.Instance?.ActiveRun is RunSession session)
                session.BossesDefeated++;
            exitGate?.SetUnlocked(true);
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
