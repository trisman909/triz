using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Audio
{
    /// <summary>
    /// Original procedural SFX and ambience bank. Clips are generated once,
    /// then replayed through a fixed voice pool without hot-path allocation.
    /// </summary>
    public sealed class GameplayAudioDirector : MonoBehaviour
    {
        private const int VoiceCount = 10;
        private AudioClip[] _clips;
        private AudioSource[] _voices;
        private AudioSource _ambience;
        private int _nextVoice;

        public int GeneratedClipCount => _clips?.Length ?? 0;
        public int CueCount { get; private set; }

        private void Awake()
        {
            int count = System.Enum.GetValues(typeof(PresentationCue)).Length;
            _clips = new AudioClip[count];
            for (int index = 0; index < count; index++)
                _clips[index] = BuildCue((PresentationCue)index);

            _voices = new AudioSource[VoiceCount];
            for (int index = 0; index < VoiceCount; index++)
            {
                AudioSource voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = .65f;
                voice.rolloffMode = AudioRolloffMode.Linear;
                voice.minDistance = 2f;
                voice.maxDistance = 28f;
                _voices[index] = voice;
            }

            _ambience = gameObject.AddComponent<AudioSource>();
            _ambience.name = "Procedural Biome Ambience";
            _ambience.clip = BuildAmbience();
            _ambience.loop = true;
            _ambience.spatialBlend = 0f;
            _ambience.Play();
            ApplyVolume();
        }

        private void OnEnable()
        {
            GameplayPresentationSignals.Cue += OnCue;
            AccessibilityRuntime.Changed += ApplyVolume;
        }

        private void OnDisable()
        {
            GameplayPresentationSignals.Cue -= OnCue;
            AccessibilityRuntime.Changed -= ApplyVolume;
        }

        private void OnCue(PresentationCue cue, Vector3 position)
        {
            if (_voices == null || _clips == null) return;
            AudioSource voice = _voices[_nextVoice++ % _voices.Length];
            voice.transform.position = position;
            voice.spatialBlend =
                cue == PresentationCue.UiConfirm ? 0f : .65f;
            voice.pitch = 1f + (((CueCount * 37) % 9) - 4) * .0125f;
            voice.PlayOneShot(
                _clips[(int)cue],
                AccessibilityRuntime.EffectsVolume);
            CueCount++;
        }

        private void ApplyVolume()
        {
            if (_ambience != null)
                _ambience.volume = .13f * AccessibilityRuntime.EffectsVolume;
        }

        private static AudioClip BuildCue(PresentationCue cue)
        {
            float[] frequencies =
            {
                92f, 180f, 520f, 260f, 110f, 340f,
                75f, 220f, 55f, 690f, 760f, 125f
            };
            float[] durations =
            {
                .08f, .18f, .11f, .3f, .1f, .35f,
                .22f, .5f, .65f, .24f, .1f, .32f
            };
            float frequency = frequencies[(int)cue];
            float duration = durations[(int)cue];
            const int sampleRate = 22050;
            float[] samples = new float[Mathf.CeilToInt(sampleRate * duration)];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float progress = index / (float)samples.Length;
                float decay = Mathf.Pow(1f - progress, 2f);
                float sweep = frequency * (1f + progress *
                    (cue == PresentationCue.Dodge ? 1.4f : -.18f));
                float fundamental = Mathf.Sin(
                    time * sweep * Mathf.PI * 2f);
                float overtone = Mathf.Sin(
                    time * sweep * 1.497f * Mathf.PI * 2f) * .35f;
                float noise = HashNoise(index + (int)cue * 997) *
                    (cue == PresentationCue.Impact ||
                     cue == PresentationCue.Hazard ? .28f : .06f);
                samples[index] =
                    (fundamental + overtone + noise) * decay * .24f;
            }
            AudioClip clip = AudioClip.Create(
                $"Lanternfall {cue}", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip BuildAmbience()
        {
            const int sampleRate = 22050;
            const int seconds = 6;
            float[] samples = new float[sampleRate * seconds];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float tide = Mathf.Sin(time * 32.7f * Mathf.PI * 2f) * .035f;
                float lantern = Mathf.Sin(time * 65.4f * Mathf.PI * 2f) *
                    (.015f + .01f * Mathf.Sin(time * .4f));
                samples[index] = tide + lantern + HashNoise(index) * .007f;
            }
            AudioClip clip = AudioClip.Create(
                "Lanternfall Ruin Ambience",
                samples.Length,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float HashNoise(int value)
        {
            uint hash = (uint)value;
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            return (hash & 0xffff) / 32767.5f - 1f;
        }
    }
}
