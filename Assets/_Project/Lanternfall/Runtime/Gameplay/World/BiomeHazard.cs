using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class BiomeHazard : MonoBehaviour
    {
        [SerializeField] private float damage = 8f;
        [SerializeField] private DamageElement element;
        [SerializeField] private float interval = 1f;
        private float _nextDamage;

        public void Configure(float amount, DamageElement damageElement)
        {
            damage = Mathf.Max(0f, amount);
            element = damageElement;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < _nextDamage ||
                other.GetComponentInParent<Health>() is not Health health)
                return;
            _nextDamage = Time.time + interval;
            health.ApplyDamage(new DamageRequest(
                damage, 0f, 0f, 1f, 0f, element, 1f));
            GameplayPresentationSignals.RaiseCue(
                PresentationCue.Hazard,
                other.transform.position);
        }
    }
}
