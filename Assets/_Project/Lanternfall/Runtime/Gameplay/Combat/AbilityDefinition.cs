using UnityEngine;

namespace Lanternfall.Gameplay.Combat
{
    public enum AbilityKind
    {
        RadiantBurst,
        GloamWell
    }

    [CreateAssetMenu(menuName = "Lanternfall/Combat/Active Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "ability.unset";
        [SerializeField] private AbilityKind kind;
        [SerializeField, Min(0f)] private float damage = 30f;
        [SerializeField, Min(0.1f)] private float radius = 5f;
        [SerializeField, Min(0.1f)] private float cooldown = 8f;
        [SerializeField] private DamageElement element = DamageElement.Ember;

        public string StableId => stableId;
        public AbilityKind Kind => kind;
        public float Damage => damage;
        public float Radius => radius;
        public float Cooldown => cooldown;
        public DamageElement Element => element;

#if UNITY_EDITOR
        public void Configure(
            string id,
            AbilityKind abilityKind,
            float baseDamage,
            float effectRadius,
            float cooldownSeconds,
            DamageElement damageElement)
        {
            stableId = id;
            kind = abilityKind;
            damage = baseDamage;
            radius = effectRadius;
            cooldown = cooldownSeconds;
            element = damageElement;
        }
#endif
    }
}

