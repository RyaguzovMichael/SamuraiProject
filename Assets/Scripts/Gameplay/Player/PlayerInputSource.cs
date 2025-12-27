using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CombatController))]
public sealed class PlayerInputSource : MonoBehaviour, IMovementInputSource
{
    [SerializeField]
    private InputProvider _inputProvider;
    private CombatController _combatController;

    private void Awake()
    {
        _combatController = GetComponent<CombatController>();
    }

    private void OnEnable()
    {
        _inputProvider.PlayerActions.Attack.performed += HandleLightAttackButton;
        _inputProvider.PlayerActions.Interact.performed += HandleHeavyAttackButton;
        _inputProvider.PlayerActions.Jump.performed += HandleParyButton;
        _inputProvider.PlayerActions.Crouch.started += HandleBlockButtonPress;
        _inputProvider.PlayerActions.Crouch.canceled += HandleBlockButonRelease;
        _combatController.OnDie += () => Destroy(gameObject);
    }

    private void OnDisable()
    {
        _inputProvider.PlayerActions.Attack.performed -= HandleLightAttackButton;
        _inputProvider.PlayerActions.Interact.performed -= HandleHeavyAttackButton;
        _inputProvider.PlayerActions.Jump.performed -= HandleParyButton;
        _inputProvider.PlayerActions.Crouch.started -= HandleBlockButtonPress;
        _inputProvider.PlayerActions.Crouch.canceled -= HandleBlockButonRelease;
        _combatController.OnDie -= () => Destroy(gameObject);
    }

    private void HandleLightAttackButton(InputAction.CallbackContext context)
    {
        _combatController.AttemptLightAttack();
    }

    private void HandleHeavyAttackButton(InputAction.CallbackContext context)
    {
        _combatController.AttemptHeavyAttack();
    }

    private void HandleParyButton(InputAction.CallbackContext context)
    {
        _combatController.AttemptParry();
    }

    private void HandleBlockButtonPress(InputAction.CallbackContext context)
    {
        _combatController.StartBlock();
    }

    private void HandleBlockButonRelease(InputAction.CallbackContext context)
    {
        _combatController.EndBlock();
    }

    public Vector2 MoveVector => _inputProvider.PlayerActions.Move.ReadValue<Vector2>();
}
