using System;
using UnityEngine;

public sealed class StaminaSystem : MonoBehaviour
{
    [SerializeField]
    private CharacterCombatConfig _config;
    [SerializeField]
    private StaminaSystemEventChannel _eventChannel;

    private float _stamina = 100.0f;
    private float _recoverRegenTimer = 0.0f;

    public bool HasStamina => _stamina > 0.01f;
    public float Stamina => _stamina;
    public float MaxStamina => _config.MaxStamina;


    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaEnd;
    public event Action OnStaminaFull;

    public bool TryConsume(float amount)
    {
        if (_stamina < amount) return false;

        _stamina -= amount;
        _recoverRegenTimer = _config.RegenDelayAfterAction;
        StaminaChanged();
        return true;
    }

    public void Drain(float amount)
    {
        _stamina -= amount;
        if (_stamina < 0)
        {
            _stamina = 0;
            StaminaEnd();
        }
        _recoverRegenTimer = _config.RegenDelayAfterAction;
        StaminaChanged();
    }

    public void Restore(float amount)
    {
        _stamina += amount;
        if (_stamina > _config.MaxStamina)
        {
            _stamina = _config.MaxStamina;
            StaminaFull();
        }
        StaminaChanged();
    }

    public void Reset()
    {
        _stamina += _config.MaxStamina;
        StaminaFull();
        StaminaChanged();
    }

    private void Awake()
    {
        _stamina = _config.MaxStamina;
    }

    private void Update()
    {
        if (_stamina == _config.MaxStamina) return;

        if (_recoverRegenTimer > 0.01f)
        {
            _recoverRegenTimer -= Time.deltaTime;
        }

        if (_recoverRegenTimer <= 0.01f)
        {
            _stamina += _config.StaminaRegenPerSecond * Time.deltaTime;
            if (_stamina > _config.MaxStamina)
            {
                _stamina = _config.MaxStamina;
                StaminaFull();
            }
            StaminaChanged();
        }
    }

    private void StaminaFull()
    {
        if (_eventChannel != null)
        {
            _eventChannel.StaminaFull();
        }
        OnStaminaFull?.Invoke();
    }

    private void StaminaEnd()
    {
        if (_eventChannel != null)
        {
            _eventChannel.StaminaEnd();
        }
        OnStaminaEnd?.Invoke();
    }

    private void StaminaChanged()
    {
        if (_eventChannel != null)
        {
            _eventChannel.StaminaChange(_stamina, _config.MaxStamina);
        }
        OnStaminaChanged?.Invoke(_stamina, _config.MaxStamina);
    }

}
