using System;
using System.Collections.Generic;
using Lanternfall.Core.Random;
using Lanternfall.Core.Run;

namespace Lanternfall.Gameplay.Run
{
    public readonly struct RunRoomPlan
    {
        public RunRoomPlan(
            int globalIndex, int biomeIndex, int roomIndex,
            RoomKind kind, ulong encounterSeed)
        {
            GlobalIndex = globalIndex;
            BiomeIndex = biomeIndex;
            RoomIndex = roomIndex;
            Kind = kind;
            EncounterSeed = encounterSeed;
        }

        public int GlobalIndex { get; }
        public int BiomeIndex { get; }
        public int RoomIndex { get; }
        public RoomKind Kind { get; }
        public ulong EncounterSeed { get; }
    }

    /// <summary>
    /// Persistent authoritative run state. Presentation scenes may reload
    /// between rooms without changing route, class, health, seed or timing.
    /// </summary>
    public sealed class RunSession
    {
        public const int BiomeCount = 5;
        public const int MainRoomsPerBiome = 8;

        private readonly List<RunRoomPlan> _rooms =
            new List<RunRoomPlan>(BiomeCount * MainRoomsPerBiome);

        public RunSession(ulong seed, string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
                throw new ArgumentException("A class stable ID is required.", nameof(classId));
            Seed = seed;
            ClassId = classId;
            BuildRoute();
        }

        public ulong Seed { get; }
        public string ClassId { get; }
        public IReadOnlyList<RunRoomPlan> Rooms => _rooms;
        public int CurrentIndex { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float CurrentHealth { get; set; } = -1f;
        public int Gold { get; set; }
        public int EnemiesDefeated { get; set; }
        public int BossesDefeated { get; set; }
        public float EstimatedDurationMinutes
        {
            get
            {
                float seconds = 0f;
                for (int index = 0; index < _rooms.Count; index++)
                    seconds += EstimatedRoomSeconds(_rooms[index].Kind);
                return seconds / 60f;
            }
        }
        public bool IsComplete => CurrentIndex >= _rooms.Count;
        public RunRoomPlan Current =>
            !IsComplete ? _rooms[CurrentIndex] : _rooms[_rooms.Count - 1];

        public void Tick(float unscaledSeconds) =>
            ElapsedSeconds += Math.Max(0f, unscaledSeconds);

        public bool Advance()
        {
            if (IsComplete) return false;
            CurrentIndex++;
            return !IsComplete;
        }

        private void BuildRoute()
        {
            var generator = new RoomGraphGenerator();
            for (int biome = 0; biome < BiomeCount; biome++)
            {
                ulong biomeSeed = SeedDerivation.Derive(Seed, $"biome-{biome}");
                IReadOnlyList<RoomNode> graph =
                    generator.Generate(biomeSeed, MainRoomsPerBiome);
                for (int room = 0; room < MainRoomsPerBiome; room++)
                {
                    RoomNode node = graph[room];
                    int global = biome * MainRoomsPerBiome + room;
                    _rooms.Add(new RunRoomPlan(
                        global,
                        biome,
                        room,
                        node.Kind,
                        SeedDerivation.Derive(biomeSeed, $"encounter-{room}")));
                }
            }
        }

        private static float EstimatedRoomSeconds(RoomKind kind)
        {
            switch (kind)
            {
                case RoomKind.Start: return 10f;
                case RoomKind.Healing: return 15f;
                case RoomKind.Combat: return 50f;
                case RoomKind.Elite: return 60f;
                case RoomKind.Challenge: return 55f;
                case RoomKind.MiniBoss: return 100f;
                case RoomKind.Boss: return 150f;
                default: return 30f;
            }
        }
    }
}
