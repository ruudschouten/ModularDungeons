using System.Collections;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Generation.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] protected bool generateOnAwake;
        
        [SerializeField] protected TileGenerator tileGenerator;
        [SerializeField] protected PathwayGenerator pathwayGenerator;
        [SerializeField] protected RoomCreator roomCreator;
         
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
            
            yield return tileGenerator.GenerateRoutine();
            
            pathwayGenerator.GeneratePathways(tileGenerator.EdgeEndpoints);
            roomCreator.CreateRooms(tileGenerator.MainTiles);
            
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
            
            tileGenerator.SetSeed(seed);
            roomCreator.SetSeed(seed);
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
    }
}
