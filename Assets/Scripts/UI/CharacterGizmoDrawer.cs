#nullable enable
using UnityEngine;
using SamuraiProject.Core.Utils;
using SamuraiProject.Combat;
using CharacterController = SamuraiProject.Character.CharacterController;
using SamuraiProject.AI;

namespace SamuraiProject.UI
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CombatController))]
    public sealed class CharacterGizmoDrawer : MonoBehaviour
    {
        private CharacterController _characterController = null!;
        private CombatController _combatController = null!;
        private IAIInputCollector? _aiInputCollector;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _combatController = GetComponent<CombatController>();
            if (TryGetComponent(out IAIInputCollector aiInputCollector))
            {
                _aiInputCollector = aiInputCollector;
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (_characterController.IsMove)
            {
                var moveVector = _characterController.MoveVector;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, moveVector.normalized * 2f);
                Gizmos.color = Color.aquamarine;
            }

            var facingDirection = _characterController.LookDirection;
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

            Vector2 facing = DirectionUtils.DirectionToVector(_characterController.LookDirection);
            Vector2 center = (Vector2)transform.position + facing * weaponConfig.AttackRange;
            Gizmos.DrawWireSphere(center, weaponConfig.AttackRadius);

            if (_aiInputCollector != null && _aiInputCollector.CurrentTarget != null)
            {
                Gizmos.color = Color.blueViolet;
                Gizmos.DrawLine(transform.position, _aiInputCollector.CurrentTarget.Transform.position);
            }
        }

    }
}

