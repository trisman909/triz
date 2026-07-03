using Lanternfall.Gameplay.Accessibility;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    /// <summary>
    /// Fixed-size procedural VFX pool for combat readability. It provides an
    /// original low-poly visual language without runtime instantiation spikes.
    /// </summary>
    public sealed class CombatVfxDirector : MonoBehaviour
    {
        private const int PoolSizeValue = 32;
        private readonly GameObject[] _objects = new GameObject[PoolSizeValue];
        private readonly Renderer[] _renderers = new Renderer[PoolSizeValue];
        private readonly float[] _remaining = new float[PoolSizeValue];
        private readonly float[] _duration = new float[PoolSizeValue];
        private readonly Color[] _colors = new Color[PoolSizeValue];
        private int _next;
        private MaterialPropertyBlock _properties;

        public int PoolSize => PoolSizeValue;
        public int ActivePulseCount { get; private set; }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            Transform pool = new GameObject("Combat VFX Pool").transform;
            pool.SetParent(transform, false);
            for (int index = 0; index < PoolSizeValue; index++)
            {
                GameObject pulse = GameObject.CreatePrimitive(
                    index % 3 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
                pulse.name = $"Memory Pulse {index + 1}";
                pulse.transform.SetParent(pool, false);
                Collider collider = pulse.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                _objects[index] = pulse;
                _renderers[index] = pulse.GetComponent<Renderer>();
                _renderers[index].sharedMaterial =
                    UrpPresentationMaterials.Transparent;
                pulse.SetActive(false);
            }
        }

        private void OnEnable() =>
            GameplayPresentationSignals.Cue += OnCue;

        private void OnDisable() =>
            GameplayPresentationSignals.Cue -= OnCue;

        private void Update()
        {
            ActivePulseCount = 0;
            for (int index = 0; index < PoolSizeValue; index++)
            {
                if (_remaining[index] <= 0f) continue;
                _remaining[index] -= Time.deltaTime;
                if (_remaining[index] <= 0f)
                {
                    _objects[index].SetActive(false);
                    continue;
                }
                ActivePulseCount++;
                float progress =
                    1f - _remaining[index] / _duration[index];
                float size = Mathf.Lerp(.12f, 1.7f, progress);
                if (AccessibilityRuntime.ReducedMotion) size *= .65f;
                _objects[index].transform.localScale =
                    Vector3.one * size;
                Color color = _colors[index];
                color.a = (1f - progress) *
                    AccessibilityRuntime.FlashIntensity;
                ApplyColor(_renderers[index], color);
            }
        }

        private void OnCue(PresentationCue cue, Vector3 position)
        {
            if (cue == PresentationCue.Footstep ||
                cue == PresentationCue.UiConfirm) return;
            int index = _next++ % PoolSizeValue;
            GameObject pulse = _objects[index];
            pulse.transform.position = position + Vector3.up * .25f;
            pulse.transform.localScale = Vector3.one * .12f;
            _duration[index] =
                cue == PresentationCue.GuardianDeath ? .9f : .42f;
            _remaining[index] = _duration[index];
            _colors[index] = ColorFor(cue);
            ApplyColor(_renderers[index], _colors[index]);
            pulse.SetActive(true);
        }

        private void ApplyColor(Renderer renderer, Color color)
        {
            renderer.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", color);
            _properties.SetColor("_Color", color);
            _properties.SetColor("_EmissionColor", color * 1.5f);
            renderer.SetPropertyBlock(_properties);
        }

        private static Color ColorFor(PresentationCue cue)
        {
            if (AccessibilityRuntime.HighContrastTelegraphs)
                return cue == PresentationCue.EnemyTelegraph ||
                       cue == PresentationCue.GuardianTelegraph
                    ? new Color(1f, .9f, .05f)
                    : new Color(.1f, .95f, 1f);
            return cue switch
            {
                PresentationCue.Dodge => new Color(.1f, .85f, 1f),
                PresentationCue.Ability => new Color(.8f, .2f, 1f),
                PresentationCue.EnemyDeath => new Color(.65f, .12f, .2f),
                PresentationCue.GuardianDeath => new Color(1f, .32f, .08f),
                PresentationCue.EchoCollected => new Color(.8f, .35f, 1f),
                _ => new Color(1f, .55f, .12f)
            };
        }
    }
}
