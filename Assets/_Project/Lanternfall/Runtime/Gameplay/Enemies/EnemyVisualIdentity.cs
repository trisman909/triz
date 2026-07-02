using UnityEngine;

namespace Lanternfall.Gameplay.Enemies
{
    /// <summary>
    /// Builds a stable silhouette/palette from authored ID and archetype.
    /// Collision remains on the shared brain prefab for balance consistency.
    /// </summary>
    public sealed class EnemyVisualIdentity : MonoBehaviour
    {
        public string IdentityKey { get; private set; }
        public int VisualVariant { get; private set; }
        public Color BaseColor { get; private set; }

        public void Configure(EnemyDefinition definition)
        {
            if (definition == null || IdentityKey == definition.StableId) return;
            IdentityKey = definition.StableId;
            uint hash = StableHash(definition.StableId);
            VisualVariant = (int)(hash % 5);
            BaseColor = Color.HSVToRGB(
                (hash % 997) / 997f,
                .48f + ((hash >> 8) % 30) / 100f,
                .65f + ((hash >> 16) % 25) / 100f);

            float width = .72f + (hash % 28) / 100f;
            float height = .8f + ((hash >> 5) % 45) / 100f;
            transform.localScale = new Vector3(width, height, width);
            if (transform.Find("Identity Ornament") == null)
                CreateOrnament(definition.Archetype, hash);
            ApplyColor();
        }

        private void CreateOrnament(EnemyArchetype archetype, uint hash)
        {
            PrimitiveType shape =
                archetype == EnemyArchetype.Flying ||
                archetype == EnemyArchetype.Explosive
                    ? PrimitiveType.Sphere :
                archetype == EnemyArchetype.Tank
                    ? PrimitiveType.Cube :
                PrimitiveType.Cylinder;
            GameObject ornament = GameObject.CreatePrimitive(shape);
            ornament.name = "Identity Ornament";
            ornament.transform.SetParent(transform, false);
            ornament.transform.localPosition = new Vector3(
                0f, .75f + (hash % 20) / 100f, 0f);
            ornament.transform.localScale =
                archetype == EnemyArchetype.Flying
                    ? new Vector3(1.7f, .16f, .45f)
                    : new Vector3(.55f, .3f + (hash % 25) / 100f, .55f);
            Collider collider = ornament.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Destroy(collider);
                else DestroyImmediate(collider);
            }
            Renderer root = GetComponentInChildren<Renderer>();
            Renderer child = ornament.GetComponent<Renderer>();
            if (root != null && child != null)
                child.sharedMaterial = root.sharedMaterial;
        }

        private void ApplyColor()
        {
            var properties = new MaterialPropertyBlock();
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.GetPropertyBlock(properties);
                properties.SetColor("_BaseColor", BaseColor);
                properties.SetColor("_Color", BaseColor);
                renderer.SetPropertyBlock(properties);
            }
        }

        private static uint StableHash(string value)
        {
            uint hash = 2166136261;
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 16777619;
            }
            return hash;
        }
    }
}
