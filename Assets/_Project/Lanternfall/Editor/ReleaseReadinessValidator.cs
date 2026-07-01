using System.Collections.Generic;
using System.IO;
using Lanternfall.Gameplay.Balance;
using Lanternfall.Gameplay.Localization;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Lanternfall.Editor
{
    /// <summary>
    /// Single authoritative content/build-configuration gate used by CI,
    /// local milestone builds and the release-candidate phase.
    /// </summary>
    public static class ReleaseReadinessValidator
    {
        private const string SettingsRoot =
            "Assets/_Project/Lanternfall/Settings/";

        [MenuItem("Lanternfall/Validate Release Readiness")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log("Lanternfall release-readiness validation passed.");
        }

        public static void ValidateOrThrow()
        {
            List<string> errors = CollectErrors();
            if (errors.Count > 0)
                throw new BuildFailedException(string.Join("\n", errors));
            Debug.Log("Lanternfall release-readiness validation passed.");
        }

        public static List<string> CollectErrors()
        {
            var errors = new List<string>();
            ContentCatalog content = AssetDatabase.LoadAssetAtPath<ContentCatalog>(
                SettingsRoot + "LanternfallContentCatalog.asset");
            AchievementCatalog achievements =
                AssetDatabase.LoadAssetAtPath<AchievementCatalog>(
                    SettingsRoot + "LanternfallAchievementCatalog.asset");
            LocalizationCatalog localization =
                AssetDatabase.LoadAssetAtPath<LocalizationCatalog>(
                    SettingsRoot + "LanternfallLocalizationCatalog.asset");
            BalanceProfile balance = AssetDatabase.LoadAssetAtPath<BalanceProfile>(
                SettingsRoot + "LanternfallBalance.asset");

            Append(errors, content?.ValidateReleaseCounts(), "Content catalog missing.");
            Append(errors, achievements?.ValidateReleaseCatalog(),
                "Achievement catalog missing.");
            Append(errors, localization?.Validate(), "Localization catalog missing.");
            Append(errors, balance?.Validate(), "Balance profile missing.");

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length != 4)
                errors.Add($"Expected four build scenes, found {scenes.Length}.");
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (!scene.enabled) errors.Add($"Build scene disabled: {scene.path}");
                if (!File.Exists(scene.path))
                    errors.Add($"Build scene missing: {scene.path}");
            }
            return errors;
        }

        private static void Append(
            List<string> destination,
            List<string> source,
            string missingMessage)
        {
            if (source == null) destination.Add(missingMessage);
            else destination.AddRange(source);
        }
    }
}
