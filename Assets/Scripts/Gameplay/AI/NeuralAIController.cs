using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIMoveInputSource))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(StaminaSystem))]
[RequireComponent(typeof(TargetScanner))]
public sealed class NeuralAIController : MonoBehaviour, IBot, ITargetable
{
    [Header("Sensors")]
    [SerializeField] private int _obstacleRaysCount = 12;
    [SerializeField] private float _viewRadius = 15f;

    [Header("Optimization")]
    [Tooltip("Interval in seconds between neural network updates")]
    [SerializeField] private float _thinkingInterval = 0.1f;

    // --- COMPONENTS ---
    private AIMoveInputSource _mover;
    private CombatController _combat;
    private StaminaSystem _stamina;
    private TargetScanner _scanner;

    // --- BRAIN & STATE ---
    private SimpleNeuralNetwork _brain;
    private bool _isAlive;
    private float[] _inputs;
    private float[] _cachedOutputs;
    private float _nextThinkTime;
    private readonly RewardEvents _rewardEvents = new();

    // --- TARGETING ---
    private ITargetable _currentTarget;
    private float _previousDistanceToTarget;
    private LayerMask _obstacleLayer;

    // --- IBot Implementation ---
    public RewardEvents RewardEvents => _rewardEvents;
    public event Action Dead;

    public float Fitness
    {
        get => _brain?.Fitness ?? 0f;
        set { if (_brain != null) _brain.Fitness = value; }
    }

    public Transform Transform => transform;
    public CombatController CombatController => _combat;


    private void Awake()
    {
        _mover = GetComponent<AIMoveInputSource>();
        _combat = GetComponent<CombatController>();
        _stamina = GetComponent<StaminaSystem>();
        _scanner = GetComponent<TargetScanner>();

        // ВХОДЫ: Rays(12) + TargetState(7) + Compass(2) + Stamina(1) + SelfState(7) = 29
        int inputSize = _obstacleRaysCount + 7 + 2 + 1 + 7;
        _inputs = new float[inputSize];
        _cachedOutputs = new float[6];

        SubscribeToCombatEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCombatEvents();
    }

    // --- IBot Methods ---

    public void Initialize(SimpleNeuralNetwork botBrain, LayerMask targetLayer, LayerMask obstacleLayer)
    {

        if (botBrain != null)
        {
            _brain = botBrain;
        }
        else
        {
            // Топология: 29 входов -> 20 скрытых -> 6 выходов
            int[] layers = new int[] { _inputs.Length, 20, 6 };
            _brain = new SimpleNeuralNetwork(layers);
        }
        _scanner.SetTargetMask(targetLayer);
        _obstacleLayer = obstacleLayer;
    }

    public void Reset()
    {
        _isAlive = true;
        _currentTarget = null;
        _previousDistanceToTarget = 0f;
        _nextThinkTime = Time.time + Random.Range(0f, _thinkingInterval);

        // Сброс физического состояния
        if (_combat != null) _combat.Reset();
        if (_brain != null) _brain.Fitness = 0f; // Сбрасываем фитнес на новый раунд

        gameObject.SetActive(true);
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        _isAlive = true;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        _isAlive = false;
    }

    public void Evolve(IBot bestBot, float mutateRate, float mutateForce)
    {
        if (bestBot is NeuralAIController otherController && otherController._brain != null)
        {
            _brain.OverwriteFrom(otherController._brain);
            _brain.Mutate(mutateRate, mutateForce);
        }
    }

    public void SaveBrainToFile(string savePath)
    {
        _brain?.SaveBinary(savePath);
    }

    // --- GAME LOOP ---

    private void FixedUpdate()
    {
        if (!_isAlive || _brain == null) return;

        // Отправляем событие тика времени для штрафа
        _rewardEvents.OnTimeTick(this, Time.fixedDeltaTime);

        if (Time.time >= _nextThinkTime)
        {
            Think();
            _nextThinkTime = Time.time + _thinkingInterval;
        }

        ApplyOutputs(_cachedOutputs);
    }

    private void Think()
    {
        FindTarget();
        UpdateRewardsForDistance();
        CollectInputs();
        _cachedOutputs = _brain.FeedForward(_inputs);
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

        if (_currentTarget != potentialTarget)
        {
            _currentTarget = potentialTarget;
            // Сбрасываем дельту, чтобы не получить рывок награды
            _previousDistanceToTarget = HasValidTarget() ? Vector2.Distance(transform.position, _currentTarget.Transform.position) : 0f;
        }
    }

