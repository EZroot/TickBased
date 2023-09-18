using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public class GridChunkMesh : MonoBehaviour
    {
        public Material MaterialToCopy;
        public MeshFilter MeshFilter;
        public MeshRenderer MeshRenderer;

        public GridManager.GridCoordinate ChunkPosition;

        void Start()
        {
            var newMaterial = new Material(MaterialToCopy);
            MeshRenderer.material = newMaterial;
        }
    }
}