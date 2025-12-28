#nullable enable
using System;
using SamuraiProject.Character;
using SamuraiProject.Combat;
using SamuraiProject.Core.Utils;
using UnityEngine;

namespace SamuraiProject.AI
{
    [RequireComponent(typeof(TargetScanner))]
    [RequireComponent(typeof(IAIInputCollector))]
    public class AIBrain : MonoBehaviour, ICharacterBrain
    {
        [SerializeField]
        protected IAIInputCollector AiInputCollector = null!;
        [SerializeField]
        private TextAsset? _aiBrainWeights;

        protected SimpleNeuralNetwork? Brain;

        public event Action? OnLightAttack;
        public event Action? OnHeavyAttack;
        public event Action? OnParry;
        public event Action? OnHoldBlock;
        public event Action? OnReleaseBlock;

        internal float InitBrain(byte[] brainWeights)
        {
            Brain = SimpleNeuralNetwork.LoadFromMemory(brainWeights);
            return Brain.Fitness;
        }

        public ThinkResult Think(float time)
        {
            var collectedInput = AiInputCollector.Collect();
            var output = Brain!.FeedForward(collectedInput);
            var moveVector = HandleOutputs(output);

            return new ThinkResult(
                moveVector.normalized,
                moveVector.magnitude,
                GetDirection(moveVector)
            );
        }

        protected void Awake()
        {
            AiInputCollector = GetComponent<IAIInputCollector>();
            if (Brain == null && _aiBrainWeights != null)
            {
                Brain = SimpleNeuralNetwork.LoadFromMemory(_aiBrainWeights.bytes);
            }
        }

        public Direction GetDirection(Vector2 moveVector)
        {
            var target = AiInputCollector.CurrentTarget;
            Vector2 result;

            if (target != null && target.Transform != null)
            {
                result = target.Transform.position - transform.position;
            }
            else
            {
                result = moveVector;
                if (result.sqrMagnitude < 0.001f)
                {
                    return DirectionUtils.VectorToDirection(transform.up);
                }
            }

            return DirectionUtils.VectorToDirection(result.normalized);
        }

        private Vector2 HandleOutputs(ReadOnlySpan<float> outputs)
        {
            var moveVector = new Vector2(outputs[0], outputs[1]);

            int bestActionIndex = -1;
            float maxVal = 0.2f;

            for (int i = 2; i <= 5; i++)
            {
                if (outputs[i] > maxVal)
                {
                    maxVal = outputs[i];
                    bestActionIndex = i;
                }
            }

            switch (bestActionIndex)
            {
                case 2:
                    OnReleaseBlock?.Invoke();
                    OnLightAttack?.Invoke();
                    break;
                case 3:
                    OnReleaseBlock?.Invoke();
                    OnHeavyAttack?.Invoke();
                    break;
                case 4:
                    OnHoldBlock?.Invoke();
                    break;
                case 5:
                    OnReleaseBlock?.Invoke();
                    OnParry?.Invoke();
                    break;
            }

            return moveVector;
        }
    }
}
