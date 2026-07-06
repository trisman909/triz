using System;

namespace Lanternfall.Gameplay.Player
{
    /// <summary>
    /// Deterministic dodge lifecycle. Presentation and movement consume this
    /// state but do not decide invulnerability independently.
    /// </summary>
    public sealed class DodgeTimingModel
    {
        private readonly float _duration;
        private readonly float _invulnerabilityDuration;
        private readonly float _cooldown;
        private float _elapsed;
        private float _cooldownRemaining;

        public DodgeTimingModel(
            float duration,
            float invulnerabilityDuration,
            float cooldown)
        {
            if (duration <= 0f) throw new ArgumentOutOfRangeException(nameof(duration));
            if (invulnerabilityDuration < 0f || invulnerabilityDuration > duration)
                throw new ArgumentOutOfRangeException(nameof(invulnerabilityDuration));
            if (cooldown < duration) throw new ArgumentOutOfRangeException(nameof(cooldown));
            _duration = duration;
            _invulnerabilityDuration = invulnerabilityDuration;
            _cooldown = cooldown;
        }

        public bool IsDodging { get; private set; }
        public bool IsInvulnerable => IsDodging && _elapsed <= _invulnerabilityDuration;
        public bool CanStart => !IsDodging && _cooldownRemaining <= 0f;
        public float NormalizedTime => IsDodging ? Math.Min(1f, _elapsed / _duration) : 1f;
        public float CooldownRemaining => _cooldownRemaining;
        public float CooldownDuration => _cooldown;
        public float Duration => _duration;

        public bool TryStart()
        {
            if (!CanStart) return false;
            IsDodging = true;
            _elapsed = 0f;
            _cooldownRemaining = _cooldown;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            _cooldownRemaining = Math.Max(0f, _cooldownRemaining - deltaTime);
            if (!IsDodging) return;
            _elapsed += deltaTime;
            if (_elapsed >= _duration) IsDodging = false;
        }
    }
}
