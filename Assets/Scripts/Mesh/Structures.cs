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

    /// <summary>
    /// Stream representation of a vertex for the renderer. Each vertex may create
    /// multiple Stream0s based on the faces around it.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Stream0
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 uv0; // 8 bytes

        public static readonly Stream0 degenerate = new Stream0
        {
            position = float3(0f, 0f, 0f),
            normal = float3(0f, 0f, 0f),
            tangent = float4(0f, 0f, 0f, 0f),
            uv0 = float2(0f, 0f)
        };
    }

    /// <summary>
    /// Use this maybe to do cleanup afterwards?
    /// </summary>
    interface IMeshStruct
    {
        bool Alive { get; set; }
    }

    /// <summary>
    /// The Vertex class describes a single corner in the mesh and is described by its
    /// position, other vertices it connects to, and the faces it is a part of.
    /// </summary>
    public class Vertex
    {
        public float3 Position { get; set; }
        public float3 Normal { get; set; }
        // is the tangent even needed?
        public float4 Tangent;

        private struct FaceIndex
        {
            public Face face;
            public int index;
        }

        private List<FaceIndex> faces = new List<FaceIndex>(4);
        public List<Edge> edges = new List<Edge>(4);

        /// <summary>
        /// Create an unconnected Vertex at the given position with the optional
        /// normal and tangent
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="normal">normal</param>
        /// <param name="tangent">tangent</param>
        /// <returns></returns>
        public static Vertex Dangling(float3 position, float3 normal = default, float4 tangent = default)
        {
            return new Vertex
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
            };
        }

        /// <summary>
        /// Create a vertex at the position of the other vertex with an edge
        /// that connects to that vertex
        /// </summary>
        /// <param name="other">other vertex</param>
        /// <returns>newly created vertex</returns>
        public static Vertex FromOtherVertexConnected(Vertex other)
        {
            Vertex self = new Vertex
            {
                Position = other.Position,
                Normal = other.Normal,
                Tangent = other.Tangent,
            };

            Edge edge = new Edge(other, self);
            self.edges.Add(edge);
            other.edges.Add(edge);

            return self;
        }

        /// <summary>
        /// Returns true if this vertex has an edge to the other vertex
        /// </summary>
        /// <param name="other">other vertex</param>
        /// <returns>true if connected</returns>
        public bool IsConnected(Vertex other)
        {
            foreach (Edge edge in edges)
            {
                if (edge.Other(this) == other)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the edge between this vertex and other vertex if it exists
        /// </summary>
        /// <param name="other">other vertex</param>
        /// <returns>The edge if it exists, otherwise null</returns>
        public Edge? GetEdgeTo(Vertex other)
        {
            foreach (Edge edge in edges)
            {
                if (edge.Other(this) == other)
                {
                    return edge;
                }
            }
            return null;
        }

        /// <summary>
        /// Convert the information in this vertex to a stream.
        /// Use WriteToStream() instead to avoid allocations.
        /// </summary>
        /// <returns>The stream</returns>
        internal Stream0[] ToStream()
        {
            var stream = new Stream0[faces.Count];
            for (int i = 0; i < faces.Count; ++i)
            {
                stream[i].position = Position;
                stream[i].normal = Normal;
                stream[i].tangent = Tangent;
                stream[i].uv0 = faces[i].face.GetUVof(this);
            }
            return stream;
        }

        /// <summary>
        /// Write the vertex to a vertex buffer without allocations
        /// </summary>
        /// <param name="stream">stream to write into</param>
        internal void WriteToStream(ref NativeArray<Stream0> stream)
        {
            Stream0 temp = default;
            temp.position = Position;
            temp.normal = Normal;
            temp.tangent = Tangent;
            for (int i = 0; i < faces.Count; ++i)
            {
                temp.uv0 = faces[i].face.GetUVof(this);
                stream[faces[i].index] = temp;
            }
        }

        public int[] Indices()
        {
            return faces.Select(props => props.index).ToArray();
        }

        /// <summary>
        /// Set stream indices for each face for rendering
        /// </summary>
        /// <param name="beginning">first free index</param>
        /// <returns>beginning + number of indices used by the vertex</returns>
        public int OptimizeIndices(int beginning)
        {
            // TODO: make some faces share indices for efficiency

            // Maybe instead of optimizing this ahead of time
            // We should get the beginning in WriteToStream() instead
            // Need to figure out a way to send the information to faces
            for (int i = 0; i < faces.Count; ++i)
            {
                var prop = faces[i];
                prop.index = beginning + i;
                faces[i] = prop;
            }
            return beginning + faces.Count;
        }

        /// <summary>
        /// Get the stream index associated with each face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Recalculate the normal based on adjacent faces.
        /// The face normals have to be calculated first, else 
        /// the effect of this function will likely be incorrect.
        /// </summary>
        public void RecalculateNormal()
        {
            Normal = 0;
            if (faces.Count > 0)
            {
                foreach (FaceIndex faceIndex in faces)
                {
                    Normal += faceIndex.face.Normal;
                }
                Normal /= faces.Count;
            }
        }

        /// <summary>
        /// Add a face to the internal faces list, but check if the face
        /// is already included first
        /// </summary>
        /// <param name="face">face to add</param>
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

        /// <summary>
        /// Check if there is an Edge between this vertex and the other vertex. 
        /// Create a new Edge if there isn't an Edge and then return the Edge.
        /// </summary>
        /// <param name="vertex">vertex to connect to</param>
        /// <returns></returns>
        internal Edge AddEdgeCheckedFromVertex(Vertex other)
        {
            foreach (Edge edge in edges)
            {
                if (edge.IsBetween(this, other))
                {
                    return edge;
                }
            }
            return AddEdgeUncheckedFromVertex(other);
        }

        /// <summary>
        /// Add an edge to the internal edges list, but check
        /// if the edge is already in the list first.
        /// </summary>
        /// <param name="edge"></param>
        internal void AddEdgeChecked(Edge edge)
        {
            foreach (Edge potentialDuplicate in edges)
            {
                if (potentialDuplicate == edge)
                {
                    return;
                }
            }
            edges.Add(edge);
        }

        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal Edge AddEdgeUncheckedFromVertex(Vertex other)
        {
            Edge newEdge = new Edge(this, other);
            edges.Add(newEdge);
            return newEdge;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: position: {Position}, Faces: {faces.Count}, Edges: {edges.Count}";
        }
    }

    public class Edge
    {
        public Vertex one;
        public Vertex two;

        public float3 Position => (one.Position + two.Position) / 2.0f;
        public float3 Normal => (one.Normal + two.Normal) / 2.0f;
        public float Length => distance(one.Position, two.Position);

        public Edge(Vertex one, Vertex two)
        {
            if (one == two)
            {
                throw new ArgumentException("Vertices must be unique");
            }
            this.one = one;
            this.two = two;
        }

        public Boolean IsBetween(Vertex vertex1, Vertex vertex2)
        {
            return ((one == vertex1 && two == vertex2) || (one == vertex2 && two == vertex1));
        }

        public Vertex Other(Vertex vertex)
        {
            if (one == vertex)
            {
                return two;
            } else if (two == vertex)
            {
                return one;
            } else
            {
                throw new ArgumentException("Given vertex must be in the edge");
            }
        }

        public void MoveRelative(float3 relative)
        {
            // need a more robust API later
            one.Position += relative;
            two.Position += relative;
        }
    }

    public enum ShadingType
    {
        Flat = 0,
        Smooth = 1,
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

        private readonly List<VertexCoordinate> vertices;
        public List<Vertex> Vertices => vertices.Select(item => item.vertex).ToList();

        public readonly List<Edge> edges; // = new List<Edge>(4);

        public int TriangleCount => GetTriangleCount();

        private float3 position;
        private float3 normal;

        private static readonly int3[] empty = new int3[0];
        private static readonly int3[] degenerate = { int3(0, 0, 0) };

        public static int3[] Empty => empty;

        public float3 Position => position;
        public float3 Normal => normal;
        public int VertexCount => vertices.Count;
        public int EdgeCount => edges.Count;

        public Face()
        {
            normal = position = Unity.Mathematics.float3.zero;
            vertices = new List<VertexCoordinate>(0);
            edges = new List<Edge>(0);
        }

        /// <summary>
        /// Arbitrary vertex count constructor
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="uv0s"></param>
        public Face(List<Vertex> vertices, List<float2> uv0s)
        {
            if (vertices.Count != uv0s.Count)
            {
                throw new ArgumentException("vertices and uv0s must have the same length");
            }

            normal = position = default;
            this.vertices = new List<VertexCoordinate>(vertices.Count);
            edges = new List<Edge>(vertices.Count);
            for (int i = 0; i < vertices.Count; ++i)
            {
                this.vertices.Add(new VertexCoordinate
                {
                    vertex = vertices[i],
                    uv0 = uv0s[i]
                });
            }
            foreach (var vert in vertices)
            {
                vert.AddFaceChecked(this);
            }
            FinalizeSetup();
        }

        public Face(List<Vertex> vertices, float2[] uv0s)
        {
            if (vertices.Count != uv0s.Length)
            {
                throw new ArgumentException("vertices and uv0s must have the same length");
            }

            normal = position = default;
            this.vertices = new List<VertexCoordinate>(vertices.Count);
            edges = new List<Edge>(vertices.Count);
            for (int i = 0; i < vertices.Count; ++i)
            {
                this.vertices.Add(new VertexCoordinate
                {
                    vertex = vertices[i],
                    uv0 = uv0s[i]
                });
            }
            foreach (var vert in vertices)
            {
                vert.AddFaceChecked(this);
            }
            FinalizeSetup();
        }

        public Face(Vertex[] vertices, float2[] uv0s)
        {
            if (vertices.Length != uv0s.Length)
            {
                throw new ArgumentException("vertices and uv0s must have the same length");
            }

            normal = position = default;
            this.vertices = new List<VertexCoordinate>(vertices.Length);
            edges = new List<Edge>(vertices.Length);
            for (int i = 0; i < vertices.Length; ++i)
            {
                this.vertices.Add(new VertexCoordinate
                {
                    vertex = vertices[i],
                    uv0 = uv0s[i]
                });
            }
            foreach (var vert in vertices)
            {
                vert.AddFaceChecked(this);
            }
            FinalizeSetup();
        }

        /// <summary>
        /// Construct from a triangle
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="uv0s"></param>
        public Face(TriangleVerts vertices, TriangleUVs uv0s = default)
        {
            normal = position = default;
            this.vertices = new List<VertexCoordinate>(3);
            edges = new List<Edge>(3);
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert0,
                uv0 = uv0s.uv0_0
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert1,
                uv0 = uv0s.uv0_1
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert2,
                uv0 = uv0s.uv0_2
            });
            foreach (var vert in this.vertices)
            {
                vert.vertex.AddFaceChecked(this);
            }
            FinalizeSetup();
        }

        /// <summary>
        /// Construct from a quad. The caller should ensure the vertices lie on the same plane
        /// and form a valid face counter clockwise, else the results might be unexpected.
        /// </summary>
        /// <param name="vertices">four vertices counter-clockwise</param>
        /// <param name="uv0s">default uv values</param>
        public Face(QuadVerts vertices, QuadUVs uv0s = default)
        {
            normal = position = default;
            this.vertices = new List<VertexCoordinate>(4);
            edges = new List<Edge>(4);
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert0,
                uv0 = uv0s.uv0_0
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert1,
                uv0 = uv0s.uv0_1
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert2,
                uv0 = uv0s.uv0_2
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.vert3,
                uv0 = uv0s.uv0_3
            });
            foreach (var vert in this.vertices)
            {
                vert.vertex.AddFaceChecked(this);
            }
            FinalizeSetup();
        }

        /// <summary>
        /// Create edges around the vertices, calculate normals and position.
        /// </summary>
        private void FinalizeSetup()
        {
            GenerateEdges();
            RecalculateNormal();
            RecalculatePosition();
        }

        private void GenerateEdges()
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                Vertex first = vertices[i].vertex;
                Vertex second = vertices[(i + 1) % vertices.Count].vertex;
                Edge edge = first.AddEdgeCheckedFromVertex(second);
                edges.Add(edge);
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

        public int GetTriangleCount()
        {
            if (vertices.Count < 3)
            {
                return 0;
            }
            else
            {
                return vertices.Count - 2;
            }
        }

        public void MoveRelative(float3 relative)
        {
            // add a more robust transform API later
            foreach(var vert in vertices)
            {
                vert.vertex.Position += relative;
            }
            position += relative;
        }

        
        /// <summary>
        /// Flip the face by reversing the order of vertices
        /// </summary>
        /// <param name="flipNormal">Whether to also flip the normal immediately</param>
        public void FlipFace(bool flipNormal)
        {
            vertices.Reverse();
            if (flipNormal)
            {
                normal = -Normal;
            }
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