using System.Collections;
using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
#nullable enable

using static Unity.Mathematics.math;

namespace Meshes
{

    /// <summary>
    /// struct to pass three vertex references, this avoids allocations
    /// </summary>
    public struct TriangleVerts
    {
        public Vertex vert0;
        public Vertex vert1;
        public Vertex vert2;

        public TriangleVerts(Vertex vert0, Vertex vert1, Vertex vert2)
        {
            this.vert0 = vert0;
            this.vert1 = vert1;
            this.vert2 = vert2;
        }
    }

    /// <summary>
    /// struct to pass four vertex references
    /// </summary>
    public struct QuadVerts
    {
        public Vertex vert0;
        public Vertex vert1;
        public Vertex vert2;
        public Vertex vert3;

        public QuadVerts(Vertex vert0, Vertex vert1, Vertex vert2, Vertex vert3)
        {
            this.vert0 = vert0;
            this.vert1 = vert1;
            this.vert2 = vert2;
            this.vert3 = vert3;
        }
    }

    /// <summary>
    /// struct to pass three UVs
    /// </summary>
    public struct TriangleUVs
    {
        public float2 uv0_0;
        public float2 uv0_1;
        public float2 uv0_2;

        public TriangleUVs(float2 uv0_0, float2 uv0_1, float2 uv0_2)
        {
            this.uv0_0 = uv0_0;
            this.uv0_1 = uv0_1;
            this.uv0_2 = uv0_2;
        }
    }

    /// <summary>
    /// struct to pass four UVs
    /// </summary>
    public struct QuadUVs
    {
        public float2 uv0_0;
        public float2 uv0_1;
        public float2 uv0_2;
        public float2 uv0_3;

        public QuadUVs(float2 uv0_0, float2 uv0_1, float2 uv0_2, float2 uv0_3)
        {
            this.uv0_0 = uv0_0;
            this.uv0_1 = uv0_1;
            this.uv0_2 = uv0_2;
            this.uv0_3 = uv0_3;
        }
    }

    partial struct UMesh
    {
        /// <summary>
        /// Create Vertex at the given position
        /// </summary>
        /// <param name="position">position of the vertex</param>
        /// <returns>reference to the vertex</returns>
        public Vertex CreateVertex(float3 position)
        {
            return CreateVertex(position, normal: back(), tangent: float4(1f, 0f, 0f, -1f));
        }

        /// <summary>
        /// Create Vertex to the given position with the given normal and tangent
        /// </summary>
        /// <param name="position">position to add to</param>
        /// <param name="normal">normal of the vertex</param>
        /// <param name="tangent">tangent of the vertex</param>
        /// <param name="texCoord0">UV coordinate of the vertex</param>
        /// <returns>reference to the vertex</returns>
        public Vertex CreateVertex(float3 position, float3 normal, float4 tangent)
        {
            var vert = Vertex.Dangling(position, normal, tangent);
            Vertices.Add(vert);
            return vert;
        }

        /// <summary>
        /// Create vertex at the given position if one does not already exist, otherwise
        /// return the reference to the existing vertex
        /// </summary>
        /// <param name="position">position to search</param>
        /// <returns>reference to the vertex</returns>
        public Vertex CreateVertexOrReturnReferenceIfExists(float3 position)
        {
            return CreateVertexOrReturnReferenceIfExists(position, normal: back(), tangent: float4(1f, 0f, 0f, -1f));
        }

        /// <summary>
        /// Create vertex at the given position with given normal and tangents
        /// if one does not already exist at that position, otherwise
        /// return the reference to the existing vertex
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="normal">normal</param>
        /// <param name="tangent">tangent</param>
        /// <returns>reference to the vertex</returns>
        public Vertex CreateVertexOrReturnReferenceIfExists(float3 position, float3 normal, float4 tangent)
        {
            Vertex? vertMaybe = FindByPosition(position);
            if (vertMaybe == null)
            {
                return CreateVertex(position, normal, tangent);
            }
            else
            {
                return vertMaybe;
            }
        }

        /// <summary>
        /// Create a new vertex connected to the other Vertex and return a reference
        /// to the new vertex, the new vertex will have the same position as the other vertex
        /// </summary>
        /// <param name="other">vertex to connect the new vertex</param>
        /// <returns>reference to the created vertex</returns>
        public Vertex CreateVertexConnectedTo(Vertex other)
        {
            Vertex vertex = Vertex.FromOtherVertexConnected(other);
            Vertices.Add(vertex);
            return vertex;
        }

