using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Pooling;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lanternfall.Gameplay.Combat
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition startingWeapon;
        [SerializeField] private AbilityDefinition startingAbility;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;

        private readonly Collider[] _abilityHits = new Collider[64];
        private PlayerInputReader _input;
        private ComponentPool<Projectile> _projectiles;
        private float _attackCooldown;
        private float _abilityCooldown;
        private float _abilityCooldownMultiplier = 1f;

        public float AttackCooldownRemaining => _attackCooldown;
        public float AbilityCooldownRemaining => _abilityCooldown;
        public float AbilityCooldownDuration =>
            startingAbility != null ? startingAbility.Cooldown : 0f;
        public string AbilityName =>
            startingAbility != null ? startingAbility.name : "Ability";
        public float AbilityCooldownMultiplier => _abilityCooldownMultiplier;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            if (projectilePrefab != null)
            {
                var root = new GameObject("Projectile Pool").transform;
                _projectiles = new ComponentPool<Projectile>(projectilePrefab, 32, root);
            }
        }

        private void Update()
        {
            _attackCooldown = Mathf.Max(0f, _attackCooldown - Time.deltaTime);
            _abilityCooldown = Mathf.Max(0f, _abilityCooldown - Time.deltaTime);
            if (_input.PrimaryFireHeld) TryFire();
            if (_input.AbilityPressedThisFrame) TryUseAbility();
        }

        public bool TryFire()
        {
            if (startingWeapon == null || _projectiles == null || _attackCooldown > 0f)
                return false;
            Projectile projectile = _projectiles.Take();
            Vector3 direction = ResolveAimDirection();
            Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 0.5f;
            projectile.Launch(
                startingWeapon, origin, direction, transform.root, _projectiles.Return);
            _attackCooldown = 1f / startingWeapon.AttacksPerSecond;
            HubController.Instance?.ReportAchievement(
                AchievementMetric.ShotsFired);
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.WeaponFire,
                origin);
            return true;
        }

        public bool TryUseAbility()
        {
            if (startingAbility == null || _abilityCooldown > 0f) return false;
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                startingAbility.Radius,
                _abilityHits,
                ~0,
                QueryTriggerInteraction.Collide);
            for (int index = 0; index < count; index++)
            {
                if (_abilityHits[index].transform.root == transform.root) continue;
                if (_abilityHits[index].GetComponentInParent<Health>() is Health target)
                    target.ApplyDamage(new DamageRequest(
                        startingAbility.Damage, 0f, 0f, 1f, 0f,
                        startingAbility.Element, 1f));
            }
            _abilityCooldown = startingAbility.Cooldown * _abilityCooldownMultiplier;
            HubController.Instance?.ReportAchievement(
                AchievementMetric.AbilitiesUsed);
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.Ability,
                transform.position);
            GameplayPresentationSignals.RaiseSubtitle(
                "BEARER",
                $"{AbilityName} released.");
            return true;
        }

        public void Configure(
            WeaponDefinition weapon,
            AbilityDefinition ability,
            Projectile prefab,
            Transform muzzlePoint)
        {
            startingWeapon = weapon;
            startingAbility = ability;
            projectilePrefab = prefab;
            muzzle = muzzlePoint;
        }

        public void ApplyLoadout(WeaponDefinition weapon, AbilityDefinition ability)
        {
            if (weapon != null) startingWeapon = weapon;
            if (ability != null) startingAbility = ability;
        }

        public void SetAbilityCooldownMultiplier(float value) =>
            _abilityCooldownMultiplier = Mathf.Clamp(value, .35f, 2f);

        private Vector3 ResolveAimDirection()
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera != null && Mouse.current != null)
            {
                Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
                var plane = new Plane(Vector3.up, transform.position);
                if (plane.Raycast(ray, out float distance))
                    return Vector3.ProjectOnPlane(ray.GetPoint(distance) - transform.position, Vector3.up).normalized;
            }
            Vector2 aim = _input.Aim;
            if (aim.sqrMagnitude > 0.1f && camera != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
                return (forward * aim.y + right * aim.x).normalized;
            }
            return transform.forward;
        }
    }
}
