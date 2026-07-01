using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.World;
using UnityEngine;

namespace Lanternfall.Gameplay.Run
{
    [RequireComponent(typeof(Health), typeof(PlayerMotor), typeof(PlayerCombat))]
    public sealed class RunPlayerInitializer : MonoBehaviour
    {
        [SerializeField] private ContentCatalog contentCatalog;
        private Health _health;
        private HubController _hub;

        public void Configure(ContentCatalog catalog) => contentCatalog = catalog;

        private void Start()
        {
            _hub = HubController.Instance;
            if (_hub?.ActiveRun == null || contentCatalog == null) return;
            CharacterClassDefinition selected = null;
            foreach (CharacterClassDefinition candidate in contentCatalog.Classes)
                if (candidate != null &&
                    candidate.StableId == _hub.ActiveRun.ClassId)
                {
                    selected = candidate;
                    break;
                }
            if (selected == null) return;

            _health = GetComponent<Health>();
            _health.Configure(selected.MaximumHealth, 5f, false);
            if (_hub.ActiveRun.CurrentHealth >= 0f)
                _health.Restore(_hub.ActiveRun.CurrentHealth);
            else
                _hub.ActiveRun.CurrentHealth = _health.Current;
            GetComponent<PlayerMotor>().ConfigureMoveSpeed(selected.MovementSpeed);
            GetComponent<PlayerCombat>().ApplyLoadout(
                selected.StartingWeapon, selected.StartingAbility);
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
        }

        private void OnHealthChanged(float current, float _) 
        {
            if (_hub?.ActiveRun != null)
                _hub.ActiveRun.CurrentHealth = current;
        }

        private void OnDied() => _hub?.FailRun();
    }
}
