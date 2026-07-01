using System;
using Lanternfall.Gameplay.Save;
using UnityEngine;

namespace Lanternfall.Gameplay.Accessibility
{
    /// <summary>
    /// Read-only runtime view of the active accessibility profile.
    /// Presentation systems use this instead of reaching into save state.
    /// </summary>
    public static class AccessibilityRuntime
    {
        public static event Action Changed;

        public static float CameraShake { get; private set; } = 1f;
        public static float FlashIntensity { get; private set; } = 1f;
        public static float UiScale { get; private set; } = 1f;
        public static float MusicVolume { get; private set; } = .8f;
        public static float EffectsVolume { get; private set; } = .9f;
        public static bool ReducedMotion { get; private set; }
        public static bool Subtitles { get; private set; } = true;
        public static bool HighContrastTelegraphs { get; private set; }

        public static void Apply(SettingsData settings)
        {
            if (settings == null) return;
            settings.masterVolume = Mathf.Clamp01(settings.masterVolume);
            settings.musicVolume = Mathf.Clamp01(settings.musicVolume);
            settings.effectsVolume = Mathf.Clamp01(settings.effectsVolume);
            settings.cameraShake = Mathf.Clamp01(settings.cameraShake);
            settings.flashIntensity = Mathf.Clamp01(settings.flashIntensity);
            settings.uiScale = Mathf.Clamp(settings.uiScale, .75f, 1.5f);
            CameraShake = settings.cameraShake;
            FlashIntensity = settings.flashIntensity;
            UiScale = settings.uiScale;
            MusicVolume = settings.musicVolume;
            EffectsVolume = settings.effectsVolume;
            ReducedMotion = settings.reducedMotion;
            Subtitles = settings.subtitles;
            HighContrastTelegraphs = settings.highContrastTelegraphs;
            AudioListener.volume = settings.masterVolume;
            Changed?.Invoke();
        }
    }
}
