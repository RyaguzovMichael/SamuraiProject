using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SamuraiProject.Input
{
    public sealed class InputProvider : MonoBehaviour
    {
        private InputSystem_Actions _inputActions;

        private InputScheme _currentScheme = InputScheme.Unset;


        public InputSystem_Actions.PlayerActions PlayerActions
        {
            get
            {
                _inputActions ??= new InputSystem_Actions();
                return _inputActions.Player;
            }
        }

        public InputSystem_Actions.UIActions UIActions
        {
            get
            {
                _inputActions ??= new InputSystem_Actions();
                return _inputActions.UI;
            }
        }

        public event Action<InputScheme> OnInputSchemeChanged;

        private void OnDestroy()
        {
            if (_inputActions == null) return;
            _inputActions.Dispose();
        }

        private void OnEnable()
        {
            PlayerActions.Enable();
            PlayerActions.Move.performed += HandleInputType;
            PlayerActions.Crouch.performed += HandleInputType;
            PlayerActions.LookMouse.performed += HandleInputType;
            PlayerActions.LookGamepad.performed += HandleInputType;
            PlayerActions.Attack.performed += HandleInputType;
            PlayerActions.Interact.performed += HandleInputType;
            PlayerActions.Jump.performed += HandleInputType;
        }

        private void OnDisable()
        {
            PlayerActions.Move.performed -= HandleInputType;
            PlayerActions.Crouch.performed -= HandleInputType;
            PlayerActions.LookMouse.performed -= HandleInputType;
            PlayerActions.LookGamepad.performed -= HandleInputType;
            PlayerActions.Attack.performed -= HandleInputType;
            PlayerActions.Interact.performed -= HandleInputType;
            PlayerActions.Jump.performed -= HandleInputType;
            PlayerActions.Disable();
        }

        private void HandleInputType(InputAction.CallbackContext context)
        {
            InputDevice device = context.control.device;

            InputScheme detectedScheme = device is Gamepad
                ? InputScheme.Gamepad
                : InputScheme.KeyboardMouse;

            if (_currentScheme != detectedScheme)
            {
                _currentScheme = detectedScheme;
                OnInputSchemeChanged?.Invoke(_currentScheme);
            }
        }
    }
}

public enum InputScheme
{
    Unset,
    Gamepad,
    KeyboardMouse
}
