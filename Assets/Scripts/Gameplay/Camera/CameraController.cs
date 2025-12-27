using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private CameraConfig _config;

    private Vector3 _currentVelocity = Vector3.zero;

    private void Awake()
    {
        if (_target == null)
        {
            Debug.LogError("Need to setup camera target in inspector");
        }
        if (_config == null)
        {
            Debug.LogError("Need to setup camera configuration in inspector");
        }
    }

    private void Start()
    {
        transform.position = _target.position + _config.Offset;
    }

    private void LateUpdate()
    {
        var targetPosition = _target.position + _config.Offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            _config.SmoothTime
        );
    }
}
