using System;
using System.Collections.Generic;
using Lanternfall.Gameplay.Save;
using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    public enum AchievementCategory { Progression, Exploration, Mastery }

    [Serializable]
    public sealed class AchievementDefinition
    {
        [SerializeField] private string stableId;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string description;
        [SerializeField] private AchievementCategory category;
        [SerializeField, Min(1)] private int target = 1;
        [SerializeField] private bool hidden;

        public string StableId => stableId;
        public string Title => title;
        public string Description => description;
        public AchievementCategory Category => category;
        public int Target => target;
        public bool Hidden => hidden;

        public AchievementDefinition(
            string id, string displayTitle, string details,
            AchievementCategory achievementCategory, int required, bool isHidden)
        {
            stableId = id;
            title = displayTitle;
            description = details;
            category = achievementCategory;
            target = Mathf.Max(1, required);
            hidden = isHidden;
        }
    }

    [CreateAssetMenu(menuName = "Lanternfall/Achievement Catalog")]
    public sealed class AchievementCatalog : ScriptableObject
    {
        [SerializeField] private AchievementDefinition[] entries =
            Array.Empty<AchievementDefinition>();

        public IReadOnlyList<AchievementDefinition> Entries => entries;

        public void Configure(AchievementDefinition[] definitions) =>
            entries = definitions ?? Array.Empty<AchievementDefinition>();

        public List<string> ValidateReleaseCatalog()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>();
            if (entries.Length != 100)
                errors.Add($"Expected exactly 100 achievements, found {entries.Length}.");
            foreach (AchievementDefinition entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.StableId))
                    errors.Add("Achievement has no stable ID.");
                else if (!ids.Add(entry.StableId))
                    errors.Add($"Duplicate achievement ID: {entry.StableId}");
                if (entry != null && string.IsNullOrWhiteSpace(entry.Title))
                    errors.Add($"Achievement {entry.StableId} has no title.");
            }
            foreach (AchievementCategory category in
                     Enum.GetValues(typeof(AchievementCategory)))
                if (Array.Find(entries, item =>
                    item != null && item.Category == category) == null)
                    errors.Add($"No achievements in category {category}.");
            return errors;
        }
    }

    /// <summary>Save-backed, idempotent achievement progress.</summary>
    public sealed class AchievementTracker
    {
        private readonly SaveData _save;
        private readonly Dictionary<string, int> _progress =
            new Dictionary<string, int>();

        public AchievementTracker(SaveData save) =>
            _save = save ?? throw new ArgumentNullException(nameof(save));

        public bool AddProgress(AchievementDefinition definition, int amount = 1)
        {
            if (definition == null || amount <= 0 ||
                _save.achievements.Contains(definition.StableId)) return false;
            _progress.TryGetValue(definition.StableId, out int current);
            current = checked(current + amount);
            _progress[definition.StableId] = current;
            if (current < definition.Target) return false;
            _save.achievements.Add(definition.StableId);
            return true;
        }

        public int Progress(string stableId) =>
            _progress.TryGetValue(stableId, out int value) ? value : 0;
    }
}
