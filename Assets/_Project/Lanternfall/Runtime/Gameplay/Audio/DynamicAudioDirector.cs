using System.Collections;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Audio
{
    public enum MusicState { Exploration, Combat, Boss, Secret }

    /// <summary>
    /// License-safe adaptive score generated from Lanternfall's own tonal motif.
    /// Authored clips can replace these voices without changing callers.
    /// </summary>
    public sealed class DynamicAudioDirector : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float musicVolume = .22f;
        [SerializeField, Range(.1f, 3f)] private float crossfadeSeconds = .8f;

        private AudioSource _current;
        private AudioSource _next;
        private Coroutine _transition;
        private MusicState _state;

        public MusicState State => _state;

        private void Awake()
        {
            _current = CreateVoice("Score A");
            _next = CreateVoice("Score B");
            _current.clip = ProceduralClip(MusicState.Exploration);
            _current.volume = musicVolume;
            _current.Play();
        }

        private void OnEnable()
        {
            BossEncounterSignals.IntroStarted += OnBossIntro;
            BossEncounterSignals.Defeated += OnBossDefeated;
            AccessibilityRuntime.Changed += ApplyVolume;
            ApplyVolume();
        }

        private void OnDisable()
        {
            BossEncounterSignals.IntroStarted -= OnBossIntro;
            BossEncounterSignals.Defeated -= OnBossDefeated;
            AccessibilityRuntime.Changed -= ApplyVolume;
        }

        public void SetState(MusicState state)
        {
            if (state == _state) return;
            _state = state;
            if (_transition != null) StopCoroutine(_transition);
            _transition = StartCoroutine(Crossfade(ProceduralClip(state)));
        }

        public void PlayUiConfirm()
        {
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
        }

        private void OnBossIntro(string _) => SetState(MusicState.Boss);
        private void OnBossDefeated(string _) => SetState(MusicState.Exploration);

        private void ApplyVolume()
        {
            musicVolume = .28f * AccessibilityRuntime.MusicVolume;
            if (_transition == null && _current != null)
                _current.volume = musicVolume;
        }

        private AudioSource CreateVoice(string voiceName)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.name = voiceName;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private IEnumerator Crossfade(AudioClip destination)
        {
            _next.clip = destination;
            _next.volume = 0f;
            _next.Play();
            float elapsed = 0f;
            float duration = Mathf.Max(.01f, crossfadeSeconds);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float blend = Mathf.Clamp01(elapsed / duration);
                _current.volume = (1f - blend) * musicVolume;
                _next.volume = blend * musicVolume;
                yield return null;
            }
            _current.Stop();
            AudioSource swap = _current;
            _current = _next;
            _next = swap;
            _transition = null;
        }

        private static AudioClip ProceduralClip(MusicState state)
        {
            float root = state switch
            {
                MusicState.Combat => 146.83f,
                MusicState.Boss => 110f,
                MusicState.Secret => 196f,
                _ => 130.81f
            };
            float pulse = state == MusicState.Boss ? 4f :
                state == MusicState.Combat ? 3f : 1.5f;
            const int sampleRate = 22050;
            const float duration = 4f;
            float[] samples = new float[(int)(sampleRate * duration)];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float envelope = .35f + .65f *
                    Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * Mathf.PI * pulse)), 4f);
                samples[index] = envelope * (
                    Mathf.Sin(time * root * Mathf.PI * 2f) * .12f +
                    Mathf.Sin(time * root * 1.5f * Mathf.PI * 2f) * .06f);
            }
            AudioClip clip = AudioClip.Create(
                $"Lanternfall {state}", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

    }
}
