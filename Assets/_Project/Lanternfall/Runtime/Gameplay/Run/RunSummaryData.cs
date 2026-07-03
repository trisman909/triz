namespace Lanternfall.Gameplay.Run
{
    /// <summary>
    /// Immutable result captured before a RunSession is released. The hub can
    /// present this after the scene transition without retaining mutable run state.
    /// </summary>
    public sealed class RunSummaryData
    {
        public RunSummaryData(
            bool victory,
            ulong seed,
            string classId,
            float elapsedSeconds,
            int roomsCleared,
            int enemiesDefeated,
            int guardiansDefeated,
            int gold,
            int echoes,
            int vowsFulfilled,
            int vowsBroken)
        {
            Victory = victory;
            Seed = seed;
            ClassId = classId;
            ElapsedSeconds = elapsedSeconds;
            RoomsCleared = roomsCleared;
            EnemiesDefeated = enemiesDefeated;
            GuardiansDefeated = guardiansDefeated;
            Gold = gold;
            Echoes = echoes;
            VowsFulfilled = vowsFulfilled;
            VowsBroken = vowsBroken;
        }

        public bool Victory { get; }
        public ulong Seed { get; }
        public string ClassId { get; }
        public float ElapsedSeconds { get; }
        public int RoomsCleared { get; }
        public int EnemiesDefeated { get; }
        public int GuardiansDefeated { get; }
        public int Gold { get; }
        public int Echoes { get; }
        public int VowsFulfilled { get; }
        public int VowsBroken { get; }
    }
}
