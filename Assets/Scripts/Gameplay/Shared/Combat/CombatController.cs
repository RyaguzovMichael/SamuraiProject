using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(StaminaSystem))]
[RequireComponent(typeof(ICharacterAiming))]
public sealed class CombatController : MonoBehaviour, IDamageReceiver
{
    [SerializeField] private WeaponConfig _config;
    [SerializeField] private LayerMask _targetLayer;

    private readonly List<Collider2D> _hitBuffer = new(10);
    private StaminaSystem _stamina;
    private ICharacterAiming _characterAiming;
    private CombatState _state = CombatState.Idle;
    private CharacterCombatStats _stats;

    public CombatState State => _state;
    public readonly CombatEvents Events = new();
    public CharacterCombatStats Stats => _stats;

    public event UnityAction OnDie;
    public event UnityAction OnKill;

    private void Awake()
    {
        _stamina = GetComponent<StaminaSystem>();
        _characterAiming = GetComponent<ICharacterAiming>();
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
    public void AttemptLightAttack()
    {
        if (_state == CombatState.Idle && _stamina.TryConsume(5.0f))
        {
            StartCoroutine(LightAttackRoutine());
        }
    }

    public void AttemptHeavyAttack()
    {
        if (_state == CombatState.Idle && _stamina.TryConsume(15.0f))
        {
            StartCoroutine(HeavyAttackRoutine());
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

    // --- LOGIC ROUTINES ---
    private IEnumerator LightAttackRoutine()
    {
        _state = CombatState.Attacking;
        yield return new WaitForSeconds(_stats.GetLightAttackTime());
        PerformAttackHitbox(AttackType.Light);
        Events.NotifyLightAttack();
        yield return new WaitForSeconds(_stats.AttackTimeRecover());
        _state = CombatState.Idle;
    }

    private IEnumerator HeavyAttackRoutine()
    {
        _state = CombatState.Attacking;
        yield return new WaitForSeconds(_stats.GetHeavyWindupTime());
        PerformAttackHitbox(AttackType.Heavy);
        Events.NotifyHeavyAttack();
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
    private void PerformAttackHitbox(AttackType type)
    {
        Vector2 facingDir = _characterAiming.GetDirectionVector();
        Vector2 origin = (Vector2)transform.position + facingDir * _config.AttackRange;

        ContactFilter2D filter = new();
        filter.SetLayerMask(_targetLayer);
        filter.useLayerMask = true;

        int hitCount = Physics2D.OverlapCircle(origin, _config.AttackRadius, filter, _hitBuffer);

        if (hitCount <= 0) return;
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
        if (attackerPosition != _characterAiming.GetDirection())
        {
            Die();
            return true;
        }

        switch (_state)
        {
            case CombatState.Parrying:
                Events.NotifyParrySuccess();
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
        Events.NotifyParryMiss();

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

        Gizmos.color = _state == CombatState.Attacking ? Color.red : Color.deepPink;
        var facing = _characterAiming.GetDirectionVector();

        Vector2 center = (Vector2)transform.position + facing * _config.AttackRange;
        Gizmos.DrawWireSphere(center, _config.AttackRadius);
    }
}

public enum AttackType { Light, Heavy }
