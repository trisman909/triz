using System.Collections.Generic;
using System.IO;
using Lanternfall.Gameplay.Balance;
using Lanternfall.Gameplay.Localization;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Presentation;
using Lanternfall.Gameplay.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
            ProductionArtCatalog productionArt =
                AssetDatabase.LoadAssetAtPath<ProductionArtCatalog>(
                    SettingsRoot + "LanternfallProductionArt.asset");

            Append(errors, content?.ValidateReleaseCounts(), "Content catalog missing.");
            Append(errors, achievements?.ValidateReleaseCatalog(),
                "Achievement catalog missing.");
            Append(errors, localization?.Validate(), "Localization catalog missing.");
            Append(errors, balance?.Validate(), "Balance profile missing.");
            Append(errors, productionArt?.Validate(),
                "Production art catalog missing.");
            ValidateProductionPresentation(errors);

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length != 10)
                errors.Add(
                    $"Expected five gameplay and five art-review scenes, " +
                    $"found {scenes.Length}.");
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (!scene.enabled) errors.Add($"Build scene disabled: {scene.path}");
                if (!File.Exists(scene.path))
                    errors.Add($"Build scene missing: {scene.path}");
            }
            foreach (string glb in Directory.GetFiles(
                         "Assets", "*.glb", SearchOption.AllDirectories))
                if (Path.GetFileName(glb).IndexOf(
                        "ConceptProxy",
                        System.StringComparison.OrdinalIgnoreCase) >= 0)
                    errors.Add(
                        $"Concept proxy must not enter runtime Assets: {glb}");
            return errors;
        }

        private static void ValidateProductionPresentation(
            List<string> errors)
        {
            const string productionRoot =
                "Assets/_Project/Lanternfall/Art/Production/";
            string[] slugs =
            {
                "DrownedNarthex",
                "SiltglassObservatory",
                "EmberOssuary",
                "GloamOrchard",
                "StormvaultFoundry"
            };
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith(
                        productionRoot,
                        System.StringComparison.Ordinal) ||
                    !path.EndsWith(
                        ".mat",
                        System.StringComparison.OrdinalIgnoreCase))
                    continue;
                Material material =
                    AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null ||
                    material.shader == null ||
                    !material.shader.isSupported ||
                    !material.shader.name.StartsWith(
                        "Universal Render Pipeline/",
                        System.StringComparison.Ordinal))
                    errors.Add(
                        $"Production material is not URP-compatible: {path}");
            }
            foreach (string slug in slugs)
            {
                string capture =
                    $"Docs/ArtDirection/Comparisons/After_{slug}.png";
                if (!File.Exists(capture) ||
                    new FileInfo(capture).Length < 100000)
                    errors.Add(
                        $"Fresh gameplay-camera capture is missing: {capture}");

                string volumePath =
                    productionRoot + $"VFX/{slug}_Volume.asset";
                VolumeProfile profile =
                    AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumePath);
                if (profile == null ||
                    !profile.TryGet(out Tonemapping tonemapping) ||
                    tonemapping.mode.value != TonemappingMode.ACES)
                    errors.Add(
                        $"ACES tonemapping is not configured: {volumePath}");
                if (profile != null)
                    foreach (VolumeComponent component in profile.components)
                        if (component == null)
                            errors.Add(
                                $"Volume contains a null component: {volumePath}");
            }
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