        /// <summary>
        /// Add a quad by creating four vertices. References to the created vertices
        /// can be accessed with Face.Vertices
        /// </summary>
        /// <param name="pos1">position 1</param>
        /// <param name="pos2">position 2</param>
        /// <param name="pos3">position 3</param>
        /// <param name="pos4">position 4</param>
        /// <returns>Reference to the face</returns>
        public Face CreateVerticesAndQuad(float3 pos1, float3 pos2, float3 pos3, float3 pos4)
        {
            var verts = new QuadVerts
            (
                CreateVertex(pos1),
                CreateVertex(pos2),
                CreateVertex(pos3),
                CreateVertex(pos4)
            );
            return CreateQuad(verts);
        }


        /// <summary>
        /// Create quad between the given four vertices using vertex indices
        /// </summary>
        /// <param name="vertices">four vertex indices</param>
        /// <param name="uv0s">four uv values</param>
        /// <returns>Reference to the face</returns>
        [Obsolete("new API does not use vertex indices")]
        public Face CreateQuad(int4 vertices, QuadUVs uv0s = default)
        {
            QuadVerts face_verts = new QuadVerts(
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z],
                this.Vertices[vertices.w]
                );
            return CreateQuad(face_verts, uv0s);
        }

        /// <summary>
        /// Create quad given the vertices
        /// </summary>
        /// <param name="verts">four vertices</param>
        /// <param name="uv0s">four uv values</param>
        /// <returns>Reference to the face</returns>
        public Face CreateQuad(QuadVerts verts, QuadUVs uv0s = default)
        {
            var face = new Face(verts, uv0s);
            Faces.Add(face);
            return face;
        }

