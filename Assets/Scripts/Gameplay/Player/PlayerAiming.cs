using UnityEngine;

public sealed class PlayerAiming : MonoBehaviour, ICharacterAiming
{
    [SerializeField]
    private Camera _camera;

    private InputProvider _inputManager;
    private TargetScanner _targetScanner;
    private IAimStrategy _dummyAimStrategy;
    private IAimStrategy _mouseAimStrategy;
    private IAimStrategy _gamepadAimStrategy;
    private IAimStrategy _gamepadBattleAimStrategy;

    // State
    private IAimStrategy _currentStrategy;
    private bool _hasTarget = false;
    private InputScheme _inputScheme = InputScheme.Unset;

    private void Awake()
    {
        _inputManager = FindFirstObjectByType<InputProvider>();
        _targetScanner = GetComponentInChildren<TargetScanner>();
    }

    private void Start()
    {
        _dummyAimStrategy = new DummyAimStrategy();
        _mouseAimStrategy = new MouseAimStrategy(
            _inputManager.PlayerActions,
            transform,
            _camera
        );
        _gamepadAimStrategy = new GamepadAimStrategy(
            _inputManager.PlayerActions
        );
        _gamepadBattleAimStrategy = new GamepadBattleAimStrategy(
            _inputManager.PlayerActions,
            transform,
            _targetScanner
        );
        _currentStrategy = _dummyAimStrategy;
    }

    private void OnEnable()
    {
        _inputManager.OnInputSchemeChanged += HandleChangeInputScheme;
    }

    private void OnDisable()
    {
        _inputManager.OnInputSchemeChanged -= HandleChangeInputScheme;
    }

    public Direction GetDirection()
    {
        var directionVector = _currentStrategy.GetCharacterViewDirection();
        return DirectionUtils.VectorToDirection(directionVector);
    }

    public Vector2 GetDirectionVector()
    {
        return _currentStrategy.GetCharacterViewDirection();
    }

    private void HandleChangeInputScheme(InputScheme scheme)
    {
        _inputScheme = scheme;
        ChooseNewAimStrategy();
    }

    private void Update()
    {
        _targetScanner.ScanEnvironment(2f);
        _hasTarget = _targetScanner.GetNearestTarget() != null;
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

public enum InputScheme
{
    Unset,
    Gamepad,
    KeyboardMouse
}