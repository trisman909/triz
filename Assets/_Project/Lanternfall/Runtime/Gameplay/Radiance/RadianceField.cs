using UnityEngine;

namespace Lanternfall.Gameplay.Radiance
{
    /// <summary>Authoritative movable light volume used by combat rules.</summary>
    public sealed class RadianceField : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float radius = 6f;
        public static RadianceField Active { get; private set; }
        public float Radius => radius;

        private void OnEnable() => Active = this;
        private void OnDisable()
        {
            if (Active == this) Active = null;
        }

        public void ConfigureRadius(float value) => radius = Mathf.Max(1f, value);

        public bool Contains(Vector3 point)
        {
            Vector3 delta = Vector3.ProjectOnPlane(
                point - transform.position, Vector3.up);
            return delta.sqrMagnitude <= radius * radius;
        }

        public static bool ContainsActive(Vector3 point) =>
            Active != null && Active.Contains(point);
    }
}
