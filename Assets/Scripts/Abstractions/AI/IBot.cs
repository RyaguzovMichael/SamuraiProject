#nullable enable
using System;
using UnityEngine;

public interface IBot
{
    float Fitness { get; set; }
    RewardEvents RewardEvents { get; }
    void SetPosition(Vector3 position);

    void Reset();
    void Activate();
    void Deactivate();
    void Initialize(
        SimpleNeuralNetwork? botBrain,
        LayerMask targetLayer,
        LayerMask obstacleLayer
    );
    void SaveBrainToFile(string savePath);
    void Evolve(IBot bestBot, float mutateRate, float mutateForce);

    event Action Dead;
}
