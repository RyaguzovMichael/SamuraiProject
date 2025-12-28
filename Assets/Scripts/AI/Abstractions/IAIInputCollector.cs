#nullable enable
using System;
using SamuraiProject.Combat;

namespace SamuraiProject.AI
{
    public interface IAIInputCollector
    {
        ITargetable? CurrentTarget { get; }
        bool HasValidTarget();
        ReadOnlySpan<float> Collect();

        event Action<ITargetable>? ChangeTarget;
    }
}
