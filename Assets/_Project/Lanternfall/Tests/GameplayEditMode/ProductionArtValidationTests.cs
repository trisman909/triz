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
                ProductionAssetMarker[] floorMarkers = markers
                    .Where(marker =>
                        marker.Kind == ProductionAssetKind.Floor ||
                        marker.Kind == ProductionAssetKind.Arena)
                    .ToArray();
                Assert.That(floorMarkers.Length, Is.GreaterThanOrEqualTo(2));
                string layout = string.Join(
                    "|",
                    markers
                        .Where(item =>
                            item.Kind != ProductionAssetKind.Floor &&
                            item.Kind != ProductionAssetKind.Arena)
                        .Select(item =>
                            $"{item.Kind}:" +
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
                AssertContinuousTraversal(floorMarkers, Slugs[biome]);

                Camera camera =
                    UnityEngine.Object.FindAnyObjectByType<Camera>();
                Assert.That(camera, Is.Not.Null);
                UniversalAdditionalCameraData cameraData =
                    camera.GetComponent<UniversalAdditionalCameraData>();
                Assert.That(cameraData, Is.Not.Null);
                Assert.That(cameraData.renderPostProcessing, Is.True);

                GameObject bearer = GameObject.Find("Bearer");
                Assert.That(bearer, Is.Not.Null, Slugs[biome]);
                Collider[] overlaps = Physics.OverlapBox(
                    bearer.transform.position + Vector3.up * .9f,
                    new Vector3(1.35f, .9f, 1.35f),
                    Quaternion.identity);
                int blocking = overlaps.Count(collider =>
                    !collider.isTrigger &&
                    collider.transform.root.gameObject != bearer);
                Assert.That(
                    blocking,
                    Is.EqualTo(0),
                    $"{Slugs[biome]} spawn is obstructed.");
                GameObject landmark = markers
                    .First(item => item.Kind == ProductionAssetKind.Landmark)
                    .gameObject;
                AssertPathExists(
                    bearer.transform.position,
                    landmark.transform.position,
                    floorMarkers,
                    Slugs[biome]);

                Bounds chamberBounds = ComputePlayableBounds(floorMarkers);
                foreach (ProductionAssetMarker marker in markers)
                    Assert.That(
                        chamberBounds.Contains(marker.transform.position),
                        Is.True,
                        $"{Slugs[biome]} contains out-of-bounds marker {marker.name}.");
                foreach (Collider collider in
                         UnityEngine.Object.FindObjectsByType<Collider>(
                             FindObjectsSortMode.None))
                {
                    if (!collider.enabled) continue;
                    if (collider.transform.root.gameObject == bearer) continue;
                    Assert.That(
                        chamberBounds.Contains(collider.bounds.center),
                        Is.True,
                        $"{Slugs[biome]} contains an out-of-bounds collider " +
                        $"{collider.name}.");
                }

                RepresentativeBiomeSwitcher switcher =
                    UnityEngine.Object.FindAnyObjectByType<
                        RepresentativeBiomeSwitcher>();
                Assert.That(switcher, Is.Not.Null, Slugs[biome]);

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

        private static void AssertContinuousTraversal(
            ProductionAssetMarker[] floorMarkers,
            string slug)
        {
            var visited = new HashSet<int> { 0 };
            var queue = new Queue<int>();
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int other = 0; other < floorMarkers.Length; other++)
                {
                    Vector3 currentPosition =
                        floorMarkers[current].transform.position;
                    Vector3 otherPosition =
                        floorMarkers[other].transform.position;
                    if (visited.Contains(other) ||
                        !AreAdjacent(currentPosition, otherPosition))
                        continue;
                    if (IsDiagonal(currentPosition, otherPosition) &&
                        !HasFloorAtMidpoint(currentPosition, otherPosition))
                        continue;
                    Assert.That(
                        HasFloorAtMidpoint(
                            currentPosition,
                            otherPosition),
                        Is.True,
                        $"{slug} has a floor gap between connected tiles at " +
                        $"{currentPosition} and {otherPosition}.");
                    visited.Add(other);
                    queue.Enqueue(other);
                }
            }
            Assert.That(
                visited.Count,
                Is.EqualTo(floorMarkers.Length),
                $"{slug} floor is not fully connected.");
        }

        private static void AssertPathExists(
            Vector3 start,
            Vector3 destination,
            ProductionAssetMarker[] floorMarkers,
            string slug)
        {
            int startIndex = NearestFloor(start, floorMarkers);
            int endIndex = NearestFloor(destination, floorMarkers);
            var visited = new HashSet<int> { startIndex };
            var queue = new Queue<int>();
            queue.Enqueue(startIndex);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (current == endIndex) return;
                for (int other = 0; other < floorMarkers.Length; other++)
                {
                    Vector3 currentPosition =
                        floorMarkers[current].transform.position;
                    Vector3 otherPosition =
                        floorMarkers[other].transform.position;
                    if (visited.Contains(other) ||
                        !AreAdjacent(currentPosition, otherPosition))
                        continue;
                    if (IsDiagonal(currentPosition, otherPosition) &&
                        !HasFloorAtMidpoint(currentPosition, otherPosition))
                        continue;
                    visited.Add(other);
                    queue.Enqueue(other);
                }
            }
            Assert.Fail($"{slug} has no connected route from spawn to landmark.");
        }

        private static int NearestFloor(
            Vector3 point,
            ProductionAssetMarker[] floorMarkers)
        {
            int nearest = 0;
            float distance = float.MaxValue;
            for (int index = 0; index < floorMarkers.Length; index++)
            {
                float candidate = Vector3.Distance(
                    point,
                    floorMarkers[index].transform.position);
                if (candidate >= distance) continue;
                distance = candidate;
                nearest = index;
            }
            return nearest;
        }

        private static Bounds ComputePlayableBounds(
            ProductionAssetMarker[] floorMarkers)
        {
            Collider firstCollider =
                floorMarkers[0].GetComponentInChildren<Collider>();
            Bounds bounds = firstCollider != null
                ? firstCollider.bounds
                : new Bounds(
                    floorMarkers[0].transform.position,
                    new Vector3(4f, 4f, 4f));
            foreach (ProductionAssetMarker marker in floorMarkers)
            {
                Collider collider = marker.GetComponentInChildren<Collider>();
                if (collider != null) bounds.Encapsulate(collider.bounds);
                else bounds.Encapsulate(marker.transform.position);
            }
            bounds.Expand(new Vector3(1f, 6f, 1f));
            return bounds;
        }

        private static bool AreAdjacent(Vector3 a, Vector3 b)
        {
            Vector2 delta = new Vector2(a.x - b.x, a.z - b.z);
            return delta.sqrMagnitude <= 32.5f;
        }

        private static bool IsDiagonal(Vector3 a, Vector3 b) =>
            Mathf.Abs(a.x - b.x) > .1f &&
            Mathf.Abs(a.z - b.z) > .1f;

        private static bool HasFloorAtMidpoint(Vector3 a, Vector3 b)
        {
            Vector3 midpoint = Vector3.Lerp(a, b, .5f);
            Vector3[] probes =
            {
                midpoint,
                midpoint + new Vector3(.9f, 0f, 0f),
                midpoint + new Vector3(-.9f, 0f, 0f),
                midpoint + new Vector3(0f, 0f, .9f),
                midpoint + new Vector3(0f, 0f, -.9f)
            };
            foreach (Vector3 probe in probes)
            {
                RaycastHit[] hits = Physics.RaycastAll(
                    probe + Vector3.up * 6f,
                    Vector3.down,
                    12f);
                foreach (RaycastHit hit in hits.OrderBy(item => item.distance))
                {
                    ProductionAssetMarker marker =
                        hit.collider.GetComponentInParent<ProductionAssetMarker>();
                    if (marker != null &&
                        (marker.Kind == ProductionAssetKind.Floor ||
                         marker.Kind == ProductionAssetKind.Arena))
                        return true;
                }
            }
            return false;
        }
    }
}
