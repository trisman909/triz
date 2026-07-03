using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Hub
{
    [RequireComponent(typeof(Collider))]
    public sealed class RunPortal : MonoBehaviour
    {
        [SerializeField] private string runScene =
            "FirstBiomeVerticalSlice";
        private PlayerInputReader _visitor;

        public void Configure(string sceneName) => runScene = sceneName;

        private void Update()
        {
            if (_visitor == null || !_visitor.InteractPressedThisFrame) return;
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                "The descent remembers your calling.");
            HubController.Instance?.StartRun(runScene);
        }

        private void OnTriggerEnter(Collider other) =>
            _visitor = other.GetComponentInParent<PlayerInputReader>();

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerInputReader>() == _visitor)
                _visitor = null;
        }
    }
}
