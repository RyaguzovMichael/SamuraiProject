using UnityEngine;

[CreateAssetMenu(fileName = "CameraConfiguration", menuName = "Scriptable Objects/CameraConfiguration")]
public sealed class CameraConfig : ScriptableObject
{
    [Tooltip("Время за которое камера догоняет цель (меньше = быстрее)")]
    [Range(0.1f, 0.5f)]
    public float SmoothTime = 0.2f;

    [Tooltip("Расположение камеры относительно цели")]
    public Vector3 Offset = new(0, 0, -100f);
}
