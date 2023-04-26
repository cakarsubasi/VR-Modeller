using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

#nullable enable
namespace Meshes
{
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

    public class IMeshProperties
    {
        public bool Alive;
        public bool Active;
        public int Index;
    }

    /// <summary>
    /// The Vertex class describes a single corner in the mesh and is described by its
    /// position, other vertices it connects to, and the faces it is a part of.
    /// </summary>
    public sealed partial class Vertex
    {
        public float3 Position { get; set; }
        public float3 Normal { get; set; }
        // is the tangent even needed?
        public float4 Tangent;

        public bool Alive { get; internal set; } = true;
        public int Index { get; internal set; } = -1;

        internal struct FaceIndex
        {
            public Face face;
            public int index;
        }

        internal List<FaceIndex> faces = new List<FaceIndex>(4);
        internal List<Edge> edges = new List<Edge>(4);

        internal IEnumerable<Face> FacesIter => GetFacesEnum();

        /// <summary>
        /// Get a copy of the edges this vertex is connected to
        /// </summary>
        public List<Edge> Edges => new List<Edge>(edges);

        /// <summary>
        /// Get a copy of the faces this vertex is connected to
        /// </summary>
        public List<Face> Faces => new List<Face>(FacesIter);
        public int FaceCount => faces.Count;
        public int EdgeCount => edges.Count;

    }

    /// <summary>
    /// The edge class describes an edge between two vertices. 
    /// </summary>
    public sealed partial class Edge
    {
        public Vertex one { get; internal set; }
        public Vertex two { get; internal set; }

        public bool Alive { get; internal set; } = true;

        public float3 Position => (one.Position + two.Position) / 2.0f;
        public float3 Normal => (one.Normal + two.Normal) / 2.0f;
        public float Length => distance(one.Position, two.Position);

        public int EdgeLoopCount => GetEdgeLoopCount();

        public List<Face> EdgeLoop => GetEdgeLoops();

    }

    public enum ShadingType
    {
        Flat = 0,
        Smooth = 1,
    }

    static class Triangle
    {
        public static readonly int3 degenerate = int3(0, 0, 0);
    }

    /// <summary>
    /// A face is a collection of vertices and edges
    /// </summary>
    public sealed partial class Face
    {
        // Assume that the list of vertices is clockwise on the face
        internal struct VertexCoordinate
        {
            public Vertex vertex;
            public float2 uv0;
            public float3 Position => vertex.Position;
        }

        //private ShadingType shading = ShadingType.Flat;

        internal List<VertexCoordinate> vertices;
        internal List<Edge> edges;

        /// <summary>
        /// Get a copy to the list of vertices
        /// </summary>
        public List<Vertex> Vertices => vertices.Select(item => item.vertex).ToList();

        /// <summary>
        /// Get a copy to the list of edges
        /// </summary>
        public List<Edge> Edges => new List<Edge>(edges);


        public bool Alive { get; internal set; } = true;
        public float3 Position { get; internal set; }
        public float3 Normal { get; internal set; }

        public int VertexCount => vertices.Count;
        public int EdgeCount => edges.Count;
        public int TriangleCount => GetTriangleCount();

        private static readonly int3[] empty = new int3[0];
        private static readonly int3[] degenerate = { int3(0, 0, 0) };

        public static int3[] Empty => empty;

    }
}