using SamuraiProject.Combat.Configs;

namespace SamuraiProject.Combat
{
    public sealed class CharacterCombatStats
    {
        private readonly WeaponConfig _baseConfig;

        public float HeavyAttackSpeedMultiplier { get; set; } = 1.0f;
        public float LightAttackSpeedMultiplier { get; set; } = 1.0f;
        public float DamageMultiplier { get; set; } = 1.0f;
        public float AttackRecoveryTimeMultiplier { get; set; } = 1.0f;

        public CharacterCombatStats(WeaponConfig config)
        {
            _baseConfig = config;
        }

        public void Reset()
        {
            HeavyAttackSpeedMultiplier = 1.0f;
            LightAttackSpeedMultiplier = 1.0f;
            DamageMultiplier = 1.0f;
            AttackRecoveryTimeMultiplier = 1.0f;
        }

        public float GetHeavyWindupTime()
        {
            return _baseConfig.HeavyAttackTime * HeavyAttackSpeedMultiplier;
        }

        public float GetLightAttackTime()
        {
            return _baseConfig.LightAttackTime * LightAttackSpeedMultiplier;
        }

        public float AttackTimeRecover()
        {
            return _baseConfig.AttackRecoveryTime * AttackRecoveryTimeMultiplier;
        }
    }
}
