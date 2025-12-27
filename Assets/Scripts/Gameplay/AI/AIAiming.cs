using UnityEngine;

[RequireComponent(typeof(TargetScanner))]
[RequireComponent(typeof(ICharacterMovement))]
public sealed class AIAiming : MonoBehaviour, ICharacterAiming
{
    private TargetScanner _targetScanner;
    private ICharacterMovement _characterMovement;

    private void Awake()
    {
        _targetScanner = GetComponent<TargetScanner>();
        _characterMovement = GetComponent<ICharacterMovement>();
    }

    public Direction GetDirection()
    {
        var target = _targetScanner.GetNearestTarget();
        Vector2 result;

        // Проверяем на null И на destroyed object
        if (target != null && target.Transform != null)
        {
            result = target.Transform.position - transform.position;
        }
        else
        {
            // Если цели нет, смотрим по движению
            result = _characterMovement.MoveVector;

            // Если стоим на месте - сохраняем текущий поворот (чтобы не сбрасываться в (0,0))
            if (result.sqrMagnitude < 0.001f)
            {
                return DirectionUtils.VectorToDirection(transform.up);
            }
        }

        return DirectionUtils.VectorToDirection(result.normalized);
    }

    public Vector2 GetDirectionVector()
    {
        return DirectionUtils.DirectionToVector(GetDirection());
    }
}