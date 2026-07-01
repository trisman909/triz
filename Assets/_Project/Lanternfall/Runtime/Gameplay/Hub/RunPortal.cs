using Lanternfall.Gameplay.Input;
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
            if (_visitor != null && _visitor.InteractPressedThisFrame)
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

