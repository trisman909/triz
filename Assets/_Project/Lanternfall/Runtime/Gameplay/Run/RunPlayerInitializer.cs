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
        private RunInventory _inventory;

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
            _inventory = GetComponent<RunInventory>();
            _health.Configure(selected.MaximumHealth, 5f, false);
            if (_hub.ActiveRun.CurrentHealth >= 0f)
                _health.Restore(_hub.ActiveRun.CurrentHealth);
            else
                _hub.ActiveRun.CurrentHealth = _health.Current;
            GetComponent<PlayerMotor>().ConfigureMoveSpeed(selected.MovementSpeed);
            GetComponent<PlayerCombat>().ApplyLoadout(
                selected.StartingWeapon, selected.StartingAbility);
            RestoreRunInventory();
            ClassPassiveController passives =
                GetComponent<ClassPassiveController>();
            if (passives != null) passives.Configure(selected, _hub.ActiveRun);
            _health.Changed += OnHealthChanged;
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
            _inventory.Changed += OnInventoryChanged;
            _inventory.Wallet.Changed += OnWalletChanged;
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Changed -= OnHealthChanged;
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
            if (_inventory != null)
            {
                _inventory.Changed -= OnInventoryChanged;
                _inventory.Wallet.Changed -= OnWalletChanged;
            }
        }

        private void OnHealthChanged(float current, float _) 
        {
            if (_hub?.ActiveRun != null)
                _hub.ActiveRun.CurrentHealth = current;
        }

        private void OnDied() => _hub?.FailRun();

        private void OnDamaged(DamageResult result) =>
            _hub?.ActiveRun?.RecordDamage(result.Amount);

        private void OnWalletChanged(CurrencyKind currency, int balance)
        {
            if (currency == CurrencyKind.Gold && _hub?.ActiveRun != null)
                _hub.ActiveRun.Gold = balance;
        }

        private void OnInventoryChanged()
        {
            if (_hub?.ActiveRun == null || _inventory == null) return;
            var ids = new string[_inventory.Owned.Count];
            for (int index = 0; index < ids.Length; index++)
                ids[index] = _inventory.Owned[index].StableId;
            _hub.ActiveRun.SetEchoes(ids);
        }

        private void RestoreRunInventory()
        {
            RunSession session = _hub.ActiveRun;
            for (int id = 0; id < session.EchoIds.Count; id++)
                for (int relic = 0; relic < contentCatalog.Relics.Count; relic++)
                    if (contentCatalog.Relics[relic].StableId ==
                        session.EchoIds[id])
                    {
                        _inventory.TryAdd(contentCatalog.Relics[relic]);
                        break;
                    }
            if (session.Gold > 0)
                _inventory.Wallet.Add(CurrencyKind.Gold, session.Gold);
        }
    }
}
