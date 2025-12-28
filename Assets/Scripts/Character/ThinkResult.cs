using SamuraiProject.Core.Utils;
using UnityEngine;

namespace SamuraiProject.Character
{
    public readonly struct ThinkResult
    {
        public Vector2 MoveDirection { get; }
        public float MoveSpeed { get; }
        public Direction LookDirection { get; }

        public ThinkResult(
            Vector2 moveDirection,
            float moveSpeed,
            Direction lookDirection
        )
        {
            MoveDirection = moveDirection;
            MoveSpeed = moveSpeed;
            LookDirection = lookDirection;
        }
    }
}
