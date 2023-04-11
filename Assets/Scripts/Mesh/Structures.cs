using Unity.Collections;
using Unity.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;

using static Unity.Mathematics.math;
using UnityEngine;

#nullable enable
namespace Meshes
{

    public class Vertex
    {
        public float3 Position; // 12 bytes

        private struct VertexProps
        {
            public int index;
            public float3 normal;
            public float4 tangent;
            public float2 uv0;

            public override string ToString()
            {
                return base.ToString();
            }
        }

        private List<VertexProps> properties = new List<VertexProps>(1);
        //// maybe add bookkeeping fields?
        private List<Face> faces = new List<Face>();
        // public List<Edge> edges = new List<Edge>();

        public static Vertex Create(float3 position, float3 normal, float4 tangent, float2 texCoord0, int index = 0)
        {
            return new Vertex
            {
                Position = position,
                properties = new List<VertexProps>()
                {
                    new VertexProps
                    {
                        index = index,
                        normal = normal,
                        tangent = tangent,
                        uv0 = texCoord0,
                    }

                }
            };
        }

        public void AddProps(float3 normal, float4 tangent, float2 texCoord0, int index = 0)
        {
            properties.Add(
                new VertexProps
                {
                    normal = normal,
                    tangent = tangent,
                    uv0 = texCoord0,
                    index = index,
                });
        }

        public Stream0[] ToStream()
        {
            var stream = new Stream0[properties.Count];
            for (int i = 0; i < properties.Count; ++i)
            {
                stream[i] = new Stream0
                {
                    position = Position,
                    normal = properties[i].normal,
                    tangent = properties[i].tangent,
                    texCoord0 = properties[i].uv0,
                };
            }
            return stream;
        }

        public int[] Indices()
        {
            return properties.Select(props => props.index).ToArray();
        }


        public int GetIndex(Face face)
        {
            return properties[0].index;
            /*
            float3 faceNorm = face.Normal;
            int bestIndex = 0;
            float bestAngle = float.NegativeInfinity;
            foreach (var prop in properties) {
                float angle = dot(faceNorm, prop.normal);
                if (angle > bestAngle)
                {
                    bestAngle = angle;
                    bestIndex = prop.index;
                }
            }
            return bestIndex;
            */
        }

        public override string ToString()
        {
            var str =  $"{base.ToString()}: position: {Position}\nSubprops: {properties.Count}";

            return str;
        }



    }

    public class Edge
    {
        public Vertex one;
        public Vertex two;

        public List<Face> faces = new List<Face>(2);
    }

    public class Face
    {
        // Assume that the list of vertices is clockwise on the face
        public List<Vertex> vertices = new List<Vertex>(4);
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

        public Face(List<Vertex> vertices)
        {
            normal = position = Unity.Mathematics.float3.zero;
            this.vertices = vertices;
            RecalculateNormal();
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
                normal = normalize(cross(vec2, vec1));
            }
        }

        public void RecalculatePosition()
        {
            throw new NotImplementedException { };
        }

        public void AddVertex()
        {
            throw new NotImplementedException { };
        }



        /// <summary>
        /// Triangulate the face and return the triangle indices
        /// </summary>
        /// <returns>Triangle indices</returns>
        public int3[] ToStream()
        {
            if (vertices.Count < 3)
            {
                return empty;
            }
            else
            {
                var indices = new int3[vertices.Count - 2];
                var ind0 = vertices[0].GetIndex(this);
                for (int i = 0; i < vertices.Count - 2; i++)
                {
                    indices[i] = int3(ind0, vertices[i + 1].GetIndex(this), vertices[i + 2].GetIndex(this));
                }
                return indices;
            }
        }
    }
}