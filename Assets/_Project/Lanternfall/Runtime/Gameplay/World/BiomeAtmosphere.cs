using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    public sealed class BiomeAtmosphere : MonoBehaviour
    {
        [SerializeField] private BiomeDefinition biome;

        public void Configure(BiomeDefinition definition) => biome = definition;

        private void Start()
        {
            if (biome == null) return;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = biome.FogColor;
            RenderSettings.fogDensity = biome.FogDensity;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = biome.AmbientColor;
        }
    }
}
