#nullable enable
using SamuraiProject.Input;
using UnityEngine;

namespace SamuraiProject.Player
{
    internal sealed class GamepadAimStrategy : IAimStrategy
    {
        private readonly InputSystem_Actions.PlayerActions _playerActions;

        public GamepadAimStrategy(InputSystem_Actions.PlayerActions playerActions)
        {
            _playerActions = playerActions;
        }

        public Vector2 GetCharacterViewDirection()
        {
            var leftStickInput = _playerActions.Move.ReadValue<Vector2>();
            var rightStickInput = _playerActions.LookGamepad.ReadValue<Vector2>();
            var input = rightStickInput.sqrMagnitude > 0.01f
                ? rightStickInput
                : leftStickInput;
            return input.magnitude > 0.1f
                ? input.normalized
                : Vector2.zero;
        }
    }
}
