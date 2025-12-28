#nullable enable
using SamuraiProject.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace SamuraiProject.UI
{
    public sealed class StaminaBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _fillImage = null!;
        [SerializeField] private CanvasGroup _canvasGroup = null!;

        [Header("Settings")]
        [SerializeField] private bool _hideWhenFull = true;

        [SerializeField] private StaminaSystem _staminaSystem = null!;

        private Camera _mainCamera;

        private void Awake()
        {
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
            transform.rotation = _mainCamera.transform.rotation;
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
}