    private void UpdateRewardsForDistance()
    {
        if (!HasValidTarget()) return;

        float currentDist = Vector2.Distance(transform.position, _currentTarget.Transform.position);
        float delta = _previousDistanceToTarget - currentDist;

        // Если бот двигался
        if (_previousDistanceToTarget > 0.001f && Mathf.Abs(delta) > 0.001f)
        {
            if (delta > 0)
            {
                // Сблизился
                _rewardEvents.OnDistanceReduce(this, delta);
            }
            else
            {
                // Убежал
                _rewardEvents.OnDistanceEnrich(this, Mathf.Abs(delta));
            }
        }

        _previousDistanceToTarget = currentDist;
    }

    private bool HasValidTarget()
    {
        return _currentTarget != null &&
               _currentTarget.Transform != null &&
               _currentTarget.CombatController.State != CombatState.Dead;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_isAlive) return;
        if (((1 << collision.gameObject.layer) & _obstacleLayer) != 0)
        {
            _rewardEvents.OnObstacleContact(this);
        }
    }

    // --- NEURAL LOGIC ---

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
            RaycastHit2D hit = Physics2D.Raycast(position, dir, _viewRadius, _obstacleLayer);
            var distance = hit.collider != null ? 1.0f - (hit.distance / _viewRadius) : 0.0f;
            _inputs[index++] = distance;
        }

        // 2. Target State
        if (HasValidTarget())
        {
            CombatState ts = _currentTarget.CombatController.State;
            for (int i = 0; i < 7; i++) _inputs[index++] = (int)ts == i ? 1f : 0f;
        }
        else
        {
            for (int i = 0; i < 7; i++) _inputs[index++] = 0f;
        }

        // 3. Compass
        if (HasValidTarget())
        {
            Vector2 toTarget = _currentTarget.Transform.position - transform.position;
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

    private void ApplyOutputs(float[] outputs)
    {
        // 0,1 - Movement
        _mover.SetMoveVector(new Vector2(outputs[0], outputs[1]));

        // Winner-Takes-All for Actions
        int bestActionIndex = -1;
        float maxVal = 0.2f; // Deadzone

        for (int i = 2; i <= 5; i++)
        {
            if (outputs[i] > maxVal)
            {
                maxVal = outputs[i];
                bestActionIndex = i;
            }
        }

        _combat.EndBlock();

        // Attack range check
        bool canAttack = true;
        if (HasValidTarget())
        {
            float distSqr = (_currentTarget.Transform.position - transform.position).sqrMagnitude;
            canAttack = distSqr <= 1.0f;
        }

        switch (bestActionIndex)
        {
            case 2: if (canAttack) _combat.AttemptLightAttack(); break;
            case 3: if (canAttack) _combat.AttemptHeavyAttack(); break;
            case 4: _combat.StartBlock(); break;
            case 5: _combat.AttemptParry(); break;
        }
    }

    // --- EVENT HANDLERS ---

    private void SubscribeToCombatEvents()
    {
        _combat.OnDie += OnDieHandler;
        _combat.OnKill += OnKillHandler;
        _combat.Events.OnLightAttack += OnLightAttackHandler;
        _combat.Events.OnHeavyAttack += OnHeavyAttackHandler;
        _combat.Events.OnParryMiss += OnParryMissHandler;
        _combat.Events.OnParrySuccess += OnParrySuccessHandler;
    }

    private void UnsubscribeFromCombatEvents()
    {
        if (_combat == null) return;
        _combat.OnDie -= OnDieHandler;
        _combat.OnKill -= OnKillHandler;
        _combat.Events.OnLightAttack -= OnLightAttackHandler;
        _combat.Events.OnHeavyAttack -= OnHeavyAttackHandler;
        _combat.Events.OnParryMiss -= OnParryMissHandler;
        _combat.Events.OnParrySuccess -= OnParrySuccessHandler;
    }

    private void OnLightAttackHandler() => _rewardEvents.OnWhiffLightAttack(this);
    private void OnHeavyAttackHandler() => _rewardEvents.OnWhiffHeavyAttack(this);
    private void OnParryMissHandler() => _rewardEvents.OnParryMiss(this);
    private void OnParrySuccessHandler() => _rewardEvents.OnSuccessParry(this);
    private void OnKillHandler() => _rewardEvents.OnKill(this);

    private void OnDieHandler()
    {
        _rewardEvents.OnDeath(this);
        _isAlive = false;
        Dead?.Invoke();
        gameObject.SetActive(false);
    }
}
