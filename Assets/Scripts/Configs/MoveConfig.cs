using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfiguration", menuName = "Scriptable Objects/PlayerConfiguration")]
public sealed class MoveConfig : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Скорость обычного бега")]
    public float MoveSpeed = 5f;

    [Tooltip("Множитель замедления по оси Y для изометрии (0.7 - стандарт)")]
    [Range(0.5f, 1f)]
    public float VerticalSpeedMultiplier = 0.7f;
}
