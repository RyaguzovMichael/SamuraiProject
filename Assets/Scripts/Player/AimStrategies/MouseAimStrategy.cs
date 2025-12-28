#nullable enable
using SamuraiProject.Input;
using UnityEngine;

namespace SamuraiProject.Player
{
    internal sealed class MouseAimStrategy : IAimStrategy
    {
        private readonly InputSystem_Actions.PlayerActions _playerActions;
        private readonly Transform _playerTransform;
        private readonly Camera _camera;

        public MouseAimStrategy(
            InputSystem_Actions.PlayerActions playerActions,
            Transform playerTransform,
            Camera camera
        )
        {
            _playerActions = playerActions;
            _playerTransform = playerTransform;
            _camera = camera;
        }

        public Vector2 GetCharacterViewDirection()
        {
            var input = _playerActions.LookMouse.ReadValue<Vector2>();

            var playerPosition = _playerTransform.position;
            float distanceToScreen = Mathf.Abs(_camera.transform.position.z - playerPosition.z);
            Vector3 screenPoint = new(input.x, input.y, distanceToScreen);
            Vector3 worldMousePos = _camera.ScreenToWorldPoint(screenPoint);
            Vector2 direction = (Vector2)worldMousePos - (Vector2)playerPosition;

            return direction.normalized;
        }
    }
}
