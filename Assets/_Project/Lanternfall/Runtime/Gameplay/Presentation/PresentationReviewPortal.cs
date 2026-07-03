using Lanternfall.Gameplay.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Presentation
{
    [RequireComponent(typeof(Collider))]
    public sealed class PresentationReviewPortal : MonoBehaviour
    {
        [SerializeField] private string destinationScene;
        [SerializeField] private string caption = "Enter art review";
        private PlayerInputReader _visitor;

        public void Configure(string destination, string prompt)
        {
            destinationScene = destination;
            caption = prompt;
        }

        private void Update()
        {
            if (_visitor == null || !_visitor.InteractPressedThisFrame ||
                string.IsNullOrWhiteSpace(destinationScene)) return;
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                caption);
            SceneManager.LoadScene(destinationScene);
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
