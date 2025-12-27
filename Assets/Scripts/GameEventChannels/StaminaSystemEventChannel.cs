using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "StaminaSystemEventChannel", menuName = "EventChannels/Combat")]
public sealed class StaminaSystemEventChannel : ScriptableObject
{
    public UnityAction<float, float> OnStaminaChange;
    public UnityAction OnStaminaEnd;
    public UnityAction OnStaminaFull;

    public void StaminaChange(float current, float max)
    {
        OnStaminaChange?.Invoke(current, max);
    }

    public void StaminaEnd()
    {
        OnStaminaEnd?.Invoke();
    }

    public void StaminaFull()
    {
        OnStaminaFull?.Invoke();
    }
}