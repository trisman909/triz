using UnityEngine;
using UnityEngine.InputSystem;

namespace Lanternfall.Gameplay.Input
{
    /// <summary>
    /// Owns the gameplay action map and exposes device-neutral intent.
    /// Rebinding persistence is added with the settings/save phase.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private InputActionMap _map;
        private InputAction _move;
        private InputAction _sprint;
        private InputAction _dodge;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SprintHeld => _sprint?.IsPressed() ?? false;
        public bool DodgePressedThisFrame => _dodge?.WasPressedThisFrame() ?? false;

        private void Awake()
        {
            _map = new InputActionMap("Gameplay");
            _move = _map.AddAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddBinding("<Gamepad>/leftStick");
            _sprint = _map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            _sprint.AddBinding("<Gamepad>/leftStickPress");
            _dodge = _map.AddAction("Dodge", InputActionType.Button, "<Keyboard>/space");
            _dodge.AddBinding("<Gamepad>/buttonEast");
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable() => _map?.Disable();
        private void OnDestroy() => _map?.Dispose();
    }
}

