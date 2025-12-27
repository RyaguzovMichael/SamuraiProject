using UnityEngine;

public class AIMoveInputSource : MonoBehaviour, IMovementInputSource
{
    private Vector2 _targetInput;

    // Это свойство забирает контроллер движения
    public Vector2 MoveVector => _targetInput;
    public bool IsMove => _targetInput.sqrMagnitude > 0.01f;

    // Сюда пишет нейросеть (мгновенные значения 0..1 или -1..1)
    public void SetMoveVector(Vector2 rawInput)
    {
        _targetInput = rawInput;
    }
}
