using UnityEngine;

namespace Generation.Dungeon.Parts
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private TileType tileType;
        [SerializeField] private RoomModifier modifier;

        #region Properties

        public int Id
        {
            get => id;
            set => id = value;
        }

        public TileType TileType
        {
            get => tileType;
            set => tileType = value;
        }

        public RoomModifier Modifier
        {
            get => modifier;
            set => modifier = value;
        }
        
        public Vector3 Center { get; set; } 

        #endregion
    }
}