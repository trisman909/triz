using System.Collections.Generic;
using Lanternfall.Core.Random;
using Lanternfall.Gameplay.Enemies;
using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    public sealed class BossEncounterDirector : MonoBehaviour
    {
        [SerializeField] private BossBrain bossPrefab;
        [SerializeField] private BossDefinition[] candidates;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyBrain minionPrefab;
        [SerializeField] private EnemyDefinition minionDefinition;
        [SerializeField] private ulong seed = 20260701UL;

        private readonly List<EnemyBrain> _minions = new List<EnemyBrain>(8);
        private BossBrain _activeBoss;

        public BossBrain ActiveBoss => _activeBoss;

        private void Start() => Begin();

        public void Configure(
            BossBrain prefab,
            BossDefinition[] guardianCandidates,
            Transform combatTarget,
            EnemyBrain summonPrefab,
            EnemyDefinition summonDefinition,
            ulong runSeed)
        {
            bossPrefab = prefab;
            candidates = guardianCandidates;
            target = combatTarget;
            minionPrefab = summonPrefab;
            minionDefinition = summonDefinition;
            seed = runSeed;
        }

        public void Begin()
        {
            if (_activeBoss != null || bossPrefab == null || candidates == null ||
                candidates.Length == 0 || target == null) return;
            var random = new DeterministicRandom(
                SeedDerivation.Derive(seed, "biome-guardian"));
            BossDefinition selected = candidates[random.NextInt(candidates.Length)];
            _activeBoss = Instantiate(
                bossPrefab,
                transform.position + Vector3.forward * 8f,
                Quaternion.identity,
                transform);
            _activeBoss.gameObject.SetActive(true);
            _activeBoss.Configure(selected, target);
            _activeBoss.SummonRequested += OnSummonRequested;
        }

        private void OnDestroy()
        {
            if (_activeBoss != null)
                _activeBoss.SummonRequested -= OnSummonRequested;
        }

        private void OnSummonRequested(BossBrain source, Vector3 position)
        {
            _minions.RemoveAll(item => item == null);
            if (_minions.Count >= 6 || minionPrefab == null || minionDefinition == null)
                return;
            EnemyBrain minion = Instantiate(
                minionPrefab, position, Quaternion.identity, transform);
            minion.gameObject.SetActive(true);
            minion.Configure(minionDefinition, target, EliteModifier.None);
            _minions.Add(minion);
        }
    }
}

