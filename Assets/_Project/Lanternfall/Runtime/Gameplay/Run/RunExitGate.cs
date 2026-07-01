using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Run
{
    [RequireComponent(typeof(Collider))]
    public sealed class RunExitGate : MonoBehaviour
    {
        private PlayerInputReader _visitor;
        public bool Unlocked { get; private set; }

        public void SetUnlocked(bool value)
        {
            Unlocked = value;
            if (TryGetComponent(out Renderer visual))
                visual.material.color = value
                    ? new Color(.2f, .9f, .65f)
                    : new Color(.4f, .08f, .08f);
        }

        private void Update()
        {
            if (!Unlocked || _visitor == null ||
                !_visitor.InteractPressedThisFrame) return;
            HubController hub = HubController.Instance;
            if (hub?.ActiveRun == null) return;
            bool hasNext = hub.ActiveRun.Advance();
            if (hasNext) SceneManager.LoadScene("RunChamber");
            else hub.CompleteRun();
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
