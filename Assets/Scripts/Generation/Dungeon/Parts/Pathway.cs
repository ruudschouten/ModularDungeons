using UnityEngine;

namespace Generation.Dungeon.Parts
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Pathway : MonoBehaviour
    {
        [SerializeField] private MeshFilter filter;
        [SerializeField] private new MeshRenderer renderer;
        [SerializeField] private new MeshCollider collider;

        [SerializeField] private Vector3[] vertices;
        [SerializeField] private int[] triangles;
        [SerializeField] private float weight;

        public float Weight
        {
            set => weight = value;
        }

        public Mesh Mesh
        {
            get => filter.mesh;
            set
            {
                filter.mesh = value;
                vertices = value.vertices;
                triangles = value.triangles;
                collider.sharedMesh = value;
            }
        }
    }
}