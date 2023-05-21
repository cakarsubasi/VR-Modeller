using System.Collections;
using System;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
#nullable enable

using static Unity.Mathematics.math;

namespace Meshes
{
    using TriangleVerts = TriangleElement<Vertex>;
    using TriangleUVs = TriangleElement<float2>;
    using TriangleEdges = TriangleElement<Edge>;

    using QuadVerts = QuadElement<Vertex>;
    using QuadUVs = QuadElement<float2>;
    using QuadEdges = QuadElement<Edge>;

    public struct TriangleElement<T> : IEnumerable<T>
    {
        public T f0;
        public T f1;
        public T f2;

        public TriangleElement(T f0, T f1, T f2)
        {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return f0;
            yield return f1;
            yield return f2;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return f0;
            yield return f1;
            yield return f2;
        }
    }

    public struct QuadElement<T> : IEnumerable<T>
    {
        public T f0;
        public T f1;
        public T f2;
        public T f3;

        public QuadElement(T f0, T f1, T f2, T f3)
        {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
            this.f3 = f3;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return f0;
            yield return f1;
            yield return f2;
            yield return f3;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return f0;
            yield return f1;
            yield return f2;
            yield return f3;
        }
    }

    partial struct UMesh
    {
        /// <summary>
        /// Create Vertex at origin (0, 0, 0)
        /// </summary>
        /// <returns>reference to the vertex</returns>
        public Vertex CreateVertex()
        {
            return CreateVertex(Unity.Mathematics.float3.zero);
        }

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
        /// Create a Vertex by copying the properties of another vertex.
        /// </summary>
        /// <param name="otherVertex">Vertex to copy properties from</param>
        /// <returns>A new unconnected vertex with the properties of otherVertex</returns>
        public Vertex CreateVertex(Vertex otherVertex)
        {
            Vertex vert = Vertex.Copy(otherVertex);
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
        public Vertex CreateVertexConnectedTo(Vertex other, out Edge edge)
        {
            Vertex vertex = Vertex.Copy(other);
            AddVertexUnchecked(vertex);
            edge = CreateEdgeUnchecked(vertex, other);
            return vertex;
        }

        /// <summary>
        /// Create an edge between the two vertices if one does not already exist.
        /// <br>Note that while it is not necessarily invalid for multiple edges to 
        /// exist between two vertices, it is generally undesirable</br>
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public Edge CreateEdge(Vertex one, Vertex two)
        {
            Edge? edgeMaybe = one.GetEdgeTo(two);
            if (edgeMaybe == null)
            {
                edgeMaybe = CreateEdgeUnchecked(one, two);
            }
            return edgeMaybe;
        }

        /// <summary>
        /// Create an edge between the given two vertices skipping checks.
        /// </summary>
        /// <param name="vertex1">vertex 1</param>
        /// <param name="vertex2">vertex 2</param>
        /// <returns>edge between them</returns>
        public Edge CreateEdgeUnchecked(Vertex one, Vertex two)
        {
            Edge edge = new(one, two);
            one.AddEdgeUnchecked(edge);
            two.AddEdgeUnchecked(edge);
            Edges.Add(edge);
            return edge;
        }


        internal void CreateFaceLoop(ICollection<Vertex> verticesIn, List<Edge> edgesOut)
        {
            var iter = verticesIn.GetEnumerator();
            // 0 element guard
            if (!iter.MoveNext())
                return;
            Vertex first = iter.Current;
            Vertex vert1;
            Vertex vert2 = first;
            while (iter.MoveNext())
            {
                vert1 = iter.Current;
                edgesOut.Add(CreateEdge(vert1, vert2));
                vert2 = vert1;
            }
            // 1 element guard
            if (vert2 == first)
                return;
            edgesOut.Add(CreateEdge(vert2, first));
        }

        internal TriangleEdges CreateFaceLoop(TriangleVerts verts)
        {
            Edge e0 = CreateEdge(verts.f0, verts.f1);
            Edge e1 = CreateEdge(verts.f1, verts.f2);
            Edge e2 = CreateEdge(verts.f2, verts.f0);
            return new TriangleEdges(e0, e1, e2);
        }

        internal QuadEdges CreateFaceLoop(QuadVerts verts)
        {
            Edge e0 = CreateEdge(verts.f0, verts.f1);
            Edge e1 = CreateEdge(verts.f1, verts.f2);
            Edge e2 = CreateEdge(verts.f2, verts.f3);
            Edge e3 = CreateEdge(verts.f3, verts.f0);
            return new QuadEdges(e0, e1, e2, e3);
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
        public Face CreateQuadOld(QuadVerts verts, QuadUVs uv0s = default)
        {
            QuadEdges faceLoop = CreateFaceLoop(verts);
            var face = new Face(verts, faceLoop, uv0s);
            Faces.Add(face);
            FixFaceIfPossible(face);
            return face;
        }

        internal Face? CreateQuadFromEdges(QuadVerts verts)
        {
            Vertex zero = verts.f0;
            Vertex one;
            Vertex two;
            Vertex three;
            if (zero.IsConnected(verts.f1))
            {
                one = verts.f1;
                if (one.IsConnected(verts.f2))
                {
                    two = verts.f2;
                    three = verts.f3;
                } else
                {
                    two = verts.f3;
                    three = verts.f2;
                }
            } else if (zero.IsConnected(verts.f2))
            {
                one = verts.f2;
                if (one.IsConnected(verts.f1))
                {
                    two = verts.f1;
                    three = verts.f3;
                }
                else
                {
                    two = verts.f3;
                    three = verts.f1;
                }
            } else
            {
                return null;
            }
            return CreateQuadOld(new QuadVerts(zero, one, two, three));
        }

        public Face CreateQuad(QuadVerts verts, QuadUVs uv0s = default)
        {
            float3 vec1 = verts.f1.Position - verts.f0.Position; // 0 and 1
            float3 vec2 = verts.f2.Position - verts.f0.Position; // 0 and 2
            float3 vec3 = verts.f3.Position - verts.f0.Position; // 0 and 3
            
            float3 norm1 = cross(vec1, vec2);
            float3 norm2 = cross(vec2, vec3);
            float3 norm3 = cross(vec3, vec1);
            Face face;

            float angle1 = Vector3.Angle(norm1, norm2);
            float angle2 = Vector3.Angle(norm2, norm3);
            float angle3 = Vector3.Angle(norm3, norm1);

            Debug.Log($"Angles: {angle1}, {angle2}, {angle3}");
            if (angle1 == 0 && angle2 == 0 && angle3 == 0)
            {
                Debug.Log($"Norms: {norm1}, {norm2}, {norm3}");
                Debug.Log($"Vecs: {vec1}, {vec2}, {vec3}");
            }

            if (angle1 > -45 && angle1 < 45)
            {
                face = CreateQuadOld(verts, uv0s);
            } else if (angle2 > -45 && angle2 < 45)
            {
                face = CreateQuadOld(new QuadVerts(verts.f0, verts.f2, verts.f3, verts.f1), uv0s);

            } else if (angle3 > -45 && angle3 < 45)
            {
                face = CreateQuadOld(new QuadVerts(verts.f0, verts.f3, verts.f1, verts.f2), uv0s);

            } else
            {
                throw new ArgumentException("Surprise result!");
            }

            //FixFaceIfPossible(face);
            return face;
        }

        private void FixFaceIfPossible(Face face)
        {
            foreach (Edge edge in face.edges)
            {
                foreach (Face otherFace in edge.GetEdgeLoopsIter())
                {
                    if (otherFace != face)
                    {
                        if (face.IsOrderedClockwise(edge.one, edge.two) == otherFace.IsOrderedClockwise(edge.one, edge.two)) {
                            face.FlipFace(true);
                            return;
                        }
                    }
                }
            }
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
            TriangleEdges faceLoop = CreateFaceLoop(vertices);
            var face = new Face(vertices, faceLoop, uv0s);
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
        public Face CreateNGon(ICollection<Vertex> vertices)
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
        public Face CreateNGon(ICollection<Vertex> vertices, ICollection<float2> uv0s)
        {
            List<Edge> faceLoop = new List<Edge>(vertices.Count);
            CreateFaceLoop(vertices, faceLoop);
            Face face = new Face(vertices, uv0s, faceLoop);
            Faces.Add(face);
            return face;
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
        /// Add a presumably externally created vertex to the EditableMesh without any checks.
        /// </summary>
        /// <param name="vertex">vertex to add</param>
        /// <returns>the vertex</returns>
        public Vertex AddVertexUnchecked(Vertex vertex)
        {
            Vertices.Add(vertex);
            return vertex;
        }

        /// <summary>
        /// Add a presumably externally created face to the EditableMesh without any checks.
        /// </summary>
        /// <param name="face">face to add</param>
        /// <returns>the face</returns>
        public Face AddFaceUnchecked(Face face)
        {
            Faces.Add(face);
            return face;
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
    }
}