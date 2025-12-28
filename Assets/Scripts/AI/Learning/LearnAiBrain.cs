#nullable enable
using System;
using SamuraiProject.AI.Abstractions;
using SamuraiProject.AI.Configs;
using SamuraiProject.Character;
using SamuraiProject.Combat;
using UnityEngine;

namespace SamuraiProject.AI
{
    [RequireComponent(typeof(TargetScanner))]
    [RequireComponent(typeof(CombatController))]
    public sealed class LearnAiBrain : AIBrain, IBot
    {
        [SerializeField]
        private CombatController _combat = null!;
        [SerializeField]
        private EvolutionConfig _config = null!;

        private bool _isAlive = false;
        private LayerMask _obstacleLayer;
        private bool _isTouchingWall = false;
        private float _previousDistanceToTarget = float.MaxValue;

        public float Fitness { get; private set; } = 0;
        public event Action? Dead;

        public new ThinkResult Think(float time)
        {
            var result = base.Think(time);
            UpdateRewardsForDistance();
            Fitness -= _config.TimePenaltyPerSec * time;
            if (_isTouchingWall)
            {
                Fitness -= _config.WallContactPenalty * time;
            }
            return result;
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
            // TODO: betsBot not contain LearnAiBrain
            if (bestBot is LearnAiBrain otherController && otherController.Brain != null)
            {
                Brain?.OverwriteFrom(otherController.Brain);
                Brain?.Mutate(mutateRate, mutateForce);
            }
        }

        public void Initialize(
            byte[] botBrain,
            LayerMask obstacleLayer
        )
        {
            Fitness = InitBrain(botBrain);
            _obstacleLayer = obstacleLayer;
        }

        public void Reset()
        {
            _isAlive = true;
            _previousDistanceToTarget = 0f;
            _combat.Reset();
            Fitness = 0;
        }

        public void SaveBrainToFile(string savePath)
        {
            Brain?.SaveBinary(savePath);
        }

        public void SetPosition(Vector3 position)
        {
            gameObject.transform.position = position;
        }

        private void UpdateRewardsForDistance()
        {
            if (!AiInputCollector.HasValidTarget()) return;

            float currentDist = Vector2.Distance(
                transform.position,
                AiInputCollector.CurrentTarget!.Transform.position
            );
            float delta = _previousDistanceToTarget - currentDist;

            if (_previousDistanceToTarget > 0.001f && Mathf.Abs(delta) > 0.001f)
            {
                if (delta > 0)
                {
                    // Приблизился
                    Fitness += _config.DistanceRewardMultiplier;
                }
                else
                {
                    // Убежал
                    Fitness -= _config.RetreatPenaltyMultiplier;
                }
            }

            _previousDistanceToTarget = currentDist;
        }

        private new void Awake()
        {
            base.Awake();
            _combat = GetComponent<CombatController>();
        }

        private void OnEnable()
        {
            AiInputCollector.ChangeTarget += ChangeTargetHandler;
            _combat.OnHeavyAttack += OnHeavyAttackHandler;
            _combat.OnLightAttack += OnLightAttackHandler;
            _combat.OnDie += OnDieHandler;
            _combat.OnKill += OnKillHandler;
            _combat.OnParryMiss += OnParryMissHandler;
            _combat.OnParrySuccess += OnParrySuccessHandler;
            _combat.OnLightAttackWhiff += OnWhiffLightAttackHandler;
            _combat.OnHeavyAttackWhiff += OnWhiffHeavyAttackHandler;
        }

        private void OnDisable()
        {
            AiInputCollector.ChangeTarget -= ChangeTargetHandler;
            _combat.OnHeavyAttack -= OnHeavyAttackHandler;
            _combat.OnLightAttack -= OnLightAttackHandler;
            _combat.OnDie -= OnDieHandler;
            _combat.OnKill -= OnKillHandler;
            _combat.OnParryMiss -= OnParryMissHandler;
            _combat.OnParrySuccess -= OnParrySuccessHandler;
            _combat.OnLightAttackWhiff -= OnWhiffLightAttackHandler;
            _combat.OnHeavyAttackWhiff -= OnWhiffHeavyAttackHandler;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & _obstacleLayer) != 0)
            {
                _isTouchingWall = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & _obstacleLayer) != 0)
            {
                _isTouchingWall = false;
            }
        }

        private void ChangeTargetHandler(ITargetable newTarget)
        {
            _previousDistanceToTarget = Vector2.Distance(
                    transform.position,
                    newTarget.Transform.position
                );
        }

        private void OnLightAttackHandler()
        {
            Fitness += _config.LightAttacReward;
        }

        private void OnHeavyAttackHandler()
        {
            Fitness += _config.HeavyAttackReward;
        }

        private void OnWhiffLightAttackHandler()
        {
            Fitness -= _config.WhiffLightAttackPenalty;
        }

        private void OnWhiffHeavyAttackHandler()
        {
            Fitness -= _config.WhiffHeavyAttackPenalty;
        }

        private void OnParryMissHandler()
        {
            Fitness -= _config.ParryMissPenalty;
        }

        private void OnParrySuccessHandler()
        {
            Fitness += _config.ParrySuccessReward;
        }

        private void OnKillHandler()
        {
            Fitness += _config.KillReward;
        }

        private void OnDieHandler()
        {
            Fitness -= _config.DeathPenalty;
            _isAlive = false;
            Dead?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
