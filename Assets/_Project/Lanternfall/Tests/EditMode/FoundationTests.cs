using System.Linq;
using Lanternfall.Core.Events;
using Lanternfall.Core.Random;
using Lanternfall.Core.Run;
using Lanternfall.Core.Stats;
using NUnit.Framework;

namespace Lanternfall.Tests
{

public sealed class FoundationTests
{
    [Test]
    public void RandomStreamIsReproducible()
    {
        var a = new DeterministicRandom(42);
        var b = new DeterministicRandom(42);
        for (int i = 0; i < 100; i++)
            Assert.That(a.NextUInt64(), Is.EqualTo(b.NextUInt64()));
    }

    [Test]
    public void EventSubscriptionCanBeDisposed()
    {
        var bus = new GameEventBus();
        int total = 0;
        System.IDisposable subscription = bus.Subscribe<Value>(x => total += x.Amount);
        bus.Publish(new Value(2));
        subscription.Dispose();
        bus.Publish(new Value(9));
        Assert.That(total, Is.EqualTo(2));
    }

    [Test]
    public void StatsComposeInDocumentedOrder()
    {
        var stat = new StatValue(100);
        stat.Add(new StatModifier("flat", 20, ModifierKind.Flat));
        stat.Add(new StatModifier("add", .3f, ModifierKind.AdditivePercent));
        stat.Add(new StatModifier("multiply", .5f, ModifierKind.MultiplicativePercent));
        Assert.That(stat.Evaluate(), Is.EqualTo(234).Within(.001));
    }

    [Test]
        public void GeneratedRouteHasRequiredRoomsAndReachableBoss()
    {
        var rooms = new RoomGraphGenerator().Generate(99, 10);
        Assert.That(rooms.First().Kind, Is.EqualTo(RoomKind.Start));
        Assert.That(rooms.Any(x => x.Kind == RoomKind.Treasure), Is.True);
        Assert.That(rooms.Any(x => x.Kind == RoomKind.Secret), Is.True);
        Assert.That(rooms.Take(10).Last().Kind, Is.EqualTo(RoomKind.Boss));
            for (int i = 0; i < 9; i++)
                Assert.That(rooms[i].Connections, Does.Contain(i + 1));
        }

        [Test]
        public void HundredsOfSeedsPreservePacingAndCriticalBranches()
        {
            var generator = new RoomGraphGenerator();
            for (ulong seed = 0; seed < 250; seed++)
            {
                var rooms = generator.Generate(seed, 12);
                Assert.That(rooms[0].Kind, Is.EqualTo(RoomKind.Start));
                Assert.That(rooms[11].Kind, Is.EqualTo(RoomKind.Boss));
                Assert.That(rooms[10].Kind, Is.EqualTo(RoomKind.Healing));
                Assert.That(rooms.Any(x => x.Kind == RoomKind.MiniBoss), Is.True);
                Assert.That(rooms.Any(x => x.Kind == RoomKind.Treasure), Is.True);
                Assert.That(rooms.Any(x => x.Kind == RoomKind.Shop), Is.True);
                Assert.That(rooms.Any(x => x.Kind == RoomKind.Secret), Is.True);
            }
        }

    private readonly struct Value
    {
        public Value(int amount) => Amount = amount;
        public int Amount { get; }
    }
}

}
