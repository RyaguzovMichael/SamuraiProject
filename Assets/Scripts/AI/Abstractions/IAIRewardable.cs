#nullable enable
using System;

namespace UnityEngine.AI.Abstractions
{
    public interface IAIRewardable
    {
        public event Func<float, float, float>? TimeTick;
        public event Func<float, float, float>? DistanceReduce;
        public event Func<float, float, float>? DistanceEnrich;
        public event Func<float, float>? Kill;
        public event Func<float, float>? SuccessParry;
        public event Func<float, float>? Hit;
        public event Func<float, float>? Death;
        public event Func<float, float>? WhiffLightAtack;
        public event Func<float, float>? WhiffHeavyAtack;
        public event Func<float, float>? ParryMiss;
        public event Func<float, float>? ObstacleContact;
    }
}

