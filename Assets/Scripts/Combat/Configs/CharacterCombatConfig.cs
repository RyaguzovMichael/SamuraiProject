using UnityEngine;

namespace SamuraiProject.Combat.Configs
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "Configs/CharacterCombatConfig")]
    public sealed class CharacterCombatConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)]
        public float MaxStamina { get; private set; } = 100.0f;

        [field: SerializeField, Min(0f)]
        public float StaminaRegenPerSecond { get; private set; } = 15.0f;

        [field: SerializeField, Min(0f)]
        public float RegenDelayAfterAction { get; private set; } = 1.5f;
    }
}
