#nullable enable
using System;

public sealed class RewardEvents
{
    public event Action<IBot, float>? TimeTick;
    public event Action<IBot, float>? DistanceReduce;
    public event Action<IBot, float>? DistanceEnrich;
    public event Action<IBot>? Kill;
    public event Action<IBot>? SuccessParry;
    public event Action<IBot>? Hit;
    public event Action<IBot>? Death;
    public event Action<IBot>? WhiffLightAtack;
    public event Action<IBot>? WhiffHeavyAtack;
    public event Action<IBot>? ParryMiss;
    public event Action<IBot>? ObstacleContact;

    public void OnTimeTick(IBot bot, float deltaTime) => TimeTick?.Invoke(bot, deltaTime);
    public void OnDistanceReduce(IBot bot, float deltaTime) => DistanceReduce?.Invoke(bot, deltaTime);
    public void OnDistanceEnrich(IBot bot, float deltaTime) => DistanceEnrich?.Invoke(bot, deltaTime);
    public void OnKill(IBot bot) => Kill?.Invoke(bot);
    public void OnSuccessParry(IBot bot) => SuccessParry?.Invoke(bot);
    public void OnHit(IBot bot) => Hit?.Invoke(bot);
    public void OnDeath(IBot bot) => Death?.Invoke(bot);
    public void OnWhiffLightAttack(IBot bot) => WhiffLightAtack?.Invoke(bot);
    public void OnWhiffHeavyAttack(IBot bot) => WhiffHeavyAtack?.Invoke(bot);
    public void OnParryMiss(IBot bot) => ParryMiss?.Invoke(bot);
    public void OnObstacleContact(IBot bot) => ObstacleContact?.Invoke(bot);
}
