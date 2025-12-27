using UnityEngine;

public interface IAimStrategy
{
    Vector2 GetCharacterViewDirection();
}

public sealed class DummyAimStrategy : IAimStrategy
{
    Vector2 IAimStrategy.GetCharacterViewDirection()
    {
        return Vector2.zero;
    }
}

public sealed class GamepadAimStrategy : IAimStrategy
{
    private readonly InputSystem_Actions.PlayerActions _playerActions;

    public GamepadAimStrategy(InputSystem_Actions.PlayerActions playerActions)
    {
        _playerActions = playerActions;
    }

    Vector2 IAimStrategy.GetCharacterViewDirection()
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

public sealed class GamepadBattleAimStrategy : IAimStrategy
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

    Vector2 IAimStrategy.GetCharacterViewDirection()
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

public sealed class MouseAimStrategy : IAimStrategy
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

    Vector2 IAimStrategy.GetCharacterViewDirection()
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