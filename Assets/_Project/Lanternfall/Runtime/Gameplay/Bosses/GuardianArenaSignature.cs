using Lanternfall.Core.Random;
using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    /// <summary>
    /// Gives every guardian pattern an immediately readable arena vocabulary.
    /// It is presentation-only and cannot alter authoritative boss selection.
    /// </summary>
    public sealed class GuardianArenaSignature : MonoBehaviour
    {
        private Transform _root;

        public BossAttackPattern Pattern { get; private set; }
        public string SignatureName { get; private set; }
        public int GeneratedPropCount { get; private set; }

        public void Build(BossDefinition definition, ulong seed)
        {
            if (definition == null) return;
            Clear();
            Pattern = definition.Pattern;
            SignatureName = NameFor(Pattern);
            _root = new GameObject($"{SignatureName} Arena Signature").transform;
            _root.SetParent(transform, false);
            var random = new DeterministicRandom(seed);
            GeneratedPropCount = 6 + (int)Pattern % 3;

            for (int index = 0; index < GeneratedPropCount; index++)
            {
                float angle = index * Mathf.PI * 2f / GeneratedPropCount;
                float radius = 6f + random.NextFloat() * 1.25f;
                CreateMarker(index, angle, radius);
            }
        }

        private void CreateMarker(int index, float angle, float radius)
        {
            PrimitiveType shape = Pattern switch
            {
                BossAttackPattern.BellShockwave => PrimitiveType.Cylinder,
                BossAttackPattern.PrismCharge => PrimitiveType.Cube,
                BossAttackPattern.RootSummon => PrimitiveType.Capsule,
                BossAttackPattern.LanternRain => PrimitiveType.Sphere,
                BossAttackPattern.TidalSweep => PrimitiveType.Cube,
                BossAttackPattern.MirrorVolley => PrimitiveType.Cube,
                BossAttackPattern.TimeFracture => PrimitiveType.Cylinder,
                _ => PrimitiveType.Cube
            };
            GameObject marker = GameObject.CreatePrimitive(shape);
            marker.name = $"{SignatureName} {index + 1}";
            marker.transform.SetParent(_root);
            marker.transform.position = new Vector3(
                Mathf.Cos(angle) * radius,
                HeightFor(Pattern),
                Mathf.Sin(angle) * radius);
            marker.transform.rotation = Quaternion.Euler(
                Pattern == BossAttackPattern.MirrorVolley ? 35f : 0f,
                -angle * Mathf.Rad2Deg,
                Pattern == BossAttackPattern.TimeFracture ? 90f : 0f);
            marker.transform.localScale = ScaleFor(Pattern, index);
            SetColor(marker, ColorFor(Pattern, index));

            if (Pattern == BossAttackPattern.LanternRain ||
                Pattern == BossAttackPattern.TimeFracture)
            {
                Collider collider = marker.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying) Destroy(collider);
                    else DestroyImmediate(collider);
                }
            }
        }

        private void Clear()
        {
            if (_root == null) return;
            if (Application.isPlaying) Destroy(_root.gameObject);
            else DestroyImmediate(_root.gameObject);
            _root = null;
        }

        private static float HeightFor(BossAttackPattern pattern) =>
            pattern == BossAttackPattern.LanternRain ? 3f : .65f;

        private static Vector3 ScaleFor(
            BossAttackPattern pattern, int index)
        {
            switch (pattern)
            {
                case BossAttackPattern.BellShockwave:
                    return new Vector3(.45f, 1.4f, .45f);
                case BossAttackPattern.PrismCharge:
                    return new Vector3(.35f, .15f, 2.4f);
                case BossAttackPattern.RootSummon:
                    return new Vector3(.7f, .9f, .7f);
                case BossAttackPattern.LanternRain:
                    return Vector3.one * (.45f + index % 3 * .12f);
                case BossAttackPattern.TidalSweep:
                    return new Vector3(2.2f, .12f, .45f);
                case BossAttackPattern.MirrorVolley:
                    return new Vector3(.25f, 1.2f, .8f);
                case BossAttackPattern.TimeFracture:
                    return new Vector3(.12f, 2.2f, .12f);
                default:
                    return new Vector3(.8f, 1.8f, .8f);
            }
        }

        private static Color ColorFor(
            BossAttackPattern pattern, int index)
        {
            float hue = ((int)pattern * .113f + index * .018f) % 1f;
            return Color.HSVToRGB(hue, .72f, .9f);
        }

        private static void SetColor(GameObject item, Color color)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer == null) return;
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
        }

        private static string NameFor(BossAttackPattern pattern) =>
            pattern switch
            {
                BossAttackPattern.BellShockwave => "Resonant Belfry",
                BossAttackPattern.PrismCharge => "Prismatic Lanes",
                BossAttackPattern.RootSummon => "Rootbound Choir",
                BossAttackPattern.LanternRain => "Falling Constellation",
                BossAttackPattern.TidalSweep => "Tidal Causeway",
                BossAttackPattern.MirrorVolley => "Mirror Gallery",
                BossAttackPattern.TimeFracture => "Broken Dial",
                _ => "Pillar Court"
            };
    }
}
