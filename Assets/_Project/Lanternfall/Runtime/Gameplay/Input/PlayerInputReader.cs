using System;
using System.Collections.Generic;
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
        private InputAction _selectBinding;
        private InputAction _startRebind;
        private InputAction _resetBindings;
        private readonly List<RebindSlot> _rebindSlots =
            new List<RebindSlot>(24);
        private InputActionRebindingExtensions.RebindingOperation _operation;

        private readonly struct RebindSlot
        {
            public RebindSlot(InputAction action, int bindingIndex)
            {
                Action = action;
                BindingIndex = bindingIndex;
            }

            public InputAction Action { get; }
            public int BindingIndex { get; }
        }

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
        public bool SelectBindingPressedThisFrame =>
            _selectBinding?.WasPressedThisFrame() ?? false;
        public bool StartRebindPressedThisFrame =>
            _startRebind?.WasPressedThisFrame() ?? false;
        public bool ResetBindingsPressedThisFrame =>
            _resetBindings?.WasPressedThisFrame() ?? false;
        public bool IsRebinding => _operation != null;
        public int RebindSlotCount => _rebindSlots.Count;

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
            _selectBinding = _map.AddAction(
                "Select Binding", InputActionType.Button, "<Keyboard>/tab");
            _selectBinding.AddBinding("<Gamepad>/dpad/down");
            _startRebind = _map.AddAction(
                "Start Rebind", InputActionType.Button, "<Keyboard>/f2");
            _startRebind.AddBinding("<Gamepad>/select");
            _resetBindings = _map.AddAction(
                "Reset Bindings", InputActionType.Button, "<Keyboard>/f3");
            _resetBindings.AddBinding("<Gamepad>/dpad/up");
            BuildRebindSlots();
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

        public string BindingDisplay(int slot)
        {
            if (slot < 0 || slot >= _rebindSlots.Count) return "NO BINDING";
            RebindSlot selected = _rebindSlots[slot];
            InputBinding binding = selected.Action.bindings[selected.BindingIndex];
            string part = string.IsNullOrWhiteSpace(binding.name)
                ? string.Empty
                : $" {binding.name.ToUpperInvariant()}";
            return $"{selected.Action.name.ToUpperInvariant()}{part}: " +
                selected.Action.GetBindingDisplayString(
                    selected.BindingIndex,
                    InputBinding.DisplayStringOptions.DontOmitDevice);
        }

        public bool BeginInteractiveRebind(
            int slot,
            Action<bool, string> completed)
        {
            if (_operation != null || slot < 0 || slot >= _rebindSlots.Count)
                return false;
            RebindSlot selected = _rebindSlots[slot];
            selected.Action.Disable();
            _operation = selected.Action
                .PerformInteractiveRebinding(selected.BindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation =>
                    FinishRebind(selected.Action, operation, false, completed))
                .OnComplete(operation =>
                    FinishRebind(selected.Action, operation, true, completed));
            _operation.Start();
            return true;
        }

        public void ResetBindingOverrides()
        {
            if (_operation != null) _operation.Cancel();
            _map?.RemoveAllBindingOverrides();
        }

        private void BuildRebindSlots()
        {
            string[] gameplayActions =
            {
                "Move", "Sprint", "Dodge", "Aim",
                "Primary Fire", "Ability", "Interact", "Pause"
            };
            foreach (string actionName in gameplayActions)
            {
                InputAction action = _map.FindAction(actionName);
                if (action == null) continue;
                for (int index = 0; index < action.bindings.Count; index++)
                {
                    InputBinding binding = action.bindings[index];
                    if (!binding.isComposite)
                        _rebindSlots.Add(new RebindSlot(action, index));
                }
            }
        }

        private void FinishRebind(
            InputAction action,
            InputActionRebindingExtensions.RebindingOperation operation,
            bool success,
            Action<bool, string> completed)
        {
            operation.Dispose();
            _operation = null;
            action.Enable();
            completed?.Invoke(success, success
                ? "BINDING SAVED"
                : "REBIND CANCELED");
        }

        private void OnEnable() => _map?.Enable();
        private void OnDisable()
        {
            if (_operation != null) _operation.Cancel();
            _map?.Disable();
        }

        private void OnDestroy()
        {
            _operation?.Dispose();
            _operation = null;
            _map?.Dispose();
        }
    }
}
