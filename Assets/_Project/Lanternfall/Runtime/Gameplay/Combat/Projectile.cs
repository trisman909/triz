using System;
using Lanternfall.Gameplay.Pooling;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        private Action<Projectile> _release;
        private WeaponDefinition _weapon;
        private Vector3 _direction;
        private float _remainingLifetime;
        private Transform _ownerRoot;
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        public void Launch(
            WeaponDefinition weapon,
            Vector3 position,
            Vector3 direction,
            Transform ownerRoot,
            Action<Projectile> release)
        {
            _weapon = weapon;
            _direction = direction.normalized;
            _ownerRoot = ownerRoot;
            _release = release;
            _remainingLifetime = 3f;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(_direction));
        }

        private void Update()
        {
            if (_weapon == null) return;
            float distance = _weapon.ProjectileSpeed * Time.deltaTime;
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                0.15f,
                _direction,
                _hits,
                distance,
                ~0,
                QueryTriggerInteraction.Collide);
            int selected = -1;
            float closest = float.MaxValue;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit candidate = _hits[index];
                if (candidate.collider.transform.root == _ownerRoot) continue;
                if (candidate.collider.isTrigger &&
                    candidate.collider.GetComponentInParent<Health>() == null)
                    continue;
                if (candidate.distance < closest)
                {
                    selected = index;
                    closest = candidate.distance;
                }
            }
            if (selected >= 0)
            {
                RaycastHit hit = _hits[selected];
                if (hit.collider.GetComponentInParent<Health>() is Health target)
                {
                    target.ApplyDamage(new DamageRequest(
                        _weapon.Damage, 0f, _weapon.CriticalChance, 2f, 0f,
                        _weapon.Element, UnityEngine.Random.value));
                    GameplayPresentationSignals.RaiseCue(
                        PresentationCue.Impact,
                        hit.point);
                }
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(_direction * _weapon.Knockback, ForceMode.Impulse);
                Release();
                return;
            }

            transform.position += _direction * distance;
            _remainingLifetime -= Time.deltaTime;
            if (_remainingLifetime <= 0f) Release();
        }

        private void Release()
        {
            Action<Projectile> release = _release;
            _release = null;
            release?.Invoke(this);
        }

        public void OnTakenFromPool() { }
        public void OnReturnedToPool()
        {
            _weapon = null;
            _release = null;
            _ownerRoot = null;
        }
    }
}
