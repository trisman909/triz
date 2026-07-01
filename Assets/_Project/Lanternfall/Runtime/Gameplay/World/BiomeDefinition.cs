using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    [CreateAssetMenu(menuName = "Lanternfall/World/Biome")]
    public sealed class BiomeDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "biome.unset";
        [SerializeField] private string displayName = "Unnamed Biome";
        [SerializeField] private Color fogColor = new Color(.05f, .07f, .1f);
        [SerializeField] private Color ambientColor = new Color(.08f, .1f, .16f);
        [SerializeField, Min(0f)] private float fogDensity = .025f;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public Color FogColor => fogColor;
        public Color AmbientColor => ambientColor;
        public float FogDensity => fogDensity;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string title,
            Color fog,
            Color ambient,
            float density)
        {
            stableId = id;
            displayName = title;
            fogColor = fog;
            ambientColor = ambient;
            fogDensity = density;
        }
#endif
    }

}
