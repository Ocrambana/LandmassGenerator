using System.Collections;
using System;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    internal static class MeshGenerator 
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings,int levelOfDetail)
        {
            int skipIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;

            int numVertsPerLine = meshSettings.numberOfVerticiesPerLine;

            Vector2 topLeft = new Vector2(-1,1) * meshSettings.meshWorldSize / 2f;

            MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

            int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
            int meshVertexIndex = 0,
                outOfMeshVertexIndex = -1;

            for (int j = 0; j < numVertsPerLine; j ++)
                for (int i = 0; i < numVertsPerLine; i ++)
                {
                    bool isOutOfMeshVertex = j == 0 || j == numVertsPerLine - 1 || i == 0 || i == numVertsPerLine - 1;
                    bool isSkippedVertex = i > 2 && i < numVertsPerLine - 3 && j > 2 && j < numVertsPerLine - 3 && ((i - 2) % skipIncrement != 0 || (j - 2) % skipIncrement != 0);

                    if(isOutOfMeshVertex)
                    {
                        vertexIndicesMap[i, j] = outOfMeshVertexIndex;
                        outOfMeshVertexIndex--;
                    }
                    else if(!isSkippedVertex)
                    {
                        vertexIndicesMap[i, j] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }

            for (int j = 0; j < numVertsPerLine; j ++)
                for(int i = 0; i < numVertsPerLine; i ++)
                {
                    bool isSkippedVertex = i > 2 && i < numVertsPerLine - 3 && j > 2 && j < numVertsPerLine - 3 && ((i - 2) % skipIncrement != 0 || (j - 2) % skipIncrement != 0);

                    if (!isSkippedVertex)
                    {
                        bool isOutOfMeshVertex = j == 0 || j == numVertsPerLine - 1 || i == 0 || i == numVertsPerLine - 1;
                        bool isMeshEdgeVertex = (j == 1 || j == numVertsPerLine - 2 || i == 1 || i == numVertsPerLine - 2) && !isOutOfMeshVertex;
                        bool isMainVertex = ((i - 2) % skipIncrement == 0 && (j - 2) % skipIncrement == 0) && !isOutOfMeshVertex && !isMeshEdgeVertex;
                        bool isEdgeConnectionVertex = (j == 2 || j == numVertsPerLine - 3 || i == 2 || i == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                        int vertexIndex = vertexIndicesMap[i, j];
                        Vector2 percent = new Vector2(i - 1, j - 1) / ( numVertsPerLine - 3 );
                        float height = heightMap[i, j];
                        Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, - percent.y) * meshSettings.meshWorldSize;

                        if(isEdgeConnectionVertex)
                        {
                            bool isVertical = i == 2 || i == numVertsPerLine - 3;

                            int distanceToMainVertexA = (isVertical ? j - 2 : i - 2) % skipIncrement;
                            int distanceToMainVertexB = skipIncrement - distanceToMainVertexA;

                            float distancePercentFromAToB = distanceToMainVertexA / (float)skipIncrement;

                            float heightMainVertexA = heightMap[(isVertical) ? i : i - distanceToMainVertexA, (isVertical) ? j - distanceToMainVertexA : j];
                            float heightMainVertexB = heightMap[(isVertical) ? i : i + distanceToMainVertexB, (isVertical) ? j + distanceToMainVertexB : j];

                            height = heightMainVertexA * (1 - distancePercentFromAToB) + heightMainVertexB * distancePercentFromAToB;
                        }

                        meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                        bool createTriangle = i < numVertsPerLine - 1 && j < numVertsPerLine - 1 && (!isEdgeConnectionVertex || ( i != 2 && j != 2));

                        if(createTriangle)
                        {
                            int currentIncrement = (isMainVertex && i != numVertsPerLine - 3 && j != numVertsPerLine - 3) ? skipIncrement : 1;

                            int a = vertexIndicesMap[i, j],
                                b = vertexIndicesMap[i + currentIncrement, j],
                                c = vertexIndicesMap[i, j + currentIncrement],
                                d = vertexIndicesMap[i + currentIncrement, j + currentIncrement];
                        
                            meshData.AddTriangle(a,d,c);
                            meshData.AddTriangle(d,a,b);
                        }
                    }
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

        private Vector3[] outOfMeshVertices;
        private int[] outOfMeshTrinagles;

        private int triangleIndex;
        private int outOfMeshTriangleIndex;

        private bool useFlatShading;

        public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
        {
            this.useFlatShading = useFlatShading;

            int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
            int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
            int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
            int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

            vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
            uvs = new Vector2[vertices.Length];

            int meshEdgeTriangles = (numVertsPerLine - 4) * 8;
            int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
            triangles = new int[(meshEdgeTriangles + numMainTriangles) * 3];

            outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
            outOfMeshTrinagles = new int[(numVertsPerLine - 2) * 24];
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        { 
            if(vertexIndex < 0)
            {
                outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
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
                outOfMeshTrinagles[outOfMeshTriangleIndex] = a;
                outOfMeshTrinagles[outOfMeshTriangleIndex + 1] = b;
                outOfMeshTrinagles[outOfMeshTriangleIndex + 2] = c;
                outOfMeshTriangleIndex += 3;
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

            int borderTriangleCount = outOfMeshTrinagles.Length / 3;
            for(int i = 0; i< borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = outOfMeshTrinagles[normalTriangleIndex];
                int vertexIndexB = outOfMeshTrinagles[normalTriangleIndex + 1];
                int vertexIndexC = outOfMeshTrinagles[normalTriangleIndex + 2];

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
            return (index < 0) ? outOfMeshVertices[-index - 1] : vertices[index]; 
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
