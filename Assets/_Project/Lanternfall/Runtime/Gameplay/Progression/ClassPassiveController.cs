using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Radiance;
using Lanternfall.Gameplay.Run;
using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    [RequireComponent(typeof(Health), typeof(PlayerCombat), typeof(RunInventory))]
    public sealed class ClassPassiveController : MonoBehaviour
    {
        private string _passiveTag;
        private PlayerCombat _combat;
        private RunInventory _inventory;

        public string ActivePassive => _passiveTag;

        public void Configure(CharacterClassDefinition definition, RunSession session)
        {
            if (definition == null) return;
            _passiveTag = definition.PassiveTag;
            _combat = GetComponent<PlayerCombat>();
            _inventory = GetComponent<RunInventory>();
            _inventory.Changed -= RefreshSequence;
            _inventory.Changed += RefreshSequence;

            switch (_passiveTag)
            {
                case "guard.radiance":
                    GetComponent<Health>().SetRuntimeArmor(20f);
                    break;
                case "mote.pin":
                    GetComponentInChildren<RadianceField>()?.ConfigureRadius(8f);
                    break;
                case "boundary.crossing":
                    if (session != null) session.OutsideRewardMultiplier = 2f;
                    break;
                case "lens.refraction":
                    _combat.SetAbilityCooldownMultiplier(.7f);
                    break;
                case "verse.sequence":
                    RefreshSequence();
                    break;
            }
        }

        private void OnDestroy()
        {
            if (_inventory != null) _inventory.Changed -= RefreshSequence;
        }

        private void RefreshSequence()
        {
            if (_passiveTag != "verse.sequence" || _combat == null) return;
            _combat.SetAbilityCooldownMultiplier(
                Mathf.Max(.55f, 1f - (_inventory?.Owned.Count ?? 0) * .12f));
        }
    }
}
