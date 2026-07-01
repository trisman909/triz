using Lanternfall.Gameplay.Combat;
using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    [CreateAssetMenu(menuName = "Lanternfall/Progression/Bearer Class")]
    public sealed class CharacterClassDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "class.unset";
        [SerializeField] private string displayName = "Unnamed Bearer";
        [SerializeField] private string passiveTag = "passive.unset";
        [SerializeField] private WeaponDefinition startingWeapon;
        [SerializeField] private AbilityDefinition startingAbility;
        [SerializeField, Min(1f)] private float maximumHealth = 100f;
        [SerializeField, Min(.1f)] private float movementSpeed = 6f;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public string PassiveTag => passiveTag;
        public WeaponDefinition StartingWeapon => startingWeapon;
        public AbilityDefinition StartingAbility => startingAbility;
        public float MaximumHealth => maximumHealth;
        public float MovementSpeed => movementSpeed;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string title,
            string passive,
            WeaponDefinition weapon,
            AbilityDefinition ability,
            float health,
            float speed)
        {
            stableId = id;
            displayName = title;
            passiveTag = passive;
            startingWeapon = weapon;
            startingAbility = ability;
            maximumHealth = health;
            movementSpeed = speed;
        }
#endif
    }
}

