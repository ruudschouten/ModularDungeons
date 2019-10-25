using System.Collections;
using MyMath.Random;
using MyMath.Random.Types;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Generation.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] protected bool generateOnAwake;
        [SerializeField] protected bool includeSmallTiles;
        
        [SerializeField] protected TileGenerator tileGenerator;
        [SerializeField] protected PathwayGenerator pathwayGenerator;
        [SerializeField] protected RoomCreator roomCreator;
        [SerializeField] protected RandomGenerationType generationType;
        [ShowIf("IsBiased")] [SerializeField] protected int rollCount;
         
        [SerializeField] protected bool useSeededRandom;
        [ShowIf("useSeededRandom")] [SerializeField] protected uint seed;
        
        [SerializeField] protected UnityEvent onGenerationDoneEvent;

        public float3 StartPosition => roomCreator.StartPosition;
        public float3 EndPosition => roomCreator.EndPosition;
        public UnityEvent OnGenerationDoneEvent => onGenerationDoneEvent;

        private void Awake()
        {
            if (!generateOnAwake) return;
            
            Generate();
        }

        public virtual void Generate()
        {
            if (!Application.isPlaying) return;
            StartCoroutine(GenerateRoutine());
        }

        public virtual IEnumerator GenerateRoutine()
        {
            Initialize();
            
            yield return tileGenerator.GenerateRoutine(includeSmallTiles);
            
            pathwayGenerator.GeneratePathways(tileGenerator.EdgeEndpoints);
            roomCreator.CreateRooms(includeSmallTiles ? tileGenerator.Tiles : tileGenerator.MainTiles);

            CleanUp();
            
            onGenerationDoneEvent.Invoke();
        }
            
        public virtual void Initialize()
        {
            RemoveChildren(tileGenerator.TileContainer);
            RemoveChildren(pathwayGenerator.PathContainer);
            RemoveChildren(roomCreator.RoomContainer);

            if (!useSeededRandom)
            {
                seed = (uint) new System.Random().Next();
            }
            
            tileGenerator.SetRandomType(generationType, seed, rollCount);
            roomCreator.SetRandomType(generationType, seed, rollCount);
        }

        public virtual void CleanUp()
        {
            RemoveChildren(tileGenerator.TileContainer);
        }

        protected void RemoveChildren(Transform t)
        {
            foreach (Transform child in t)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Used solely for the `showIf` Attribute
        private bool IsBiased()
        {
            return generationType == RandomGenerationType.Biased;
        }
    }
}
