using UnityEngine;

public interface ITargetable
{
    Transform Transform { get; }
    CombatController CombatController { get; }
}