using UnityEngine;

namespace SamuraiProject.Combat
{
    public interface IDamageReceiver
    {
        CombatState State { get; }
        bool ReceiveHit(AttackType attackType, Vector3 attackerPos, float staminaDrain);
    }
}
