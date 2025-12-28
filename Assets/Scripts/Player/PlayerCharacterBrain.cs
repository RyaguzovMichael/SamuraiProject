#nullable enable
using System;
using SamuraiProject.Character;
using SamuraiProject.Combat;
using SamuraiProject.Core.Utils;
using SamuraiProject.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SamuraiProject.Player
{
    [RequireComponent(typeof(TargetScanner))]
    public class PlayerCharacterBrain : MonoBehaviour, ICharacterBrain
    {
        [SerializeField]
        private InputProvider _inputProvider = null!;
        [SerializeField]
        private Camera _camera = null!;
        [SerializeField]
        private TargetScanner _targetScanner;

        private IAimStrategy _dummyAimStrategy;
        private IAimStrategy _mouseAimStrategy;
        private IAimStrategy _gamepadAimStrategy;
        private IAimStrategy _gamepadBattleAimStrategy;

        private IAimStrategy _currentStrategy;
        private bool _hasTarget = false;
        private InputScheme _inputScheme = InputScheme.Unset;

        public event Action? OnLightAttack;
        public event Action? OnHeavyAttack;
        public event Action? OnParry;
        public event Action? OnHoldBlock;
        public event Action? OnReleaseBlock;

        public ThinkResult Think(float time)
        {
            Vector2 moveVector = _inputProvider.PlayerActions.Move.ReadValue<Vector2>();
            _targetScanner.ScanEnvironment(2f);
            _hasTarget = _targetScanner.GetNearestTarget() != null;
            ChooseNewAimStrategy();
            Vector2 viewVector = _currentStrategy.GetCharacterViewDirection();
            return new ThinkResult(
                moveVector.normalized,
                moveVector.magnitude,
                DirectionUtils.VectorToDirection(viewVector)
            );
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            if (_targetScanner == null)
            {
                _targetScanner = GetComponent<TargetScanner>();
            }
        }
        private void Start()
        {
            _dummyAimStrategy = new DummyAimStrategy();
            _mouseAimStrategy = new MouseAimStrategy(
                _inputProvider.PlayerActions,
                transform,
                _camera
            );
            _gamepadAimStrategy = new GamepadAimStrategy(
                _inputProvider.PlayerActions
            );
            _gamepadBattleAimStrategy = new GamepadBattleAimStrategy(
                _inputProvider.PlayerActions,
                transform,
                _targetScanner
            );
            _currentStrategy = _dummyAimStrategy;
        }

        private void OnEnable()
        {
            _inputProvider.PlayerActions.Attack.performed += HandleLightAttackButton;
            _inputProvider.PlayerActions.Interact.performed += HandleHeavyAttackButton;
            _inputProvider.PlayerActions.Jump.performed += HandleParyButton;
            _inputProvider.PlayerActions.Crouch.started += HandleBlockButtonPress;
            _inputProvider.PlayerActions.Crouch.canceled += HandleBlockButonRelease;
            _inputProvider.OnInputSchemeChanged += HandleChangeInputScheme;
        }

        private void OnDisable()
        {
            _inputProvider.PlayerActions.Attack.performed -= HandleLightAttackButton;
            _inputProvider.PlayerActions.Interact.performed -= HandleHeavyAttackButton;
            _inputProvider.PlayerActions.Jump.performed -= HandleParyButton;
            _inputProvider.PlayerActions.Crouch.started -= HandleBlockButtonPress;
            _inputProvider.PlayerActions.Crouch.canceled -= HandleBlockButonRelease;
            _inputProvider.OnInputSchemeChanged -= HandleChangeInputScheme;
        }

        private void HandleLightAttackButton(InputAction.CallbackContext context)
        {
            OnLightAttack?.Invoke();
        }

        private void HandleHeavyAttackButton(InputAction.CallbackContext context)
        {
            OnHeavyAttack?.Invoke();
        }

        private void HandleParyButton(InputAction.CallbackContext context)
        {
            OnParry?.Invoke();
        }

        private void HandleBlockButtonPress(InputAction.CallbackContext context)
        {
            OnHoldBlock?.Invoke();
        }

        private void HandleBlockButonRelease(InputAction.CallbackContext context)
        {
            OnReleaseBlock?.Invoke();
        }

        private void HandleChangeInputScheme(InputScheme scheme)
        {
            _inputScheme = scheme;
            ChooseNewAimStrategy();
        }

        private void ChooseNewAimStrategy()
        {
            switch (_inputScheme)
            {
                case InputScheme.Unset:
                    _currentStrategy = _dummyAimStrategy;
                    break;
                case InputScheme.Gamepad:
                    _currentStrategy = _hasTarget
                        ? _gamepadBattleAimStrategy
                        : _gamepadAimStrategy;
                    break;
                case InputScheme.KeyboardMouse:
                    _currentStrategy = _mouseAimStrategy;
                    break;
            }
        }

    }

}
