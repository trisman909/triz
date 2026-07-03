using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Hub
{
    [RequireComponent(typeof(Collider))]
    public sealed class ClassSelectionPedestal : MonoBehaviour
    {
        [SerializeField] private string classId;
        private PlayerInputReader _visitor;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public void Configure(string stableId) => classId = stableId;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _properties = new MaterialPropertyBlock();
            if (HubController.Instance != null)
                HubController.Instance.ClassSelected += OnClassSelected;
            RefreshVisual();
        }

        private void OnDestroy()
        {
            if (HubController.Instance != null)
                HubController.Instance.ClassSelected -= OnClassSelected;
        }

        private void Update()
        {
            if (_visitor == null || !_visitor.InteractPressedThisFrame) return;
            if (HubController.Instance?.SelectClass(classId) != true) return;
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                $"{HubController.Instance.SelectedClassName} calling selected.");
        }

        private void OnTriggerEnter(Collider other) =>
            _visitor = other.GetComponentInParent<PlayerInputReader>();

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerInputReader>() == _visitor)
                _visitor = null;
        }

        private void OnClassSelected(string _) => RefreshVisual();

        private void RefreshVisual()
        {
            if (_renderer == null) return;
            bool selected = HubController.Instance?.SelectedClassId == classId;
            _renderer.GetPropertyBlock(_properties);
            _properties.SetColor(
                BaseColor,
                selected
                    ? new Color(.2f, 1f, .75f)
                    : new Color(.35f, .18f, .5f));
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
