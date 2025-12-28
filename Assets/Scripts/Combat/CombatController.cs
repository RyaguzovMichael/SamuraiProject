#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using SamuraiProject.Combat.Configs;
using SamuraiProject.Core.Utils;
using UnityEngine;

namespace SamuraiProject.Combat
{
    [RequireComponent(typeof(StaminaSystem))]
    public sealed class CombatController : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private WeaponConfig _config;
        [SerializeField] private LayerMask _targetLayer;

        private readonly List<Collider2D> _hitBuffer = new(1);
        private StaminaSystem _stamina;
        private CombatState _state = CombatState.Idle;
        private CharacterCombatStats _stats;
        private Direction _lookDirection = Direction.S;

        public CombatState State => _state;
        public CharacterCombatStats Stats => _stats;
        public WeaponConfig WeaponConfig => _config;

        public event Action? OnParrySuccess;
        public event Action? OnParryMiss;
        public event Action? OnLightAttack;
        public event Action? OnHeavyAttack;
        public event Action? OnLightAttackWhiff;
        public event Action? OnHeavyAttackWhiff;
        public event Action? OnDie;
        public event Action? OnKill;

        private void Awake()
        {
            _stamina = GetComponent<StaminaSystem>();
            _stats = new CharacterCombatStats(_config);
        }

        private void OnEnable()
        {
            _stamina.OnStaminaEnd += HandleStaminaEnd;
        }

        private void OnDisable()
        {
            _stamina.OnStaminaEnd -= HandleStaminaEnd;
        }

        public void Reset()
        {
            _state = CombatState.Idle;
            if (_stamina != null)
            {
                _stamina.Reset();
            }
            _stats.Reset();
        }

        // --- INPUT HANDLERS ---
        public void AttemptLightAttack(Direction attackDirection)
        {
            if (_state == CombatState.Idle && _stamina.TryConsume(5.0f))
            {
                StartCoroutine(LightAttackRoutine(attackDirection));
            }
        }

        public void AttemptHeavyAttack(Direction attackDirection)
        {
            if (_state == CombatState.Idle && _stamina.TryConsume(15.0f))
            {
                StartCoroutine(HeavyAttackRoutine(attackDirection));
            }
        }

        public void AttemptParry()
        {
            if (_state != CombatState.Idle) return;
            if (!_stamina.HasStamina) return;

            StartCoroutine(ParryRoutine());
        }

        public void StartBlock()
        {
            if (_state == CombatState.Idle && _stamina.HasStamina)
            {
                StartCoroutine(BlockRoutine());
            }
        }

        public void EndBlock()
        {
            if (_state == CombatState.Blocking)
            {
                _state = CombatState.Idle;
            }
        }

        public void ChangeLookDirection(Direction direction)
        {
            _lookDirection = direction;
        }

        // --- LOGIC ROUTINES ---
        private IEnumerator LightAttackRoutine(Direction attackDirection)
        {
            _state = CombatState.Attacking;
            yield return new WaitForSeconds(_stats.GetLightAttackTime());
            PerformAttackHitbox(AttackType.Light, attackDirection);
            OnLightAttack?.Invoke();
            yield return new WaitForSeconds(_stats.AttackTimeRecover());
            _state = CombatState.Idle;
        }

        private IEnumerator HeavyAttackRoutine(Direction attackDirection)
        {
            _state = CombatState.Attacking;
            yield return new WaitForSeconds(_stats.GetHeavyWindupTime());
            PerformAttackHitbox(AttackType.Heavy, attackDirection);
            OnHeavyAttack?.Invoke();
            yield return new WaitForSeconds(_stats.AttackTimeRecover());
            _state = CombatState.Idle;
        }

        private IEnumerator ParryRoutine()
        {
            _state = CombatState.Parrying;
            float timer = 0f;

            while (timer < _config.ParryWindowDuration)
            {
                if (_state != CombatState.Parrying) yield break;

                timer += Time.deltaTime;
                yield return null;
            }

            HandleParryMiss();
        }

        private IEnumerator BlockRoutine()
        {
            _state = CombatState.Blocking;
            while (_state == CombatState.Blocking)
            {
                // TODO: move to config drain amount
                _stamina.Drain(5.0f * Time.deltaTime);
                yield return null;
            }
        }

        private IEnumerator RecoveryRoutine(float duration)
        {
            _state = CombatState.Recovery;
            yield return new WaitForSeconds(duration);
            _state = CombatState.Idle;
        }

        // --- ATTACK LOGIC ---
        private void PerformAttackHitbox(AttackType type, Direction attackDirection)
        {
            Vector2 facingDir = DirectionUtils.DirectionToVector(attackDirection);
            Vector2 origin = (Vector2)transform.position + facingDir * _config.AttackRange;

            ContactFilter2D filter = new();
            filter.SetLayerMask(_targetLayer);
            filter.useLayerMask = true;

            int hitCount = Physics2D.OverlapCircle(origin, _config.AttackRadius, filter, _hitBuffer);

            if (hitCount <= 0)
            {
                switch (type)
                {
                    case AttackType.Light:
                        OnLightAttackWhiff?.Invoke();
                        return;
                    case AttackType.Heavy:
                        OnHeavyAttackWhiff?.Invoke();
                        return;
                    default:
                        return;
                }
            }
            var victimCollider = _hitBuffer[0];
            if (victimCollider.gameObject == gameObject) return;

            if (victimCollider.TryGetComponent<IDamageReceiver>(out var victim))
            {
                float staminaDamage = type == AttackType.Heavy ? 25f : 10f;
                if (victim.ReceiveHit(type, transform.position, staminaDamage))
                {
                    OnKill?.Invoke();
                }
            }
        }

        // --- DAMAGE RECEIVER ---
        public bool ReceiveHit(AttackType attackType, Vector3 attackerPos, float staminaDrain)
        {
            var vectorToAttacker = attackerPos - transform.position;
            var attackerPosition = DirectionUtils.VectorToDirection(vectorToAttacker);
            if (attackerPosition != _lookDirection)
            {
                Die();
                return true;
            }

            switch (_state)
            {
                case CombatState.Parrying:
                    OnParrySuccess?.Invoke();
                    _state = CombatState.Idle;
                    return false;

                case CombatState.Blocking:
                    _stamina.Drain(staminaDrain);
                    return false;

                case CombatState.Recovery:
                case CombatState.Idle:
                case CombatState.Attacking:
                case CombatState.Stunned:
                    Die();
                    return true;

                case CombatState.Dead:
                default:
                    return false;
            }
        }

        private void HandleParryMiss()
        {
            OnParryMiss?.Invoke();

            _stamina.Drain(_config.ParryMissPenaltyCost);

            StartCoroutine(RecoveryRoutine(_config.ParryMissRecoveryTime));
        }

        private void HandleStaminaEnd()
        {
            if (_state == CombatState.Blocking)
            {
                _state = CombatState.Idle;
            }
        }

        private void Die()
        {
            _state = CombatState.Dead;
            OnDie?.Invoke();
            // Запуск анимации смерти
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

        }
    }

    public enum AttackType { Light, Heavy }
}

