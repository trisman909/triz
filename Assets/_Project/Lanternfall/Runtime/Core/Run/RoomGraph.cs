using System;
using System.Collections.Generic;
using Lanternfall.Core.Random;

namespace Lanternfall.Core.Run
{

public enum RoomKind { Start, Combat, Elite, Treasure, Secret, Healing, Boss }

public sealed class RoomNode
{
    internal RoomNode(int id, RoomKind kind)
    { Id = id; Kind = kind; Connections = new List<int>(); }
    public int Id { get; }
    public RoomKind Kind { get; }
    public List<int> Connections { get; }
}

/// <summary>Creates a reachable high-level route; spatial templates are separate.</summary>
public sealed class RoomGraphGenerator
{
    public IReadOnlyList<RoomNode> Generate(ulong seed, int length)
    {
        if (length < 6) throw new ArgumentOutOfRangeException(nameof(length));
        var random = new DeterministicRandom(SeedDerivation.Derive(seed, "rooms"));
        var rooms = new List<RoomNode>();
        for (int index = 0; index < length; index++)
        {
            RoomKind kind = index == 0 ? RoomKind.Start :
                index == length - 1 ? RoomKind.Boss :
                index == length - 2 ? RoomKind.Healing :
                random.NextFloat() < .2f ? RoomKind.Elite : RoomKind.Combat;
            rooms.Add(new RoomNode(index, kind));
            if (index > 0) rooms[index - 1].Connections.Add(index);
        }
        int origin = 1 + random.NextInt(length - 3);
        var treasure = new RoomNode(rooms.Count, RoomKind.Treasure);
        rooms.Add(treasure);
        rooms[origin].Connections.Add(treasure.Id);
        treasure.Connections.Add(origin + 1);
        var secret = new RoomNode(rooms.Count, RoomKind.Secret);
        rooms.Add(secret);
        rooms[1 + random.NextInt(length - 3)].Connections.Add(secret.Id);
        return rooms;
    }
}

}
