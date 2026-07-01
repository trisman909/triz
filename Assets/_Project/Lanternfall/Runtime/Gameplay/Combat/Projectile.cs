using System;
using Lanternfall.Gameplay.Pooling;
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
        private int _ownerLayer;

        public void Launch(
            WeaponDefinition weapon,
            Vector3 position,
            Vector3 direction,
            int ownerLayer,
            Action<Projectile> release)
        {
            _weapon = weapon;
            _direction = direction.normalized;
            _ownerLayer = ownerLayer;
            _release = release;
            _remainingLifetime = 3f;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(_direction));
        }

        private void Update()
        {
            if (_weapon == null) return;
            float distance = _weapon.ProjectileSpeed * Time.deltaTime;
            if (Physics.SphereCast(
                transform.position,
                0.15f,
                _direction,
                out RaycastHit hit,
                distance,
                ~0,
                QueryTriggerInteraction.Collide))
            {
                if (hit.collider.gameObject.layer != _ownerLayer &&
                    hit.collider.GetComponentInParent<Health>() is Health target)
                {
                    target.ApplyDamage(new DamageRequest(
                        _weapon.Damage, 0f, _weapon.CriticalChance, 2f, 0f,
                        _weapon.Element, UnityEngine.Random.value));
                    if (hit.rigidbody != null)
                        hit.rigidbody.AddForce(_direction * _weapon.Knockback, ForceMode.Impulse);
                }
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
        }
    }
}
