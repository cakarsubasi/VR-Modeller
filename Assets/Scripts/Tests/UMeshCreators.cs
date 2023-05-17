using static Unity.Mathematics.math;

namespace Meshes.Test
{
    public static class Creators
    {

        public static UMesh Empty => CreateEmpty();
        public static UMesh CreateEmpty()
        {
            UMesh mesh = UMesh.Create();
            return mesh;
        }

        public static UMesh Quad => CreateQuad();
        public static UMesh CreateQuad()
        {
            UMesh mesh = UMesh.Create();
            var vert1 = float3(0f, 0f, 0f);
            var vert2 = float3(1f, 0f, 0f);
            var vert3 = float3(1f, 1f, 0f);
            var vert4 = float3(0f, 1f, 0f);
            mesh.CreateVerticesAndQuad(vert1, vert2, vert3, vert4);
            mesh.OptimizeIndices();
            mesh.RecalculateNormals();
            return mesh;
        }

        public static UMesh Cube => CreateCube();
        public static UMesh CreateCube()
        {
            UMesh mesh = UMesh.Create();

            Vertex v1 = mesh.CreateVertex(float3(-1f, -1f, -1f));
            Vertex v2 = mesh.CreateVertex(float3(1f, -1f, -1f));
            Vertex v3 = mesh.CreateVertex(float3(1f, -1f, 1f));
            Vertex v4 = mesh.CreateVertex(float3(-1f, -1f, 1f));

            Vertex v5 = mesh.CreateVertex(float3(-1f, 1f, -1f));
            Vertex v6 = mesh.CreateVertex(float3(1f, 1f, -1f));
            Vertex v7 = mesh.CreateVertex(float3(1f, 1f, 1f));
            Vertex v8 = mesh.CreateVertex(float3(-1f, 1f, 1f));

            mesh.CreateQuad(new QuadElement<Vertex>(v1, v2, v3, v4));
            mesh.CreateQuad(new QuadElement<Vertex>(v8, v7, v6, v5));

            mesh.CreateQuad(new QuadElement<Vertex>(v1, v4, v8, v5));
            mesh.CreateQuad(new QuadElement<Vertex>(v1, v5, v6, v2));

            mesh.CreateQuad(new QuadElement<Vertex>(v2, v6, v7, v3));
            mesh.CreateQuad(new QuadElement<Vertex>(v3, v7, v8, v4));

            return mesh;
        }
    }
}