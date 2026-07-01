using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Hub;
using UnityEngine;

namespace Lanternfall.Gameplay.Run
{
    [RequireComponent(typeof(Collider))]
    public sealed class VowPedestal : MonoBehaviour
    {
        [SerializeField] private VowKind vow;
        private PlayerInputReader _visitor;

        public void Configure(VowKind value)
        {
            vow = value;
            name = $"Vow — {Title(value)}";
        }

        private void Update()
        {
            if (_visitor == null || !_visitor.InteractPressedThisFrame) return;
            RunSession session =
                HubController.Instance?.ActiveRun;
            if (session != null && session.ActivateVow(vow))
                gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other) =>
            _visitor = other.GetComponentInParent<PlayerInputReader>();

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerInputReader>() == _visitor)
                _visitor = null;
        }

        public static string Title(VowKind value)
        {
            switch (value)
            {
                case VowKind.UnbrokenFlame: return "Take no damage";
                case VowKind.SwiftPassage: return "Clear within 75 seconds";
                default: return "Defeat 3 beyond the lantern";
            }
        }
    }
}
