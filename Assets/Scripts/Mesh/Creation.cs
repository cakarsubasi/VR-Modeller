using System.Collections;
using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
#nullable enable

using static Unity.Mathematics.math;

namespace Meshes
{
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

    partial struct EditableMeshImpl
    {
        /// <summary>
        /// Add Vertex to the given position
        /// </summary>
        /// <param name="position">position of the vertex</param>
        /// <returns>index of the vertex</returns>
        public Vertex AddVertex(float3 position)
        {
            return AddVertex(position, normal: back(), tangent: float4(1f, 0f, 0f, -1f));
        }

        /// <summary>
        /// Add Vertex to the given position with the given normal and tangent
        /// </summary>
        /// <param name="position">position to add to</param>
        /// <param name="normal">normal of the vertex</param>
        /// <param name="tangent">tangent of the vertex</param>
        /// <param name="texCoord0">UV coordinate of the vertex</param>
        /// <returns>index of the vertex</returns>
        public Vertex AddVertex(float3 position, float3 normal, float4 tangent)
        {
            var vert = Vertex.Dangling(position, normal, tangent);
            Vertices.Add(vert);
            return vert;
        }

        public Vertex AddVertexPossiblyOverlapping(float3 position, float3 normal, float4 tangent)
        {
            Vertex? vertMaybe = FindByPosition(position);
            if (vertMaybe == null)
            {
                return AddVertex(position, normal, tangent);
            }
            else
            {
                return vertMaybe;
            }
        }

        public Vertex AddVertexOn(int index)
        {
            return AddVertex(Vertices[index].Position);
        }

        /// <summary>
        /// Add a quad by creating four vertices
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="pos3"></param>
        /// <param name="pos4"></param>
        public Face AddFace(float3 pos1, float3 pos2, float3 pos3, float3 pos4)
        {
            var verts = new QuadVerts
            (
                AddVertex(pos1),
                AddVertex(pos2),
                AddVertex(pos3),
                AddVertex(pos4)
            );
            return AddFace(verts);
        }

        /// <summary>
        /// Add a quad between the given four vertices using vertex indices
        /// </summary>
        /// <param name="vertices"></param>
        public Face AddFace(int4 vertices, QuadUVs uv0s = default)
        {
            QuadVerts face_verts = new QuadVerts(
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z],
                this.Vertices[vertices.w]
                );
            return AddFace(face_verts, uv0s);
        }

        public Face AddFace(QuadVerts verts, QuadUVs uv0s = default)
        {
            var face = new Face(verts, uv0s);
            Faces.Add(face);
            return face;
        }

        public Face AddTriangle(int3 vertices, TriangleUVs uv0s = default)
        {
            TriangleVerts face_verts = new TriangleVerts(
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z]
                );
            return AddTriangle(face_verts, uv0s);
        }

        public Face AddTriangle(TriangleVerts vertices, TriangleUVs uv0s = default)
        {
            var face = new Face(vertices, uv0s);
            Faces.Add(face);
            return face;
        }

        public Face AddNGon(int[] vertices, float2[] uv0s)
        {
            List<Vertex> face_verts = new List<Vertex>(vertices.Length);
            
            //var face = new Face(face_verts, uv0s);
            throw new NotImplementedException { };
        }

        public Face AddNGon(List<Vertex> vertices, float2[] uv0s)
        {
            throw new NotImplementedException { };
        }

        public void Extrude()
        {
            throw new NotImplementedException { };
        }
    }
}