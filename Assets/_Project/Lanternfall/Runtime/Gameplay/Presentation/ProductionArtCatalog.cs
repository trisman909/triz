using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    public enum ProductionAssetKind
    {
        Floor,
        Wall,
        Corner,
        Gate,
        Column,
        Landmark,
        Decoration,
        Hazard,
        Lantern,
        Arena,
        Breakable
    }

    [Serializable]
    public sealed class ProductionBiomeArtProfile
    {
        [SerializeField] private string stableId;
        [SerializeField] private string displayName;
        [SerializeField] private string environmentalStory;
        [SerializeField] private Material surfaceMaterial;
        [SerializeField] private Material structuralMaterial;
        [SerializeField] private Material accentMaterial;
        [SerializeField] private Material emissiveMaterial;
        [SerializeField] private Material hazardMaterial;
        [SerializeField] private GameObject[] modularPrefabs;
        [SerializeField] private Color fogColor;
        [SerializeField] private Color ambientColor;
        [SerializeField] private Color keyLightColor;
        [SerializeField] private Color particleColor;
        [SerializeField] private float fogDensity;
        [SerializeField] private float keyLightIntensity;

        public string StableId => stableId;
        public string DisplayName => displayName;
        public string EnvironmentalStory => environmentalStory;
        public Material SurfaceMaterial => surfaceMaterial;
        public Material StructuralMaterial => structuralMaterial;
        public Material AccentMaterial => accentMaterial;
        public Material EmissiveMaterial => emissiveMaterial;
        public Material HazardMaterial => hazardMaterial;
        public IReadOnlyList<GameObject> ModularPrefabs => modularPrefabs;
        public Color FogColor => fogColor;
        public Color AmbientColor => ambientColor;
        public Color KeyLightColor => keyLightColor;
        public Color ParticleColor => particleColor;
        public float FogDensity => fogDensity;
        public float KeyLightIntensity => keyLightIntensity;

#if UNITY_EDITOR
        public ProductionBiomeArtProfile(
            string id,
            string name,
            string story,
            Material surface,
            Material structure,
            Material accent,
            Material emissive,
            Material hazard,
            GameObject[] prefabs,
            Color fog,
            Color ambient,
            Color key,
            Color particles,
            float density,
            float keyIntensity)
        {
            stableId = id;
            displayName = name;
            environmentalStory = story;
            surfaceMaterial = surface;
            structuralMaterial = structure;
            accentMaterial = accent;
            emissiveMaterial = emissive;
            hazardMaterial = hazard;
            modularPrefabs = prefabs;
            fogColor = fog;
            ambientColor = ambient;
            keyLightColor = key;
            particleColor = particles;
            fogDensity = density;
            keyLightIntensity = keyIntensity;
        }
#endif
    }

    [CreateAssetMenu(menuName = "Lanternfall/Presentation/Production Art Catalog")]
    public sealed class ProductionArtCatalog : ScriptableObject
    {
        [SerializeField] private ProductionBiomeArtProfile[] biomes =
            Array.Empty<ProductionBiomeArtProfile>();

        public IReadOnlyList<ProductionBiomeArtProfile> Biomes => biomes;

        public List<string> Validate()
        {
            var errors = new List<string>();
            if (biomes == null || biomes.Length != 5)
            {
                errors.Add("Production art catalog requires exactly five biomes.");
                return errors;
            }
            var ids = new HashSet<string>();
            var surfaceFamilies = new HashSet<Texture>();
            var responseFamilies = new HashSet<string>();
            var roleMeshes =
                new Dictionary<ProductionAssetKind, HashSet<Mesh>>();
            foreach (ProductionBiomeArtProfile biome in biomes)
            {
                if (biome == null || string.IsNullOrWhiteSpace(biome.StableId) ||
                    !ids.Add(biome.StableId))
                {
                    errors.Add("Production biome has a missing or duplicate ID.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(biome.EnvironmentalStory))
                    errors.Add(
                        $"{biome.StableId} has no environmental story.");
                Material[] materials =
                {
                    biome.SurfaceMaterial,
                    biome.StructuralMaterial,
                    biome.AccentMaterial,
                    biome.EmissiveMaterial,
                    biome.HazardMaterial
                };
                foreach (Material material in materials)
                {
                    if (material == null)
                        errors.Add($"{biome.StableId} has a missing material.");
                    else if (material.mainTexture == null)
                        errors.Add(
                            $"{biome.StableId}/{material.name} has no production texture.");
                    else if (material.shader == null ||
                             !material.shader.isSupported ||
                             !material.shader.name.StartsWith(
                                 "Universal Render Pipeline/",
                                 StringComparison.Ordinal))
                        errors.Add(
                            $"{biome.StableId}/{material.name} is not " +
                            "URP-compatible.");
                }
                if (biome.SurfaceMaterial != null)
                {
                    surfaceFamilies.Add(
                        biome.SurfaceMaterial.mainTexture);
                    responseFamilies.Add(
                        $"{biome.SurfaceMaterial.GetFloat("_Metallic"):F3}:" +
                        $"{biome.SurfaceMaterial.GetFloat("_Smoothness"):F3}");
                }
                if (biome.ModularPrefabs == null ||
                    biome.ModularPrefabs.Count < 11)
                {
                    errors.Add($"{biome.StableId} needs eleven modular prefab roles.");
                    continue;
                }
                var roles = new HashSet<ProductionAssetKind>();
                foreach (GameObject prefab in biome.ModularPrefabs)
                {
                    if (prefab == null)
                    {
                        errors.Add($"{biome.StableId} has a null modular prefab.");
                        continue;
                    }
                    ProductionAssetMarker marker =
                        prefab.GetComponent<ProductionAssetMarker>();
                    MeshFilter mesh = prefab.GetComponentInChildren<MeshFilter>();
                    Renderer renderer = prefab.GetComponentInChildren<Renderer>();
                    if (marker == null)
                        errors.Add($"{prefab.name} has no production marker.");
                    else
                    {
                        roles.Add(marker.Kind);
                        if (marker.TriangleCount <= 0 ||
                            marker.TriangleCount > 8000)
                            errors.Add(
                                $"{prefab.name} triangle budget is invalid: " +
                                $"{marker.TriangleCount}.");
                    }
                    if (mesh?.sharedMesh == null)
                        errors.Add($"{prefab.name} has no authored mesh.");
                    else if (marker != null)
                    {
                        if (!roleMeshes.TryGetValue(
                                marker.Kind, out HashSet<Mesh> meshes))
                        {
                            meshes = new HashSet<Mesh>();
                            roleMeshes.Add(marker.Kind, meshes);
                        }
                        meshes.Add(mesh.sharedMesh);
                    }
                    if (renderer?.sharedMaterial == null)
                        errors.Add($"{prefab.name} has no production material.");
                    else if (renderer.sharedMaterial.shader == null ||
                             !renderer.sharedMaterial.shader.name.StartsWith(
                                 "Universal Render Pipeline/",
                                 StringComparison.Ordinal))
                        errors.Add(
                            $"{prefab.name} uses a non-URP material.");
                }
                if (roles.Count != 11)
                    errors.Add(
                        $"{biome.StableId} covers {roles.Count}/11 asset roles.");
            }
            if (surfaceFamilies.Count != 5)
                errors.Add(
                    "Five independent biome surface texture families are required.");
            if (responseFamilies.Count != 5)
                errors.Add(
                    "Five independent biome material responses are required.");
            foreach (ProductionAssetKind role in
                     Enum.GetValues(typeof(ProductionAssetKind)))
                if (!roleMeshes.TryGetValue(role, out HashSet<Mesh> meshes) ||
                    meshes.Count != 5)
                    errors.Add(
                        $"{role} requires a distinct authored mesh in every biome.");
            return errors;
        }

#if UNITY_EDITOR
        public void Configure(ProductionBiomeArtProfile[] profiles) =>
            biomes = profiles ?? Array.Empty<ProductionBiomeArtProfile>();
#endif
    }
}
