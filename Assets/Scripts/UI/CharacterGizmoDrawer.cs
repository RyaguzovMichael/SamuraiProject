#nullable enable
using UnityEngine;
using SamuraiProject.Core.Utils;
using SamuraiProject.Character;
using SamuraiProject.Combat;

namespace SamuraiProject.UI
{
    [RequireComponent(typeof(CharacterContoller))]
    [RequireComponent(typeof(CombatController))]
    public sealed class CharacterGizmoDrawer : MonoBehaviour
    {
        private CharacterContoller _characterContoller = null!;
        private CombatController _combatController = null!;

        private void Awake()
        {
            _characterContoller = GetComponent<CharacterContoller>();
            _combatController = GetComponent<CombatController>();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (_characterContoller.IsMove)
            {
                var moveVector = _characterContoller.MoveVector;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, moveVector.normalized * 2f);
                Gizmos.color = Color.aquamarine;
            }

            var facingDirection = _characterContoller.LookDirection;
            var (sectorStart, sectorEnd) = DirectionUtils.DirectionToSector(facingDirection);
            Gizmos.DrawRay(transform.position, sectorStart * 2f);
            Gizmos.DrawRay(transform.position, sectorEnd * 2f);
            Gizmos.DrawRay(
                transform.position + (Vector3)sectorStart * 2f,
                sectorEnd * 2f - sectorStart * 2f
            );

            Gizmos.color = _combatController.State == CombatState.Attacking
                ? Color.red
                : Color.deepPink;

            var weaponConfig = _combatController.WeaponConfig;

            Vector2 facing = DirectionUtils.DirectionToVector(_characterContoller.LookDirection);
            Vector2 center = (Vector2)transform.position + facing * weaponConfig.AttackRange;
            Gizmos.DrawWireSphere(center, weaponConfig.AttackRadius);
        }

    }
}

