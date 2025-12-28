#nullable enable
using SamuraiProject.Input;
using UnityEngine;
using SamuraiProject.Combat;

namespace SamuraiProject.Player
{
    internal sealed class GamepadBattleAimStrategy : IAimStrategy
    {
        private readonly InputSystem_Actions.PlayerActions _playerActions;
        private readonly Transform _playerPosition;
        private readonly TargetScanner _targetScanner;

        public GamepadBattleAimStrategy(
            InputSystem_Actions.PlayerActions playerActions,
            Transform playerPosition,
            TargetScanner targetScanner
        )
        {
            _playerActions = playerActions;
            _playerPosition = playerPosition;
            _targetScanner = targetScanner;
        }

        public Vector2 GetCharacterViewDirection()
        {
            var rightStickInput = _playerActions.LookGamepad.ReadValue<Vector2>();

            if (rightStickInput.sqrMagnitude > 0.01f)
            {
                return rightStickInput.normalized;
            }

            var target = _targetScanner.GetNearestTarget();
            var targetPosition = target.Transform.position;
            Vector2 vectorToTarget = targetPosition - _playerPosition.position;
            return vectorToTarget.normalized;
        }
    }
}
