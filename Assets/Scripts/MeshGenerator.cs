using System.Collections;
using System;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal static class MeshGenerator 
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
        {
            int width = heightMap.GetLength(0),
                height = heightMap.GetLength(1),
                meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2,
                verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

            float   topLeftX = (width - 1) / -2f,
                    topLeftZ = (height - 1) / 2f;

            MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
            int vertexIndex = 0;

            for(int j = 0; j < height; j+= meshSimplificationIncrement)
                for(int i = 0; i < width; i += meshSimplificationIncrement)
                {
                    meshData.vertices[vertexIndex] = new Vector3(topLeftX + i,
                        heightCurve.Evaluate(heightMap[i, j])* heightMultiplier,
                        topLeftZ - j);
                    meshData.uvs[vertexIndex] = new Vector2( i / (float)width, j /(float)height);

                    if(i < width - 1 && j < height - 1)
                    {
                        meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                        meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex , vertexIndex + 1);
                    }

                    vertexIndex++;
                }

            return meshData;
        }
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        private int triangleIndex;

        public MeshData(int meshWidth, int meshHeight)
        {
            vertices = new Vector3[meshHeight * meshWidth];
            uvs = new Vector2[meshHeight * meshWidth];
            triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
