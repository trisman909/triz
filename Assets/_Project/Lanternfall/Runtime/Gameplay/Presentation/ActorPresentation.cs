using Lanternfall.Gameplay.Accessibility;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    /// <summary>
    /// Coherent procedural low-poly mantle and motion layer for actors.
    /// Gameplay collision and authoritative transforms remain untouched.
    /// </summary>
    public sealed class ActorPresentation : MonoBehaviour
    {
        private Transform _rig;
        private Renderer[] _renderers;
        private Color _color = new Color(.2f, .8f, 1f);
        private float _scale = 1f;
        private bool _guardian;
        private bool _usesAuthoredMaterial;
        private MaterialPropertyBlock _properties;

        public int OrnamentCount => _rig != null ? _rig.childCount : 0;

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            Build();
        }

        public void Configure(Color color, float scale, bool guardian)
        {
            _color = color;
            _scale = Mathf.Max(.2f, scale);
            _guardian = guardian;
            if (_rig != null)
            {
                Transform previousRig = _rig;
                _rig = null;
                _renderers = null;
                if (Application.isPlaying) Destroy(previousRig.gameObject);
                else DestroyImmediate(previousRig.gameObject);
            }
            Build();
        }

        private void Update()
        {
            if (_rig == null) return;
            float amplitude = AccessibilityRuntime.ReducedMotion ? .015f : .08f;
            _rig.localPosition = Vector3.up *
                (Mathf.Sin(Time.time * (_guardian ? 1.4f : 2.6f)) *
                 amplitude);
            if (!AccessibilityRuntime.ReducedMotion)
                _rig.Rotate(Vector3.up, (_guardian ? 9f : 18f) * Time.deltaTime);
        }

        private void Build()
        {
            if (_rig != null) return;
            Renderer sourceRenderer = GetComponentInChildren<Renderer>();
            _usesAuthoredMaterial =
                sourceRenderer != null &&
                sourceRenderer.sharedMaterial != null &&
                UrpPresentationMaterials.PreferCompatible(
                    sourceRenderer.sharedMaterial) ==
                sourceRenderer.sharedMaterial;
            Material presentationMaterial =
                UrpPresentationMaterials.PreferCompatible(
                    sourceRenderer != null
                        ? sourceRenderer.sharedMaterial
                        : null);
            _rig = new GameObject("Living Mantle").transform;
            _rig.SetParent(transform, false);
            int count = _guardian ? 6 : 3;
            for (int index = 0; index < count; index++)
            {
                GameObject ornament = GameObject.CreatePrimitive(
                    index % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                ornament.name = $"Mantle Shard {index + 1}";
                ornament.transform.SetParent(_rig, false);
                float angle = index * Mathf.PI * 2f / count;
                ornament.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * .55f * _scale,
                    (.25f + index % 2 * .25f) * _scale,
                    Mathf.Sin(angle) * .55f * _scale);
                ornament.transform.localScale =
                    new Vector3(.12f, .32f, .08f) * _scale;
                ornament.GetComponent<Renderer>().sharedMaterial =
                    presentationMaterial;
                Collider collider = ornament.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying) Destroy(collider);
                    else DestroyImmediate(collider);
                }
            }
            _renderers = _rig.GetComponentsInChildren<Renderer>();
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (_renderers == null) return;
            if (_usesAuthoredMaterial)
            {
                foreach (Renderer renderer in _renderers)
                    renderer.SetPropertyBlock(null);
                return;
            }
            _properties ??= new MaterialPropertyBlock();
            foreach (Renderer renderer in _renderers)
            {
                renderer.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", _color);
                _properties.SetColor("_Color", _color);
                _properties.SetColor("_EmissionColor", _color * .8f);
                renderer.SetPropertyBlock(_properties);
            }
        }
    }
}
