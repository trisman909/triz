using System;
using System.Collections.Generic;

namespace Lanternfall.Gameplay.Progression
{
    public readonly struct ResonanceSummary
    {
        public ResonanceSummary(int harmonies, int clashes, bool awakened, float potency)
        {
            Harmonies = harmonies;
            Clashes = clashes;
            Awakened = awakened;
            PotencyMultiplier = potency;
        }
        public int Harmonies { get; }
        public int Clashes { get; }
        public bool Awakened { get; }
        public float PotencyMultiplier { get; }
    }

    public sealed class ResonanceChain
    {
        private readonly RelicDefinition[] _slots = new RelicDefinition[3];
        public IReadOnlyList<RelicDefinition> Slots => _slots;

        public void Equip(int slot, RelicDefinition relic)
        {
            if (slot < 0 || slot >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(slot));
            _slots[slot] = relic;
        }

        public ResonanceSummary Evaluate()
        {
            int harmonies = 0;
            int clashes = 0;
            for (int index = 0; index < 2; index++)
            {
                RelicDefinition left = _slots[index];
                RelicDefinition right = _slots[index + 1];
                if (left == null || right == null) continue;
                if (left.Aspect == right.Aspect) harmonies++;
                else if (AreOpposed(left.Aspect, right.Aspect)) clashes++;
            }

            bool awakened = _slots[0] != null && _slots[1] != null && _slots[2] != null &&
                _slots[0].Aspect != _slots[1].Aspect &&
                _slots[1].Aspect != _slots[2].Aspect &&
                _slots[0].Aspect != _slots[2].Aspect;
            float multiplier = 1f + harmonies * 0.2f + clashes * 0.1f +
                (awakened ? 0.35f : 0f);
            return new ResonanceSummary(harmonies, clashes, awakened, multiplier);
        }

        private static bool AreOpposed(MemoryAspect left, MemoryAspect right)
        {
            return (left == MemoryAspect.Ember && right == MemoryAspect.Tide) ||
                (left == MemoryAspect.Tide && right == MemoryAspect.Ember) ||
                (left == MemoryAspect.Gloam && right == MemoryAspect.Radiance) ||
                (left == MemoryAspect.Radiance && right == MemoryAspect.Gloam) ||
                (left == MemoryAspect.Storm && right == MemoryAspect.Stone) ||
                (left == MemoryAspect.Stone && right == MemoryAspect.Storm);
        }
    }
}

