using System;
using System.Collections.Generic;
using Lanternfall.Gameplay.Combat;
using UnityEngine;

namespace Lanternfall.Gameplay.Enemies
{
    public sealed class EncounterDirector : MonoBehaviour
    {
        [SerializeField] private EnemyBrain enemyPrefab;
        [SerializeField] private EnemyDefinition[] roster;
        [SerializeField] private Transform target;
        [SerializeField] private int initialEnemyCount = 6;

        private readonly List<EnemyBrain> _active = new List<EnemyBrain>(32);
        private int _summonBudget = 3;

        public event Action EncounterStarted;
        public event Action EncounterCleared;
        public int ActiveCount => _active.Count;

        private void Start() => BeginEncounter();

        public void Configure(
            EnemyBrain prefab,
            EnemyDefinition[] definitions,
            Transform pursuitTarget,
            int count)
        {
            enemyPrefab = prefab;
            roster = definitions;
            target = pursuitTarget;
            initialEnemyCount = Mathf.Max(1, count);
        }

        public void BeginEncounter()
        {
            if (enemyPrefab == null || roster == null || roster.Length == 0 || target == null)
                return;
            EncounterStarted?.Invoke();
            for (int index = 0; index < initialEnemyCount; index++)
            {
                float angle = index * Mathf.PI * 2f / initialEnemyCount;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * 7.5f, 1f, Mathf.Sin(angle) * 5.5f);
                EliteModifier elite = index == initialEnemyCount - 1
                    ? EliteModifier.Frenzied
                    : EliteModifier.None;
                Spawn(roster[index % roster.Length], position, elite);
            }
        }

        private void Spawn(
            EnemyDefinition definition,
            Vector3 position,
            EliteModifier elite)
        {
            EnemyBrain enemy = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
            enemy.gameObject.SetActive(true);
            enemy.Configure(definition, target, elite);
            enemy.SummonRequested += OnSummonRequested;
            enemy.Died += OnEnemyDied;
            _active.Add(enemy);
        }

        private void OnSummonRequested(EnemyBrain source, Vector3 position)
        {
            if (_summonBudget <= 0 || roster.Length == 0) return;
            _summonBudget--;
            Spawn(roster[0], position, EliteModifier.None);
        }

        private void OnEnemyDied(EnemyBrain enemy)
        {
            if (enemy != null)
            {
                enemy.SummonRequested -= OnSummonRequested;
                enemy.Died -= OnEnemyDied;
            }
            _active.Remove(enemy);
            if (_active.Count == 0) EncounterCleared?.Invoke();
        }
    }
}
