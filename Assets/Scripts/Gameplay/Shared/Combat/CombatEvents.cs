using System;

public sealed class CombatEvents
{
    public event Action OnParrySuccess;
    public event Action OnParryMiss;
    public event Action OnLightAttack;
    public event Action OnHeavyAttack;

    // Обертки для безопасного вызова
    public void NotifyParrySuccess() => OnParrySuccess?.Invoke();
    public void NotifyParryMiss() => OnParryMiss?.Invoke();
    public void NotifyLightAttack() => OnLightAttack?.Invoke();
    public void NotifyHeavyAttack() => OnHeavyAttack?.Invoke();
}