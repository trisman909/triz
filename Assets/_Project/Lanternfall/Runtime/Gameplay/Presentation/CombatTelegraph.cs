using Lanternfall.Gameplay.Accessibility;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    /// <summary>
    /// Color-safe ground telegraph shared by enemies and guardians.
    /// </summary>
    public sealed class CombatTelegraph : MonoBehaviour
    {
        private GameObject _disc;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        private float _duration;
        private float _remaining;
        private float _radius;

        public bool Visible => _disc != null && _disc.activeSelf;
        public Color CurrentColor { get; private set; }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            _disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _disc.name = "Accessible Attack Telegraph";
            _disc.transform.SetParent(transform, false);
            _disc.transform.localPosition = new Vector3(0f, .04f, 0f);
            Collider collider = _disc.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            _renderer = _disc.GetComponent<Renderer>();
            _renderer.sharedMaterial = UrpPresentationMaterials.Transparent;
            _disc.SetActive(false);
            AccessibilityRuntime.Changed += ApplyAccessibility;
        }

        private void OnDestroy() =>
            AccessibilityRuntime.Changed -= ApplyAccessibility;

        private void Update()
        {
            if (!Visible) return;
            _remaining -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(
                _remaining / Mathf.Max(.01f, _duration));
            float pulse = AccessibilityRuntime.ReducedMotion
                ? 1f
                : .92f + Mathf.Sin(progress * Mathf.PI * 6f) * .08f;
            _disc.transform.localScale =
                new Vector3(_radius * pulse, .035f, _radius * pulse);
            Color color = CurrentColor;
            color.a = Mathf.Lerp(.35f, .9f, progress) *
                AccessibilityRuntime.FlashIntensity;
            ApplyColor(color);
        }

        public void Show(float radius, float duration)
        {
            _radius = Mathf.Clamp(radius, .8f, 8f);
            _duration = Mathf.Max(.05f, duration);
            _remaining = _duration;
            ApplyAccessibility();
            _disc.SetActive(true);
        }

        public void Hide()
        {
            if (_disc != null) _disc.SetActive(false);
        }

        private void ApplyAccessibility()
        {
            CurrentColor = AccessibilityRuntime.HighContrastTelegraphs
                ? new Color(1f, .92f, .05f, .8f)
                : new Color(1f, .08f, .025f, .72f);
            if (_renderer != null) ApplyColor(CurrentColor);
        }

        private void ApplyColor(Color color)
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", color);
            _properties.SetColor("_Color", color);
            _properties.SetColor("_EmissionColor", color * 1.8f);
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
