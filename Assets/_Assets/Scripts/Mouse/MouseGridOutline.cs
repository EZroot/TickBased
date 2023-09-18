using System;
using System.Collections;
using System.Collections.Generic;
using FearProj.ServiceLocator;
using TickBased.Utils;
using UnityEngine;

public class MouseGridOutline : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;

    public GridManager.GridCoordinate GridCoordinate;

    public void GenerateMouseGrid(GridManager.GridCoordinate gridCoordinate, int tileSize,  Color defaultColor )
    {
        // Create and populate a new Mesh object
        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];
        Color[] colors = new Color[4];
        int vertexIndex = 0, triangleIndex = 0;

        MeshUtils.CreateSquareByColor(gridCoordinate.X, gridCoordinate.Y, tileSize, vertices, triangles, colors,defaultColor, ref vertexIndex, ref triangleIndex);

        // Create a new GameObject to hold the Mesh
        GameObject tileObj = new GameObject("Tile");
        MeshFilter meshFilter = tileObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = tileObj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        _meshFilter.mesh = mesh;
        GridCoordinate = gridCoordinate;
    }

    public void SetPosition(Vector2 pos)
    {
        transform.localPosition = new Vector3(pos.x,pos.y,-0.2f);
    }
}
