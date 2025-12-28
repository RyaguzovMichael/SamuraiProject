using UnityEngine;

namespace SamuraiProject.Combat
{
    public interface ITargetable
    {
        public Transform Transform { get; }
        public CombatController CombatController { get; }
    }
}
