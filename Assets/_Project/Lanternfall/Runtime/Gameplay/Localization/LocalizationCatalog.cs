using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lanternfall.Gameplay.Localization
{
    [Serializable]
    public sealed class LocalizedEntry
    {
        [SerializeField] private string key;
        [SerializeField, TextArea] private string english;

        public string Key => key;
        public string English => english;

        public LocalizedEntry(string stableKey, string englishText)
        {
            key = stableKey;
            english = englishText;
        }
    }

    [CreateAssetMenu(menuName = "Lanternfall/Localization Catalog")]
    public sealed class LocalizationCatalog : ScriptableObject
    {
        [SerializeField] private LocalizedEntry[] entries =
            Array.Empty<LocalizedEntry>();

        public IReadOnlyList<LocalizedEntry> Entries => entries;

        public void Configure(LocalizedEntry[] values) =>
            entries = values ?? Array.Empty<LocalizedEntry>();

        public List<string> Validate()
        {
            var errors = new List<string>();
            var keys = new HashSet<string>();
            for (int index = 0; index < entries.Length; index++)
            {
                LocalizedEntry entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                    errors.Add($"Localization entry {index} has no key.");
                else if (!keys.Add(entry.Key))
                    errors.Add($"Duplicate localization key: {entry.Key}");
                if (entry != null && string.IsNullOrWhiteSpace(entry.English))
                    errors.Add($"Localization entry {entry.Key} has no English source.");
            }
            if (entries.Length < 20)
                errors.Add("Release localization catalog needs at least 20 UI entries.");
            return errors;
        }
    }

    /// <summary>
    /// Allocation-at-initialization lookup with English fallback and a
    /// deterministic pseudo-locale for clipping/unsupported-glyph QA.
    /// </summary>
    public sealed class LocalizationTable
    {
        private readonly Dictionary<string, string> _english;
        private readonly bool _pseudo;

        public LocalizationTable(LocalizationCatalog catalog, string locale)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            _pseudo = string.Equals(
                locale, "qps-ploc", StringComparison.OrdinalIgnoreCase);
            _english = new Dictionary<string, string>(
                catalog.Entries.Count, StringComparer.Ordinal);
            foreach (LocalizedEntry entry in catalog.Entries)
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Key))
                    _english[entry.Key] = entry.English;
        }

        public string Get(string key, string fallback = null)
        {
            if (!_english.TryGetValue(key, out string value))
                value = fallback ?? $"[{key}]";
            return _pseudo ? PseudoLocalize(value) : value;
        }

        public static string PseudoLocalize(string value)
        {
            if (string.IsNullOrEmpty(value)) return "[]";
            var output = new System.Text.StringBuilder(value.Length + 8);
            output.Append('[');
            foreach (char character in value)
            {
                output.Append(character switch
                {
                    'a' => 'á', 'e' => 'ë', 'i' => 'ï', 'o' => 'ø',
                    'u' => 'ü', 'A' => 'Å', 'E' => 'Ë', 'I' => 'Ï',
                    'O' => 'Ø', 'U' => 'Ü', _ => character
                });
            }
            output.Append(" !!!]");
            return output.ToString();
        }
    }

    public static class LocalizationRuntime
    {
        private static LocalizationTable _table;

        public static void Initialize(LocalizationCatalog catalog, string locale) =>
            _table = new LocalizationTable(catalog, locale);

        public static string Get(string key, string fallback) =>
            _table?.Get(key, fallback) ?? fallback;
    }

    public sealed class LocalizationBootstrap : MonoBehaviour
    {
        [SerializeField] private LocalizationCatalog catalog;
        [SerializeField] private string locale = "en";

        public void Configure(LocalizationCatalog value, string language)
        {
            catalog = value;
            locale = string.IsNullOrWhiteSpace(language) ? "en" : language;
        }

        private void Awake()
        {
            if (catalog != null) LocalizationRuntime.Initialize(catalog, locale);
        }
    }
}
