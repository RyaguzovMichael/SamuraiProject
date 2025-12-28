#nullable enable
using System;
using SamuraiProject.Combat;
using UnityEngine;

namespace SamuraiProject.AI.Bots
{
    [RequireComponent(typeof(TargetScanner))]
    [RequireComponent(typeof(StaminaSystem))]
    [RequireComponent(typeof(CombatController))]
    public class V1BotInputCollector : MonoBehaviour, IAIInputCollector
    {
        [Header("Sensors")]
        [SerializeField] private int _obstacleRaysCount = 12;
        [SerializeField] private float _viewRadius = 15f;

        [SerializeField] private TargetScanner _scanner = null!;
        [SerializeField] private StaminaSystem _stamina = null!;
        [SerializeField] private CombatController _combat = null!;
        [SerializeField] private LayerMask _obstacleLayer;

        private float[] _inputs;

        public event Action<ITargetable>? ChangeTarget;

        public ITargetable? CurrentTarget { get; private set; }

        public ReadOnlySpan<float> Collect()
        {
            FindTarget();
            CollectInputs();
            return _inputs;
        }

        public bool HasValidTarget()
        {
            return CurrentTarget != null &&
                   CurrentTarget.Transform != null &&
                   CurrentTarget.CombatController.State != CombatState.Dead;
        }

        public void Init(LayerMask targetLayer)
        {
            _scanner.SetTargetMask(targetLayer);
        }

        private void Awake()
        {
            _scanner = GetComponent<TargetScanner>();
            _stamina = GetComponent<StaminaSystem>();
            _combat = GetComponent<CombatController>();
            int inputSize = _obstacleRaysCount + 7 + 2 + 1 + 7;
            _inputs = new float[inputSize];
        }

        private void FindTarget()
        {
            // 1. Ближний поиск
            _scanner.ScanEnvironment(_viewRadius);
            ITargetable potentialTarget = _scanner.GetNearestTarget();

            // 2. Глобальный поиск (если никого рядом)
            if (potentialTarget == null)
            {
                _scanner.ScanEnvironment(500f);
                potentialTarget = _scanner.GetNearestTarget();
            }

            if (CurrentTarget != potentialTarget)
            {
                CurrentTarget = potentialTarget;
                ChangeTarget?.Invoke(CurrentTarget);
            }
        }

        private void CollectInputs()
        {
            int index = 0;
            Vector2 position = transform.position;
            float angleStep = 360f / _obstacleRaysCount;

            // 1. Rays
            for (int i = 0; i < _obstacleRaysCount; i++)
            {
                float angle = i * angleStep;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                RaycastHit2D hit = Physics2D.Raycast(
                    position,
                    dir,
                    _viewRadius,
                    _obstacleLayer
                );
                var distance = hit.collider != null ? 1.0f - (hit.distance / _viewRadius) : 0.0f;
                _inputs[index++] = distance;
            }

            // 2. Target State
            if (HasValidTarget())
            {
                CombatState ts = CurrentTarget!.CombatController.State;
                for (int i = 0; i < 7; i++) _inputs[index++] = (int)ts == i ? 1f : 0f;
            }
            else
            {
                for (int i = 0; i < 7; i++) _inputs[index++] = 0f;
            }

            // 3. Compass
            if (HasValidTarget())
            {
                Vector2 toTarget = CurrentTarget!.Transform.position - transform.position;
                _inputs[index++] = Mathf.Clamp01(toTarget.magnitude / _viewRadius);
                _inputs[index++] = Vector2.SignedAngle(transform.up, toTarget) / 180f;
            }
            else
            {
                _inputs[index++] = 1f; _inputs[index++] = 0f;
            }

            // 4. Stamina
            _inputs[index++] = _stamina.Stamina / _stamina.MaxStamina;

            // 5. Self State
            CombatState ss = _combat.State;
            for (int i = 0; i < 7; i++) _inputs[index++] = (int)ss == i ? 1f : 0f;
        }

    }
}
