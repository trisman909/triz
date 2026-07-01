using System;

namespace Lanternfall.Gameplay.Progression
{
    public enum CurrencyKind { Gold, Souls, AncientCrystals }

    public sealed class RunWallet
    {
        private readonly int[] _balances = new int[3];
        public event Action<CurrencyKind, int> Changed;

        public int Get(CurrencyKind currency) => _balances[(int)currency];

        public void Add(CurrencyKind currency, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            int index = (int)currency;
            checked { _balances[index] += amount; }
            Changed?.Invoke(currency, _balances[index]);
        }

        public bool TrySpend(CurrencyKind currency, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            int index = (int)currency;
            if (_balances[index] < amount) return false;
            _balances[index] -= amount;
            Changed?.Invoke(currency, _balances[index]);
            return true;
        }
    }
}

