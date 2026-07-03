using System;
using System.Collections.Generic;
using System.IO;
using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Audio;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Camera;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Performance;
using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Presentation;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.UI;
using Lanternfall.Gameplay.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Lanternfall.Editor
{
    /// <summary>
    /// Reproducible, original production-art foundation. The external concept
    /// proxy is deliberately never imported or referenced here.
    /// </summary>
    public static class ProductionArtBuilder
    {
        public const string CatalogPath =
            "Assets/_Project/Lanternfall/Settings/LanternfallProductionArt.asset";
        private const string Root =
            "Assets/_Project/Lanternfall/Art/Production/";
        private const string TextureRoot = Root + "Textures/";
        private const string MaterialRoot = Root + "Materials/";
        private const string MeshRoot = Root + "Meshes/";
        private const string PrefabRoot = Root + "Prefabs/";
        private const string CharacterRoot = Root + "Characters/";
        private const string EnemyRoot = Root + "Enemies/";
        private const string GuardianRoot = Root + "Guardians/";
        private const string UiResourceRoot =
            Root + "UI/Resources/UI/";
        private const string VfxRoot = Root + "VFX/";
        private const string PresentationResourceRoot =
            "Assets/_Project/Lanternfall/Resources/Presentation/";
        private const string SceneRoot =
            "Assets/_Project/Lanternfall/Scenes/";
        private const string ComparisonRoot =
            "Docs/ArtDirection/Comparisons/";
        private static readonly string[] SourceTextures =
        {
            TextureRoot + "Source_DrownedRuinStone.png",
            TextureRoot + "Source_SiltglassObservatory.png",
            TextureRoot + "Source_EmberOssuary.png",
            TextureRoot + "Source_GloamOrchard.png",
            TextureRoot + "Source_StormvaultFoundry.png"
        };

        private static readonly string[] BiomeSlugs =
        {
            "DrownedNarthex",
            "SiltglassObservatory",
            "EmberOssuary",
            "GloamOrchard",
            "StormvaultFoundry"
        };

        private static readonly string[] BiomeIds =
        {
            "biome.drowned_narthex",
            "biome.siltglass_observatory",
            "biome.ember_ossuary",
            "biome.gloam_orchard",
            "biome.stormvault_foundry"
        };

        private static readonly string[] BiomeNames =
        {
            "The Drowned Narthex",
            "Siltglass Observatory",
            "The Ember Ossuary",
            "The Gloam Orchard",
            "Stormvault Foundry"
        };

        private static readonly string[] Stories =
        {
            "A drowned procession left its bells above the tide.",
            "A broken orrery still maps stars beneath the silt.",
            "The keepers fed names and bones into the final furnace.",
            "Roots learned the shape of every pilgrim who vanished here.",
            "A sealed dynamo keeps working for masters long gone."
        };

        private static readonly Color[] Tints =
        {
            new Color(.56f, .76f, .78f),
            new Color(.72f, .58f, .88f),
            new Color(.9f, .48f, .24f),
            new Color(.42f, .73f, .48f),
            new Color(.5f, .72f, .92f)
        };

        private static readonly Color[] Accents =
        {
            new Color(.55f, .95f, .9f),
            new Color(.82f, .42f, 1f),
            new Color(1f, .28f, .045f),
            new Color(.38f, 1f, .48f),
            new Color(.28f, .75f, 1f)
        };

        private static readonly Color[] FogColors =
        {
            new Color(.035f, .12f, .14f),
            new Color(.08f, .055f, .16f),
            new Color(.16f, .045f, .018f),
            new Color(.035f, .1f, .055f),
            new Color(.035f, .07f, .13f)
        };

        private static readonly float[] MetallicFamilies =
            { .035f, .18f, .08f, .01f, .84f };
        private static readonly float[] SmoothFamilies =
            { .48f, .4f, .16f, .1f, .68f };

        [MenuItem("Lanternfall/Build Representative Production Art")]
        public static void BuildMenu()
        {
            BuildRepresentativePass();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Lanternfall representative production-art pass built.");
        }

        [MenuItem("Lanternfall/Diagnostics/Capture Drowned Material Isolation")]
        public static void CaptureDrownedMaterialIsolation()
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(
                MaterialRoot + "DrownedNarthex_Surface.mat");
            if (source == null)
                throw new BuildFailedException(
                    "Drowned Narthex surface material is missing.");

            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.32f, .32f, .32f);
            RenderSettings.skybox = null;
            var lightObject = new GameObject("Neutral Diagnostic Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            Material[] variants =
            {
                new Material(source) { name = "Production" },
                new Material(source) { name = "Without Normal" },
                new Material(source) { name = "Without Mask" },
                new Material(source) { name = "Albedo Only" }
            };
            variants[1].SetTexture("_BumpMap", null);
            variants[1].DisableKeyword("_NORMALMAP");
            variants[2].SetTexture("_MetallicGlossMap", null);
            variants[2].DisableKeyword("_METALLICSPECGLOSSMAP");
            variants[3].SetTexture("_BumpMap", null);
            variants[3].SetTexture("_MetallicGlossMap", null);
            variants[3].SetTexture("_EmissionMap", null);
            variants[3].DisableKeyword("_NORMALMAP");
            variants[3].DisableKeyword("_METALLICSPECGLOSSMAP");
            variants[3].DisableKeyword("_EMISSION");
            variants[3].SetFloat("_Metallic", 0f);
            variants[3].SetFloat("_Smoothness", .25f);

            for (int index = 0; index < variants.Length; index++)
            {
                GameObject swatch =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                swatch.name = variants[index].name;
                swatch.transform.position =
                    new Vector3(-6f + index * 4f, 0f, 0f);
                swatch.transform.localScale = new Vector3(3.5f, .5f, 4f);
                swatch.GetComponent<Renderer>().sharedMaterial =
                    variants[index];
            }

            string[] familyNames =
                { "Surface", "Structure", "Accent", "Emissive", "Hazard" };
            for (int index = 0; index < familyNames.Length; index++)
            {
                Material family = AssetDatabase.LoadAssetAtPath<Material>(
                    MaterialRoot + "DrownedNarthex_" +
                    familyNames[index] + ".mat");
                GameObject swatch =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                swatch.name = familyNames[index] + " Family";
                swatch.transform.position =
                    new Vector3(-8f + index * 4f, 0f, 4f);
                swatch.transform.localScale = new Vector3(3.5f, .5f, 3.5f);
                swatch.GetComponent<Renderer>().sharedMaterial = family;
            }

            UnityEngine.Camera camera = CreateCaptureCamera();
            camera.transform.position = new Vector3(0f, 8f, -7f);
            camera.transform.LookAt(Vector3.zero);
            camera.orthographicSize = 7.5f;
            CaptureCamera(
                camera,
                "Docs/ArtDirection/Diagnostics/" +
                "DrownedMaterialIsolation.png");
            Debug.Log(
                $"Drowned diagnostic shader={source.shader.name}; " +
                $"base={AssetDatabase.GetAssetPath(source.GetTexture("_BaseMap"))}; " +
                $"normal={AssetDatabase.GetAssetPath(source.GetTexture("_BumpMap"))}; " +
                $"mask={AssetDatabase.GetAssetPath(source.GetTexture("_MetallicGlossMap"))}; " +
                $"keywords={string.Join(",", source.enabledKeywords)}");
            foreach (Material variant in variants)
                UnityEngine.Object.DestroyImmediate(variant);
        }

        [MenuItem("Lanternfall/Diagnostics/Capture Drowned Scene Isolation")]
        public static void CaptureDrownedSceneIsolation()
        {
            EditorSceneManager.OpenScene(
                SceneRoot + "ArtReview_DrownedNarthex.unity",
                OpenSceneMode.Single);
            UnityEngine.Camera camera =
                UnityEngine.Object.FindAnyObjectByType<UnityEngine.Camera>();
            UniversalAdditionalCameraData cameraData =
                camera.GetComponent<UniversalAdditionalCameraData>();
            bool postProcessing = cameraData.renderPostProcessing;
            cameraData.renderPostProcessing = false;
            CaptureCamera(
                camera,
                "Docs/ArtDirection/Diagnostics/" +
                "DrownedScene_NoPost.png");

            Color ambient = RenderSettings.ambientLight;
            AmbientMode ambientMode = RenderSettings.ambientMode;
            bool fog = RenderSettings.fog;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.28f, .28f, .28f);
            Light[] lights =
                UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsSortMode.None);
            var colors = new Color[lights.Length];
            for (int index = 0; index < lights.Length; index++)
            {
                colors[index] = lights[index].color;
                lights[index].color = Color.white;
            }
            CaptureCamera(
                camera,
                "Docs/ArtDirection/Diagnostics/" +
                "DrownedScene_NeutralLight.png");
            for (int index = 0; index < lights.Length; index++)
                lights[index].color = colors[index];
            RenderSettings.ambientLight = ambient;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.fog = fog;
            cameraData.renderPostProcessing = postProcessing;
        }

        [MenuItem("Lanternfall/Capture Representative Art Review")]
        public static void CaptureReviewScenes()
        {
            for (int biome = 0; biome < BiomeSlugs.Length; biome++)
                CaptureScene(
                    SceneRoot + $"ArtReview_{BiomeSlugs[biome]}.unity",
                    ComparisonRoot + $"After_{BiomeSlugs[biome]}.png");
            AssetDatabase.Refresh();
            Debug.Log(
                "Lanternfall representative gameplay-camera captures complete.");
        }

        public static string[] BuildRepresentativePass()
        {
            EnsureFolders();
            EnsureRuntimePresentationAssets();
            GenerateTextureLibrary();
            GenerateUiAndVfxTextures();
            Material[][] materials = CreateMaterialLibrary();
            GameObject[][] biomePrefabs = new GameObject[5][];
            GameObject[] characters = CreateCharacterPrefabs(materials);
            GameObject[] enemies = CreateDisplayActorPrefabs(
                EnemyRoot, "Enemy", materials, false);
            GameObject[] guardians = CreateDisplayActorPrefabs(
                GuardianRoot, "Guardian", materials, true);
            for (int biome = 0; biome < 5; biome++)
                biomePrefabs[biome] =
                    CreateBiomePrefabs(biome, materials[biome]);

            ProductionArtCatalog catalog =
                CreateCatalog(materials, biomePrefabs);
            List<string> errors = catalog.Validate();
            if (errors.Count > 0)
                throw new BuildFailedException(string.Join("\n", errors));

            string[] scenes = new string[5];
            for (int biome = 0; biome < 5; biome++)
            {
                scenes[biome] = SceneRoot +
                    $"ArtReview_{BiomeSlugs[biome]}.unity";
                CaptureBefore(biome);
                BuildReviewScene(
                    biome,
                    scenes[biome],
                    catalog.Biomes[biome],
                    characters[biome],
                    enemies[biome],
                    guardians[biome],
                    biome == 4
                        ? "LanternfallHub"
                        : $"ArtReview_{BiomeSlugs[biome + 1]}");
                CaptureScene(
                    scenes[biome],
                    ComparisonRoot + $"After_{BiomeSlugs[biome]}.png");
            }
            AddReviewPortalToHub(
                biomePrefabs[0][(int)ProductionAssetKind.Gate],
                materials[0][3]);
            UpdateBuildSettings(scenes);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return scenes;
        }

        private static void UpdateBuildSettings(string[] reviewScenes)
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene scene in
                     EditorBuildSettings.scenes)
                if (scene.path.IndexOf(
                        "/ArtReview_",
                        StringComparison.OrdinalIgnoreCase) < 0)
                    scenes.Add(new EditorBuildSettingsScene(
                        scene.path, true));
            foreach (string reviewScene in reviewScenes)
                scenes.Add(new EditorBuildSettingsScene(
                    reviewScene, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                TextureRoot, MaterialRoot, MeshRoot, PrefabRoot,
                CharacterRoot, EnemyRoot, GuardianRoot,
                UiResourceRoot, VfxRoot, PresentationResourceRoot,
                ComparisonRoot
            };
            foreach (string folder in folders)
                Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        public static void EnsureRuntimePresentationAssets()
        {
            Directory.CreateDirectory(PresentationResourceRoot);
            AssetDatabase.Refresh();
            CreateRuntimePresentationMaterial(
                "RuntimeLit", "Universal Render Pipeline/Lit", false, false);
            CreateRuntimePresentationMaterial(
                "RuntimeEmissive", "Universal Render Pipeline/Lit", true, false);
            CreateRuntimePresentationMaterial(
                "RuntimeTransparent",
                "Universal Render Pipeline/Unlit",
                false,
                true);
            CreateRuntimePresentationMaterial(
                "RuntimeParticle",
                "Universal Render Pipeline/Particles/Unlit",
                false,
                true);
            AssetDatabase.SaveAssets();
        }

        private static void CreateRuntimePresentationMaterial(
            string name,
            string shaderName,
            bool emissive,
            bool transparent)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null || !shader.isSupported)
                throw new BuildFailedException(
                    $"Required URP presentation shader is unavailable: " +
                    shaderName);
            string path = PresentationResourceRoot + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }
            material.enableInstancing = true;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", emissive ? .1f : .02f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", emissive ? .55f : .3f);
            if (emissive)
            {
                material.SetColor("_EmissionColor", Color.white);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat(
                    "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_Surface", 0f);
                material.SetFloat("_ZWrite", 1f);
                material.SetOverrideTag("RenderType", "Opaque");
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Geometry;
            }
            EditorUtility.SetDirty(material);
        }

        private static void GenerateTextureLibrary()
        {
            const int size = 512;
            for (int biome = 0; biome < 5; biome++)
            {
                string sourcePath = SourceTextures[biome];
                TextureImporter sourceImporter =
                    AssetImporter.GetAtPath(sourcePath) as TextureImporter;
                if (sourceImporter == null)
                    throw new BuildFailedException(
                        $"Production texture source missing: {sourcePath}");
                if (!sourceImporter.isReadable)
                {
                    sourceImporter.isReadable = true;
                    sourceImporter.textureCompression =
                        TextureImporterCompression.Compressed;
                    sourceImporter.wrapMode = TextureWrapMode.Repeat;
                    sourceImporter.SaveAndReimport();
                }
                Texture2D source =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
                Color[] albedo = new Color[size * size];
                Color[] normal = new Color[size * size];
                Color[] mask = new Color[size * size];
                Color[] emission = new Color[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    Color sample = source.GetPixelBilinear(
                        (x + .5f) / size,
                        (y + .5f) / size);
                    float value = Mathf.Max(sample.r, sample.g, sample.b);
                    float variation = .82f + Hash01(x, y, biome) * .22f;
                    Color paletteNudge = new Color(
                        sample.r * Tints[biome].r,
                        sample.g * Tints[biome].g,
                        sample.b * Tints[biome].b,
                        1f);
                    Color tinted =
                        Color.Lerp(sample, paletteNudge, .12f) * variation;
                    tinted.a = 1f;
                    albedo[y * size + x] = tinted;

                    float left = Luminance(source.GetPixelBilinear(
                        (x - .5f) / size, (y + .5f) / size));
                    float right = Luminance(source.GetPixelBilinear(
                        (x + 1.5f) / size, (y + .5f) / size));
                    float down = Luminance(source.GetPixelBilinear(
                        (x + .5f) / size, (y - .5f) / size));
                    float up = Luminance(source.GetPixelBilinear(
                        (x + .5f) / size, (y + 1.5f) / size));
                    Vector3 n = new Vector3(
                        (left - right) * 3.2f,
                        (down - up) * 3.2f,
                        1f).normalized;
                    normal[y * size + x] =
                        new Color(n.x * .5f + .5f, n.y * .5f + .5f,
                            n.z * .5f + .5f, 1f);

                    float edge = Mathf.Clamp01(
                        Mathf.Abs(right - left) + Mathf.Abs(up - down));
                    float metallic = MetallicFamilies[biome];
                    float smooth = SmoothFamilies[biome];
                    mask[y * size + x] = new Color(
                        metallic * (.65f + value * .35f),
                        .72f + value * .25f,
                        edge,
                        Mathf.Clamp01(smooth + edge * .2f));
                    float glow = AccentAffinity(sample, biome);
                    glow = Mathf.Max(
                        glow,
                        edge > .25f ? edge * .35f : 0f);
                    emission[y * size + x] =
                        Color.Lerp(Color.black, Accents[biome], glow);
                }
                string slug = BiomeSlugs[biome];
                WritePng(TextureRoot + $"{slug}_Albedo.png", size, albedo);
                WritePng(TextureRoot + $"{slug}_Normal.png", size, normal);
                WritePng(TextureRoot + $"{slug}_Mask.png", size, mask);
                WritePng(TextureRoot + $"{slug}_Emission.png", size, emission);
            }
            AssetDatabase.Refresh();
            for (int biome = 0; biome < 5; biome++)
            {
                ConfigureTexture(
                    TextureRoot + $"{BiomeSlugs[biome]}_Albedo.png",
                    TextureImporterType.Default, true);
                ConfigureTexture(
                    TextureRoot + $"{BiomeSlugs[biome]}_Normal.png",
                    TextureImporterType.NormalMap, false);
                ConfigureTexture(
                    TextureRoot + $"{BiomeSlugs[biome]}_Mask.png",
                    TextureImporterType.Default, false);
                ConfigureTexture(
                    TextureRoot + $"{BiomeSlugs[biome]}_Emission.png",
                    TextureImporterType.Default, false);
            }
        }

        private static void GenerateUiAndVfxTextures()
        {
            const int atlasSize = 512;
            var pixels = new Color[atlasSize * atlasSize];
            for (int icon = 0; icon < 16; icon++)
            {
                int cellX = icon % 4;
                int cellY = icon / 4;
                Color accent = Accents[icon % 5];
                for (int y = 0; y < 128; y++)
                for (int x = 0; x < 128; x++)
                {
                    float nx = (x - 63.5f) / 58f;
                    float ny = (y - 63.5f) / 58f;
                    float radius = Mathf.Sqrt(nx * nx + ny * ny);
                    float angle = Mathf.Atan2(ny, nx);
                    float petals = .52f + .14f *
                        Mathf.Cos(angle * (3 + icon % 5));
                    bool border = radius < .92f && radius > .82f;
                    bool glyph = radius < petals &&
                        radius > .18f + .05f * Mathf.Sin(angle * 2f);
                    Color color = border
                        ? new Color(.75f, .58f, .28f, 1f)
                        : glyph ? accent : Color.clear;
                    int px = cellX * 128 + x;
                    int py = cellY * 128 + y;
                    pixels[py * atlasSize + px] = color;
                }
            }
            WritePng(
                UiResourceRoot + "Lanternfall_UI_IconAtlas.png",
                atlasSize, pixels);

            const int particleSize = 128;
            var particle = new Color[particleSize * particleSize];
            for (int y = 0; y < particleSize; y++)
            for (int x = 0; x < particleSize; x++)
            {
                float nx = (x - 63.5f) / 63.5f;
                float ny = (y - 63.5f) / 63.5f;
                float radial = Mathf.Clamp01(1f -
                    Mathf.Sqrt(nx * nx + ny * ny));
                float cross = Mathf.Clamp01(
                    1f - Mathf.Min(Mathf.Abs(nx), Mathf.Abs(ny)) * 8f);
                float alpha = Mathf.Pow(radial, 2f) *
                    Mathf.Lerp(.55f, 1f, cross);
                particle[y * particleSize + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
            WritePng(
                VfxRoot + "Lanternfall_Particle.png",
                particleSize, particle);
            AssetDatabase.Refresh();
            ConfigureTexture(
                UiResourceRoot + "Lanternfall_UI_IconAtlas.png",
                TextureImporterType.Sprite, true);
            ConfigureTexture(
                VfxRoot + "Lanternfall_Particle.png",
                TextureImporterType.Sprite, true);
        }

        private static Material[][] CreateMaterialLibrary()
        {
            var result = new Material[5][];
            for (int biome = 0; biome < 5; biome++)
            {
                string slug = BiomeSlugs[biome];
                Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    TextureRoot + $"{slug}_Albedo.png");
                Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    TextureRoot + $"{slug}_Normal.png");
                Texture2D mask = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    TextureRoot + $"{slug}_Mask.png");
                Texture2D emission = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    TextureRoot + $"{slug}_Emission.png");
                Color[] surfaceTints =
                {
                    new Color(.52f, 1f, .78f),
                    new Color(1f, .88f, .7f),
                    new Color(.82f, .62f, .5f),
                    new Color(.68f, .9f, .58f),
                    new Color(.68f, .84f, 1f)
                };
                result[biome] = new[]
                {
                    CreateMaterial(
                        $"{slug}_Surface", albedo, normal, mask, emission,
                        surfaceTints[biome], MetallicFamilies[biome],
                        SmoothFamilies[biome], Color.black, biome),
                    CreateMaterial(
                        $"{slug}_Structure", albedo, normal, mask, emission,
                        new Color(.72f, .74f, .76f),
                        Mathf.Clamp01(MetallicFamilies[biome] + .12f),
                        Mathf.Clamp01(SmoothFamilies[biome] - .12f),
                        Color.black, biome),
                    CreateMaterial(
                        $"{slug}_Accent", albedo, normal, mask, emission,
                        Color.Lerp(Color.white, Accents[biome], .45f),
                        Mathf.Clamp01(MetallicFamilies[biome] + .38f),
                        Mathf.Clamp01(SmoothFamilies[biome] + .12f),
                        Color.black, biome),
                    CreateMaterial(
                        $"{slug}_Emissive", albedo, normal, mask, emission,
                        Color.Lerp(Color.white, Accents[biome], .25f),
                        MetallicFamilies[biome], SmoothFamilies[biome],
                        Accents[biome] * 2.8f, biome),
                    CreateMaterial(
                        $"{slug}_Hazard", albedo, normal, mask, emission,
                        Color.Lerp(Color.white, Accents[biome], .5f),
                        MetallicFamilies[biome],
                        Mathf.Clamp01(SmoothFamilies[biome] + .2f),
                        Accents[biome] * 4f, biome)
                };
            }
            return result;
        }

        private static Material CreateMaterial(
            string name,
            Texture2D albedo,
            Texture2D normal,
            Texture2D mask,
            Texture2D emission,
            Color tint,
            float metallic,
            float smoothness,
            Color emissionColor,
            int biome)
        {
            string path = MaterialRoot + name + ".mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported)
                throw new BuildFailedException(
                    "The active URP installation does not provide a supported " +
                    "Universal Render Pipeline/Lit shader.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = name
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            material.DisableKeyword("_NORMALMAP");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            material.DisableKeyword("_EMISSION");
            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_BumpMap", normal);
            material.SetTexture("_MetallicGlossMap", mask);
            material.SetTexture("_EmissionMap", emission);
            float[] scales = { .72f, 1.35f, .9f, .58f, 1.65f };
            Vector2 textureScale = Vector2.one * scales[biome];
            material.SetTextureScale("_BaseMap", textureScale);
            material.SetTextureScale("_BumpMap", textureScale);
            material.SetTextureScale("_MetallicGlossMap", textureScale);
            material.SetTextureScale("_EmissionMap", textureScale);
            material.SetColor("_BaseColor", tint);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_BumpScale", .8f);
            material.SetColor("_EmissionColor", emissionColor);
            material.SetFloat("_Surface", 0f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_ZWrite", 1f);
            material.SetOverrideTag("RenderType", "Opaque");
            material.renderQueue = (int)RenderQueue.Geometry;
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            if (emissionColor.maxColorComponent > 0f)
                material.EnableKeyword("_EMISSION");
            else
                material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags =
                emissionColor.maxColorComponent > 0f
                    ? MaterialGlobalIlluminationFlags.RealtimeEmissive
                    : MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject[] CreateBiomePrefabs(
            int biome, Material[] materials)
        {
            var prefabs = new GameObject[11];
            foreach (ProductionAssetKind kind in
                     Enum.GetValues(typeof(ProductionAssetKind)))
            {
                Mesh mesh = SaveMesh(
                    BuildRoleMesh(biome, kind),
                    $"{BiomeSlugs[biome]}_{kind}");
                Material material =
                    kind == ProductionAssetKind.Floor ? materials[0] :
                    kind == ProductionAssetKind.Hazard ? materials[4] :
                    kind == ProductionAssetKind.Lantern ? materials[3] :
                    kind == ProductionAssetKind.Decoration ? materials[2] :
                    materials[1];
                prefabs[(int)kind] = SavePrefab(
                    biome, kind, mesh, material,
                    PrefabRoot +
                    $"LF_{BiomeSlugs[biome]}_{kind}_A.prefab");
            }
            return prefabs;
        }

        private static Mesh BuildRoleMesh(
            int biome, ProductionAssetKind kind)
        {
            var builder = new MeshBuilder();
            switch (kind)
            {
                case ProductionAssetKind.Floor:
                    builder.AddBox(Vector3.zero, new Vector3(4f, .28f, 4f));
                    BuildFloorDetails(builder, biome);
                    break;
                case ProductionAssetKind.Wall:
                    BuildWall(builder, biome);
                    break;
                case ProductionAssetKind.Corner:
                    BuildCorner(builder, biome);
                    break;
                case ProductionAssetKind.Gate:
                    BuildGate(builder, biome);
                    break;
                case ProductionAssetKind.Column:
                    BuildColumn(builder, biome);
                    break;
                case ProductionAssetKind.Landmark:
                    BuildLandmark(builder, biome);
                    break;
                case ProductionAssetKind.Decoration:
                    BuildDecoration(builder, biome);
                    break;
                case ProductionAssetKind.Hazard:
                    BuildHazard(builder, biome);
                    break;
                case ProductionAssetKind.Lantern:
                    builder.AddPrism(Vector3.up * .15f, .52f, .3f, 8, .46f);
                    builder.AddPrism(Vector3.up * 1.2f, .36f, 1.8f, 8, .28f);
                    builder.AddPrism(Vector3.up * 2.2f, .55f, .35f, 8, .08f);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * Mathf.PI * .5f;
                        builder.AddBox(new Vector3(
                            Mathf.Cos(angle) * .42f, 1.2f,
                            Mathf.Sin(angle) * .42f),
                            new Vector3(.08f, 1.75f, .08f));
                    }
                    break;
                case ProductionAssetKind.Arena:
                    builder.AddPrism(Vector3.up * .22f, 2.5f, .44f, 16, 2.25f);
                    builder.AddRing(
                        Vector3.up * .48f,
                        biome == 3 ? 1.15f : 1.45f,
                        biome == 3 ? 1.85f : 1.7f,
                        .12f,
                        biome == 1 || biome == 4 ? 32 : 24);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * Mathf.PI * .5f + Mathf.PI * .25f;
                        builder.AddSpike(new Vector3(
                            Mathf.Cos(angle) * 2f, .55f,
                            Mathf.Sin(angle) * 2f),
                            .18f + biome * .025f,
                            1.05f + biome * .12f,
                            biome == 2 ? 5 : 7);
                    }
                    break;
                default:
                    for (int i = 0; i < 7; i++)
                    {
                        float angle = i * 2.399f;
                        builder.AddPrism(new Vector3(
                            Mathf.Cos(angle) * .55f, .35f + i * .04f,
                            Mathf.Sin(angle) * .55f),
                            .22f + i % 3 * .08f,
                            .6f + i % 2 * .35f,
                            7, .12f);
                    }
                    break;
            }
            return builder.Create($"{BiomeSlugs[biome]} {kind}");
        }

        private static void BuildFloorDetails(MeshBuilder builder, int biome)
        {
            if (biome == 0)
            {
                builder.AddRing(
                    new Vector3(0f, .16f, 0f), .7f, .86f, .06f, 20);
                for (int i = 0; i < 4; i++)
                    builder.AddBox(new Vector3(
                        i % 2 == 0 ? 1.35f : -1.35f,
                        .17f,
                        i < 2 ? 1.35f : -1.35f),
                        new Vector3(.5f, .06f, .5f));
            }
            else if (biome == 1)
            {
                builder.AddRing(
                    new Vector3(0f, .17f, 0f), .8f, 1.08f, .07f, 28);
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * Mathf.PI / 3f;
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * 1.35f, .19f,
                        Mathf.Sin(angle) * 1.35f), .16f, .26f, 5);
                }
            }
            else if (biome == 2)
            {
                builder.AddBox(
                    new Vector3(0f, .18f, 0f),
                    new Vector3(.45f, .08f, 3.6f));
                builder.AddBox(
                    new Vector3(0f, .18f, 0f),
                    new Vector3(3.6f, .08f, .45f));
                for (int i = 0; i < 4; i++)
                    builder.AddSpike(new Vector3(
                        i < 2 ? -1.5f : 1.5f, .2f,
                        i % 2 == 0 ? -1.5f : 1.5f), .12f, .38f, 5);
            }
            else if (biome == 3)
            {
                for (int i = 0; i < 7; i++)
                {
                    float angle = i * 2.399f;
                    builder.AddBox(new Vector3(
                        Mathf.Cos(angle) * (1f + i * .12f),
                        .18f,
                        Mathf.Sin(angle) * (1f + i * .12f)),
                        new Vector3(.16f, .09f, 2.4f - i * .16f));
                }
            }
            else
            {
                for (int i = -2; i <= 2; i++)
                {
                    builder.AddBox(
                        new Vector3(i * .72f, .18f, 0f),
                        new Vector3(.18f, .09f, 3.7f));
                    builder.AddPrism(
                        new Vector3(i * .72f, .24f, 0f),
                        .11f, .16f, 8, .11f);
                }
            }
        }

        private static void BuildWall(MeshBuilder builder, int biome)
        {
            if (biome == 3)
            {
                for (int i = 0; i < 5; i++)
                {
                    float x = -1.6f + i * .8f;
                    builder.AddPrism(
                        new Vector3(x, 1.45f + i % 2 * .25f, 0f),
                        .28f + i % 2 * .08f,
                        3f + i % 2 * .5f,
                        7, .12f);
                    builder.AddSpike(
                        new Vector3(x + .35f, 2.45f, -.1f),
                        .11f, 1.3f, 5);
                }
                return;
            }

            builder.AddBox(
                Vector3.up * 1.6f,
                new Vector3(4f, 3.2f, biome == 4 ? .58f : .38f));
            if (biome == 0)
            {
                builder.AddPrism(new Vector3(-1.55f, 1.6f, -.15f),
                    .32f, 3.5f, 8, .24f);
                builder.AddPrism(new Vector3(1.55f, 1.6f, -.15f),
                    .32f, 3.5f, 8, .24f);
                builder.AddRing(new Vector3(0f, 2.1f, -.22f),
                    .45f, .62f, .18f, 16);
            }
            else if (biome == 1)
            {
                for (int i = 0; i < 5; i++)
                    builder.AddSpike(new Vector3(
                        -1.6f + i * .8f, 3.05f, -.25f),
                        .22f, .9f + i % 2 * .4f, 5);
                builder.AddRing(new Vector3(0f, 1.7f, -.28f),
                    .75f, .9f, .14f, 24);
            }
            else if (biome == 2)
            {
                for (int i = 0; i < 6; i++)
                    builder.AddSpike(new Vector3(
                        -1.65f + i * .66f, .25f, -.28f),
                        .2f, 2.2f + i % 2 * .55f, 5);
                builder.AddBox(new Vector3(0f, 2.6f, -.32f),
                    new Vector3(3.4f, .35f, .22f));
            }
            else
            {
                for (int y = 0; y < 3; y++)
                    builder.AddBox(
                        new Vector3(0f, .65f + y * 1.05f, -.35f),
                        new Vector3(3.6f, .18f, .18f));
                for (int x = 0; x < 5; x++)
                    builder.AddPrism(new Vector3(
                        -1.55f + x * .78f, 1.65f, -.4f),
                        .12f, 3.15f, 8, .12f);
            }
        }

        private static void BuildCorner(MeshBuilder builder, int biome)
        {
            float thickness = biome == 4 ? .55f : .38f;
            builder.AddBox(
                new Vector3(-.9f, 1.5f, 0f),
                new Vector3(2.2f, 3f, thickness));
            builder.AddBox(
                new Vector3(0f, 1.5f, .9f),
                new Vector3(thickness, 3f, 2.2f));
            if (biome == 1)
            {
                builder.AddSpike(
                    new Vector3(0f, 2.7f, 0f), .65f, 1.6f, 6);
            }
            else if (biome == 2)
            {
                for (int i = 0; i < 4; i++)
                    builder.AddSpike(
                        new Vector3(0f, .45f + i * .65f, -.28f),
                        .24f, .75f, 5);
            }
            else if (biome == 3)
            {
                builder.AddPrism(
                    Vector3.up * 1.7f, .52f, 3.4f, 7, .18f);
                builder.AddSpike(
                    new Vector3(.45f, 2.7f, .45f), .18f, 1.5f, 5);
            }
            else
            {
                builder.AddPrism(
                    Vector3.up * 1.6f,
                    biome == 0 ? .42f : .58f,
                    3.3f,
                    biome == 0 ? 8 : 12,
                    biome == 0 ? .3f : .46f);
                if (biome == 4)
                    builder.AddRing(
                        new Vector3(0f, 1.65f, 0f),
                        .62f, .78f, .18f, 18);
            }
        }

        private static void BuildGate(MeshBuilder builder, int biome)
        {
            int sides = biome == 1 ? 6 : biome == 4 ? 12 : 9;
            float radius = biome == 3 ? .5f : .36f;
            builder.AddPrism(new Vector3(-1.25f, 1.6f, 0f),
                radius, 3.2f, sides, biome == 3 ? .15f : .28f);
            builder.AddPrism(new Vector3(1.25f, 1.6f, 0f),
                radius, 3.2f, sides, biome == 3 ? .15f : .28f);
            if (biome == 1)
            {
                builder.AddRing(new Vector3(0f, 2.75f, 0f),
                    1.15f, 1.45f, .25f, 28);
                builder.AddSpike(Vector3.up * 3.4f, .32f, 1.1f, 6);
            }
            else if (biome == 3)
            {
                for (int i = 0; i < 6; i++)
                    builder.AddBox(new Vector3(
                        -1.25f + i * .5f, 3f + i % 2 * .18f, 0f),
                        new Vector3(.16f, .42f, .35f));
            }
            else
            {
                builder.AddBox(new Vector3(0f, 3.05f, 0f),
                    new Vector3(3f, biome == 4 ? .75f : .55f, .55f));
                int crown = biome == 2 ? 7 : 5;
                for (int i = 0; i < crown; i++)
                    builder.AddSpike(
                        new Vector3(
                            -1.2f + i * (2.4f / (crown - 1)),
                            3.65f, 0f),
                        biome == 2 ? .19f : .15f,
                        biome == 4 ? .35f : .55f,
                        biome == 2 ? 5 : 6);
                if (biome == 4)
                    builder.AddRing(
                        new Vector3(0f, 3.05f, -.35f),
                        .55f, .78f, .16f, 18);
            }
        }

        private static void BuildColumn(MeshBuilder builder, int biome)
        {
            if (biome == 1)
            {
                builder.AddSpike(Vector3.zero, .85f, 3.8f, 6);
                builder.AddRing(Vector3.up * 1.6f, .55f, .8f, .12f, 20);
                return;
            }
            if (biome == 2)
            {
                for (int i = 0; i < 4; i++)
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(i * Mathf.PI * .5f) * .3f,
                        0f,
                        Mathf.Sin(i * Mathf.PI * .5f) * .3f),
                        .28f, 3.25f, 5);
                builder.AddPrism(Vector3.up * .28f, .8f, .55f, 10, .58f);
                return;
            }
            if (biome == 3)
            {
                builder.AddPrism(
                    Vector3.up * 1.65f, .58f, 3.3f, 7, .2f);
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * 2.2f;
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * .55f,
                        1.2f + i * .28f,
                        Mathf.Sin(angle) * .55f), .12f, 1.1f, 5);
                }
                return;
            }
            builder.AddPrism(new Vector3(0f, .18f, 0f),
                biome == 4 ? .86f : .75f, .36f,
                biome == 4 ? 12 : 10, .68f);
            builder.AddPrism(new Vector3(0f, 1.65f, 0f),
                biome == 4 ? .62f : .48f, 2.75f,
                biome == 4 ? 12 : 10, .36f);
            builder.AddPrism(new Vector3(0f, 3.08f, 0f),
                biome == 4 ? .88f : .7f, .35f,
                biome == 4 ? 12 : 10, .62f);
            if (biome == 4)
                for (int i = 0; i < 3; i++)
                    builder.AddRing(
                        Vector3.up * (.85f + i * .8f),
                        .66f, .82f, .15f, 20);
        }

        private static void BuildHazard(MeshBuilder builder, int biome)
        {
            int segments = biome == 1 || biome == 4 ? 28 : 20;
            builder.AddRing(Vector3.up * .05f,
                biome == 3 ? .85f : 1.2f,
                biome == 3 ? 1.8f : 1.75f,
                .1f, segments);
            int count = biome == 2 ? 12 : biome == 3 ? 7 : 8;
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                float radius = biome == 3
                    ? 1.1f + i % 2 * .45f
                    : 1.45f;
                builder.AddSpike(new Vector3(
                    Mathf.Cos(angle) * radius, .16f,
                    Mathf.Sin(angle) * radius),
                    biome == 1 ? .2f : .16f,
                    biome == 4 ? .32f : .5f,
                    biome == 2 ? 5 : biome == 1 ? 6 : 7);
            }
            if (biome == 4)
                for (int i = -2; i <= 2; i++)
                    builder.AddBox(
                        new Vector3(i * .55f, .12f, 0f),
                        new Vector3(.12f, .08f, 2.3f));
        }

        private static void BuildLandmark(MeshBuilder builder, int biome)
        {
            if (biome == 0)
            {
                builder.AddPrism(Vector3.up * 1.8f, 1.3f, 2.7f, 16, .75f);
                builder.AddRing(Vector3.up * .55f, .7f, 1.25f, .35f, 20);
                builder.AddBox(Vector3.up * 3.4f, new Vector3(.18f, 1.6f, .18f));
            }
            else if (biome == 1)
            {
                builder.AddPrism(Vector3.up * .75f, .45f, 1.5f, 10, .35f);
                builder.AddRing(Vector3.up * 1.8f, 1.25f, 1.38f, .12f, 24);
                builder.AddRing(Vector3.up * 1.8f, .72f, .84f, .12f, 20);
                builder.AddSpike(Vector3.up * 1.8f, .45f, 2.6f, 8);
            }
            else if (biome == 2)
            {
                builder.AddPrism(Vector3.up * .6f, 1.35f, 1.2f, 12, .9f);
                for (int i = 0; i < 10; i++)
                {
                    float angle = i * Mathf.PI * .2f;
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * 1.15f, 1.1f,
                        Mathf.Sin(angle) * 1.15f), .18f, 1.5f, 6);
                }
            }
            else if (biome == 3)
            {
                builder.AddPrism(Vector3.up * 1.7f, .75f, 3.4f, 9, .38f);
                for (int i = 0; i < 7; i++)
                {
                    float angle = i * 2.1f;
                    builder.AddBox(new Vector3(
                        Mathf.Cos(angle) * .8f, 2.7f + i % 2 * .25f,
                        Mathf.Sin(angle) * .8f),
                        new Vector3(.25f, 1.8f, .25f));
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * 1.2f, 3.65f,
                        Mathf.Sin(angle) * 1.2f), .35f, .9f, 7);
                }
            }
            else
            {
                builder.AddPrism(Vector3.up * 1.5f, .85f, 3f, 12, .65f);
                for (int y = 0; y < 4; y++)
                    builder.AddRing(
                        Vector3.up * (.65f + y * .7f),
                        .92f, 1.12f, .18f, 24);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI * .25f;
                    builder.AddBox(new Vector3(
                        Mathf.Cos(angle) * 1.45f, 1.5f,
                        Mathf.Sin(angle) * 1.45f),
                        new Vector3(.2f, 2.5f, .2f));
                }
            }
        }

        private static void BuildDecoration(MeshBuilder builder, int biome)
        {
            int count = 5 + biome;
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.399f;
                Vector3 center = new Vector3(
                    Mathf.Cos(angle) * .65f,
                    .35f + i % 3 * .15f,
                    Mathf.Sin(angle) * .65f);
                if (biome == 1 || biome == 4)
                    builder.AddSpike(center, .22f, 1.1f + i * .08f, 6);
                else if (biome == 3)
                    builder.AddPrism(center, .24f, .9f, 7, .1f);
                else
                    builder.AddBox(center, new Vector3(
                        .35f + i % 2 * .15f,
                        .55f + i % 3 * .2f,
                        .35f));
            }
        }

        private static GameObject[] CreateCharacterPrefabs(
            Material[][] materials)
        {
            string[] classes =
            {
                "Vanguard", "Wayfinder", "Cantor", "Gloamstep", "Artificer"
            };
            var result = new GameObject[5];
            for (int index = 0; index < 5; index++)
            {
                var builder = new MeshBuilder();
                builder.AddPrism(Vector3.up * 1.15f,
                    index == 0 ? .62f : .48f, 1.55f, 10,
                    index == 3 ? .26f : .38f);
                builder.AddPrism(Vector3.up * 2.15f, .38f, .55f, 8, .28f);
                builder.AddSpike(
                    new Vector3(index % 2 == 0 ? .72f : -.72f, 1.35f, 0f),
                    .12f, index == 0 ? 2.4f : 1.7f, 6);
                for (int shard = 0; shard < 4 + index; shard++)
                {
                    float angle = shard * Mathf.PI * 2f / (4 + index);
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * .55f, .65f,
                        Mathf.Sin(angle) * .55f), .12f, .65f, 5);
                }
                Mesh mesh = SaveMesh(
                    builder.Create($"Bearer {classes[index]}"),
                    $"Character_{classes[index]}");
                result[index] = SaveSimplePrefab(
                    $"LF_Character_{classes[index]}",
                    mesh,
                    materials[index][2],
                    CharacterRoot + $"LF_Character_{classes[index]}.prefab");
            }
            return result;
        }

        private static GameObject[] CreateDisplayActorPrefabs(
            string folder,
            string role,
            Material[][] materials,
            bool guardian)
        {
            var result = new GameObject[5];
            for (int biome = 0; biome < 5; biome++)
            {
                var builder = new MeshBuilder();
                float bodyRadius = guardian ? 1.05f : .62f;
                float bodyHeight = guardian ? 2.7f : 1.55f;
                builder.AddPrism(
                    Vector3.up * bodyHeight * .55f,
                    bodyRadius, bodyHeight, guardian ? 12 : 9,
                    bodyRadius * .65f);
                builder.AddPrism(
                    Vector3.up * (bodyHeight + .35f),
                    bodyRadius * .55f, bodyRadius * .8f,
                    8, bodyRadius * .38f);
                int features = guardian ? 10 : 5;
                for (int i = 0; i < features; i++)
                {
                    float angle = i * Mathf.PI * 2f / features;
                    builder.AddSpike(new Vector3(
                        Mathf.Cos(angle) * bodyRadius,
                        bodyHeight * (.45f + (i % 3) * .16f),
                        Mathf.Sin(angle) * bodyRadius),
                        guardian ? .2f : .12f,
                        guardian ? 1.35f : .7f,
                        6);
                }
                if (biome == 4)
                    builder.AddRing(
                        Vector3.up * (bodyHeight * .7f),
                        bodyRadius * 1.05f,
                        bodyRadius * 1.25f,
                        .16f,
                        20);
                Mesh mesh = SaveMesh(
                    builder.Create($"{BiomeNames[biome]} {role}"),
                    $"{role}_{BiomeSlugs[biome]}");
                result[biome] = SaveSimplePrefab(
                    $"LF_{role}_{BiomeSlugs[biome]}",
                    mesh,
                    materials[biome][guardian ? 3 : 2],
                    folder + $"LF_{role}_{BiomeSlugs[biome]}.prefab");
            }
            return result;
        }

        private static Mesh SaveMesh(Mesh source, string name)
        {
            string path = MeshRoot + name + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = source;
                AssetDatabase.CreateAsset(mesh, path);
            }
            else
            {
                EditorUtility.CopySerialized(source, mesh);
                UnityEngine.Object.DestroyImmediate(source);
            }
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static GameObject SavePrefab(
            int biome,
            ProductionAssetKind kind,
            Mesh mesh,
            Material material,
            string path)
        {
            GameObject source = BuildPrefabSource(
                $"LF_{BiomeSlugs[biome]}_{kind}_A", mesh, material);
            source.AddComponent<ProductionAssetMarker>().Configure(
                BiomeIds[biome], kind, mesh.triangles.Length / 3);
            if (kind == ProductionAssetKind.Lantern)
            {
                GameObject lightObject = new GameObject("Lantern Light");
                lightObject.transform.SetParent(source.transform, false);
                lightObject.transform.localPosition = Vector3.up * 1.25f;
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Accents[biome];
                light.range = 6f;
                light.intensity = 2.8f;
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            UnityEngine.Object.DestroyImmediate(source);
            return prefab;
        }

        private static GameObject SaveSimplePrefab(
            string name, Mesh mesh, Material material, string path)
        {
            GameObject source = BuildPrefabSource(name, mesh, material);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            UnityEngine.Object.DestroyImmediate(source);
            return prefab;
        }

        private static GameObject BuildPrefabSource(
            string name, Mesh mesh, Material material)
        {
            var source = new GameObject(name);
            source.AddComponent<MeshFilter>().sharedMesh = mesh;
            source.AddComponent<MeshRenderer>().sharedMaterial = material;
            BoxCollider collider = source.AddComponent<BoxCollider>();
            collider.center = mesh.bounds.center;
            collider.size = mesh.bounds.size;
            return source;
        }

        private static ProductionArtCatalog CreateCatalog(
            Material[][] materials,
            GameObject[][] prefabs)
        {
            ProductionArtCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ProductionArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ProductionArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var profiles = new ProductionBiomeArtProfile[5];
            for (int biome = 0; biome < 5; biome++)
            {
                profiles[biome] = new ProductionBiomeArtProfile(
                    BiomeIds[biome],
                    BiomeNames[biome],
                    Stories[biome],
                    materials[biome][0],
                    materials[biome][1],
                    materials[biome][2],
                    materials[biome][3],
                    materials[biome][4],
                    prefabs[biome],
                    FogColors[biome],
                    Color.Lerp(FogColors[biome], Accents[biome], .2f),
                    Color.Lerp(Color.white, Accents[biome], .45f),
                    Accents[biome],
                    .018f + biome * .0015f,
                    1.25f + biome * .08f);
            }
            catalog.Configure(profiles);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void BuildReviewScene(
            int biome,
            string scenePath,
            ProductionBiomeArtProfile profile,
            GameObject character,
            GameObject enemy,
            GameObject guardian,
            string nextScene)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureGlobalAtmosphere(profile);
            BuildPostProcessing(biome, profile);
            BuildLighting(profile);
            BuildParticles(biome, profile);

            GameObject[] prefabs = new GameObject[profile.ModularPrefabs.Count];
            for (int i = 0; i < prefabs.Length; i++)
                prefabs[i] = profile.ModularPrefabs[i];
            BuildBiomeComposition(biome, prefabs);

            Vector3[][] hazardPositions =
            {
                new[]
                {
                    new Vector3(-3.2f, .08f, -1.2f),
                    new Vector3(3.2f, .08f, -1.2f),
                    new Vector3(-3.2f, .08f, 4.5f),
                    new Vector3(3.2f, .08f, 4.5f)
                },
                new[]
                {
                    new Vector3(-3.7f, .08f, 2.2f),
                    new Vector3(2.1f, .08f, -2.8f),
                    new Vector3(3.8f, .08f, 2f)
                },
                new[]
                {
                    new Vector3(-1.55f, .08f, -1.7f),
                    new Vector3(1.55f, .08f, .8f),
                    new Vector3(-1.55f, .08f, 3.3f),
                    new Vector3(1.55f, .08f, 5.8f)
                },
                new[]
                {
                    new Vector3(-2.8f, .08f, -1.4f),
                    new Vector3(3.6f, .08f, 1.8f),
                    new Vector3(-.4f, .08f, 4.7f)
                },
                new[]
                {
                    new Vector3(-4.4f, .08f, -.8f),
                    new Vector3(0f, .08f, 1.7f),
                    new Vector3(4.4f, .08f, -.8f),
                    new Vector3(0f, .08f, 5.2f)
                }
            };
            for (int index = 0;
                 index < hazardPositions[biome].Length;
                 index++)
            {
                GameObject hazard = Place(
                    prefabs[(int)ProductionAssetKind.Hazard],
                    hazardPositions[biome][index],
                    index * 37f + biome * 11f);
                hazard.name =
                    $"{BiomeNames[biome]} Traversal Hazard {index + 1}";
                hazard.GetComponent<Collider>().isTrigger = true;
                hazard.AddComponent<BiomeHazard>().Configure(
                    5f, ElementFor(biome));
            }

            Vector3[] playerSpawns =
            {
                new Vector3(0f, 1f, -5.2f),
                new Vector3(0f, 1f, -5.4f),
                new Vector3(0f, 1f, -5.2f),
                new Vector3(-2.9f, 1f, -4.3f),
                new Vector3(0f, 1f, -5.1f)
            };
            GameObject player = BuildPlayer(character, playerSpawns[biome]);
            GameObject displayEnemy =
                (GameObject)PrefabUtility.InstantiatePrefab(enemy);
            displayEnemy.name = "Enemy Silhouette Display";
            displayEnemy.transform.position = new Vector3(-4.1f, .2f, 1.6f);
            displayEnemy.transform.rotation = Quaternion.Euler(0f, 145f, 0f);
            displayEnemy.AddComponent<ActorPresentation>().Configure(
                Accents[biome], .85f, false);
            GameObject displayGuardian =
                (GameObject)PrefabUtility.InstantiatePrefab(guardian);
            displayGuardian.name = "Guardian Silhouette Display";
            displayGuardian.transform.position = new Vector3(4.1f, .2f, 1.6f);
            displayGuardian.transform.rotation = Quaternion.Euler(0f, -145f, 0f);
            displayGuardian.AddComponent<ActorPresentation>().Configure(
                Accents[biome], 1.2f, true);
            BossAttackPattern[] showcasePatterns =
            {
                BossAttackPattern.BellShockwave,
                BossAttackPattern.PrismCharge,
                BossAttackPattern.LanternRain,
                BossAttackPattern.RootSummon,
                BossAttackPattern.StonePillars
            };
            displayGuardian.AddComponent<GuardianSpectaclePresenter>();
            displayGuardian.AddComponent<GuardianReviewShowcase>().Configure(
                showcasePatterns[biome],
                Accents[biome]);

            AddWorldLabel(
                new Vector3(0f, 4.7f, 5.7f),
                $"{profile.DisplayName}\n{profile.EnvironmentalStory}",
                52, .065f);
            AddWorldLabel(
                new Vector3(-4.1f, 3.25f, 1.6f),
                "ENEMY SILHOUETTE", 38, .05f);
            AddWorldLabel(
                new Vector3(4.1f, 4.25f, 1.6f),
                "GUARDIAN SILHOUETTE", 38, .05f);

            Vector3[] exitPositions =
            {
                new Vector3(0f, 0f, 9f),
                new Vector3(0f, 0f, 7.5f),
                new Vector3(0f, 0f, 9f),
                new Vector3(1.5f, 0f, 7.7f),
                new Vector3(0f, 0f, 9f)
            };
            float[] exitYaw = { 0f, 0f, 0f, -12f, 0f };
            GameObject portal = Place(
                prefabs[(int)ProductionAssetKind.Gate],
                exitPositions[biome], exitYaw[biome]);
            portal.name = "Review Exit Portal";
            portal.GetComponent<Collider>().isTrigger = true;
            portal.AddComponent<PresentationReviewPortal>().Configure(
                nextScene,
                nextScene == "LanternfallHub"
                    ? "Return to the Lantern Court."
                    : "Enter the next remembered realm.");
            AddWorldLabel(
                exitPositions[biome] + Vector3.up * 3.8f,
                nextScene == "LanternfallHub"
                    ? "RETURN TO HUB"
                    : "NEXT BIOME REVIEW",
                42, .055f);

            BuildCamera(player.transform, biome != 0);
            BuildExperienceSystems(biome);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void BuildBiomeComposition(
            int biome, GameObject[] prefabs)
        {
            void Module(ProductionAssetKind kind, float x, float z, float yaw = 0f) =>
                Place(prefabs[(int)kind], new Vector3(x, 0f, z), yaw);
            void Floor(float x, float z, float yaw = 0f) =>
                Module(ProductionAssetKind.Floor, x, z, yaw);

            Module(ProductionAssetKind.Arena, 0f, -.5f);
            switch (biome)
            {
                case 0: // Processional drowned nave: ordered aisle and broken transept.
                    for (int z = -2; z <= 2; z++) Floor(0f, z * 4f);
                    for (int z = -1; z <= 1; z++)
                    {
                        Floor(-4f, z * 4f);
                        Floor(4f, z * 4f);
                    }
                    Floor(-8f, 4f);
                    Floor(8f, 4f);
                    Module(ProductionAssetKind.Wall, -4f, 8f);
                    Module(ProductionAssetKind.Wall, 4f, 8f);
                    Module(ProductionAssetKind.Corner, -6f, 6f, 90f);
                    Module(ProductionAssetKind.Corner, 6f, 6f, 270f);
                    for (int z = -2; z <= 5; z += 3)
                    {
                        Module(ProductionAssetKind.Column, -4.8f, z);
                        Module(ProductionAssetKind.Column, 4.8f, z);
                    }
                    Module(ProductionAssetKind.Landmark, 0f, 4.5f);
                    Module(ProductionAssetKind.Decoration, -2.8f, 6f, 20f);
                    Module(ProductionAssetKind.Decoration, 3.1f, 5.2f, -25f);
                    Module(ProductionAssetKind.Breakable, -4.6f, -4f, 12f);
                    Module(ProductionAssetKind.Breakable, 4.5f, -3.4f, 75f);
                    Module(ProductionAssetKind.Lantern, -3.1f, 1.8f);
                    Module(ProductionAssetKind.Lantern, 3.1f, 1.8f);
                    break;
                case 1: // Radial observatory: rotated plates orbit a fractured orrery.
                    Floor(0f, 0f);
                    Floor(0f, -8f);
                    for (int spoke = 0; spoke < 8; spoke++)
                    {
                        float angle = spoke * 45f;
                        float radians = angle * Mathf.Deg2Rad;
                        Floor(Mathf.Sin(radians) * 4.3f,
                            Mathf.Cos(radians) * 4.3f, angle);
                    }
                    Module(ProductionAssetKind.Landmark, 0f, 1.4f, 18f);
                    for (int spoke = 0; spoke < 4; spoke++)
                    {
                        float angle = spoke * 90f + 45f;
                        float radians = angle * Mathf.Deg2Rad;
                        Module(ProductionAssetKind.Column,
                            Mathf.Sin(radians) * 5.6f,
                            Mathf.Cos(radians) * 5.6f, -angle);
                    }
                    Module(ProductionAssetKind.Wall, -5.5f, 5.5f, 45f);
                    Module(ProductionAssetKind.Wall, 5.5f, 5.5f, -45f);
                    Module(ProductionAssetKind.Corner, -6f, -1f, 45f);
                    Module(ProductionAssetKind.Corner, 6f, -1f, -45f);
                    Module(ProductionAssetKind.Decoration, -3.2f, 2.8f, 35f);
                    Module(ProductionAssetKind.Decoration, 3.4f, -2.7f, -70f);
                    Module(ProductionAssetKind.Breakable, -5.2f, -4.3f);
                    Module(ProductionAssetKind.Lantern, 5.1f, 4.5f);
                    break;
                case 2: // Ossuary furnace: long iron trench with a dangerous centerline.
                    for (int z = -2; z <= 2; z++)
                    {
                        Floor(-2f, z * 4f);
                        Floor(2f, z * 4f);
                    }
                    Floor(-6f, 0f);
                    Floor(6f, 0f);
                    Floor(-6f, 4f);
                    Floor(6f, 4f);
                    Module(ProductionAssetKind.Wall, -6f, 2f, 90f);
                    Module(ProductionAssetKind.Wall, 6f, 2f, 270f);
                    Module(ProductionAssetKind.Corner, -6f, 7f, 90f);
                    Module(ProductionAssetKind.Corner, 6f, 7f, 180f);
                    Module(ProductionAssetKind.Landmark, 0f, 6.2f);
                    Module(ProductionAssetKind.Column, -5.1f, -2.2f);
                    Module(ProductionAssetKind.Column, 5.1f, 4.2f);
                    Module(ProductionAssetKind.Decoration, -4.2f, 4.8f, 90f);
                    Module(ProductionAssetKind.Decoration, 4.4f, -.8f, -90f);
                    for (int z = -3; z <= 4; z += 3)
                        Module(ProductionAssetKind.Breakable, -3.8f, z, z * 13f);
                    Module(ProductionAssetKind.Lantern, -2.3f, 6f);
                    Module(ProductionAssetKind.Lantern, 2.3f, 6f);
                    break;
                case 3: // Orchard clearing: asymmetric, encroaching roots and shrine clusters.
                    Floor(-3.4f, -7.5f, 18f);
                    Floor(0f, 0f, 8f);
                    Floor(-3.8f, .4f, -12f);
                    Floor(3.9f, -.5f, 17f);
                    Floor(-3.2f, 4.1f, 24f);
                    Floor(1f, 4.2f, -18f);
                    Floor(4.6f, 4f, 10f);
                    Floor(-1.6f, -4f, -9f);
                    Floor(3.2f, -3.8f, 21f);
                    Module(ProductionAssetKind.Wall, -5f, 5f, 34f);
                    Module(ProductionAssetKind.Wall, 5.7f, 3.8f, -28f);
                    Module(ProductionAssetKind.Corner, -6f, -.6f, 62f);
                    Module(ProductionAssetKind.Column, -4f, 1.8f, 18f);
                    Module(ProductionAssetKind.Column, 4.8f, -1.7f, -25f);
                    Module(ProductionAssetKind.Landmark, -1f, 3.4f, -15f);
                    Module(ProductionAssetKind.Decoration, -4.4f, -2.8f, 44f);
                    Module(ProductionAssetKind.Decoration, 3.7f, 3.7f, -36f);
                    Module(ProductionAssetKind.Decoration, 5f, .8f, 80f);
                    Module(ProductionAssetKind.Breakable, -2.5f, -4.4f, 22f);
                    Module(ProductionAssetKind.Lantern, 2.2f, 4.7f);
                    break;
                default: // Foundry: hard grid, paired pylons and machine-gate axis.
                    for (int z = -1; z <= 1; z++)
                    for (int x = -2; x <= 2; x++) Floor(x * 4f, z * 4f);
                    for (int x = -1; x <= 1; x++)
                    {
                        Floor(x * 4f, -8f);
                        Floor(x * 4f, 8f);
                    }
                    Module(ProductionAssetKind.Gate, 0f, -5.8f, 180f);
                    Module(ProductionAssetKind.Wall, -6f, 2f, 90f);
                    Module(ProductionAssetKind.Wall, 6f, 2f, 270f);
                    Module(ProductionAssetKind.Corner, -6f, 7f, 90f);
                    Module(ProductionAssetKind.Corner, 6f, 7f, 180f);
                    for (int z = -1; z <= 6; z += 3)
                    {
                        Module(ProductionAssetKind.Column, -4.7f, z);
                        Module(ProductionAssetKind.Column, 4.7f, z);
                    }
                    Module(ProductionAssetKind.Landmark, 0f, 5.4f);
                    Module(ProductionAssetKind.Decoration, -3.6f, 4.2f, 90f);
                    Module(ProductionAssetKind.Decoration, 3.6f, 4.2f, -90f);
                    Module(ProductionAssetKind.Breakable, -4.2f, -3.8f);
                    Module(ProductionAssetKind.Breakable, 4.2f, -3.8f);
                    Module(ProductionAssetKind.Lantern, -2.7f, 2.3f);
                    Module(ProductionAssetKind.Lantern, 2.7f, 2.3f);
                    break;
            }
        }

        private static GameObject BuildPlayer(
            GameObject characterPrefab, Vector3 spawn)
        {
            var player = new GameObject("Bearer");
            player.transform.position = spawn;
            CharacterController controller =
                player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = .48f;
            player.AddComponent<PlayerInputReader>();
            player.AddComponent<PlayerMotor>();
            player.AddComponent<Health>().Configure(180f, 5f, false);
            player.AddComponent<RunInventory>();
            player.AddComponent<GameHud>();
            GameObject model =
                (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
            model.name = "Production Bearer Model";
            model.transform.SetParent(player.transform, false);
            model.transform.localPosition = Vector3.down;
            return player;
        }

        private static void BuildCamera(
            Transform target, bool renderPostProcessing)
        {
            var cameraObject = new GameObject("Presentation Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera =
                cameraObject.AddComponent<UnityEngine.Camera>();
            UniversalAdditionalCameraData cameraData =
                cameraObject.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
                cameraData =
                    cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = renderPostProcessing;
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.008f, .012f, .02f);
            cameraObject.AddComponent<AudioListener>();
            IsometricCameraRig rig =
                cameraObject.AddComponent<IsometricCameraRig>();
            rig.SetTarget(target);
            // Review scenes use a gentle forward look-ahead so the player
            // remains in the lower third while the biome landmark frames the
            // route ahead through the real gameplay camera.
            rig.ConfigureView(new Vector3(0f, 13f, -9f), 9.3f);
            cameraObject.transform.position =
                target.position + new Vector3(0f, 13f, -9f);
            cameraObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
        }

        private static void BuildExperienceSystems(int biome)
        {
            var systems = new GameObject("Production Presentation Systems");
            systems.AddComponent<FrameBudgetMonitor>();
            systems.AddComponent<GameplayAudioDirector>();
            systems.AddComponent<CombatVfxDirector>();
            systems.AddComponent<DynamicAudioDirector>();
            systems.AddComponent<BiomeAmbientAudio>().Configure(biome);
        }

        private static void BuildLighting(ProductionBiomeArtProfile profile)
        {
            var keyObject = new GameObject("Realm Key Light");
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = profile.KeyLightColor;
            key.intensity = profile.KeyLightIntensity;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = .72f;
            keyObject.transform.rotation = Quaternion.Euler(52f, -38f, 0f);
            RenderSettings.sun = key;

            var rimObject = new GameObject("Lantern Rim Light");
            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Point;
            rim.color = profile.ParticleColor;
            rim.intensity = 4f;
            rim.range = 12f;
            rim.shadows = LightShadows.None;
            rimObject.transform.position = new Vector3(0f, 4f, -1f);
        }

        private static void ConfigureGlobalAtmosphere(
            ProductionBiomeArtProfile profile)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = profile.FogColor;
            RenderSettings.fogDensity = profile.FogDensity;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor =
                Color.Lerp(profile.AmbientColor, Color.black, .1f);
            RenderSettings.ambientEquatorColor =
                Color.Lerp(profile.AmbientColor, Color.black, .5f);
            RenderSettings.ambientGroundColor =
                Color.Lerp(profile.FogColor, Color.black, .72f);
            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode =
                DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;
            RenderSettings.reflectionIntensity = .18f;
            RenderSettings.reflectionBounces = 1;
        }

        private static void BuildPostProcessing(
            int biome, ProductionBiomeArtProfile profile)
        {
            string path = Root +
                $"VFX/{BiomeSlugs[biome]}_Volume.asset";
            VolumeProfile volume =
                AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (volume == null)
            {
                volume = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(volume, path);
            }
            UnityEngine.Object[] subAssets =
                AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (UnityEngine.Object subAsset in subAssets)
                if (subAsset is VolumeComponent)
                    UnityEngine.Object.DestroyImmediate(subAsset, true);
            volume.components.Clear();
            // URP 17's bloom path on this D3D device folds the Drowned
            // cyan HDR emission toward violet. The controlled no-post capture
            // proves the material inputs are correct, so Drowned uses emissive
            // geometry and local lights without bloom.
            if (biome != 0)
            {
                Bloom bloom = AddVolumeComponent<Bloom>(volume);
                bloom.active = true;
                bloom.intensity.Override(.32f + biome * .035f);
                bloom.threshold.Override(1.05f);
                bloom.scatter.Override(.52f);
            }
            Tonemapping tonemapping =
                AddVolumeComponent<Tonemapping>(volume);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);
            Vignette vignette = AddVolumeComponent<Vignette>(volume);
            vignette.active = true;
            vignette.intensity.Override(.24f);
            vignette.smoothness.Override(.58f);
            // The Drowned palette is graded through ACES, fog, and its
            // cyan/green light rig. URP 17's ColorAdjustments LUT folds this
            // biome's intense cyan emission toward violet, so it is
            // deliberately excluded rather than compensating with a tint.
            if (biome != 0)
            {
                ColorAdjustments color =
                    AddVolumeComponent<ColorAdjustments>(volume);
                color.active = true;
                color.postExposure.Override(.2f);
                color.contrast.Override(18f);
                color.saturation.Override(-6f);
                color.colorFilter.Override(
                    Color.Lerp(Color.white, profile.KeyLightColor, .16f));
            }
            EditorUtility.SetDirty(volume);

            GameObject volumeObject = new GameObject("Realm Color Grade");
            Volume component = volumeObject.AddComponent<Volume>();
            component.isGlobal = true;
            component.priority = 10f;
            component.sharedProfile = volume;
        }

        private static T AddVolumeComponent<T>(VolumeProfile profile)
            where T : VolumeComponent
        {
            T component = profile.Add<T>();
            AssetDatabase.AddObjectToAsset(component, profile);
            EditorUtility.SetDirty(component);
            return component;
        }

        private static void BuildParticles(
            int biome, ProductionBiomeArtProfile profile)
        {
            GameObject particleObject = new GameObject("Realm Motif Particles");
            ParticleSystem particles =
                particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            float[] lifetimes = { 2.8f, 6.5f, 2.2f, 7f, .7f };
            float[] speeds = { .6f, .12f, 1.25f, .08f, 2.6f };
            float[] sizes = { .055f, .09f, .075f, .14f, .045f };
            int[] maximums = { 140, 180, 170, 150, 90 };
            main.startLifetime = lifetimes[biome];
            main.startSpeed = speeds[biome];
            main.startSize = sizes[biome];
            main.maxParticles = maximums[biome];
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = profile.ParticleColor;
            main.gravityModifier = biome == 0 ? .18f :
                biome == 2 ? -.08f : biome == 3 ? .015f : 0f;
            ParticleSystem.EmissionModule emission = particles.emission;
            float[] rates = { 24f, 16f, 34f, 12f, 10f };
            emission.rateOverTime = rates[biome];
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 5f, 14f);
            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = biome == 1 || biome == 3;
            noise.strength = biome == 3 ? .42f : .16f;
            noise.frequency = biome == 3 ? .2f : .08f;
            ParticleSystem.VelocityOverLifetimeModule velocity =
                particles.velocityOverLifetime;
            velocity.enabled = biome == 0 || biome == 2;
            velocity.y = biome == 0 ? -1.1f : 1.4f;
            particleObject.transform.position = Vector3.up * 2.5f;
            ParticleSystemRenderer renderer =
                particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode =
                biome == 0 || biome == 2 || biome == 4
                    ? ParticleSystemRenderMode.Stretch
                    : ParticleSystemRenderMode.Billboard;
            renderer.velocityScale = biome == 4 ? .32f : .18f;
            renderer.lengthScale = biome == 0 ? .65f :
                biome == 2 ? .45f : .8f;
            renderer.sharedMaterial = CreateParticleMaterial(profile);
        }

        private static Material CreateParticleMaterial(
            ProductionBiomeArtProfile profile)
        {
            string path = MaterialRoot +
                profile.StableId.Replace('.', '_') + "_Particles.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(
                    Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                VfxRoot + "Lanternfall_Particle.png");
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", profile.ParticleColor);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AddReviewPortalToHub(
            GameObject gatePrefab, Material material)
        {
            Scene scene = EditorSceneManager.OpenScene(
                "Assets/_Project/Lanternfall/Scenes/LanternfallHub.unity",
                OpenSceneMode.Single);
            GameObject old = GameObject.Find("Production Art Review Portal");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            GameObject portal = (GameObject)PrefabUtility.InstantiatePrefab(
                gatePrefab);
            portal.name = "Production Art Review Portal";
            portal.transform.position = new Vector3(10f, 0f, 5.5f);
            portal.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            portal.GetComponentInChildren<Renderer>().sharedMaterial = material;
            portal.GetComponent<Collider>().isTrigger = true;
            portal.AddComponent<PresentationReviewPortal>().Configure(
                $"ArtReview_{BiomeSlugs[0]}",
                "Enter the five-biome production art review.");
            AddWorldLabel(
                new Vector3(10f, 3.8f, 5.5f),
                "PRODUCTION ART REVIEW", 42, .055f);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CaptureBefore(int biome)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Material flat = new Material(
                Shader.Find("Universal Render Pipeline/Lit"));
            flat.color = Color.Lerp(Color.gray, Tints[biome], .25f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -.5f, 0f);
            floor.transform.localScale = new Vector3(14f, 1f, 12f);
            floor.GetComponent<Renderer>().sharedMaterial = flat;
            for (int index = 0; index < 12; index++)
            {
                float angle = index * Mathf.PI * 2f / 12f;
                GameObject prop = GameObject.CreatePrimitive(
                    index % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                prop.transform.position = new Vector3(
                    Mathf.Cos(angle) * 5f, 1f,
                    Mathf.Sin(angle) * 4f);
                prop.transform.localScale =
                    new Vector3(.65f, 2f + index % 3, .65f);
                prop.GetComponent<Renderer>().sharedMaterial = flat;
            }
            var lightObject = new GameObject("Flat Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            UnityEngine.Camera camera = CreateCaptureCamera();
            CaptureCamera(
                camera,
                ComparisonRoot + $"Before_{BiomeSlugs[biome]}.png");
            UnityEngine.Object.DestroyImmediate(flat);
        }

        private static void CaptureScene(string scenePath, string output)
        {
            Scene scene = EditorSceneManager.OpenScene(
                scenePath, OpenSceneMode.Single);
            UnityEngine.Camera camera =
                UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (camera == null) camera = CreateCaptureCamera();
            bool drowned = scenePath.IndexOf(
                "DrownedNarthex",
                StringComparison.OrdinalIgnoreCase) >= 0;
            UniversalAdditionalCameraData cameraData =
                camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
                cameraData.renderPostProcessing = !drowned;
            if (drowned)
            {
                GameObject grade = GameObject.Find("Realm Color Grade");
                if (grade != null) grade.SetActive(false);
            }
            GameObject bearer = GameObject.Find("Bearer");
            if (bearer != null)
            {
                camera.transform.position =
                    bearer.transform.position +
                    new Vector3(0f, 13f, -9f);
                camera.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            }
            camera.orthographic = true;
            camera.orthographicSize = 9.3f;
            CaptureCamera(camera, output);
        }

        private static UnityEngine.Camera CreateCaptureCamera()
        {
            GameObject cameraObject = new GameObject("Comparison Camera");
            UnityEngine.Camera camera =
                cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.015f, .018f, .025f);
            cameraObject.transform.position = new Vector3(0f, 12f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            return camera;
        }

        private static void CaptureCamera(
            UnityEngine.Camera camera, string output)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var render = new RenderTexture(1280, 720, 24);
            var image = new Texture2D(
                1280, 720, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            if (!render.Create())
                throw new BuildFailedException(
                    $"Could not create comparison render target for {output}.");
            camera.targetTexture = render;
            camera.Render();
            RenderTexture.active = render;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            File.WriteAllBytes(output, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            render.Release();
            UnityEngine.Object.DestroyImmediate(render);
            UnityEngine.Object.DestroyImmediate(image);
        }

        private static GameObject Place(
            GameObject prefab, Vector3 position, float yaw = 0f)
        {
            GameObject item =
                (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            item.transform.position = position;
            item.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return item;
        }

        private static void AddWorldLabel(
            Vector3 position, string copy, int fontSize, float characterSize)
        {
            GameObject labelObject = new GameObject("World Label");
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = copy;
            label.fontSize = fontSize;
            label.characterSize = characterSize;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = new Color(.95f, .82f, .55f);
        }

        private static DamageElement ElementFor(int biome) =>
            biome switch
            {
                0 => DamageElement.Frost,
                1 => DamageElement.Radiance,
                2 => DamageElement.Ember,
                3 => DamageElement.Gloam,
                _ => DamageElement.Storm
            };

        private static void ConfigureTexture(
            string path, TextureImporterType type, bool sRgb)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = type;
            importer.sRGBTexture = sRgb;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression =
                TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }

        private static void WritePng(
            string path, int size, Color[] pixels)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.RGBA32, false, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static float Luminance(Color color) =>
            color.r * .2126f + color.g * .7152f + color.b * .0722f;

        private static float AccentAffinity(Color color, int biome)
        {
            float value = Mathf.Max(color.r, color.g, color.b);
            if (value < .22f) return 0f;
            float affinity = biome switch
            {
                0 => Mathf.Min(color.g, color.b) - color.r * .75f,
                1 => color.b - color.r * .82f,
                2 => color.r - color.g * 1.25f,
                3 => color.g - color.r * 1.05f,
                _ => color.b - color.r * .9f
            };
            return Mathf.Clamp01(affinity * 3.2f);
        }

        private static float Hash01(int x, int y, int seed)
        {
            uint hash = (uint)(x * 374761393 + y * 668265263 +
                               seed * 1442695041);
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            return (hash & 0xffff) / 65535f;
        }

        private sealed class MeshBuilder
        {
            private readonly List<Vector3> _vertices = new List<Vector3>(2048);
            private readonly List<int> _triangles = new List<int>(4096);
            private readonly List<Vector2> _uv = new List<Vector2>(2048);

            public void AddBox(Vector3 center, Vector3 size)
            {
                Vector3 half = size * .5f;
                Vector3[] corners =
                {
                    center + new Vector3(-half.x, -half.y, -half.z),
                    center + new Vector3(half.x, -half.y, -half.z),
                    center + new Vector3(half.x, half.y, -half.z),
                    center + new Vector3(-half.x, half.y, -half.z),
                    center + new Vector3(-half.x, -half.y, half.z),
                    center + new Vector3(half.x, -half.y, half.z),
                    center + new Vector3(half.x, half.y, half.z),
                    center + new Vector3(-half.x, half.y, half.z)
                };
                int[,] faces =
                {
                    { 0, 2, 1, 0, 3, 2 },
                    { 5, 6, 4, 6, 7, 4 },
                    { 4, 7, 0, 7, 3, 0 },
                    { 1, 2, 5, 2, 6, 5 },
                    { 3, 7, 2, 7, 6, 2 },
                    { 4, 0, 5, 0, 1, 5 }
                };
                for (int face = 0; face < 6; face++)
                {
                    int start = _vertices.Count;
                    int[] unique =
                    {
                        faces[face, 0], faces[face, 1], faces[face, 2],
                        faces[face, 4]
                    };
                    foreach (int corner in unique) AddVertex(corners[corner]);
                    _triangles.Add(start);
                    _triangles.Add(start + 1);
                    _triangles.Add(start + 2);
                    _triangles.Add(start);
                    _triangles.Add(start + 2);
                    _triangles.Add(start + 3);
                }
            }

            public void AddPrism(
                Vector3 center,
                float bottomRadius,
                float height,
                int sides,
                float topRadius)
            {
                int bottomCenter = AddVertex(
                    center + Vector3.down * height * .5f);
                int topCenter = AddVertex(
                    center + Vector3.up * height * .5f);
                for (int side = 0; side < sides; side++)
                {
                    float a0 = side * Mathf.PI * 2f / sides;
                    float a1 = (side + 1) * Mathf.PI * 2f / sides;
                    Vector3 b0 = center + new Vector3(
                        Mathf.Cos(a0) * bottomRadius, -height * .5f,
                        Mathf.Sin(a0) * bottomRadius);
                    Vector3 b1 = center + new Vector3(
                        Mathf.Cos(a1) * bottomRadius, -height * .5f,
                        Mathf.Sin(a1) * bottomRadius);
                    Vector3 t0 = center + new Vector3(
                        Mathf.Cos(a0) * topRadius, height * .5f,
                        Mathf.Sin(a0) * topRadius);
                    Vector3 t1 = center + new Vector3(
                        Mathf.Cos(a1) * topRadius, height * .5f,
                        Mathf.Sin(a1) * topRadius);
                    int start = _vertices.Count;
                    AddVertex(b0); AddVertex(b1); AddVertex(t1); AddVertex(t0);
                    _triangles.Add(start);
                    _triangles.Add(start + 2);
                    _triangles.Add(start + 1);
                    _triangles.Add(start);
                    _triangles.Add(start + 3);
                    _triangles.Add(start + 2);
                    _triangles.Add(bottomCenter);
                    _triangles.Add(start + 1);
                    _triangles.Add(start);
                    _triangles.Add(topCenter);
                    _triangles.Add(start + 3);
                    _triangles.Add(start + 2);
                }
            }

            public void AddSpike(
                Vector3 baseCenter,
                float radius,
                float height,
                int sides) =>
                AddPrism(
                    baseCenter + Vector3.up * height * .5f,
                    radius, height, sides, 0f);

            public void AddRing(
                Vector3 center,
                float innerRadius,
                float outerRadius,
                float height,
                int segments)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    float a0 = segment * Mathf.PI * 2f / segments;
                    float a1 = (segment + 1) * Mathf.PI * 2f / segments;
                    Vector3 radial0 = new Vector3(
                        Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                    Vector3 radial1 = new Vector3(
                        Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                    Vector3 middle =
                        center + (radial0 + radial1).normalized *
                        ((innerRadius + outerRadius) * .5f);
                    float chord = 2f * outerRadius *
                        Mathf.Sin(Mathf.PI / segments);
                    AddBox(
                        middle,
                        new Vector3(
                            outerRadius - innerRadius,
                            height,
                            chord));
                }
            }

            public Mesh Create(string name)
            {
                var mesh = new Mesh { name = name };
                if (_vertices.Count > 65535)
                    mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(_vertices);
                mesh.SetTriangles(_triangles, 0);
                mesh.SetUVs(0, _uv);
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                mesh.RecalculateBounds();
                return mesh;
            }

            private int AddVertex(Vector3 vertex)
            {
                int index = _vertices.Count;
                _vertices.Add(vertex);
                _uv.Add(new Vector2(
                    vertex.x * .22f + vertex.z * .15f,
                    vertex.y * .22f + vertex.z * .08f));
                return index;
            }
        }
    }
}
