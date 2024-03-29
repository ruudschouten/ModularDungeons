using System;
using System.Collections.Generic;
using Generation.Dungeon.Parts;
using MyMath.Primitives;
using MyMath.Random;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Generation.Dungeon
{
    public class RoomCreator : RandomGenerationComponent
    {
        [SerializeField] private Transform roomContainer;

        [Tooltip("The minimum amount of regular rooms there should be")] 
        [SerializeField] private int minimumRegularRooms;
        [Tooltip("Extra chance the generator uses to use room modifiers")]
        [SerializeField] private int extraModifierChanceProbability;
        [Space] 
        [SerializeField] [ReorderableList] private List<GameObject> floors;
        [SerializeField] [ReorderableList] private List<GameObject> ceilings;
        [SerializeField] [ReorderableList] private List<Column> columns;

        public Transform RoomContainer => roomContainer;
        public Dictionary<RoomModifier, List<Room>> Rooms => _rooms;

        public float LowestPoint => _lowestPoint;
        public float3 StartPosition => _startPosition;
        public float3 EndPosition => _endPosition;

        private float _lowestPoint = int.MaxValue;
        private float _highestPoint = -1;
        private float3 _startPosition;
        private float3 _endPosition;
        
        private Dictionary<RoomModifier, List<Room>> _rooms;

        private void Initialize()
        {
            _lowestPoint = int.MaxValue;
            _highestPoint = -1;
            _rooms = new Dictionary<RoomModifier, List<Room>>();
        }

        public void CreateRooms(IReadOnlyList<Tile> tiles)
        {
            Initialize();

            SetStaircases(tiles, out var highest, out var lowest);

            AssignModifiers(tiles, highest, lowest);

            CleanupTiles(tiles);
        }

        private void AssignModifiers(IReadOnlyList<Tile> tiles, Tile highest, Tile lowest)
        {
            // Calculate special tiles
            var remainingTiles = tiles.Count - 2;
            var specialTiles = remainingTiles > extraModifierChanceProbability
                ? Random.NextInt(0 + extraModifierChanceProbability, remainingTiles)
                : remainingTiles;

            // Remove the minimum regular tiles that are required
            specialTiles = specialTiles > minimumRegularRooms
                ? specialTiles - minimumRegularRooms
                : 0;

            CreateRooms(tiles, highest, lowest, specialTiles);

            _startPosition = lowest.BottomRectangle.Center;
            _endPosition = highest.BottomRectangle.Center;
            
            if (!_endPosition.Equals(_startPosition)) return;
            for (var i = tiles.Count - 1; i > 0; i--)
            {
                if (tiles[i].Type == TileType.Regular) continue;
                // Set endPosition to the last main room and stop looking for other positions
                _endPosition = tiles[i].BottomRectangle.Center;
                break;
            }
        }

        private void CreateRooms(IReadOnlyList<Tile> tiles, Tile highest, Tile lowest, int specialTiles)
        {
            var roomModifiers = Enum.GetNames(typeof(RoomModifier)).Length;
            foreach (var tile in tiles)
            {
                var modifier = RoomModifier.None;
                if (tile == highest || tile == lowest)
                {
                    CreateRoom(tile, RoomModifier.None, tile.Type);
                    continue;
                }

                if (specialTiles > 0)
                {
                    modifier = (RoomModifier) Random.NextInt(0, roomModifiers);
                    specialTiles--;
                }
                
                if (!_rooms.ContainsKey(modifier))
                {
                    _rooms.Add(modifier, new List<Room>());
                }
                
                _rooms[modifier].Add(CreateRoom(tile, modifier, tile.Type));
            }
        }

        private void SetStaircases(IReadOnlyList<Tile> tiles, out Tile highest, out Tile lowest)
        {
            // Get highest and lowest tile
            var highestIndex = 0;
            var lowestIndex = tiles.Count - 1;

            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile.HighestPoint > _highestPoint)
                {
                    _highestPoint = tile.HighestPoint;
                    highestIndex = i;
                    // Continue so the highest point can't also be the lowest point
                    continue;
                }

                if (tile.LowestPoint < _lowestPoint)
                {
                    _lowestPoint = tile.LowestPoint;
                    lowestIndex = i;
                }
            }

            tiles[lowestIndex].Type = TileType.StaircaseDown;
            tiles[highestIndex].Type = TileType.StaircaseUp;
            
            highest = tiles[highestIndex];
            lowest = tiles[lowestIndex];
        }

        protected Room CreateRoom(Tile tile, RoomModifier modifier, TileType type)
        {
            var tileScale = tile.transform.localScale;

            var room = new GameObject("Complete room").AddComponent<Room>();
            room.transform.SetParent(roomContainer);

            room.Id = tile.Id;
            room.TileType = tile.Type;
            room.Modifier = modifier;

            var floor = CreateFloor(tile, room, tileScale);

            switch (type)
            {
                case TileType.Regular:
                    break;
                case TileType.StaircaseDown:
                case TileType.StaircaseUp:
                    CreateStairRoom(tile, room, tileScale, floor);

                    break;
                case TileType.Main:
                    CreateMainRoom(tile, room, tileScale, floor);
                    break;
            }

            room.Center = tile.BottomRectangle.Center + new float3(0, .5f, 0);

            return room;
        }

        protected virtual void CleanupTiles(IReadOnlyList<Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                Destroy(tile.gameObject);
            }
        }

        private void CreateStairRoom(Tile tile, Room room, Vector3 tileScale, GameObject floor)
        {
            for (var i = 0; i < 4; i++)
            {
                var height = tile.TopRectangle.CornerFromIndex(i).y - tile.BottomRectangle.CornerFromIndex(i).y;
                CreateColumn(room, tileScale, tile.BottomRectangle, height / 1.5f,
                    floor.transform.localScale.y, 1f, i);
            }
        }

        private void CreateMainRoom(Tile tile, Room room, Vector3 tileScale, GameObject floor)
        {
            var ceiling = CreateCeiling(tile, room, tileScale);

            for (var i = 0; i < 4; i++)
            {
                var height = tile.TopRectangle.CornerFromIndex(i).y - tile.BottomRectangle.CornerFromIndex(i).y;
                CreateColumn(room, tileScale, tile.BottomRectangle, height,
                    floor.transform.localScale.y, ceiling.transform.localScale.y, i);
            }
        }

        private GameObject CreateFloor(Tile tile, Room room, Vector3 scale)
        {
            // Instantiate a randomly selected floor prefab in the center of the lower rectangle of the tile.
            var floor = Instantiate(RandomFromList(floors), tile.BottomRectangle.Center, Quaternion.identity,
                room.transform);
            floor.transform.localScale = new Vector3(scale.x, floor.transform.localScale.y, scale.z);

            return floor;
        }

        private GameObject CreateCeiling(Tile tile, Room room, Vector3 scale)
        {
            // Instantiate a randomly selected ceiling prefab in the center of the upper rectangle of the tile.
            var ceiling = Instantiate(RandomFromList(ceilings), tile.TopRectangle.Center, Quaternion.identity,
                room.transform);
            ceiling.transform.localScale = new Vector3(scale.x, ceiling.transform.localScale.y, scale.z);

            return ceiling;
        }

        private void CreateColumn(Room room, Vector3 scale, Rectangle bottom, float height,
            float floorThickness, float ceilingThickness, int index)
        {
            // Get a random column
            var column = Instantiate(RandomFromList(columns), room.transform);

            // Put the column in one of the four corners of the tile
            column.SetSize(
                bottom.CornerFromIndex(index),
                height,
                bottom.SideFromIndex(index),
                floorThickness,
                ceilingThickness,
                scale
            );
        }
    }
}
