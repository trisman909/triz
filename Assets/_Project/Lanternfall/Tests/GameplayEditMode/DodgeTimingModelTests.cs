using Lanternfall.Gameplay.Player;
using NUnit.Framework;

namespace Lanternfall.Tests
{
    public sealed class DodgeTimingModelTests
    {
        [Test]
        public void DodgeHasBoundedInvulnerabilityAndCooldown()
        {
            var dodge = new DodgeTimingModel(0.3f, 0.2f, 0.7f);
            Assert.That(dodge.TryStart(), Is.True);
            Assert.That(dodge.IsInvulnerable, Is.True);
            dodge.Tick(0.21f);
            Assert.That(dodge.IsInvulnerable, Is.False);
            Assert.That(dodge.IsDodging, Is.True);
            dodge.Tick(0.1f);
            Assert.That(dodge.IsDodging, Is.False);
            Assert.That(dodge.TryStart(), Is.False);
            dodge.Tick(0.4f);
            Assert.That(dodge.TryStart(), Is.True);
        }
    }
}

