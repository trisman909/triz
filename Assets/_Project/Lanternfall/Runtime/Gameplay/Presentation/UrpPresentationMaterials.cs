using UnityEngine;
using UnityEngine.Rendering;

namespace Lanternfall.Gameplay.Presentation
{
    /// <summary>
    /// Shared URP-safe fallbacks for presentation geometry created at runtime.
    /// Unity primitives otherwise retain the Built-in pipeline material and
    /// render violet or incorrectly lit under URP.
    /// </summary>
    public static class UrpPresentationMaterials
    {
        private static Material _lit;
        private static Material _emissive;
        private static Material _transparent;
        private static Material _particle;

        public static Material Lit =>
            _lit != null
                ? _lit
                : _lit = LoadOrCreate(
                    "Presentation/RuntimeLit",
                    "Universal Render Pipeline/Lit",
                    false,
                    false);

        public static Material Emissive =>
            _emissive != null
                ? _emissive
                : _emissive = LoadOrCreate(
                    "Presentation/RuntimeEmissive",
                    "Universal Render Pipeline/Lit",
                    true,
                    false);

        public static Material Transparent =>
            _transparent != null
                ? _transparent
                : _transparent = LoadOrCreate(
                    "Presentation/RuntimeTransparent",
                    "Universal Render Pipeline/Unlit",
                    false,
                    true);

        public static Material Particle =>
            _particle != null
                ? _particle
                : _particle = LoadOrCreate(
                    "Presentation/RuntimeParticle",
                    "Universal Render Pipeline/Particles/Unlit",
                    false,
                    true);

        public static Material PreferCompatible(Material preferred)
        {
            if (preferred != null &&
                preferred.shader != null &&
                preferred.shader.isSupported &&
                preferred.shader.name.StartsWith(
                    "Universal Render Pipeline/",
                    System.StringComparison.Ordinal))
                return preferred;
            return Lit;
        }

        private static Material LoadOrCreate(
            string resourcePath,
            string shaderName,
            bool emissive,
            bool transparent)
        {
            Material authored = Resources.Load<Material>(resourcePath);
            if (authored != null) return authored;

            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError(
                    $"Required presentation material and shader are missing: " +
                    $"{resourcePath} / {shaderName}.");
                return null;
            }
            var material = new Material(shader)
            {
                name = emissive
                    ? "Lanternfall Runtime Emissive"
                    : transparent
                        ? "Lanternfall Runtime Transparent"
                        : "Lanternfall Runtime Lit",
                hideFlags = HideFlags.HideAndDontSave,
                enableInstancing = true
            };
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
            if (transparent)
            {
                material.renderQueue = (int)RenderQueue.Transparent;
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat(
                    "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            return material;
        }
    }
}
