using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    public enum MemoryAspect { Ember, Tide, Storm, Stone, Gloam, Radiance }
    public enum RelicRarity { Common, Uncommon, Rare, Legendary }

    [CreateAssetMenu(menuName = "Lanternfall/Progression/Echo Relic")]
    public sealed class RelicDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "relic.unset";
        [SerializeField] private string displayName = "Unnamed Echo";
        [SerializeField] private MemoryAspect aspect;
        [SerializeField] private RelicRarity rarity;
        [SerializeField, Min(0.01f)] private float rewardWeight = 1f;
        [SerializeField] private string effectTag = "effect.unset";
        [SerializeField] private float potency = 0.1f;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public MemoryAspect Aspect => aspect;
        public RelicRarity Rarity => rarity;
        public float RewardWeight => rewardWeight;
        public string EffectTag => effectTag;
        public float Potency => potency;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string title,
            MemoryAspect memory,
            RelicRarity tier,
            float weight,
            string tag,
            float value)
        {
            stableId = id;
            displayName = title;
            aspect = memory;
            rarity = tier;
            rewardWeight = weight;
            effectTag = tag;
            potency = value;
        }
#endif
    }
}

