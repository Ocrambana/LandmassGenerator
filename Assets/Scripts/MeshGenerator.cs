using System.Collections;
using System;
using UnityEngine;

namespace Ocrambana.LandmassGeneration
{
    internal static class MeshGenerator 
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail, bool useFlatShading)
        {
            AnimationCurve myHeightCurve = new AnimationCurve(heightCurve.keys);
            int borderedSize = heightMap.GetLength(0),
                meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2,
                meshSize = borderedSize - 2 * meshSimplificationIncrement,
                meshSizeUnsimplified = borderedSize - 2,
                verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

            float   topLeftX = (meshSizeUnsimplified - 1) / -2f,
                    topLeftZ = (meshSizeUnsimplified - 1) / 2f;

            MeshData meshData = new MeshData(verticesPerLine, useFlatShading);

            int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
            int meshVertexIndex = 0,
                borderVertexIndex = -1;

            for (int j = 0; j < borderedSize; j += meshSimplificationIncrement)
                for (int i = 0; i < borderedSize; i += meshSimplificationIncrement)
                {
                    bool isBorderVertex = j == 0 || j == borderedSize - 1 || i == 0 || i == borderedSize - 1;

                    if(isBorderVertex)
                    {
                        vertexIndicesMap[i, j] = borderVertexIndex;
                        borderVertexIndex--;
                    }
                    else
                    {
                        vertexIndicesMap[i, j] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }

            for (int j = 0; j < borderedSize; j += meshSimplificationIncrement)
                for(int i = 0; i < borderedSize; i += meshSimplificationIncrement)
                {
                    int vertexIndex = vertexIndicesMap[i, j];
                    Vector2 percent = new Vector2( ( i - meshSimplificationIncrement) / (float)meshSize, ( j - meshSimplificationIncrement ) / (float)meshSize);
                    float height = myHeightCurve.Evaluate(heightMap[i, j]) * heightMultiplier;
                    Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                    meshData.AddVertex(vertexPosition, percent, vertexIndex);

                    if(i < borderedSize - 1 && j < borderedSize - 1)
                    {
                        int a = vertexIndicesMap[i, j],
                            b = vertexIndicesMap[i + meshSimplificationIncrement, j],
                            c = vertexIndicesMap[i, j + meshSimplificationIncrement],
                            d = vertexIndicesMap[i + meshSimplificationIncrement, j + meshSimplificationIncrement];
                        
                        meshData.AddTriangle(a,d,c);
                        meshData.AddTriangle(d,a,b);
                    }

                    vertexIndex++;
                }

            meshData.FinalizeMesh();

            return meshData;
        }
    }

    public class MeshData
    {
        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uvs;
        private Vector3[] bakedNormals;

        private Vector3[] borderVertices;
        private int[] borderTrinagles;

        private int triangleIndex;
        private int borderTriangleIndex;

        private bool useFlatShading;

        public MeshData(int verticesPerLine, bool useFlatShading)
        {
            this.useFlatShading = useFlatShading;

            vertices = new Vector3[verticesPerLine * verticesPerLine];
            uvs = new Vector2[verticesPerLine * verticesPerLine];
            triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

            borderVertices = new Vector3[verticesPerLine * 4 + 4];
            borderTrinagles = new int[24 * verticesPerLine];
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        { 
            if(vertexIndex < 0)
            {
                borderVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if(a < 0 || b < 0 || c < 0)
            {
                borderTrinagles[borderTriangleIndex] = a;
                borderTrinagles[borderTriangleIndex + 1] = b;
                borderTrinagles[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
            
        }

        private Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;

            for(int i = 0; i< triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            int borderTriangleCount = borderTrinagles.Length / 3;
            for(int i = 0; i< borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = borderTrinagles[normalTriangleIndex];
                int vertexIndexB = borderTrinagles[normalTriangleIndex + 1];
                int vertexIndexC = borderTrinagles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

                if(vertexIndexA >= 0)
                {
                    vertexNormals[vertexIndexA] += triangleNormal;
                }

                if(vertexIndexB >= 0)
                {
                    vertexNormals[vertexIndexB] += triangleNormal;
                }

                if(vertexIndexC >= 0)
                {
                    vertexNormals[vertexIndexC] += triangleNormal;
                }
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals; 
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = GetVertex(indexA);
            Vector3 pointB = GetVertex(indexB);
            Vector3 pointC = GetVertex(indexC);

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        private Vector3 GetVertex(int index)
        {
            return (index < 0) ? borderVertices[-index - 1] : vertices[index]; 
        }

        public void FinalizeMesh()
        {
            if(useFlatShading)
            {
                FlatShading();
            }
            else
            {
                BakeNormals();
            }
        }

        private void BakeNormals()
        {
            bakedNormals = CalculateNormals();
        }

        private void FlatShading()
        {
            Vector3[] flatShadedVertices = new Vector3[triangles.Length];
            Vector2[] flatShadedUvs = new Vector2[triangles.Length];

            for(int i = 0; i < triangles.Length; i++)
            {
                flatShadedVertices[i] = vertices[triangles[i]];
                flatShadedUvs[i] = uvs[triangles[i]];
                triangles[i] = i;
            }

            vertices = flatShadedVertices;
            uvs = flatShadedUvs;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            if(useFlatShading)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = bakedNormals;
            }

            return mesh;
        }
    }
}
