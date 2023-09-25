using UnityEngine;

namespace TickBased.Utils
{
    public static class MeshUtils
    {

        public static void CreateSquareUV(int x, int y, float tileSize, Vector3[] vertices, int[] triangles,
            Color[] colors, Vector2[] uvs, float uvX, float uvY, float uvWidth, float uvHeight,
            ref int vertexIndex, ref int triangleIndex)
        {
            float alpha = 1;

            vertices[vertexIndex] = new Vector3(x * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 1] = new Vector3((x + 1) * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 2] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, 0);
            vertices[vertexIndex + 3] = new Vector3(x * tileSize, (y + 1) * tileSize, 0);

            // Generate UVs
            uvs[vertexIndex] = new Vector2(uvX, uvY);
            uvs[vertexIndex + 1] = new Vector2(uvX + uvWidth, uvY);
            uvs[vertexIndex + 2] = new Vector2(uvX + uvWidth, uvY + uvHeight);
            uvs[vertexIndex + 3] = new Vector2(uvX, uvY + uvHeight);
            
            // Flip the normals by reversing the triangle winding order
            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 3;
            triangles[triangleIndex + 2] = vertexIndex + 1;
            triangles[triangleIndex + 3] = vertexIndex + 3;
            triangles[triangleIndex + 4] = vertexIndex + 2;
            triangles[triangleIndex + 5] = vertexIndex + 1;

            Color color = new Color(0, 0, 0, alpha);

            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;

            vertexIndex += 4;
            triangleIndex += 6;
        }

        public static void CreateSquare(int x, int y, float tileSize, Vector3[] vertices, Vector2[] uvs, int[] triangles,
            Color[] colors, ref int vertexIndex, ref int triangleIndex)
        {
            float alpha = 1;

            vertices[vertexIndex] = new Vector3(x * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 1] = new Vector3((x + 1) * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 2] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, 0);
            vertices[vertexIndex + 3] = new Vector3(x * tileSize, (y + 1) * tileSize, 0);

            // Generate UVs
            uvs[vertexIndex] = new Vector2(0, 0);
            uvs[vertexIndex + 1] = new Vector2(1, 0);
            uvs[vertexIndex + 2] = new Vector2(1, 1);
            uvs[vertexIndex + 3] = new Vector2(0, 1);
            
            // Flip the normals by reversing the triangle winding order
            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 3;
            triangles[triangleIndex + 2] = vertexIndex + 1;
            triangles[triangleIndex + 3] = vertexIndex + 3;
            triangles[triangleIndex + 4] = vertexIndex + 2;
            triangles[triangleIndex + 5] = vertexIndex + 1;

            Color color = new Color(0, 0, 0, alpha);

            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;

            vertexIndex += 4;
            triangleIndex += 6;
        }

        public static void CreateSquareByColor(int x, int y, float tileSize, Vector3[] vertices,  Vector2[] uvs,int[] triangles,
            Color[] colors, Color defaultColor, ref int vertexIndex, ref int triangleIndex)
        {
            float alpha = 1;

            vertices[vertexIndex] = new Vector3(x * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 1] = new Vector3((x + 1) * tileSize, y * tileSize, 0);
            vertices[vertexIndex + 2] = new Vector3((x + 1) * tileSize, (y + 1) * tileSize, 0);
            vertices[vertexIndex + 3] = new Vector3(x * tileSize, (y + 1) * tileSize, 0);

            // Generate UVs
            uvs[vertexIndex] = new Vector2(0, 0);
            uvs[vertexIndex + 1] = new Vector2(1, 0);
            uvs[vertexIndex + 2] = new Vector2(1, 1);
            uvs[vertexIndex + 3] = new Vector2(0, 1);
            
            // Flip the normals by reversing the triangle winding order
            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 3;
            triangles[triangleIndex + 2] = vertexIndex + 1;
            triangles[triangleIndex + 3] = vertexIndex + 3;
            triangles[triangleIndex + 4] = vertexIndex + 2;
            triangles[triangleIndex + 5] = vertexIndex + 1;

            Color color = defaultColor;

            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;

            vertexIndex += 4;
            triangleIndex += 6;
        }
    }
}