using UnityEngine;
using UnityEngine.UI;

public sealed class StaminaBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _fillImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Settings")]
    [SerializeField] private bool _hideWhenFull = true;

    private StaminaSystem _staminaSystem;
    private Camera _mainCamera;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _mainCamera = Camera.main;

        _staminaSystem = GetComponentInParent<StaminaSystem>();

        if (_staminaSystem == null)
        {
            Debug.LogError($"StaminaSystem not found in parent of {gameObject.name}");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_staminaSystem != null)
        {
            _staminaSystem.OnStaminaChanged += UpdateBar;
            UpdateBar(_staminaSystem.Stamina, 100f);
        }
    }

    private void OnDisable()
    {
        if (_staminaSystem != null)
        {
            _staminaSystem.OnStaminaChanged -= UpdateBar;
        }
    }

    private void LateUpdate()
    {
        _transform.rotation = _mainCamera.transform.rotation;
    }

    private void UpdateBar(float current, float max)
    {
        float percent = current / max;
        _fillImage.fillAmount = percent;

        if (_canvasGroup != null && _hideWhenFull)
        {
            bool isFull = percent >= 0.99f;
            _canvasGroup.alpha = isFull ? 0f : 1f;
        }
    }
}