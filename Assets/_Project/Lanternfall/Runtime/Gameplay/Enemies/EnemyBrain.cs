using System;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Radiance;
using UnityEngine;

namespace Lanternfall.Gameplay.Enemies
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        private enum BrainState { Dormant, Chase, Telegraph, Recover, Dead }

        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private EliteModifier eliteModifier;
        [SerializeField] private Transform target;

        private CharacterController _controller;
        private Health _health;
        private Renderer _renderer;
        private EnemyVisualIdentity _identity;
        private MaterialPropertyBlock _properties;
        private BrainState _state;
        private float _stateTimer;
        private float _burrowTimer;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public event Action<EnemyBrain, Vector3> SummonRequested;
        public event Action<EnemyBrain> Died;
        public EnemyDefinition Definition => definition;
        public bool IsAlive => _state != BrainState.Dead;
        public bool InRadiance { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _renderer = GetComponentInChildren<Renderer>();
            _identity = GetComponent<EnemyVisualIdentity>() ??
                gameObject.AddComponent<EnemyVisualIdentity>();
            _properties = new MaterialPropertyBlock();
            if (definition != null) ApplyDefinition();
            _health.Died += OnDied;
            _state = target != null ? BrainState.Chase : BrainState.Dormant;
        }

        private void OnDestroy()
        {
            if (_health != null) _health.Died -= OnDied;
        }

        private void Update()
        {
            if (_state == BrainState.Dead || target == null || definition == null) return;
            UpdateRadianceResponse();
            float distance = Vector3.Distance(
                Vector3.ProjectOnPlane(transform.position, Vector3.up),
                Vector3.ProjectOnPlane(target.position, Vector3.up));

            switch (_state)
            {
                case BrainState.Dormant:
                    ChangeState(BrainState.Chase, 0f);
                    break;
                case BrainState.Chase:
                    if (distance <= definition.AttackRange)
                        ChangeState(BrainState.Telegraph, AdjustedWindup());
                    else
                        MoveForArchetype(distance);
                    break;
                case BrainState.Telegraph:
                    FaceTarget();
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        PerformAttack();
                        ChangeState(BrainState.Recover, definition.Recovery);
                    }
                    break;
                case BrainState.Recover:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f) ChangeState(BrainState.Chase, 0f);
                    break;
            }
        }

        public void Configure(
            EnemyDefinition enemyDefinition,
            Transform pursuitTarget,
            EliteModifier modifier)
        {
            definition = enemyDefinition;
            target = pursuitTarget;
            eliteModifier = modifier;
            if (Application.isPlaying && _health != null)
            {
                ApplyDefinition();
                ChangeState(BrainState.Chase, 0f);
            }
        }

        private void ApplyDefinition()
        {
            _identity.Configure(definition);
            float healthMultiplier = eliteModifier == EliteModifier.Bulwark ? 1.8f : 1f;
            float armorBonus = eliteModifier == EliteModifier.Bulwark ? 30f : 0f;
            _health.Configure(
                definition.Health * healthMultiplier,
                definition.Armor + armorBonus,
                false);
        }

        private void MoveForArchetype(float distance)
        {
            Vector3 toTarget = Vector3.ProjectOnPlane(
                target.position - transform.position, Vector3.up).normalized;
            Vector3 direction = toTarget;
            if (definition.Archetype == EnemyArchetype.Archer)
            {
                if (distance < definition.AttackRange * 0.65f) direction = -toTarget;
                else if (distance < definition.AttackRange * 0.95f)
                    direction = Vector3.Cross(Vector3.up, toTarget);
            }
            else if (definition.Archetype == EnemyArchetype.Assassin)
            {
                direction = (toTarget + Vector3.Cross(Vector3.up, toTarget) * 0.7f).normalized;
            }
            else if (definition.Archetype == EnemyArchetype.Burrowing)
            {
                _burrowTimer += Time.deltaTime;
                if (_burrowTimer > 2.5f)
                {
                    _burrowTimer = 0f;
                    direction *= 2.2f;
                }
            }

            float speed = definition.MoveSpeed;
            speed *= RadianceSpeedMultiplier();
            if (eliteModifier == EliteModifier.Frenzied) speed *= 1.35f;
            _controller.Move(direction * speed * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    1f - Mathf.Exp(-12f * Time.deltaTime));
        }

        private void PerformAttack()
        {
            if (definition.Archetype == EnemyArchetype.Summoner)
                SummonRequested?.Invoke(this, transform.position - transform.forward * 1.5f);

            if (Vector3.Distance(transform.position, target.position) <= definition.AttackRange * 1.25f &&
                target.GetComponentInParent<Health>() is Health targetHealth)
            {
                float multiplier = eliteModifier == EliteModifier.Frenzied ? 1.25f : 1f;
                if (!InRadiance) multiplier *= 1.25f;
                targetHealth.ApplyDamage(new DamageRequest(
                    definition.AttackDamage * multiplier, 0f, 0f, 1f, 0f,
                    DamageElement.Physical, 1f));
            }

            if (definition.Archetype == EnemyArchetype.Explosive)
                _health.ApplyDamage(new DamageRequest(
                    _health.Current + 1f, 0f, 0f, 1f, 0f,
                    DamageElement.Ember, 1f));
        }

        private float AdjustedWindup()
        {
            float result = eliteModifier == EliteModifier.Frenzied
                ? definition.Windup * 0.72f
                : definition.Windup;
            if (InRadiance && definition.Archetype == EnemyArchetype.Summoner)
                result *= 1.35f;
            if (InRadiance && definition.Archetype == EnemyArchetype.Explosive)
                result *= .72f;
            return result;
        }

        private void UpdateRadianceResponse()
        {
            InRadiance = RadianceField.ContainsActive(transform.position);
            if (definition.Archetype == EnemyArchetype.Tank)
                _health.SetRuntimeArmor(InRadiance ? -definition.Armor : 0f);
        }

        private float RadianceSpeedMultiplier()
        {
            if (!InRadiance) return 1f;
            switch (definition.Archetype)
            {
                case EnemyArchetype.Assassin:
                case EnemyArchetype.Burrowing:
                    return .65f;
                case EnemyArchetype.Flying:
                case EnemyArchetype.Explosive:
                    return 1.25f;
                default:
                    return .9f;
            }
        }

        private void ChangeState(BrainState next, float duration)
        {
            _state = next;
            _stateTimer = duration;
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_properties);
            Color color = next == BrainState.Telegraph
                ? new Color(1f, 0.08f, 0.03f)
                : Color.Lerp(
                    _identity.BaseColor,
                    ArchetypeColor(definition.Archetype),
                    .35f);
            _properties.SetColor(BaseColor, color);
            _renderer.SetPropertyBlock(_properties);
        }

        private void FaceTarget()
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                target.position - transform.position, Vector3.up);
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        private void OnDied()
        {
            Died?.Invoke(this);
            _state = BrainState.Dead;
            enabled = false;
            _controller.enabled = false;
            if (_renderer != null) _renderer.enabled = false;
            Destroy(gameObject, 0.15f);
        }

        private static Color ArchetypeColor(EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Archer: return new Color(0.45f, 0.75f, 0.3f);
                case EnemyArchetype.Tank: return new Color(0.35f, 0.3f, 0.24f);
                case EnemyArchetype.Flying: return new Color(0.35f, 0.7f, 0.9f);
                case EnemyArchetype.Burrowing: return new Color(0.55f, 0.28f, 0.13f);
                case EnemyArchetype.Explosive: return new Color(1f, 0.32f, 0.05f);
                case EnemyArchetype.Summoner: return new Color(0.65f, 0.2f, 0.85f);
                case EnemyArchetype.Assassin: return new Color(0.15f, 0.12f, 0.2f);
                default: return new Color(0.55f, 0.48f, 0.4f);
            }
        }
    }
}
