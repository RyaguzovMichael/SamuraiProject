using System;
using UnityEngine;

public static class DirectionUtils
{
    private const float StepSize = 360f / 8f;
    private const float Offset = StepSize / 2f;

    public static Direction VectorToDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return Direction.S;
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        float angleOffset = angle + Offset;
        int index = Mathf.FloorToInt(angleOffset / StepSize) % 8;
        return index switch
        {
            0 => Direction.E,
            1 => Direction.NE,
            2 => Direction.N,
            3 => Direction.NW,
            4 => Direction.W,
            5 => Direction.SW,
            6 => Direction.S,
            7 => Direction.SE,
            _ => Direction.S,
        };
    }

    public static Vector2 DirectionToVector(Direction direction)
    {
        return direction switch
        {
            Direction.S => Vector2.down,
            Direction.SE => (Vector2.down + Vector2.right).normalized,
            Direction.E => Vector2.right,
            Direction.NE => (Vector2.right + Vector2.up).normalized,
            Direction.N => Vector2.up,
            Direction.NW => (Vector2.up + Vector2.left).normalized,
            Direction.W => Vector2.left,
            Direction.SW => (Vector2.left + Vector2.down).normalized,
            _ => throw new ApplicationException("Unexpected input"),
        };
    }

    public static (Vector2, Vector2) DirectionToSector(Direction direction)
    {
        var directionVector = DirectionToVector(direction);
        var sectorStart = Quaternion.Euler(0, 0, Offset) * directionVector;
        var sectorEnd = Quaternion.Euler(0, 0, -Offset) * directionVector;
        return (sectorStart, sectorEnd);
    }
}

public enum Direction
{
    S,  // South (Вниз)
    SE, // South East
    E,  // East (Вправо)
    NE, // North East
    N,  // North (Вверх)
    NW, // North West
    W,  // West (Влево)
    SW  // South West
}
