#nullable enable

namespace SamuraiProject.AI.Abstractions
{
    public interface IAIGoal
    {
        float RewardAmount { get; }

        bool GoalIsFinished(IBot bot);
    }
}
