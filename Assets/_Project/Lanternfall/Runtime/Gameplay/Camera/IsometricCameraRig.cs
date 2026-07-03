using UnityEngine;
using Lanternfall.Gameplay.Accessibility;

namespace Lanternfall.Gameplay.Camera
{
    public sealed class IsometricCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -10f);
        [SerializeField, Min(0.01f)] private float followSmoothTime = 0.14f;
        [SerializeField, Min(0.01f)] private float zoomSmoothTime = 0.25f;

        private Vector3 _followVelocity;
        private float _zoomVelocity;
        private float _baseZoom = 8.5f;
        private float _targetZoom = 8.5f;
        private float _shakeTime;
        private float _shakeAmplitude;

        public void SetTarget(Transform value) => target = value;
        public void SetBossZoom(bool active) =>
            _targetZoom = active ? _baseZoom + 3f : _baseZoom;

        /// <summary>
        /// Applies a scene-specific framing profile without changing the
        /// camera's deterministic follow behavior.
        /// </summary>
        public void ConfigureView(Vector3 cameraOffset, float zoom)
        {
            offset = cameraOffset;
            _baseZoom = Mathf.Clamp(zoom, 4f, 14f);
            _targetZoom = _baseZoom;
            if (TryGetComponent(out UnityEngine.Camera camera))
                camera.orthographicSize = _baseZoom;
        }

        public void Shake(float amplitude, float duration)
        {
            if (AccessibilityRuntime.ReducedMotion) return;
            _shakeAmplitude = Mathf.Max(
                _shakeAmplitude, amplitude * AccessibilityRuntime.CameraShake);
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }

        private void Awake()
        {
            if (TryGetComponent(out UnityEngine.Camera camera))
            {
                camera.orthographic = true;
                camera.orthographicSize = _baseZoom;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 destination = target.position + offset;
            transform.position = Vector3.SmoothDamp(
                transform.position, destination, ref _followVelocity, followSmoothTime);
            transform.rotation = Quaternion.Euler(48f, 0f, 0f);

            if (TryGetComponent(out UnityEngine.Camera camera))
                camera.orthographicSize = Mathf.SmoothDamp(
                    camera.orthographicSize, _targetZoom, ref _zoomVelocity, zoomSmoothTime);

            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.deltaTime;
                transform.position += Random.insideUnitSphere * _shakeAmplitude;
                _shakeAmplitude *= Mathf.Exp(-8f * Time.deltaTime);
            }
        }
    }
}
