using UnityEngine;

namespace SamuraiProject.Combat.Configs
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [Header("AttackCharacteristics")]
        public float AttackRadius = 5.0f;
        public float AttackRange = 5.0f;

        [Header("Costs")]
        public float LightAttackStaminaCost = 10f;
        public float HeavyAttackStaminaCost = 25f;
        public float BlockStaminaDrainPerSec = 5f;
        public float ParryMissPenaltyCost = 2f;

        [Header("Timing")]
        public float ParryWindowDuration = 0.2f;
        public float HeavyAttackTime = 0.5f;
        public float LightAttackTime = 0.05f;
        public float AttackRecoveryTime = 0.05f;
        public float ParryMissRecoveryTime = 0.5f;
    }
}
