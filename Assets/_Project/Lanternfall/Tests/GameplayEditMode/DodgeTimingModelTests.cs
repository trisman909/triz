using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Combat;
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

        [Test]
        public void DamagePipelineAppliesCriticalHitThenArmorCurve()
        {
            DamageResult result = DamageResolver.Resolve(new DamageRequest(
                100f, 0.2f, 0.5f, 2f, 100f, DamageElement.Storm, 0.1f));

            Assert.That(result.Critical, Is.True);
            Assert.That(result.Amount, Is.EqualTo(120f).Within(0.001f));
            Assert.That(result.Element, Is.EqualTo(DamageElement.Storm));
        }
    }
}
