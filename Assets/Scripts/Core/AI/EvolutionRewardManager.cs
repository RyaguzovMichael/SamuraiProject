#nullable enable
using System;

public sealed class EvolutionRewardManager : IDisposable
{
    private readonly RewardEvents _events;
    private readonly AIConfig _config;
    private readonly bool _enableEvolution;

    public EvolutionRewardManager(
        RewardEvents events,
        AIConfig config,
        bool enableEvolution
    )
    {
        _events = events;
        _config = config;
        _enableEvolution = enableEvolution;

        if (!_enableEvolution) return;

        _events.TimeTick += TimeTickHandle;
        _events.DistanceReduce += DistanceReduceHandle;
        _events.DistanceEnrich += DistanceEnrichHandle;
        _events.Kill += KillHandle;
        _events.SuccessParry += SuccessParryHandle;
        _events.Hit += HitHandle;
        _events.Death += DeathHandle;
        _events.WhiffLightAtack += WhiffLightAtackHandle;
        _events.WhiffHeavyAtack += WhiffHeavyAtackHandle;
        _events.ParryMiss += ParryMissHandle;
        _events.ObstacleContact += ObstacleContactHandle;
    }

    public void Dispose()
    {
        if (!_enableEvolution) return;

        _events.TimeTick -= TimeTickHandle;
        _events.DistanceReduce -= DistanceReduceHandle;
        _events.DistanceEnrich -= DistanceEnrichHandle;
        _events.Kill -= KillHandle;
        _events.SuccessParry -= SuccessParryHandle;
        _events.Hit -= HitHandle;
        _events.Death -= DeathHandle;
        _events.WhiffLightAtack -= WhiffLightAtackHandle;
        _events.WhiffHeavyAtack -= WhiffHeavyAtackHandle;
        _events.ParryMiss -= ParryMissHandle;
        _events.ObstacleContact -= ObstacleContactHandle;
    }

    private void TimeTickHandle(IBot bot, float deltaTime)
    {
        var value = _config.TimePenaltyPerSec * deltaTime;
        bot.Fitness -= value;
    }

    private void DistanceReduceHandle(IBot bot, float deltaTime)
    {
        var value = _config.DistanceRewardMultiplier * deltaTime;
        bot.Fitness += value;
    }

    private void DistanceEnrichHandle(IBot bot, float deltaTime)
    {
        var value = _config.RetreatPenaltyMultiplier * deltaTime;
        bot.Fitness -= value;
    }

    private void KillHandle(IBot bot)
    {
        bot.Fitness += _config.KillReward;
    }

    private void SuccessParryHandle(IBot bot)
    {
        bot.Fitness += _config.ParrySuccessReward;
    }

    private void HitHandle(IBot bot)
    {
        bot.Fitness += _config.HitReward;
    }

    private void DeathHandle(IBot bot)
    {
        bot.Fitness -= _config.DeathPenalty;
    }

    private void WhiffLightAtackHandle(IBot bot)
    {
        bot.Fitness -= _config.WhiffLightAttackPenalty;
    }

    private void WhiffHeavyAtackHandle(IBot bot)
    {
        bot.Fitness -= _config.WhiffHeavyAttackPenalty;
    }

    private void ParryMissHandle(IBot bot)
    {
        bot.Fitness -= _config.ParryMissPenalty;
    }

    private void ObstacleContactHandle(IBot bot)
    {
        bot.Fitness -= _config.WallContactPenalty;
    }
}
