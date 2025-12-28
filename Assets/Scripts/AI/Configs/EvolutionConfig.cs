#nullable enable
using UnityEngine;

namespace SamuraiProject.AI.Configs
{
    [CreateAssetMenu(fileName = "EvolutionConfig", menuName = "AIConfigs/EvolutionConfig")]
    public sealed class EvolutionConfig : ScriptableObject
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
        public float HitReward = 20f;
        public float HeavyAttackReward = 3.0f;
        public float LightAttacReward = 3.0f;

        [Header("Combat Penalties")]
        public float DeathPenalty = 50f;
        public float WhiffLightAttackPenalty = 3.0f;
        public float WhiffHeavyAttackPenalty = 5.0f;
        public float ParryMissPenalty = 5.0f;

        [Header("Environment Penalties")]
        [Tooltip("Penalty per frame for touching a wall")]
        public float WallContactPenalty = 0.1f;
    }
}

