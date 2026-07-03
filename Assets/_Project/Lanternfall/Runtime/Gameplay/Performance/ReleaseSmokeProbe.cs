using System;
using System.Collections;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Run;
using Lanternfall.Gameplay.World;
using Lanternfall.Gameplay.Audio;
using Lanternfall.Gameplay.Presentation;
using Lanternfall.Gameplay.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Performance
{
    /// <summary>
    /// Opt-in packaged-player startup probe. It is inert in normal play and
    /// gives CI an unambiguous process exit code plus log marker.
    /// </summary>
    public sealed class ReleaseSmokeProbe : MonoBehaviour
    {
        private static bool _running;

        private void Start()
        {
            if (!HasArgument("-smokeTest") || _running) return;
            _running = true;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(VerifyAfterStartup());
        }

        private IEnumerator VerifyAfterStartup()
        {
            for (int frame = 0; frame < 5; frame++) yield return null;
            PlayerInputReader hubInput =
                FindFirstObjectByType<PlayerInputReader>();
            GameplayAudioDirector hubAudio =
                FindFirstObjectByType<GameplayAudioDirector>();
            CombatVfxDirector hubVfx =
                FindFirstObjectByType<CombatVfxDirector>();
            bool hubValid =
                UnityEngine.Camera.main != null &&
                hubInput != null &&
                hubInput.RebindSlotCount >= 18 &&
                FindFirstObjectByType<Health>() != null &&
                FindFirstObjectByType<FrameBudgetMonitor>() != null &&
                FindFirstObjectByType<GameHud>() != null &&
                FindFirstObjectByType<ActorPresentation>() != null &&
                hubAudio != null &&
                hubAudio.GeneratedClipCount == 12 &&
                hubVfx != null &&
                hubVfx.PoolSize == 32;
            if (!hubValid)
            {
                Fail("required hub services missing");
                yield break;
            }

            HubController.Instance.PrepareRun(20260701UL, "class.vanguard");
            SceneManager.LoadScene("RunChamber");
            for (int frame = 0; frame < 10; frame++) yield return null;
            BiomeChamberPresenter biome =
                FindFirstObjectByType<BiomeChamberPresenter>();
            GameHud hud = FindFirstObjectByType<GameHud>();
            bool runValid =
                UnityEngine.Camera.main != null &&
                FindFirstObjectByType<PlayerInputReader>() != null &&
                FindFirstObjectByType<Health>() != null &&
                FindFirstObjectByType<BiomeAtmosphere>() != null &&
                biome != null &&
                biome.BiomeIndex == 0 &&
                biome.GeneratedPropCount == 14 &&
                hud != null &&
                hud.RouteText.Contains("ROOM 1/8") &&
                FindFirstObjectByType<GameplayAudioDirector>() != null &&
                FindFirstObjectByType<CombatVfxDirector>() != null &&
                FindFirstObjectByType<RunChamberController>() != null &&
                FindFirstObjectByType<RunExitGate>() != null;
            if (!runValid)
            {
                Fail("required vertical-slice services missing");
                yield break;
            }

            string[] reviewScenes =
            {
                "ArtReview_DrownedNarthex",
                "ArtReview_SiltglassObservatory",
                "ArtReview_EmberOssuary",
                "ArtReview_GloamOrchard",
                "ArtReview_StormvaultFoundry"
            };
            for (int biomeIndex = 0;
                 biomeIndex < reviewScenes.Length;
                 biomeIndex++)
            {
                SceneManager.LoadScene(reviewScenes[biomeIndex]);
                for (int frame = 0; frame < 8; frame++) yield return null;
                BiomeAmbientAudio ambience =
                    FindFirstObjectByType<BiomeAmbientAudio>();
                bool allUrp = true;
                foreach (Renderer renderer in
                         FindObjectsByType<Renderer>(
                             FindObjectsSortMode.None))
                {
                    if (renderer.GetComponent<TextMesh>() != null) continue;
                    foreach (Material material in renderer.sharedMaterials)
                        if (material == null ||
                            material.shader == null ||
                            !material.shader.isSupported ||
                            !material.shader.name.StartsWith(
                                "Universal Render Pipeline/",
                                StringComparison.Ordinal))
                            allUrp = false;
                }
                bool reviewValid =
                    UnityEngine.Camera.main != null &&
                    FindObjectsByType<ProductionAssetMarker>(
                        FindObjectsSortMode.None).Length >= 11 &&
                    FindObjectsByType<ActorPresentation>(
                        FindObjectsSortMode.None).Length >= 2 &&
                    FindFirstObjectByType<PresentationReviewPortal>() != null &&
                    FindFirstObjectByType<FrameBudgetMonitor>() != null &&
                    ambience != null &&
                    ambience.BiomeIndex == biomeIndex &&
                    ambience.GeneratedClipCount == 2 &&
                    allUrp;
                if (!reviewValid)
                {
                    Fail(
                        $"representative review failed: " +
                        $"{reviewScenes[biomeIndex]}");
                    yield break;
                }
            }
            Debug.Log("LANTERNFALL_SMOKE_PASS");
            Application.Quit(0);
        }

        private static void Fail(string reason)
        {
            Debug.LogError($"LANTERNFALL_SMOKE_FAIL: {reason}");
            Application.Quit(2);
        }

        private static bool HasArgument(string expected)
        {
            foreach (string argument in Environment.GetCommandLineArgs())
                if (string.Equals(
                    argument, expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
