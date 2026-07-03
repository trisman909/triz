using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    /// <summary>
    /// Identifies the biome role and verified geometry budget of a generated
    /// production-art module. Kept as a dedicated script so Unity can persist
    /// the component reference in prefab assets.
    /// </summary>
    public sealed class ProductionAssetMarker : MonoBehaviour
    {
        [SerializeField] private string biomeId;
        [SerializeField] private ProductionAssetKind kind;
        [SerializeField] private int triangleCount;

        public string BiomeId => biomeId;
        public ProductionAssetKind Kind => kind;
        public int TriangleCount => triangleCount;

        public void Configure(
            string stableBiomeId,
            ProductionAssetKind assetKind,
            int triangles)
        {
            biomeId = stableBiomeId;
            kind = assetKind;
            triangleCount = Mathf.Max(0, triangles);
        }
    }
}
