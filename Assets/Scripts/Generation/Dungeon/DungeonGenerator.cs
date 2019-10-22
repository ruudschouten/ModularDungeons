using System.Collections;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Generation.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] private bool generateOnAwake;
        
        [SerializeField] private TileGenerator tileGenerator;
        [SerializeField] private PathwayGenerator pathwayGenerator;
        [SerializeField] private RoomCreator roomCreator;
         
        [SerializeField] private bool useSeededRandom;
        [ShowIf("useSeededRandom")] [SerializeField] private uint seed;
        
        [SerializeField] private UnityEvent onGenerationDoneEvent;

        public float3 StartPosition => roomCreator.StartPosition;
        public float3 EndPosition => roomCreator.EndPosition;
        public UnityEvent OnGenerationDoneEvent => onGenerationDoneEvent;
        
        private void Awake()
        {
            if (!generateOnAwake) return;
            
            Generate();
        }

        protected virtual void Generate()
        {
            if (!Application.isPlaying) return;
            StartCoroutine(GenerateRoutine());
        }

        protected virtual IEnumerator GenerateRoutine()
        {
            Initialize();
            
            yield return tileGenerator.GenerateRoutine();
            
            pathwayGenerator.GeneratePathways(tileGenerator.EdgeEndpoints);
            roomCreator.CreateRooms(tileGenerator.MainTiles);
            
            CleanUp();
            
            onGenerationDoneEvent.Invoke();
        }
            
        protected virtual void Initialize()
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

        protected virtual void CleanUp()
        {
            RemoveChildren(tileGenerator.TileContainer);
        }

        private void RemoveChildren(Transform t)
        {
            foreach (Transform child in t)
            {
                Destroy(child.gameObject);
            }
        }
    }
}