public interface IAIGoal
{
    float RewardAmount { get; }

    bool GoalIsFinished(IBot bot);
}