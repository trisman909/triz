using UnityEngine;
using UnityEngine.InputSystem;

namespace Lanternfall.Gameplay.Input
{
    /// <summary>
    /// Owns the gameplay action map and exposes device-neutral intent.
    /// Owns default bindings and serializes Input System override JSON.
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
        private InputAction _pause;
        private InputAction _uiScaleUp;
        private InputAction _uiScaleDown;
        private InputAction _reducedMotion;
        private InputAction _subtitles;
        private InputAction _highContrast;

        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SprintHeld => _sprint?.IsPressed() ?? false;
        public bool DodgePressedThisFrame => _dodge?.WasPressedThisFrame() ?? false;
        public Vector2 Aim => _aim?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool PrimaryFireHeld => _primaryFire?.IsPressed() ?? false;
        public bool AbilityPressedThisFrame => _ability?.WasPressedThisFrame() ?? false;
        public bool InteractPressedThisFrame => _interact?.WasPressedThisFrame() ?? false;
        public bool PausePressedThisFrame => _pause?.WasPressedThisFrame() ?? false;
        public bool UiScaleUpPressedThisFrame =>
            _uiScaleUp?.WasPressedThisFrame() ?? false;
        public bool UiScaleDownPressedThisFrame =>
            _uiScaleDown?.WasPressedThisFrame() ?? false;
        public bool ReducedMotionPressedThisFrame =>
            _reducedMotion?.WasPressedThisFrame() ?? false;
        public bool SubtitlesPressedThisFrame =>
            _subtitles?.WasPressedThisFrame() ?? false;
        public bool HighContrastPressedThisFrame =>
            _highContrast?.WasPressedThisFrame() ?? false;

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
            _pause = _map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            _pause.AddBinding("<Gamepad>/start");
            _uiScaleUp = _map.AddAction(
                "UI Scale Up", InputActionType.Button, "<Keyboard>/equals");
            _uiScaleUp.AddBinding("<Gamepad>/dpad/right");
            _uiScaleDown = _map.AddAction(
                "UI Scale Down", InputActionType.Button, "<Keyboard>/minus");
            _uiScaleDown.AddBinding("<Gamepad>/dpad/left");
            _reducedMotion = _map.AddAction(
                "Reduced Motion", InputActionType.Button, "<Keyboard>/r");
            _reducedMotion.AddBinding("<Gamepad>/rightStickPress");
            _subtitles = _map.AddAction(
                "Subtitles", InputActionType.Button, "<Keyboard>/t");
            _subtitles.AddBinding("<Gamepad>/buttonNorth");
            _highContrast = _map.AddAction(
                "High Contrast", InputActionType.Button, "<Keyboard>/h");
            _highContrast.AddBinding("<Gamepad>/leftStickPress");
        }

        public string SaveBindingOverrides() =>
            _map?.SaveBindingOverridesAsJson() ?? string.Empty;

        public void LoadBindingOverrides(string json)
        {
            if (_map == null || string.IsNullOrWhiteSpace(json)) return;
            _map.LoadBindingOverridesFromJson(json);
        }

        public bool ApplyBindingOverride(
            string actionName, int bindingIndex, string controlPath)
        {
            InputAction action = _map?.FindAction(actionName);
            if (action == null || bindingIndex < 0 ||
                bindingIndex >= action.bindings.Count ||
                string.IsNullOrWhiteSpace(controlPath)) return false;
            action.ApplyBindingOverride(bindingIndex, controlPath);
            return true;
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable() => _map?.Disable();
        private void OnDestroy() => _map?.Dispose();
    }
}
