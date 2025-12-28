#nullable enable
using System;
using UnityEngine;

namespace SamuraiProject.AI.Abstractions
{
    public interface IBot
    {
        float Fitness { get; }
        void SetPosition(Vector3 position);

        void Reset();
        void Activate();
        void Deactivate();
        void Initialize(
            byte[] botBrain,
            LayerMask obstacleLayer
        );
        void SaveBrainToFile(string savePath);
        void Evolve(IBot bestBot, float mutateRate, float mutateForce);

        event Action Dead;
    }
}

