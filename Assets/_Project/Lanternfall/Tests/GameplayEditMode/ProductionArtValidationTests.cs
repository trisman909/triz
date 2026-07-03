using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lanternfall.Gameplay.Presentation;
using Lanternfall.Gameplay.World;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Lanternfall.Tests
{
    public sealed class ProductionArtValidationTests
    {
        private const string Root =
            "Assets/_Project/Lanternfall/";
        private static readonly string[] Slugs =
        {
            "DrownedNarthex",
            "SiltglassObservatory",
            "EmberOssuary",
            "GloamOrchard",
            "StormvaultFoundry"
        };

        [Test]
        public void CatalogProvidesFiveIndependentUrpArtFamilies()
        {
            ProductionArtCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ProductionArtCatalog>(
                    Root + "Settings/LanternfallProductionArt.asset");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Validate(), Is.Empty);
            Assert.That(catalog.Biomes.Count, Is.EqualTo(5));

            var surfaces = new HashSet<Texture>();
            var responses = new HashSet<string>();
            var roleMeshes =
                new Dictionary<ProductionAssetKind, HashSet<Mesh>>();
            foreach (ProductionBiomeArtProfile biome in catalog.Biomes)
            {
                surfaces.Add(biome.SurfaceMaterial.mainTexture);
                responses.Add(
                    $"{biome.SurfaceMaterial.GetFloat("_Metallic"):F3}:" +
                    $"{biome.SurfaceMaterial.GetFloat("_Smoothness"):F3}");
                foreach (GameObject prefab in biome.ModularPrefabs)
                {
                    ProductionAssetMarker marker =
                        prefab.GetComponent<ProductionAssetMarker>();
                    Mesh mesh =
                        prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
                    if (!roleMeshes.TryGetValue(
                            marker.Kind, out HashSet<Mesh> meshes))
                    {
                        meshes = new HashSet<Mesh>();
                        roleMeshes.Add(marker.Kind, meshes);
                    }
                    meshes.Add(mesh);
                    AssertUrp(
                        prefab.GetComponentInChildren<Renderer>().sharedMaterial,
                        prefab.name);
                }
            }

            Assert.That(surfaces.Count, Is.EqualTo(5));
            Assert.That(responses.Count, Is.EqualTo(5));
            foreach (ProductionAssetKind role in
                     Enum.GetValues(typeof(ProductionAssetKind)))
                Assert.That(
                    roleMeshes[role].Count,
                    Is.EqualTo(5),
                    $"{role} is reusing a biome mesh.");
        }

        [Test]
        public void RepresentativeScenesHaveDistinctTraversalAndCleanUrpState()
        {
            var layouts = new HashSet<string>();
            int[] hazardCounts = { 4, 3, 4, 3, 4 };
            for (int biome = 0; biome < Slugs.Length; biome++)
            {
                EditorSceneManager.OpenScene(
                    Root + $"Scenes/ArtReview_{Slugs[biome]}.unity");
                ProductionAssetMarker[] markers =
                    UnityEngine.Object.FindObjectsByType<ProductionAssetMarker>(
                        FindObjectsSortMode.None);
                Assert.That(markers.Length, Is.GreaterThanOrEqualTo(11));
                string layout = string.Join(
                    "|",
                    markers
                        .Where(item =>
                            item.Kind == ProductionAssetKind.Floor)
                        .Select(item =>
                            $"{item.transform.position.x:F1}," +
                            $"{item.transform.position.z:F1}," +
                            $"{item.transform.eulerAngles.y:F0}")
                        .OrderBy(value => value));
                Assert.That(
                    layouts.Add(layout),
                    Is.True,
                    $"{Slugs[biome]} reuses another biome's floor grammar.");
                Assert.That(
                    UnityEngine.Object.FindObjectsByType<BiomeHazard>(
                        FindObjectsSortMode.None).Length,
                    Is.EqualTo(hazardCounts[biome]));

                Camera camera =
                    UnityEngine.Object.FindAnyObjectByType<Camera>();
                Assert.That(camera, Is.Not.Null);
                UniversalAdditionalCameraData cameraData =
                    camera.GetComponent<UniversalAdditionalCameraData>();
                Assert.That(cameraData, Is.Not.Null);
                Assert.That(
                    cameraData.renderPostProcessing,
                    Is.EqualTo(biome != 0));

                Volume volume =
                    UnityEngine.Object.FindAnyObjectByType<Volume>();
                Assert.That(volume?.sharedProfile, Is.Not.Null);
                Assert.That(
                    volume.sharedProfile.TryGet(
                        out Tonemapping tonemapping),
                    Is.True);
                Assert.That(
                    tonemapping.mode.value,
                    Is.EqualTo(TonemappingMode.ACES));
                Assert.That(
                    volume.sharedProfile.components,
                    Has.None.Null);

                foreach (Renderer renderer in
                         UnityEngine.Object.FindObjectsByType<Renderer>(
                             FindObjectsSortMode.None))
                {
                    if (renderer.GetComponent<TextMesh>() != null) continue;
                    foreach (Material material in renderer.sharedMaterials)
                        AssertUrp(
                            material,
                            $"{Slugs[biome]}/{renderer.name}");
                }
            }
            Assert.That(layouts.Count, Is.EqualTo(5));
        }

        [Test]
        public void CleanCapturesContainNoErrorMagentaAndDrownedStaysCyan()
        {
            foreach (string slug in Slugs)
            {
                string path =
                    $"Docs/ArtDirection/Comparisons/After_{slug}.png";
                Assert.That(File.Exists(path), Is.True, path);
                Assert.That(new FileInfo(path).Length, Is.GreaterThan(100000));
                Texture2D image = Load(path);
                int magenta = 0;
                int sampled = 0;
                double red = 0;
                double green = 0;
                double blue = 0;
                for (int y = 0; y < image.height; y += 4)
                for (int x = 0; x < image.width; x += 4)
                {
                    Color32 pixel = image.GetPixel(x, y);
                    if (pixel.r > 225 &&
                        pixel.b > 225 &&
                        pixel.g < 35)
                        magenta++;
                    int brightness = pixel.r + pixel.g + pixel.b;
                    if (brightness < 30 || brightness > 705) continue;
                    red += pixel.r;
                    green += pixel.g;
                    blue += pixel.b;
                    sampled++;
                }
                Assert.That(
                    magenta / (float)(image.width * image.height / 16),
                    Is.LessThan(.001f),
                    $"{slug} contains error-magenta pixels.");
                if (slug == "DrownedNarthex")
                {
                    Assert.That(green / sampled, Is.GreaterThan(red / sampled * 1.15));
                    Assert.That(blue / sampled, Is.GreaterThan(red / sampled * 1.15));
                }
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        [Test]
        public void AuthoredActorMaterialsAreNotOverriddenAndFallbacksUseUrp()
        {
            Material authored = AssetDatabase.LoadAssetAtPath<Material>(
                Root + "Art/Production/Materials/" +
                "DrownedNarthex_Accent.mat");
            GameObject actor =
                GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actor.GetComponent<Renderer>().sharedMaterial = authored;
            ActorPresentation presentation =
                actor.AddComponent<ActorPresentation>();
            presentation.Configure(Color.cyan, 1f, true);

            Renderer rootRenderer = actor.GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            foreach (Renderer renderer in
                     actor.GetComponentsInChildren<Renderer>())
            {
                if (renderer == rootRenderer) continue;
                Assert.That(renderer.sharedMaterial, Is.SameAs(authored));
                renderer.GetPropertyBlock(block);
                Assert.That(block.isEmpty, Is.True);
            }
            AssertUrp(UrpPresentationMaterials.Lit, "runtime lit fallback");
            AssertUrp(
                UrpPresentationMaterials.Emissive,
                "runtime emissive fallback");
            AssertUrp(
                UrpPresentationMaterials.Transparent,
                "runtime transparent fallback");
            UnityEngine.Object.DestroyImmediate(actor);
        }

        private static Texture2D Load(string path)
        {
            var image =
                new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Assert.That(
                ImageConversion.LoadImage(image, File.ReadAllBytes(path)),
                Is.True);
            return image;
        }

        private static void AssertUrp(Material material, string context)
        {
            Assert.That(material, Is.Not.Null, context);
            Assert.That(material.shader, Is.Not.Null, context);
            Assert.That(material.shader.isSupported, Is.True, context);
            Assert.That(
                material.shader.name,
                Does.StartWith("Universal Render Pipeline/"),
                context);
        }
    }
}
