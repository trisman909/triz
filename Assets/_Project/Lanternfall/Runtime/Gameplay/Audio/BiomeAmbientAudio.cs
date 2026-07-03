using Lanternfall.Gameplay.Accessibility;
using UnityEngine;

namespace Lanternfall.Gameplay.Audio
{
    /// <summary>
    /// Lightweight procedural ambience for the five representative art
    /// chambers. Each biome uses a different spectral and rhythmic identity.
    /// </summary>
    public sealed class BiomeAmbientAudio : MonoBehaviour
    {
        [SerializeField, Range(0, 4)] private int biomeIndex;
        private AudioSource _bed;
        private AudioSource _detail;
        private AudioClip _detailClip;
        private float _detailTimer;
        private int _detailSequence;

        public int BiomeIndex => biomeIndex;
        public int GeneratedClipCount { get; private set; }

        public void Configure(int index) =>
            biomeIndex = Mathf.Clamp(index, 0, 4);

        private void Awake()
        {
            _bed = gameObject.AddComponent<AudioSource>();
            _bed.name = "Biome Atmosphere Bed";
            _bed.clip = BuildBed(biomeIndex);
            _bed.loop = true;
            _bed.spatialBlend = 0f;
            _bed.Play();

            _detail = gameObject.AddComponent<AudioSource>();
            _detail.name = "Biome Environmental Details";
            _detail.spatialBlend = .35f;
            _detail.rolloffMode = AudioRolloffMode.Linear;
            _detail.maxDistance = 24f;
            _detailClip = BuildDetail(biomeIndex);
            _detailTimer = 2.5f;
            GeneratedClipCount = 2;
            ApplyVolume();
        }

        private void OnEnable()
        {
            AccessibilityRuntime.Changed += ApplyVolume;
        }

        private void OnDisable()
        {
            AccessibilityRuntime.Changed -= ApplyVolume;
        }

        private void Update()
        {
            _detailTimer -= Time.unscaledDeltaTime;
            if (_detailTimer > 0f || _detail == null) return;
            _detailSequence++;
            _detail.pitch =
                .94f + (_detailSequence % 5) * .025f;
            _detail.transform.position = new Vector3(
                Mathf.Sin(_detailSequence * 2.3f) * 5f,
                1.5f + _detailSequence % 3,
                Mathf.Cos(_detailSequence * 1.7f) * 4f);
            _detail.PlayOneShot(_detailClip);
            _detailTimer =
                4.5f + ((_detailSequence * 37 + biomeIndex * 11) % 40) / 10f;
        }

        private void ApplyVolume()
        {
            float effects = AccessibilityRuntime.EffectsVolume;
            if (_bed != null) _bed.volume = .055f * effects;
            if (_detail != null) _detail.volume = .14f * effects;
        }

        private static AudioClip BuildBed(int biome)
        {
            const int sampleRate = 22050;
            const int duration = 8;
            float[] roots = { 43.65f, 55f, 36.71f, 49f, 65.41f };
            float[] pulses = { .18f, .08f, .42f, .11f, .65f };
            float[] samples = new float[sampleRate * duration];
            for (int index = 0; index < samples.Length; index++)
            {
                float t = index / (float)sampleRate;
                float root = roots[biome];
                float motion = .6f + .4f *
                    Mathf.Sin(t * Mathf.PI * 2f * pulses[biome]);
                float tone =
                    Mathf.Sin(t * root * Mathf.PI * 2f) * .055f +
                    Mathf.Sin(t * root * (biome == 1 ? 1.618f : 1.5f) *
                                Mathf.PI * 2f) * .025f;
                float texture = HashNoise(index + biome * 1709) *
                    (biome == 2 || biome == 4 ? .018f : .008f);
                samples[index] = (tone * motion + texture) * .7f;
            }
            AudioClip clip = AudioClip.Create(
                $"Lanternfall {biome} Atmosphere",
                samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip BuildDetail(int biome)
        {
            const int sampleRate = 22050;
            float[] durations = { .9f, 1.4f, .7f, 1.2f, .45f };
            float[] frequencies = { 190f, 720f, 110f, 260f, 980f };
            float duration = durations[biome];
            float[] samples =
                new float[Mathf.CeilToInt(sampleRate * duration)];
            for (int index = 0; index < samples.Length; index++)
            {
                float t = index / (float)sampleRate;
                float progress = index / (float)samples.Length;
                float envelope = Mathf.Sin(progress * Mathf.PI) *
                    (1f - progress * .35f);
                float sweep = frequencies[biome] *
                    Mathf.Lerp(biome == 4 ? 1.6f : .85f, 1f, progress);
                float tone = Mathf.Sin(t * sweep * Mathf.PI * 2f);
                float noise = HashNoise(index + biome * 613) *
                    (biome == 0 || biome == 2 ? .5f : .16f);
                samples[index] = (tone + noise) * envelope * .15f;
            }
            AudioClip clip = AudioClip.Create(
                $"Lanternfall {biome} Environmental Detail",
                samples.Length, 1, sampleRate, false);
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
