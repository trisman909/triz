using System;
using System.Collections.Generic;
using Lanternfall.Core.Random;

namespace Lanternfall.Core.Run
{

public enum RoomKind
{
    Start,
    Combat,
    Elite,
    Treasure,
    Shop,
    Shrine,
    Puzzle,
    Secret,
    Challenge,
    MiniBoss,
    Boss,
    Event,
    Healing
}

public sealed class RoomNode
{
    internal RoomNode(int id, int depth, RoomKind kind)
    { Id = id; Depth = depth; Kind = kind; Connections = new List<int>(); }
    public int Id { get; }
    public int Depth { get; }
    public RoomKind Kind { get; }
    public List<int> Connections { get; }
}

/// <summary>Creates a reachable high-level route; spatial templates are separate.</summary>
public sealed class RoomGraphGenerator
{
    public IReadOnlyList<RoomNode> Generate(ulong seed, int length)
    {
        if (length < 6) throw new ArgumentOutOfRangeException(nameof(length));
        var random = new DeterministicRandom(SeedDerivation.Derive(seed, "room-graph"));
        var rooms = new List<RoomNode>();
        for (int index = 0; index < length; index++)
        {
            RoomKind kind = index == 0 ? RoomKind.Start :
                index == length - 1 ? RoomKind.Boss :
                index == length - 2 ? RoomKind.Healing :
                index == length / 2 ? RoomKind.MiniBoss :
                SelectPacedKind(index, random);
            rooms.Add(new RoomNode(index, index, kind));
            if (index > 0) rooms[index - 1].Connections.Add(index);
        }

        int treasureOrigin = 1 + random.NextInt(length - 4);
        var treasure = new RoomNode(rooms.Count, treasureOrigin + 1, RoomKind.Treasure);
        rooms.Add(treasure);
        rooms[treasureOrigin].Connections.Add(treasure.Id);
        treasure.Connections.Add(treasureOrigin + 1);

        int shopOrigin = Math.Min(length - 4, treasureOrigin + 2);
        var shop = new RoomNode(rooms.Count, shopOrigin + 1, RoomKind.Shop);
        rooms.Add(shop);
        rooms[shopOrigin].Connections.Add(shop.Id);
        shop.Connections.Add(shopOrigin + 1);

        int secretOrigin = 1 + random.NextInt(length - 3);
        var secret = new RoomNode(rooms.Count, secretOrigin + 1, RoomKind.Secret);
        rooms.Add(secret);
        rooms[secretOrigin].Connections.Add(secret.Id);
        return rooms;
    }

    private static RoomKind SelectPacedKind(int depth, DeterministicRandom random)
    {
        float roll = random.NextFloat();
        if (depth > 2 && roll < 0.14f) return RoomKind.Elite;
        if (roll < 0.24f) return RoomKind.Event;
        if (roll < 0.32f) return RoomKind.Shrine;
        if (roll < 0.39f) return RoomKind.Puzzle;
        if (roll < 0.45f) return RoomKind.Challenge;
        return RoomKind.Combat;
    }
}

}
