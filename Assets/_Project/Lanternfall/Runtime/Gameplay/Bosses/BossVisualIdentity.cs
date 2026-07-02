using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    public sealed class BossVisualIdentity : MonoBehaviour
    {
        public string IdentityKey { get; private set; }
        public int CrownCount { get; private set; }
        public Color BaseColor { get; private set; }
        public float ScaleMultiplier { get; private set; } = 1f;

        public void Configure(BossDefinition definition)
        {
            if (definition == null || IdentityKey == definition.StableId) return;
            IdentityKey = definition.StableId;
            uint hash = StableHash(definition.StableId);
            CrownCount = 2 + (int)(hash % 5);
            BaseColor = Color.HSVToRGB(
                ((int)definition.Pattern * .11f + (hash % 997) / 997f) % 1f,
                .55f + ((hash >> 8) % 25) / 100f,
                .72f + ((hash >> 16) % 24) / 100f);
            ScaleMultiplier = 1.35f + (hash % 30) / 100f;
            transform.localScale = Vector3.one * ScaleMultiplier;
            if (transform.Find("Guardian Crown") == null) CreateCrown(hash);
            ApplyColor();
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

        private void CreateCrown(uint hash)
        {
            GameObject crown = new GameObject("Guardian Crown");
            crown.transform.SetParent(transform, false);
            Renderer root = GetComponentInChildren<Renderer>();
            for (int index = 0; index < CrownCount; index++)
            {
                float angle = index * Mathf.PI * 2f / CrownCount;
                GameObject tine =
                    GameObject.CreatePrimitive(
                        index % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                tine.name = $"Crown {index + 1}";
                tine.transform.SetParent(crown.transform, false);
                tine.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * .65f, 1.05f,
                    Mathf.Sin(angle) * .65f);
                tine.transform.localScale =
                    new Vector3(.2f, .45f + (hash % 20) / 100f, .2f);
                Collider collider = tine.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying) Destroy(collider);
                    else DestroyImmediate(collider);
                }
                Renderer renderer = tine.GetComponent<Renderer>();
                if (root != null && renderer != null)
                    renderer.sharedMaterial = root.sharedMaterial;
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
