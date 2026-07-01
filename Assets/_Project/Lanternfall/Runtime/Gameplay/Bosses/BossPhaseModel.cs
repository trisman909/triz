using System;

namespace Lanternfall.Gameplay.Bosses
{
    public sealed class BossPhaseModel
    {
        public int Phase { get; private set; } = 1;
        public bool Enraged { get; private set; }

        public bool Update(float normalizedHealth)
        {
            if (normalizedHealth < 0f || normalizedHealth > 1f)
                throw new ArgumentOutOfRangeException(nameof(normalizedHealth));
            int previous = Phase;
            Phase = normalizedHealth <= 0.33f ? 3 :
                normalizedHealth <= 0.66f ? 2 : 1;
            Enraged = normalizedHealth <= 0.15f;
            return previous != Phase;
        }
    }
}

