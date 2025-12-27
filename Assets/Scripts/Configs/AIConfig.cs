#nullable enable
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/AIConfig")]
public sealed class AIConfig : ScriptableObject
{
    [Header("Survival & Movement")]
    [Tooltip("Penalty per second just for existing (Time Pressure)")]
    public float TimePenaltyPerSec = 1.0f;

    [Tooltip("Reward multiplier for moving closer to enemy")]
    public float DistanceRewardMultiplier = 3.0f;

    [Tooltip("Penalty multiplier for moving away from enemy")]
    public float RetreatPenaltyMultiplier = 4.0f;

    [Header("Combat Rewards")]
    public float KillReward = 200f;
    public float ParrySuccessReward = 50f;
    public float HitReward = 20f; // Награда за попадание (нанесение урона)

    [Header("Combat Penalties")]
    public float DeathPenalty = 50f;
    public float WhiffLightAttackPenalty = 3.0f; // Промах легкой
    public float WhiffHeavyAttackPenalty = 5.0f; // Промах тяжелой
    public float ParryMissPenalty = 5.0f;

    [Header("Environment Penalties")]
    [Tooltip("Penalty per frame for touching a wall")]
    public float WallContactPenalty = 0.1f;
}
