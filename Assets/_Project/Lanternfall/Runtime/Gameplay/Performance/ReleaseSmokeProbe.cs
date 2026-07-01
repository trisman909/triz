using System;
using System.Collections;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.World;
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
            bool hubValid =
                UnityEngine.Camera.main != null &&
                FindFirstObjectByType<PlayerInputReader>() != null &&
                FindFirstObjectByType<Health>() != null &&
                FindFirstObjectByType<FrameBudgetMonitor>() != null;
            if (!hubValid)
            {
                Fail("required hub services missing");
                yield break;
            }

            SceneManager.LoadScene("FirstBiomeVerticalSlice");
            for (int frame = 0; frame < 10; frame++) yield return null;
            bool runValid =
                UnityEngine.Camera.main != null &&
                FindFirstObjectByType<PlayerInputReader>() != null &&
                FindFirstObjectByType<Health>() != null &&
                FindFirstObjectByType<BiomeAtmosphere>() != null &&
                FindFirstObjectByType<BossEncounterDirector>() != null;
            if (!runValid)
            {
                Fail("required vertical-slice services missing");
                yield break;
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
