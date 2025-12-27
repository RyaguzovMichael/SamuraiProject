using UnityEngine;

public interface IDamageReceiver
{
    CombatState State { get; }
    bool ReceiveHit(AttackType attackType, Vector3 attackerPos, float staminaDrain);
}