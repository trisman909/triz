using Lanternfall.Core.Random;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.World
{
    public sealed class BiomeChamberPresenter : MonoBehaviour
    {
        private Transform _root;
        public int BiomeIndex { get; private set; } = -1;
        public int GeneratedPropCount { get; private set; }
        public string ArchitectureName { get; private set; }

        public void Build(int biomeIndex, ulong seed, BiomeDefinition biome)
        {
            if (_root != null)
            {
                if (Application.isPlaying) Destroy(_root.gameObject);
                else DestroyImmediate(_root.gameObject);
            }
            BiomeIndex = Mathf.Clamp(biomeIndex, 0, 4);
            ArchitectureName = NameFor(BiomeIndex);
            _root = new GameObject($"{ArchitectureName} Architecture").transform;
            _root.SetParent(transform, false);
            var random = new DeterministicRandom(seed);
            GeneratedPropCount = 0;
            for (int index = 0; index < 12; index++)
            {
                float angle = (index + random.NextFloat() * .35f) *
                    Mathf.PI * 2f / 12f;
                float radius = 6.5f + random.NextFloat() * 3.8f;
                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * radius, 0f,
                    Mathf.Sin(angle) * radius);
                CreateProp(index, position, biome);
            }
            CreateHazard(-5f, 2.5f, biome);
            CreateHazard(5f, -1.5f, biome);
        }

        private void CreateProp(
            int index, Vector3 position, BiomeDefinition biome)
        {
            PrimitiveType shape = BiomeIndex switch
            {
                0 => PrimitiveType.Cylinder,
                1 => PrimitiveType.Cube,
                2 => PrimitiveType.Capsule,
                3 => index % 2 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Sphere,
                _ => index % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Cylinder
            };
            GameObject prop = GameObject.CreatePrimitive(shape);
            prop.name = $"{ArchitectureName} Prop {index + 1}";
            prop.transform.SetParent(_root);
            prop.transform.position = position + Vector3.up;
            float tall = 1.4f + (index % 4) * .45f;
            prop.transform.localScale = new Vector3(
                .55f + (index % 3) * .18f, tall, .55f);
            prop.transform.rotation =
                Quaternion.Euler(0f, index * 37f + BiomeIndex * 19f, 0f);
            SetColor(prop, Color.Lerp(
                biome.AmbientColor,
                Color.HSVToRGB((BiomeIndex * .19f + index * .025f) % 1f, .55f, .8f),
                .55f));
            GeneratedPropCount++;
        }

        private void CreateHazard(
            float x, float z, BiomeDefinition biome)
        {
            GameObject hazard =
                GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazard.name = $"{ArchitectureName} Hazard";
            hazard.transform.SetParent(_root);
            hazard.transform.position = new Vector3(x, .08f, z);
            hazard.transform.localScale = new Vector3(2.2f, .08f, 2.2f);
            DamageElement element = BiomeIndex switch
            {
                0 => DamageElement.Frost,
                1 => DamageElement.Radiance,
                2 => DamageElement.Ember,
                3 => DamageElement.Gloam,
                _ => DamageElement.Storm
            };
            hazard.AddComponent<BiomeHazard>().Configure(5f + BiomeIndex * 2f, element);
            SetColor(hazard, Color.Lerp(biome.FogColor, Color.white, .28f));
            GeneratedPropCount++;
        }

        private static void SetColor(GameObject item, Color color)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null) return;
            renderer.sharedMaterial = UrpPresentationMaterials.Lit;
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
        }

        private static string NameFor(int biome) => biome switch
        {
            0 => "Drowned Narthex",
            1 => "Siltglass Orrery",
            2 => "Ember Ossuary",
            3 => "Gloam Orchard",
            _ => "Stormvault Foundry"
        };
    }
}
