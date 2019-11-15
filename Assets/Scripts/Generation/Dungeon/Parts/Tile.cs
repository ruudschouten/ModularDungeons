using MyMath.Primitives;
using NaughtyAttributes;
using UnityEngine;

namespace Generation.Dungeon.Parts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Tile : MonoBehaviour
    {
        #region Editor Fields
        
        [SerializeField] private int id;
        [SerializeField] private bool isColliding;
        [SerializeField] private TileType type;
        [ShowIf("isColliding")] [SerializeField] private Vector3 direction;
        [ShowIf("isColliding")] [SerializeField] private Tile collidesWith;
        [SerializeField] private new Collider collider;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private MeshRenderer meshRenderer;
        
        [SerializeField] private float forceMultiplier = 500;
        [SerializeField] private Rectangle topRectangle;
        [SerializeField] private Rectangle bottomRectangle;
        [SerializeField] private bool debug;
        [ShowIf("debug")] [SerializeField] private float sphereRadius;

        #endregion
        
        #region Properties
        
        public int Id {
            get => id;
            set => id = value;
        }
        public TileType Type
        {
            get => type;
            set => type = value;
        }

        public MeshRenderer MeshRenderer => meshRenderer;
        public Collider Collider => collider;

        public bool IsColliding => isColliding;
        public float CollectiveSize => transform.localScale.x + transform.localScale.y + transform.localScale.z;
        public float Width => transform.localScale.x;
        public float Depth => transform.localScale.z;
        public float HorizontalSize => (Width + Depth) / 2;
        public Vector3 Center => collider.bounds.center;
        public float HighestPoint => TopRectangle.Center.y;
        public float LowestPoint => BottomRectangle.Center.y;
        public Rectangle TopRectangle
        {
            get
            {
                var scale = transform.localScale / 2;
                var topCenter = new Vector3(Center.x, Center.y + (scale).y, Center.z);

                topRectangle = new Rectangle(
                    new Vector3(topCenter.x - scale.x, topCenter.y, topCenter.z + scale.z),
                    new Vector3(topCenter.x + scale.x, topCenter.y, topCenter.z + scale.z),
                    new Vector3(topCenter.x + scale.x, topCenter.y, topCenter.z - scale.z),
                    new Vector3(topCenter.x - scale.x, topCenter.y, topCenter.z - scale.z),
                    topCenter
                );

                return topRectangle;
            }
        }
        public Rectangle BottomRectangle
        {
            get
            {
                var scale = transform.localScale / 2;
                var bottomCenter = new Vector3(Center.x, Center.y - (scale).y, Center.z);
                    
                bottomRectangle = new Rectangle(
                    new Vector3(bottomCenter.x - scale.x, bottomCenter.y, bottomCenter.z + scale.z),
                    new Vector3(bottomCenter.x + scale.x, bottomCenter.y, bottomCenter.z + scale.z),
                    new Vector3(bottomCenter.x + scale.x, bottomCenter.y, bottomCenter.z - scale.z),
                    new Vector3(bottomCenter.x - scale.x, bottomCenter.y, bottomCenter.z - scale.z),
                    bottomCenter
                );

                return bottomRectangle;
            }
        }

        #endregion
        
        private void OnDrawGizmos()
        {
            if (!debug) return;
            
            Gizmos.color = new Color(1f, 0f, 0f);
            Gizmos.DrawWireSphere(TopRectangle.LeftCenter, sphereRadius);
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(TopRectangle.RightCenter, sphereRadius);
            Gizmos.color = new Color(1f, 1f, 0f);
            Gizmos.DrawWireSphere(TopRectangle.TopCenter, sphereRadius);
            Gizmos.color = new Color(0.5f, 1f, 0f);
            Gizmos.DrawWireSphere(TopRectangle.BottomCenter, sphereRadius);

            Gizmos.color = new Color(0f, 1f, 0f);
            Gizmos.DrawWireSphere(BottomRectangle.LeftCenter, sphereRadius);
            Gizmos.color = new Color(0f, 1f, 1f);
            Gizmos.DrawWireSphere(BottomRectangle.RightCenter, sphereRadius);
            Gizmos.color = new Color(0f, 0f, 1f);
            Gizmos.DrawWireSphere(BottomRectangle.TopCenter, sphereRadius);
            Gizmos.color = new Color(0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(BottomRectangle.BottomCenter, sphereRadius);
        }

        private void FixedUpdate()
        {
            if (!isColliding) return;
            if (collidesWith == null)
            {
                direction = Vector3.zero;
            }
        }

        #region Collision

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Tile")) return;

            isColliding = true;
            direction = (collider.bounds.center - other.bounds.center).normalized;
            collidesWith = other.gameObject.GetComponent<Tile>();
            rigidbody.AddForce(direction * forceMultiplier);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.gameObject.CompareTag("Tile")) return;

            isColliding = true;
            direction = (collider.bounds.center - other.bounds.center).normalized;
            collidesWith = other.gameObject.GetComponent<Tile>();
            rigidbody.AddForce(direction * forceMultiplier);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag("Tile")) return;

            isColliding = false;
            direction = Vector3.zero;
            collidesWith = null;
            rigidbody.velocity = Vector3.zero;
        }

        #endregion
    }
}