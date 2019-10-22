using MyMath.Primitives;
using UnityEngine;

namespace Generation.Dungeon.Parts
{
    public class Column : MonoBehaviour
    {
        [SerializeField] private Transform topPart;
        [SerializeField] private Transform pillar;
        [SerializeField] private Transform bottomPart;

        public void SetSize(Vector3 bottom, Vector3 top, RectangleSide direction, float floorThickness, 
            float ceilingThickness, Vector3 tileScale)
        {
            var height = (top.y - bottom.y);

            var smallerScale = tileScale.x < tileScale.z ? tileScale.x : tileScale.z;

            topPart.localScale =
                new Vector3(smallerScale / 4, topPart.localScale.y * (tileScale.y / 2), smallerScale / 4);
            bottomPart.localScale =
                new Vector3(smallerScale / 4, bottomPart.localScale.y * (tileScale.y / 2), smallerScale / 4);
            pillar.localScale = new Vector3(smallerScale / 8, height - ceilingThickness, smallerScale / 8);


            pillar.localPosition = new Vector3(0, height / 2, 0);
            topPart.localPosition = new Vector3(0, height - (ceilingThickness * 0.75f), 0);
            bottomPart.localPosition = new Vector3(0, floorThickness * 0.75f, 0);

            switch (direction)
            {
                case RectangleSide.TopLeft:
                    transform.localPosition = new Vector3(
                        bottom.x - (bottomPart.localScale.x / 2),
                        bottom.y,
                        bottom.z - (topPart.localScale.z / 2));
                    break;
                case RectangleSide.TopRight:
                    transform.localPosition = new Vector3(
                        bottom.x - (bottomPart.localScale.x / 2),
                        bottom.y,
                        bottom.z + (topPart.localScale.z / 2));
                    break;
                case RectangleSide.BottomLeft:
                    transform.localPosition = new Vector3(
                        bottom.x + (bottomPart.localScale.x / 2),
                        bottom.y,
                        bottom.z - (topPart.localScale.z / 2));
                    break;
                case RectangleSide.BottomRight:
                    transform.localPosition = new Vector3(
                        bottom.x + (bottomPart.localScale.x / 2),
                        bottom.y,
                        bottom.z + (topPart.localScale.z / 2));
                    break;
            }
        }
    }
}