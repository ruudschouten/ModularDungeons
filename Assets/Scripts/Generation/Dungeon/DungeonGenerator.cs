using System.Collections;
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

        [SerializeField] private bool showEvents;
        [ShowIf("showEvents")] [SerializeField] protected UnityEvent onGenerationStartEvent;
        [ShowIf("showEvents")] [SerializeField] protected UnityEvent onGenerationDoneEvent;

        public float3 StartPosition => roomCreator.StartPosition;
        public float3 EndPosition => roomCreator.EndPosition;
        public UnityEvent OnGenerationStartEvent => onGenerationStartEvent;
        public UnityEvent OnGenerationDoneEvent => onGenerationDoneEvent;

        public TileGenerator TileGenerator => tileGenerator;
        public uint Seed => seed;

        protected virtual void Awake()
        {
            if (!generateOnAwake) return;
            
            Generate();
        }

        [Button("Generate")]
        public virtual void Generate()
        {
            if (!Application.isPlaying) return;
            StartCoroutine(GenerateRoutine());
        }

        public virtual IEnumerator GenerateRoutine()
        {
            Initialize();
            
            onGenerationStartEvent.Invoke();
            
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
        protected bool IsBiased()
        {
            return generationType == RandomGenerationType.Biased;
        }
    }
}
