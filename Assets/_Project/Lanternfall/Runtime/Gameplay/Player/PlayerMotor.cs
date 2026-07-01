using Lanternfall.Gameplay.Input;
using UnityEngine;

namespace Lanternfall.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 6.5f;
        [SerializeField, Min(1f)] private float sprintMultiplier = 1.35f;
        [SerializeField, Min(0.1f)] private float acceleration = 35f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 18f;
        [SerializeField, Min(0.1f)] private float dodgeSpeed = 14f;
        [SerializeField] private float gravity = -25f;

        private CharacterController _controller;
        private PlayerInputReader _input;
        private DodgeTimingModel _dodge;
        private Vector3 _velocity;
        private Vector3 _dodgeDirection = Vector3.forward;

        public bool IsInvulnerable => _dodge != null && _dodge.IsInvulnerable;

        public void ConfigureMoveSpeed(float speed) =>
            moveSpeed = Mathf.Max(.1f, speed);

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
            _dodge = new DodgeTimingModel(0.32f, 0.20f, 0.72f);
        }

        private void Update()
        {
            if (_input == null) return;
            Step(_input.Move, _input.SprintHeld, _input.DodgePressedThisFrame, Time.deltaTime);
        }

        public void Step(Vector2 moveInput, bool sprint, bool dodgePressed, float deltaTime)
        {
            if (_controller == null) _controller = GetComponent<CharacterController>();
            if (_dodge == null) _dodge = new DodgeTimingModel(0.32f, 0.20f, 0.72f);
            Vector3 desired = CameraRelative(moveInput);
            if (dodgePressed && _dodge.TryStart())
                _dodgeDirection = desired.sqrMagnitude > 0.01f ? desired.normalized : transform.forward;

            _dodge.Tick(deltaTime);
            Vector3 planar;
            if (_dodge.IsDodging)
            {
                float ease = 1f - 0.35f * _dodge.NormalizedTime;
                planar = _dodgeDirection * dodgeSpeed * ease;
            }
            else
            {
                float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
                Vector3 target = desired * speed;
                planar = Vector3.MoveTowards(
                    new Vector3(_velocity.x, 0f, _velocity.z),
                    target,
                    acceleration * deltaTime);
            }

            _velocity.x = planar.x;
            _velocity.z = planar.z;
            _velocity.y = _controller.isGrounded ? -2f : _velocity.y + gravity * deltaTime;
            _controller.Move(_velocity * deltaTime);

            if (planar.sqrMagnitude > 0.05f)
            {
                Quaternion facing = Quaternion.LookRotation(planar, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    facing,
                    1f - Mathf.Exp(-rotationSpeed * deltaTime));
            }
        }

        private static Vector3 CameraRelative(Vector2 input)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            Vector3 forward = camera != null ? camera.transform.forward : Vector3.forward;
            Vector3 right = camera != null ? camera.transform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            return Vector3.ClampMagnitude(
                forward.normalized * input.y + right.normalized * input.x,
                1f);
        }
    }
}