        /// <summary>
        /// Create triangle between the given three vertices using vertex indices
        /// </summary>
        /// <param name="vertices">three vertex indices</param>
        /// <param name="uv0s">three uv values</param>
        /// <returns>Reference to the face</returns>
        [Obsolete("new API does not use vertex indices")]
        public Face CreateTriangle(int3 vertices, TriangleUVs uv0s = default)
        {
            TriangleVerts face_verts = new TriangleVerts(
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z]
                );
            return CreateTriangle(face_verts, uv0s);
        }

        /// <summary>
        /// Create triangle with the given three vertices
        /// </summary>
        /// <param name="vertices">three vertices</param>
        /// <param name="uv0s">three uv values</param>
        /// <returns>Reference to the face</returns>
        public Face CreateTriangle(TriangleVerts vertices, TriangleUVs uv0s = default)
        {
            var face = new Face(vertices, uv0s);
            Faces.Add(face);
            return face;
        }

        /// <summary>
        /// Create NGon with the given vertex indices
        /// </summary>
        /// <param name="vertices">N vertex indices</param>
        /// <returns>Reference to the face</returns>
        [Obsolete("new API does not use vertex indices")]
        public Face CreateNGon(params int[] vertices)
        {
            float2[] uv0s = new float2[vertices.Length];
            return CreateNGon(vertices, uv0s);
        }

        /// <summary>
        /// Create NGon with the given vertex indices
        /// </summary>
        /// <param name="vertices">N vertex indices</param>
        /// <param name="uv0s">N uv values</param>
        /// <returns>Reference to the face</returns>
        [Obsolete("new API does not use vertex indices")]
        public Face CreateNGon(int[] vertices, float2[] uv0s)
        {
            List<Vertex> face_verts = new List<Vertex>(vertices.Length);
            foreach(int index in vertices)
            {
                face_verts.Add(this.Vertices[index]);
            }

            return CreateNGon(face_verts, uv0s);
        }

        /// <summary>
        /// Create NGon with the given vertices
        /// </summary>
        /// <param name="vertices">N vertices</param>
        /// <returns>Reference to the face</returns>
        public Face CreateNGon(List<Vertex> vertices)
        {
            float2[] uv0s = new float2[vertices.Count];
            return CreateNGon(vertices, uv0s);
        }

        /// <summary>
        /// Create NGon with the given vertices
        /// </summary>
        /// <param name="vertices">N vertices</param>
        /// <returns>Reference to the face</returns>
        public Face CreateNGon(params Vertex[] vertices)
        {
            float2[] uv0s = new float2[vertices.Length];
            return CreateNGon(vertices, uv0s);
        }

        /// <summary>
        /// Create NGon with the given vertices
        /// </summary>
        /// <param name="vertices">N vertices</param>
        /// <param name="uv0s">N uv values</param>
        /// <returns>Reference to the face</returns>
        public Face CreateNGon(Vertex[] vertices, float2[] uv0s)
        {
            Face face = new Face(vertices, uv0s);
            Faces.Add(face);
            return face;
        }

        /// <summary>
        /// Create NGon with the given vertices
        /// </summary>
        /// <param name="vertices">N vertices</param>
        /// <param name="uv0s">N uv values</param>
        /// <returns>Reference to the face</returns>
        public Face CreateNGon(List<Vertex> vertices, float2[] uv0s)
        {
            Face face = new Face(vertices, uv0s);
            Faces.Add(face);
            return face;
        }

        /// <summary>
        /// Create NGon with the given vertices
        /// </summary>
        /// <param name="vertices">N vertices</param>
        /// <param name="uv0s">N uv values</param>
        /// <returns>Reference to the face</returns>
        public Face CreateNGon(List<Vertex> vertices, List<float2> uv0s)
        {
            Face face = new Face(vertices, uv0s);
            Faces.Add(face);
            return face;
        }

        public struct ExtrusionResult
        {
            public List<Vertex> vertices;
            public List<Face> faces;
        }

        public Vertex Extrude(Vertex input)
        {
            throw new NotImplementedException { };
        }

        public Edge Extrude(Edge input)
        {
            throw new NotImplementedException { };
        }

        public Face Extrude(Face face)
        {
            throw new NotImplementedException { };
        }

        public void Extrude(in List<Vertex> selection, out List<Vertex> created)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Add an outside Vertex to the EditableMesh. You probably do not need
        /// to use this method unless you are creating a Mesh from scratch. This 
        /// method checks if the vertex already exists in the list, but does not
        /// check if faces referenced by the vertex exists in the EditableMesh.
        /// </summary>
        /// <param name="vertex">vertex to add</param>
        /// <returns></returns>
        public Vertex AddVertex(Vertex vertex)
        {
            if (Vertices.Contains(vertex)) {
                return vertex;
            } else
            {
                return AddVertexUnchecked(vertex);
            }
        }

        /// <summary>
        /// Add a presumably externally created vertex to the EditableMesh.
        /// </summary>
        /// <param name="vertex">vertex to add</param>
        /// <returns>the vertex</returns>
        public Vertex AddVertexUnchecked(Vertex vertex)
        {
            Vertices.Add(vertex);
            return vertex;
        }

        /// <summary>
        /// Add multiple vertices to the editable mesh, checking if each one already exists
        /// </summary>
        /// <param name="vertices">vertices to add</param>
        /// <returns>the list of vertices</returns>
        public Vertex[] AddVertices(params Vertex[] vertices)
        {
            Vertex[] return_list = new Vertex[vertices.Length];
            for (int i = 0; i < vertices.Length; ++i)
            {
                return_list[i] = AddVertex(vertices[i]);
            }
            return return_list;
        }

        /// <summary>
        /// Add multiple vertices to the editable mesh, without any checks
        /// </summary>
        /// <param name="vertices">vertices to add</param>
        /// <returns>the list of vertices</returns>
        public Vertex[] AddVerticesUnchecked(params Vertex[] vertices)
        {
            foreach (Vertex vertex in vertices)
            {
                AddVertexUnchecked(vertex);
            }
            return vertices;
        }

        /// <summary>
        /// Add an edge between the given two vertices if one does
        /// not already exist and return the edge.
        /// Right now, this function doesn't have much purpose unless we
        /// expose edges more to the outside.
        /// </summary>
        /// <param name="vertex1">vertex 1</param>
        /// <param name="vertex2">vertex 2</param>
        /// <returns>edge between them</returns>
        public Edge AddEdge(Vertex vertex1, Vertex vertex2)
        {
            throw new NotImplementedException { };
        }

        public Vertex MergeVertex(Vertex vert1, Vertex vert2)
        {
            throw new NotImplementedException { };
        }
    }
}