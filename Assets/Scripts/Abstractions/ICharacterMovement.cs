using UnityEngine;

public interface ICharacterMovement
{
    bool IsMove { get; }
    Vector2 MoveVector { get; }
}