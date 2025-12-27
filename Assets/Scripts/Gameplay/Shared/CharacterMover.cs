using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(IMovementInputSource))]
public sealed class CharacterMover : MonoBehaviour, ICharacterMovement
{
    [SerializeField] private MoveConfig _config;
    private Rigidbody2D _rg;
    private IMovementInputSource _inputSource;

    //State
    private Vector2 _moveVector;

    public bool IsMove => _moveVector.sqrMagnitude > 0;
    public Vector2 MoveVector => _moveVector;

    private void Awake()
    {
        _rg = GetComponent<Rigidbody2D>();
        _inputSource = GetComponent<IMovementInputSource>();
    }

    private void Update()
    {
        var moveInput = _inputSource.MoveVector;
        var moveDirection = moveInput.normalized;
        var inputScale = Math.Min(moveInput.magnitude, 1);
        var moveVector = _config.MoveSpeed * inputScale * moveDirection;
        moveVector.y *= _config.VerticalSpeedMultiplier;
        _moveVector = moveVector;
    }

    private void FixedUpdate()
    {
        _rg.linearVelocity = _moveVector;
    }
}