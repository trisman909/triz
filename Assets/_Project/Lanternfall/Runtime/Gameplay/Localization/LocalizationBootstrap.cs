using UnityEngine;

namespace Lanternfall.Gameplay.Localization
{
    public sealed class LocalizationBootstrap : MonoBehaviour
    {
        [SerializeField] private LocalizationCatalog catalog;
        [SerializeField] private string locale = "en";

        public void Configure(LocalizationCatalog value, string language)
        {
            catalog = value;
            locale = string.IsNullOrWhiteSpace(language) ? "en" : language;
            if (Application.isPlaying && catalog != null)
                LocalizationRuntime.Initialize(catalog, locale);
        }

        private void Awake()
        {
            if (catalog != null) LocalizationRuntime.Initialize(catalog, locale);
        }
    }
}
