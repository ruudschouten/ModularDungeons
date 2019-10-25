using System;
using System.Collections.Generic;
using MyMath.Random;
using MyMath.Random.Interface;
using MyMath.Random.Types;
using UnityEngine;

namespace Generation.Dungeon
{
    public abstract class RandomGenerationComponent : MonoBehaviour
    {
        protected IRandom Random;
        
        public void SetRandomType(RandomGenerationType type, uint seed, int rollCount)
        {
            switch (type)
            {
                case RandomGenerationType.Regular:
                    Random = new RegularRandom(seed);
                    break;
                case RandomGenerationType.Biased:
                    Random = new BiasedRandom(seed, rollCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        protected T RandomFromList<T>(IReadOnlyList<T> list)
        {
            return list[Random.NextInt(0, list.Count - 1)];
        }
    }
}