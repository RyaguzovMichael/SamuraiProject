#nullable enable
using System;
using SamuraiProject.Character.Configs;
using SamuraiProject.Combat;
using SamuraiProject.Core.Utils;
using UnityEngine;

namespace SamuraiProject.Character
{
    [RequireComponent(typeof(ICharacterBrain))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CombatController))]
    public sealed class CharacterContoller : MonoBehaviour
    {
        [SerializeField]
        private ICharacterBrain _characterBrain = null!;
        [SerializeField]
        private Rigidbody2D _rg = null!;
        [SerializeField]
        private CombatController _combatController = null!;
        [SerializeField]
        private CharacterConfig _config = null!;

        private Vector3 _moveVector = Vector3.zero;
        private Direction _lookDirection = Direction.S;

        public Direction LookDirection => _lookDirection;
        public bool IsMove => _moveVector.magnitude > 0;
        public Vector3 MoveVector => _moveVector;

        private void Awake()
        {
            _characterBrain = GetComponent<ICharacterBrain>();
            _rg = GetComponent<Rigidbody2D>();
            _combatController = GetComponent<CombatController>();
        }

        private void OnEnable()
        {
            _characterBrain.OnLightAttack += LightAttackHandler;
            _characterBrain.OnHeavyAttack += HeavyAttackHandler;
            _characterBrain.OnParry += ParryHandle;
            _characterBrain.OnHoldBlock += HoldBlockHandler;
            _characterBrain.OnReleaseBlock += ReleaseBlockHandler;
        }

        private void OnDisable()
        {
            _characterBrain.OnLightAttack -= LightAttackHandler;
            _characterBrain.OnHeavyAttack -= HeavyAttackHandler;
            _characterBrain.OnParry -= ParryHandle;
            _characterBrain.OnHoldBlock -= HoldBlockHandler;
            _characterBrain.OnReleaseBlock -= ReleaseBlockHandler;
        }

        private void Update()
        {
            var thinkResult = _characterBrain.Think(Time.deltaTime);

            if (_lookDirection != thinkResult.LookDirection)
            {
                _lookDirection = thinkResult.LookDirection;
                _combatController.ChangeLookDirection(_lookDirection);
            }

            var inputScale = Math.Min(thinkResult.MoveSpeed, 1);
            var moveVector = _config.MoveSpeed * inputScale * thinkResult.MoveDirection;
            moveVector.y *= _config.VerticalSpeedMultiplier;
            _moveVector = moveVector;
        }

        private void FixedUpdate()
        {
            _rg.linearVelocity = _moveVector;
        }

        #region Handlers

        private void LightAttackHandler()
        {
            _combatController.AttemptLightAttack(_lookDirection);
        }

        private void HeavyAttackHandler()
        {
            _combatController.AttemptHeavyAttack(_lookDirection);
        }

        private void ParryHandle()
        {
            _combatController.AttemptParry();
        }

        private void HoldBlockHandler()
        {
            _combatController.StartBlock();
        }

        private void ReleaseBlockHandler()
        {
            _combatController.EndBlock();
        }

        #endregion
    }
}
