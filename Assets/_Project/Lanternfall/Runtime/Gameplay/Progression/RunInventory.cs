using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    public sealed class RunInventory : MonoBehaviour
    {
        private readonly List<RelicDefinition> _owned = new List<RelicDefinition>(32);
        private readonly ResonanceChain _chain = new ResonanceChain();
        private readonly RunWallet _wallet = new RunWallet();

        public IReadOnlyList<RelicDefinition> Owned => _owned;
        public ResonanceChain Chain => _chain;
        public RunWallet Wallet => _wallet;
        public event Action Changed;

        public bool TryAdd(RelicDefinition relic)
        {
            if (relic == null || _owned.Contains(relic) || _owned.Count >= 3)
                return false;
            _owned.Add(relic);
            for (int slot = 0; slot < _chain.Slots.Count; slot++)
            {
                if (_chain.Slots[slot] == null)
                {
                    _chain.Equip(slot, relic);
                    break;
                }
            }
            Changed?.Invoke();
            return true;
        }

        public bool Contains(string stableId)
        {
            for (int index = 0; index < _owned.Count; index++)
                if (_owned[index].StableId == stableId) return true;
            return false;
        }
    }
}
