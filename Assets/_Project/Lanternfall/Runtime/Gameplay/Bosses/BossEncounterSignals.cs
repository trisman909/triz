using System;

namespace Lanternfall.Gameplay.Bosses
{
    public static class BossEncounterSignals
    {
        public static event Action<string> IntroStarted;
        public static event Action<float, float> HealthChanged;
        public static event Action<int> PhaseChanged;
        public static event Action<string> Defeated;

        internal static void RaiseIntro(string name) => IntroStarted?.Invoke(name);
        internal static void RaiseHealth(float current, float maximum) =>
            HealthChanged?.Invoke(current, maximum);
        internal static void RaisePhase(int phase) => PhaseChanged?.Invoke(phase);
        internal static void RaiseDefeated(string id) => Defeated?.Invoke(id);
    }
}

