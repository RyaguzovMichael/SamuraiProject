#nullable enable
using System;
using SamuraiProject.Combat;
using UnityEngine;

namespace SamuraiProject.AI
{
    public interface IAIInputCollector
    {
        void Init(LayerMask targetLayer);

        ITargetable? CurrentTarget { get; }
        bool HasValidTarget();
        ReadOnlySpan<float> Collect();

        event Action<ITargetable>? ChangeTarget;
    }
}
