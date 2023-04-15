using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;

using static Unity.Mathematics.math;
using UnityEngine;

#nullable enable
namespace Meshes
{
    static class Triangle
    {
        public static readonly int3 degenerate = int3(ushort.MaxValue - 1, ushort.MaxValue - 1, ushort.MaxValue - 1);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Stream0
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 texCoord0; // 8 bytes

        public static readonly Stream0 degenerate = new Stream0
        {
            position = float3(0f, 0f, 0f),
            normal = float3(0f, 0f, 0f),
            tangent = float4(0f, 0f, 0f, 0f),

        };
    }

    /// <summary>
    /// Use this maybe to do cleanup afterwards?
    /// </summary>
    interface IMeshStruct
    {
        bool Alive { get; set; }
    }

    public class Vertex
    {
        public float3 Position;
        public float3 Normal;
        public float4 Tangent;

        private struct FaceIndex
        {
            public Face face;
            public int index;
        }

        private List<FaceIndex> faces = new List<FaceIndex>();
        // public List<Edge> edges = new List<Edge>();

        public static Vertex Create(float3 position, float3 normal, float4 tangent, float2 texCoord0, int index = 0)
        {
            return new Vertex
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
            };
        }

        public Stream0[] ToStream()
        {
            var stream = new Stream0[faces.Count];
            for (int i = 0; i < faces.Count; ++i)
            {
                stream[i].position = Position;
                stream[i].normal = Normal;
                stream[i].tangent = Tangent;
                stream[i].texCoord0 = faces[i].face.GetUVof(this);
            }
            return stream;
        }

        /// <summary>
        /// Write the vertex to a vertex buffer without allocations
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(ref NativeArray<Stream0> stream)
        {
            Stream0 temp = default;
            temp.position = Position;
            temp.normal = Normal;
            temp.tangent = Tangent;
            for (int i = 0; i < faces.Count; ++i)
            {
                temp.texCoord0 = faces[i].face.GetUVof(this);
                stream[faces[i].index] = temp;
            }
        }

        public int[] Indices()
        {
            return faces.Select(props => props.index).ToArray();
        }

        public int OptimizeIndices(int beginning)
        {
            for (int i = 0; i < faces.Count; ++i)
            {
                var prop = faces[i];
                prop.index = beginning + i;
                faces[i] = prop;
            }
            return beginning + faces.Count;
        }

        public int GetIndex(Face face)
        {
            foreach (FaceIndex faceIndex in faces)
            {
                if (faceIndex.face == face)
                {
                    return faceIndex.index;
                }
            }
            return -1;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: position: {Position}, Faces: {faces.Count}";
        }

        internal void AddFaceChecked(Face face)
        {
            if (!faces.Select(structure => structure.face).Contains(face))
            {
                faces.Add(new FaceIndex { face = face });
            } else
            {
                Debug.Log($"Tried adding existing face: {face}");
            }
        }
    }

    public class Edge
    {
        public Vertex one;
        public Vertex two;

        public List<Face> faces = new List<Face>(2);

        public Edge(Vertex one, Vertex two)
        {
            this.one = one;
            this.two = two;
        }
    }

    public class Face
    {
        // Assume that the list of vertices is clockwise on the face
        struct VertexCoordinate
        {
            public Vertex vertex;
            public float2 uv0;
            public float3 Position => vertex.Position;
        }

        private List<VertexCoordinate> vertices = new List<VertexCoordinate>(4);
        public int TriangleCount => GetTriangleCount();
        
        // public List<Edge> edges = new List<Edge>(4);

        private float3 position;
        private float3 normal;

        private static readonly int3[] empty = new int3[0];
        private static readonly int3[] degenerate = { int3(0, 0, 0) };

        public static int3[] Empty => empty;

        public float3 Position => position;
        public float3 Normal => normal;
        public Face()
        {
            normal = position = Unity.Mathematics.float3.zero;
        }

        public Face(List<Vertex> vertices, List<float2> uv0s)
        {
            normal = position = Unity.Mathematics.float3.zero;
            //this.vertices = vertices.Select(vertex => new VertexCoordinate { vertex = vertex }).ToList();
            this.vertices = vertices.Zip(uv0s, (vertex, uv0) => new VertexCoordinate { vertex = vertex, uv0 = uv0 }).ToList();
            foreach (var vert in vertices)
            {
                vert.AddFaceChecked(this);
            }
            RecalculateNormal();
            RecalculatePosition();
        }

        public int GetTriangleCount()
        {
            if (vertices.Count < 3)
            {
                return 0;
            } else
            {
                return vertices.Count - 2;
            }
        }

        public void RecalculateNormal()
        {
            if (vertices.Count < 3)
            {
                normal = 0;
            }
            else
            {
                var vec1 = vertices[1].Position - vertices[0].Position;
                var vec2 = vertices[2].Position - vertices[0].Position;
                normal = normalize(cross(vec1, vec2));
            }
        }

        public void RecalculatePosition()
        {
            position = 0;
            foreach (var vert in vertices)
            {
                position += vert.Position;
            }
            position /= vertices.Count;
        }

        public void AddVertex()
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Get uv0 associated with a certain vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public float2 GetUVof(Vertex vertex)
        {
            foreach (var vert in vertices)
            {
                if (vert.vertex == vertex)
                {
                    return vert.uv0;
                }
            }
            return float2(0f, 0f);
        }

        /// <summary>
        /// Write the triangle indices to a native array without allocations
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public int WriteToStream(ref NativeArray<int3> array, int startIndex)
        {
            int triangles = TriangleCount;
            var ind0 = vertices[0].vertex.GetIndex(this);
            for (int i = 0; i < triangles; i++)
            {
                array[startIndex + i] = int3(ind0, vertices[i + 1].vertex.GetIndex(this), vertices[i + 2].vertex.GetIndex(this));
            }
            return startIndex + triangles;
        } 

        public override string ToString()
        {
            var str = $"{base.ToString()}: verts: {vertices.Count}, position: {position}, normal: {normal}\n\tindices:";
            foreach (var vertex in vertices)
            {
                str = $"{str} {vertex.vertex.GetIndex(this)}";
            }
            return str;
        }
    }
}