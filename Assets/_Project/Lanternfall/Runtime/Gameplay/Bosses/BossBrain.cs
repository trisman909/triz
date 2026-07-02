using System;
using Lanternfall.Gameplay.Combat;
using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class BossBrain : MonoBehaviour
    {
        private enum State { Dormant, Intro, Chase, Telegraph, Recover, Dying }

        [SerializeField] private BossDefinition definition;
        [SerializeField] private Transform target;

        private CharacterController _controller;
        private Health _health;
        private Renderer _renderer;
        private BossVisualIdentity _identity;
        private MaterialPropertyBlock _properties;
        private BossPhaseModel _phases;
        private State _state;
        private float _timer;
        private float _deathProgress;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public event Action<BossBrain, Vector3> SummonRequested;
        public int Phase => _phases?.Phase ?? 1;
        public bool Enraged => _phases != null && _phases.Enraged;
        public BossDefinition Definition => definition;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _renderer = GetComponentInChildren<Renderer>();
            _identity = GetComponent<BossVisualIdentity>() ??
                gameObject.AddComponent<BossVisualIdentity>();
            _properties = new MaterialPropertyBlock();
            _phases = new BossPhaseModel();
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
            if (definition != null) ApplyDefinition();
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
        }

        private void Update()
        {
            if (definition == null || target == null) return;
            if (_state == State.Intro)
            {
                _timer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(_timer / Mathf.Max(.01f, definition.IntroDuration));
                transform.localScale = Vector3.one *
                    (_identity.ScaleMultiplier * Mathf.Lerp(.2f, 1.6f, t));
                if (_timer <= 0f) ChangeState(State.Chase, 0f);
                return;
            }
            if (_state == State.Dying)
            {
                _deathProgress += Time.deltaTime;
                transform.localScale = Vector3.one *
                    (_identity.ScaleMultiplier *
                     Mathf.Lerp(1.6f, 0f, _deathProgress));
                transform.Rotate(Vector3.up, 360f * Time.deltaTime);
                if (_deathProgress >= 1f) Destroy(gameObject);
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            switch (_state)
            {
                case State.Chase:
                    if (distance <= PreferredRange())
                        ChangeState(State.Telegraph, Adjusted(definition.Telegraph));
                    else
                        Chase();
                    break;
                case State.Telegraph:
                    FaceTarget();
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f)
                    {
                        ExecutePattern(distance);
                        ChangeState(State.Recover, Adjusted(definition.Recovery));
                    }
                    break;
                case State.Recover:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f) ChangeState(State.Chase, 0f);
                    break;
            }
        }

        public void Configure(BossDefinition bossDefinition, Transform combatTarget)
        {
            definition = bossDefinition;
            target = combatTarget;
            if (Application.isPlaying && _health != null) ApplyDefinition();
        }

        private void ApplyDefinition()
        {
            _identity.Configure(definition);
            _health.Configure(definition.Health, definition.Armor, false);
            transform.localScale =
                Vector3.one * (_identity.ScaleMultiplier * .2f);
            BossEncounterSignals.RaiseIntro(definition.DisplayName);
            ChangeState(State.Intro, definition.IntroDuration);
        }

        private void Chase()
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                target.position - transform.position, Vector3.up).normalized;
            float speed = definition.MoveSpeed * (Enraged ? 1.35f : 1f);
            _controller.Move(direction * speed * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
            if (direction.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    1f - Mathf.Exp(-8f * Time.deltaTime));
        }

        private void ExecutePattern(float distance)
        {
            float damage = definition.AttackDamage * (1f + (Phase - 1) * .3f) *
                (Enraged ? 1.25f : 1f);
            switch (definition.Pattern)
            {
                case BossAttackPattern.BellShockwave:
                    if (distance <= 8f) DamageTarget(damage, DamageElement.Storm);
                    break;
                case BossAttackPattern.PrismCharge:
                    Vector3 direction = Vector3.ProjectOnPlane(
                        target.position - transform.position, Vector3.up).normalized;
                    _controller.Move(direction * Mathf.Min(6f + Phase * 2f, distance));
                    if (distance <= 10f) DamageTarget(damage, DamageElement.Radiance);
                    break;
                case BossAttackPattern.RootSummon:
                    SummonRequested?.Invoke(this, transform.position + transform.right * 2f);
                    if (Phase >= 2)
                        SummonRequested?.Invoke(this, transform.position - transform.right * 2f);
                    if (distance <= 4f) DamageTarget(damage, DamageElement.Gloam);
                    break;
                case BossAttackPattern.LanternRain:
                    if (distance <= 12f)
                        DamageTarget(damage * (Phase >= 3 ? 1.35f : 1f), DamageElement.Ember);
                    break;
                case BossAttackPattern.TidalSweep:
                    if (distance <= 7f) DamageTarget(damage, DamageElement.Frost);
                    _controller.Move(-transform.forward * 2f);
                    break;
                case BossAttackPattern.MirrorVolley:
                    if (distance <= 14f)
                        DamageTarget(damage * .75f * Phase, DamageElement.Radiance);
                    break;
                case BossAttackPattern.TimeFracture:
                    if (distance <= 10f)
                        DamageTarget(damage, DamageElement.Gloam);
                    ChangeState(State.Telegraph, Adjusted(definition.Telegraph) * .5f);
                    break;
                case BossAttackPattern.StonePillars:
                    if (distance <= 6f + Phase)
                        DamageTarget(damage * 1.2f, DamageElement.Physical);
                    break;
            }
        }

        private void DamageTarget(float damage, DamageElement element)
        {
            if (target.GetComponentInParent<Health>() is Health health)
                health.ApplyDamage(new DamageRequest(
                    damage, 0f, 0f, 1f, 0f, element, 1f));
        }

        private void OnHealthChanged(float current, float maximum)
        {
            BossEncounterSignals.RaiseHealth(current, maximum);
            if (_phases.Update(maximum <= 0f ? 0f : current / maximum))
            {
                BossEncounterSignals.RaisePhase(_phases.Phase);
                ChangeState(State.Telegraph, Adjusted(definition.Telegraph) * .6f);
            }
        }

        private void OnDied()
        {
            _controller.enabled = false;
            _state = State.Dying;
            _deathProgress = 0f;
            BossEncounterSignals.RaiseDefeated(definition.StableId);
        }

        private float PreferredRange()
        {
            switch (definition.Pattern)
            {
                case BossAttackPattern.BellShockwave: return 6.5f;
                case BossAttackPattern.PrismCharge: return 9f;
                default: return 5f;
            }
        }

        private float Adjusted(float duration)
        {
            return duration / (1f + (Phase - 1) * .18f) * (Enraged ? .7f : 1f);
        }

        private void ChangeState(State state, float duration)
        {
            _state = state;
            _timer = duration;
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_properties);
            Color color = state == State.Telegraph
                ? new Color(1f, .05f, .02f)
                : Color.Lerp(
                    _identity.BaseColor,
                    Color.white,
                    .08f * Phase);
            _properties.SetColor(BaseColor, color);
            _renderer.SetPropertyBlock(_properties);
        }

        private void FaceTarget()
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                target.position - transform.position, Vector3.up);
            if (direction.sqrMagnitude > .01f)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
