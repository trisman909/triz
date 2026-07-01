using System.Collections.Generic;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Enemies;
using Lanternfall.Gameplay.Progression;
using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    [CreateAssetMenu(menuName = "Lanternfall/Content/Catalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        [SerializeField] private CharacterClassDefinition[] classes;
        [SerializeField] private BiomeDefinition[] biomes;
        [SerializeField] private EnemyDefinition[] enemies;
        [SerializeField] private BossDefinition[] bosses;
        [SerializeField] private RelicDefinition[] relics;

        public IReadOnlyList<CharacterClassDefinition> Classes => classes;
        public IReadOnlyList<BiomeDefinition> Biomes => biomes;
        public IReadOnlyList<EnemyDefinition> Enemies => enemies;
        public IReadOnlyList<BossDefinition> Bosses => bosses;
        public IReadOnlyList<RelicDefinition> Relics => relics;

        public List<string> ValidateReleaseCounts()
        {
            var errors = new List<string>();
            Validate(classes, 5, "classes", item => item.StableId, errors);
            Validate(biomes, 5, "biomes", item => item.StableId, errors);
            Validate(enemies, 40, "enemies", item => item.StableId, errors);
            Validate(bosses, 15, "bosses", item => item.StableId, errors);
            Validate(relics, 6, "relics", item => item.StableId, errors);
            return errors;
        }

#if UNITY_EDITOR
        public void Configure(
            CharacterClassDefinition[] classDefinitions,
            BiomeDefinition[] biomeDefinitions,
            EnemyDefinition[] enemyDefinitions,
            BossDefinition[] bossDefinitions,
            RelicDefinition[] relicDefinitions)
        {
            classes = classDefinitions;
            biomes = biomeDefinitions;
            enemies = enemyDefinitions;
            bosses = bossDefinitions;
            relics = relicDefinitions;
        }
#endif

        private static void Validate<T>(
            T[] items,
            int minimum,
            string label,
            System.Func<T, string> id,
            List<string> errors)
            where T : Object
        {
            if (items == null || items.Length < minimum)
            {
                errors.Add($"Catalog needs at least {minimum} {label}.");
                return;
            }
            var ids = new HashSet<string>();
            for (int index = 0; index < items.Length; index++)
            {
                if (items[index] == null)
                    errors.Add($"{label}[{index}] is null.");
                else if (string.IsNullOrWhiteSpace(id(items[index])) ||
                    !ids.Add(id(items[index])))
                    errors.Add($"{label}[{index}] has a missing or duplicate stable ID.");
            }
        }
    }
}
