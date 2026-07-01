using Lanternfall.Gameplay.Input;
using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    [RequireComponent(typeof(Collider))]
    public sealed class RewardPedestal : MonoBehaviour
    {
        [SerializeField] private RelicDefinition relic;
        private RunInventory _candidate;

        public void Configure(RelicDefinition definition) => relic = definition;

        private void Update()
        {
            if (_candidate == null || relic == null) return;
            PlayerInputReader input = _candidate.GetComponent<PlayerInputReader>();
            if (input != null && input.InteractPressedThisFrame)
                TryCollect(_candidate);
        }

        public bool TryCollect(RunInventory inventory)
        {
            if (inventory == null || relic == null || !inventory.TryAdd(relic))
                return false;
            gameObject.SetActive(false);
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            _candidate = other.GetComponentInParent<RunInventory>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<RunInventory>() == _candidate)
                _candidate = null;
        }
    }
}
