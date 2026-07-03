using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Hub
{
    [RequireComponent(typeof(Collider))]
    public sealed class HubNpc : MonoBehaviour
    {
        [SerializeField] private NpcDefinition definition;
        [SerializeField, Min(1)] private int questSteps = 3;
        [SerializeField] private string completionUnlock;
        private PlayerInputReader _visitor;

        public void Configure(
            NpcDefinition npc,
            int steps,
            string unlock)
        {
            definition = npc;
            questSteps = Mathf.Max(1, steps);
            completionUnlock = unlock;
        }

        private void Update()
        {
            if (_visitor == null || !_visitor.InteractPressedThisFrame) return;
            if (HubController.Instance?.AdvanceQuest(
                    definition, questSteps, completionUnlock) != true) return;
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.UiConfirm,
                transform.position);
            GameplayPresentationSignals.RaiseSubtitle(
                definition.DisplayName,
                definition.Greeting,
                4f);
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
