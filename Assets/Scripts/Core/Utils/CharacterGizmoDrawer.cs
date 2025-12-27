using UnityEngine;

[RequireComponent(typeof(ICharacterMovement))]
[RequireComponent(typeof(ICharacterAiming))]
public sealed class CharacterGizmoDrawer : MonoBehaviour
{
    private ICharacterMovement _characterMovement;
    private ICharacterAiming _characterAiming;

    private void Awake()
    {
        _characterMovement = GetComponent<ICharacterMovement>();
        _characterAiming = GetComponent<ICharacterAiming>();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (_characterMovement.IsMove)
        {
            var moveVector = _characterMovement.MoveVector;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveVector.normalized * 2f);
            Gizmos.color = Color.aquamarine;
        }

        var facingDirection = _characterAiming.GetDirection();
        var (sectorStart, sectorEnd) = DirectionUtils.DirectionToSector(facingDirection);
        Gizmos.DrawRay(transform.position, sectorStart * 2f);
        Gizmos.DrawRay(transform.position, sectorEnd * 2f);
        Gizmos.DrawRay(
            transform.position + (Vector3)sectorStart * 2f,
            sectorEnd * 2f - sectorStart * 2f
        );
    }

}