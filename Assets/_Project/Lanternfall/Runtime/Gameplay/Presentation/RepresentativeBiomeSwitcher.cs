using System.Collections;
using Lanternfall.Gameplay.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Presentation
{
    public sealed class RepresentativeBiomeSwitcher : MonoBehaviour
    {
        [SerializeField] private int currentBiomeIndex;
        [SerializeField] private string[] reviewScenes;
        [SerializeField] private string[] biomeNames;
        private bool _switching;

        public void Configure(
            int biomeIndex,
            string[] scenes,
            string[] names)
        {
            currentBiomeIndex = Mathf.Clamp(biomeIndex, 0, 4);
            reviewScenes = (string[])scenes.Clone();
            biomeNames = (string[])names.Clone();
        }

        private IEnumerator Start()
        {
            yield return null;
            AnnounceCurrentBiome();
        }

        private void Update()
        {
            if (_switching) return;
            int requestedBiome = ReadRequestedBiome();
            if (requestedBiome < 0 ||
                requestedBiome == currentBiomeIndex ||
                reviewScenes == null ||
                requestedBiome >= reviewScenes.Length ||
                string.IsNullOrWhiteSpace(reviewScenes[requestedBiome]))
                return;
            SwitchToBiome(requestedBiome);
        }

        public void SwitchToBiome(int biomeIndex)
        {
            if (_switching || reviewScenes == null) return;
            int clamped = Mathf.Clamp(biomeIndex, 0, reviewScenes.Length - 1);
            if (clamped == currentBiomeIndex) return;
            _switching = true;
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
            SceneManager.LoadScene(reviewScenes[clamped]);
        }

        private void AnnounceCurrentBiome()
        {
            string name = ResolveBiomeName(currentBiomeIndex);
            FindFirstObjectByType<GameHud>()?.ShowTitleCard(
                $"{name.ToUpperInvariant()}\n<size=22>1-5 SWITCH BIOMES</size>",
                3.25f);
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                $"{name}. Keys 1 through 5 switch remembered realms.");
        }

        private int ReadRequestedBiome()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return -1;
            if (keyboard.digit1Key.wasPressedThisFrame ||
                keyboard.numpad1Key.wasPressedThisFrame) return 0;
            if (keyboard.digit2Key.wasPressedThisFrame ||
                keyboard.numpad2Key.wasPressedThisFrame) return 1;
            if (keyboard.digit3Key.wasPressedThisFrame ||
                keyboard.numpad3Key.wasPressedThisFrame) return 2;
            if (keyboard.digit4Key.wasPressedThisFrame ||
                keyboard.numpad4Key.wasPressedThisFrame) return 3;
            if (keyboard.digit5Key.wasPressedThisFrame ||
                keyboard.numpad5Key.wasPressedThisFrame) return 4;
            return -1;
        }

        private string ResolveBiomeName(int biomeIndex)
        {
            if (biomeNames == null ||
                biomeIndex < 0 ||
                biomeIndex >= biomeNames.Length ||
                string.IsNullOrWhiteSpace(biomeNames[biomeIndex]))
                return "Review Chamber";
            return biomeNames[biomeIndex];
        }
    }
}
