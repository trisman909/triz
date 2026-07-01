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
        private InputAction _aim;
        private InputAction _primaryFire;
        private InputAction _ability;
        private InputAction _interact;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SprintHeld => _sprint?.IsPressed() ?? false;
        public bool DodgePressedThisFrame => _dodge?.WasPressedThisFrame() ?? false;
        public Vector2 Aim => _aim?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool PrimaryFireHeld => _primaryFire?.IsPressed() ?? false;
        public bool AbilityPressedThisFrame => _ability?.WasPressedThisFrame() ?? false;
        public bool InteractPressedThisFrame => _interact?.WasPressedThisFrame() ?? false;

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
            _aim = _map.AddAction("Aim", InputActionType.Value, "<Gamepad>/rightStick");
            _primaryFire = _map.AddAction("Primary Fire", InputActionType.Button, "<Mouse>/leftButton");
            _primaryFire.AddBinding("<Gamepad>/rightTrigger");
            _ability = _map.AddAction("Ability", InputActionType.Button, "<Keyboard>/q");
            _ability.AddBinding("<Gamepad>/leftShoulder");
            _interact = _map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            _interact.AddBinding("<Gamepad>/buttonSouth");
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable() => _map?.Disable();
        private void OnDestroy() => _map?.Dispose();
    }
}
