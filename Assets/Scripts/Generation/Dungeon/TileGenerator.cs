using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Generation.Dungeon.Parts;
using MyMath;
using MyMath.Primitives;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

namespace Generation.Dungeon
{
    public class TileGenerator : RandomGenerationComponent
    {
        #region Editor Fields
        
        [SerializeField] private Tile prefab;
        [SerializeField] private Transform tileContainer;
        [SerializeField] private int radius;
        [SerializeField] private int tileAmount;
        [Tooltip("The size of the tiles\nxMin\t yMin\t zMin\nxMax\t yMax\t zMax")]
        [SerializeField] private float2x3 tileSize;

        [MinMaxSlider(-25, 25)] [SerializeField] private Vector2 verticalPosition;
        [SerializeField] 
        [Tooltip("Time to wait for generation before removing all tiles not in a correct position")] 
        private float maxTimeToWait = 2.5f;

        [SerializeField] [MinValue(.5f)] 
        [Tooltip("Decides when a tile becomes a main tile \n(tileSize > averageSize * meanFactor)\n" +
                 "Is Ignored if DungeonGenerator has `includeSmallTiles` enabled")] 
        private float meanFactor;
        
        #endregion

        #region Properties
        
        public Transform TileContainer => tileContainer;
        public List<EdgeEndpoint> EdgeEndpoints => _endpoints;
        public List<Tile> MainTiles => _mainTiles;
        public List<Tile> Tiles => _tiles;

        public int Radius
        {
            get => radius;
            set => radius = value;
        }

        public int TileAmount
        {
            get => tileAmount;
            set => tileAmount = value;
        }

        #endregion

        #region Private Fields
        
        private VectorMean _vectorMean;
        private List<Tile> _tiles = new List<Tile>();
        private List<Tile> _mainTiles = new List<Tile>();
        private List<EdgeEndpoint> _endpoints = new List<EdgeEndpoint>();
        private bool _includeSmallTiles;

        #endregion
        
        public IEnumerator GenerateRoutine(bool includeSmallTiles)
        {
            _includeSmallTiles = includeSmallTiles;
            
            Initialize();

            GeneratePoints();

            SelectMainTiles();

            yield return WaitForCollisionResolved();
        }

        private void Initialize()
        {
            _endpoints = new List<EdgeEndpoint>();
            _vectorMean = new VectorMean();
            
            _tiles = new List<Tile>();
            _mainTiles = new List<Tile>();
        }

        private void GeneratePoints()
        {
            for (var i = 0; i < tileAmount; i++)
            {
                var pos = UnityEngine.Random.insideUnitSphere * radius;
                pos.y = Random.NextFloat(verticalPosition.x, verticalPosition.y);
                var tile = Instantiate(prefab, tileContainer);
                tile.transform.localPosition = pos;
                tile.Type = TileType.Regular;
                AlterTile(tile.transform);
                _tiles.Add(tile);
            }
        }

        private void AlterTile(Transform trans)
        {
            var newScale = new Vector3(
                Random.NextFloat(tileSize.c0.x, tileSize.c0.y),
                Random.NextFloat(tileSize.c1.x, tileSize.c1.y),
                Random.NextFloat(tileSize.c2.x, tileSize.c2.y));

            trans.localScale = newScale;

            _vectorMean.Add(newScale);
        }

        private void SelectMainTiles()
        {
            var mean = _vectorMean.CalculateMeans() * meanFactor;
            var index = 0;
            
            foreach (var tile in _tiles)
            {
                tile.Id = index;
                index++;
                
                if (tile.CollectiveSize <= mean) continue; 
                
                tile.Type = TileType.Main;
            }
        }

        private IEnumerator WaitForCollisionResolved()
        {
            yield return new WaitForEndOfFrame();

            var timer = 0f;
            yield return null;
            // Wait until either the tiles are no longer colliding, or until the timer hits maxTimeToWait
            while (_tiles.Count(x => x.IsColliding) > 0)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer > maxTimeToWait) break;
            }

            yield return null;

            // Remove any leftover tiles that are still colliding
            if (_tiles.Any(x => x.IsColliding))
            {
                var cachedTiles = new List<Tile>(_tiles);
                foreach (var tilePair in cachedTiles)
                {
                    if (!tilePair.IsColliding) continue;

                    _tiles.Remove(tilePair);
                    Destroy(tilePair.gameObject);
                }
            }

            yield return null;
            
            
            _mainTiles = _tiles.Where(x => x.Type == TileType.Main).ToList();
            
            // Loop through all tiles and add their positions to the _endpoints list.
            // If includeSmallTiles is set to true, it'll include all tiles in the calculation
            foreach (var tile in _includeSmallTiles ? _tiles : _mainTiles)
            {
                var pos = tile.BottomRectangle.Center;
                _endpoints.Add(new EdgeEndpoint(pos, tile));
            }

            yield return null;
        }
    }
}