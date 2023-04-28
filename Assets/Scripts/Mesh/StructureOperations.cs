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

    partial class Vertex
    {

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
        private static Vertex FromOtherVertexConnected(Vertex other, out Edge connection)
        {
            Vertex self = new Vertex
            {
                Position = other.Position,
                Normal = other.Normal,
                Tangent = other.Tangent,
            };

            connection = new Edge(other, self);
            self.edges.Add(connection);
            other.edges.Add(connection);

            return self;
        }

        public static Vertex FromOtherVertexUnconnected(Vertex other)
        {
            Vertex self = new Vertex
            {
                Position = other.Position,
                Normal = other.Normal,
                Tangent = other.Tangent,
            };
            return self;
        }

        /// <summary>
        /// A Vertex is manifold geometry if and only if:
        /// <list type="number">
        /// <item>It contains at least two edges</item>
        /// <item>All of its edges are manifold geometry</item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        internal bool IsManifold()
        {
            if (edges.Count < 2)
                return false;

            foreach (Edge edge in edges)
            {
                if (!edge.IsManifold())
                    return false;
            }
            return true;
        }

        public List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>(this.faces.Count);
            foreach (FaceIndex faceIndex in this.faces)
            {
                faces.Add(faceIndex.face);
            }
            return faces;
        }

        public List<Vertex> GetConnectedVertices()
        {
            List<Vertex> vertices = new List<Vertex>(this.edges.Count);
            foreach (Edge edge in edges)
            {
                vertices.Add(edge.Other(this));
            }
            return vertices;
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

        public bool IsConnected(Edge edge)
        {
            return edges.Contains(edge);
        }

        public bool IsConnected(Face face)
        {
            foreach (FaceIndex faceIndex in faces)
            {
                if (faceIndex.face == face)
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
                temp.uv0 = faces[i].uv0;
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
            if (faces.Count == 0)
            {
                return beginning;
            }
            
            var firstProp = faces[0];
            float2 uv0 = firstProp.face.GetUVof(this);
            firstProp.index = beginning;
            firstProp.uv0 = uv0;
            faces[0] = firstProp;

            int used = 1;
            for (int i = 1; i < faces.Count; ++i)
            {
                var prop = faces[i];
                float2 uv0next = prop.face.GetUVof(this);
                if (uv0next.Equals(uv0))
                {
                    prop.index = beginning;
                    prop.uv0 = uv0;
                } else
                {
                    prop.index = beginning + used;
                    prop.uv0 = uv0next;
                    used++;
                }
                faces[i] = prop;

            }
            return beginning + used;
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
            }
            else
            {
                Debug.Log($"Tried adding existing face: {face}");
            }
        }

        /// <summary>
        /// Check if there is an Edge between this vertex and the other vertex. 
        /// Return the edge. If a new edge is created
        /// </summary>
        /// <param name="vertex">vertex to connect to</param>
        /// <returns></returns>
        internal bool AddEdgeCheckedFromVertex(Vertex other, out Edge edgeBetween)
        {
            foreach (Edge edge in edges)
            {
                if (edge.IsBetween(this, other))
                {
                    edgeBetween = edge;
                    return false;
                }
            }

            edgeBetween = AddEdgeUncheckedFromVertex(other);
            return false;
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

        internal void AddEdgeUnchecked(Edge edge)
        {
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
            other.edges.Add(newEdge);
            return newEdge;
        }

        internal void Delete()
        {
            // clear edges first
            foreach (Edge edge in edges)
            {
                edge.Delete();
                edge.Other(this).RemoveEdge(edge);
            }
            edges.Clear();

            foreach (FaceIndex face in faces)
            {
                face.face.Delete();
            }
            faces.Clear();
            Alive = false;
        }

        internal void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
        }

        internal void RemoveUnconnectedEdges()
        {
            edges.RemoveAll(edge => !edge.Contains(this));
        }

        internal bool RemoveFaceUnchecked(Face face)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i].face == face)
                {
                    faces.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool HasFace(Face face)
        {
            foreach (FaceIndex candidate in faces)
            {
                if (candidate.face == face)
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerable<Face> GetFacesEnum()
        {
            foreach (FaceIndex face in faces)
            {
                yield return face.face;
            }
        }

        internal void CommonFaces(Vertex other, List<Face> common)
        {
            foreach (FaceIndex face in faces)
            {
                if (other.HasFace(face.face))
                {
                    common.Add(face.face);
                }
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}: position: {Position}, Faces: {faces.Count}, Edges: {edges.Count}";
        }
    }

    partial class Edge
    {
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

        public Boolean Contains(Vertex vertex)
        {
            return ((one == vertex) || (two == vertex));
        }

        public Vertex Other(Vertex vertex)
        {
            if (one == vertex)
            {
                return two;
            }
            else if (two == vertex)
            {
                return one;
            }
            else
            {
                throw new ArgumentException("Given vertex must be in the edge");
            }
        }

        public void ExchangeVertex(Vertex current, Vertex replacement)
        {
            if (one == current)
            {
                one = replacement;
            }
            else if (two == current)
            {
                two = replacement;
            }
            else
            {
                throw new ArgumentException("current must be a part of the edge");
            }
        }

        /// <summary>
        /// A manifold edge may only have one or two faces. Generally speaking, we
        /// are interested in avoiding non-manifold geometry. 
        /// </summary>
        /// <param name="oFaces">(output) faces of this edge</param>
        public void GetEdgeLoops(List<Face> outFaces)
        {
            outFaces.Clear();
            foreach (Face face in one.FacesIter)
            {
                if (two.FacesIter.Contains(face))
                {
                    outFaces.Add(face);
                }
            }
        }

        public List<Face> GetEdgeLoops()
        {
            List<Face> ret = new();
            GetEdgeLoops(ret);
            return ret;
        }

        public IEnumerable<Face> GetEdgeLoopsIter()
        {
            foreach (Face face in one.FacesIter)
            {
                if (two.FacesIter.Contains(face))
                {
                    yield return face;
                }
            }
        }

        public void MoveRelative(float3 relative)
        {
            // need a more robust API later
            one.Position += relative;
            two.Position += relative;
        }

        public void Delete()
        {
            foreach (Face face in GetEdgeLoopsIter())
            {
                face.Delete();
            }
            Alive = false;
        }

        internal int GetEdgeLoopCount()
        {
            int loops = 0;
            foreach (Face face in one.FacesIter)
            {
                if (two.FacesIter.Contains(face))
                {
                    loops++;
                }
            }
            return loops;
        }

        /// <summary>
        /// An edge is manifold geometry iff its edge loop contains exactly 2 elements.
        /// </summary>
        /// <returns></returns>
        internal bool IsManifold()
        {
            return (EdgeLoopCount == 2);
        }
    }

    partial class Face
    {
        /// <summary>
        /// Construct degenerate face
        /// </summary>
        public Face()
        {
            Normal = Position = Unity.Mathematics.float3.zero;
            vertices = new List<VertexCoordinate>(0);
            edges = new List<Edge>(0);
        }

        /// <summary>
        /// Arbitrary vertex count constructor
        /// </summary>
        /// <exception cref="ArgumentException">vertices and uv0s have different size</exception>
        /// <param name="vertices">vertices</param>
        /// <param name="uv0s">uv coordinates</param>
        public Face(ICollection<Vertex> vertices, ICollection<float2> uv0s, List<Edge> edges)
        {
            if (vertices.Count != uv0s.Count || uv0s.Count != edges.Count)
            {
                throw new ArgumentException("vertices and uv0s must have the same length");
            }
            
            Normal = Position = default;
            this.vertices = new List<VertexCoordinate>(vertices.Count);
            this.edges = edges;

            foreach ((Vertex vert, float2 uv) in vertices.Zip(uv0s, Tuple.Create)) 
            {
                this.vertices.Add(new VertexCoordinate
                {
                    vertex = vert,
                    uv0 = uv
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
        public Face(TriangleElement<Vertex> vertices, TriangleElement<Edge> edges, TriangleElement<float2> uv0s = default)
        {
            Normal = Position = default;
            this.vertices = new List<VertexCoordinate>(3);
            this.edges = new List<Edge>(3);
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f0,
                uv0 = uv0s.f0
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f1,
                uv0 = uv0s.f1
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f2,
                uv0 = uv0s.f2
            });
            this.edges.Add(edges.f0);
            this.edges.Add(edges.f1);
            this.edges.Add(edges.f2);
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
        public Face(QuadElement<Vertex> vertices, QuadElement<Edge> edges, QuadElement<float2> uv0s = default)
        {
            Normal = Position = default;
            this.vertices = new List<VertexCoordinate>(4);
            this.edges = new List<Edge>(4);
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f0,
                uv0 = uv0s.f0
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f1,
                uv0 = uv0s.f1
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f2,
                uv0 = uv0s.f2
            });
            this.vertices.Add(new VertexCoordinate
            {
                vertex = vertices.f3,
                uv0 = uv0s.f3
            });
            foreach (var vert in this.vertices)
            {
                vert.vertex.AddFaceChecked(this);
            }
            this.edges.Add(edges.f0);
            this.edges.Add(edges.f1);
            this.edges.Add(edges.f2);
            this.edges.Add(edges.f3);
            FinalizeSetup();
        }

        /// <summary>
        /// Create edges around the vertices, calculate normals and position.
        /// </summary>
        private void FinalizeSetup()
        {
            RecalculateNormal();
            RecalculatePosition();
        }

        internal void DiscoverEdgesFromVertices()
        {
            edges.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex vert1 = vertices[i].vertex;
                Vertex vert2 = vertices[(i + 1) % vertices.Count].vertex;
                Edge? edge = vert1.GetEdgeTo(vert2);
                edges.Add(edge);
            }
        }

        /// <summary>
        /// Add vertices to a collection without temporary allocations
        /// </summary>
        /// <param name="collection">the collection</param>
        public void AddVerticesTo(ICollection<Vertex> collection)
        {
            foreach (VertexCoordinate vertex in vertices)
            {
                collection.Add(vertex.vertex);
            }
        }

        public void RecalculateNormal()
        {
            if (vertices.Count < 3)
            {
                Normal = 0f;
            }
            else
            {
                var vec1 = vertices[1].Position - vertices[0].Position;
                var vec2 = vertices[2].Position - vertices[0].Position;
                Normal = normalize(cross(vec1, vec2));
            }
        }

        public void RecalculatePosition()
        {
            Position = 0f;
            foreach (var vert in vertices)
            {
                Position += vert.Position;
            }
            Position /= vertices.Count;
        }

        /// <summary>
        /// Get the number of triangles required to show this face
        /// </summary>
        /// <returns>0 if less than 3 vertices, number of vertices minus 2 otherwise</returns>
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

        public bool ContainsVertex(Vertex vertex)
        {
            foreach (VertexCoordinate vertexCoordinate in vertices)
            {
                if (vertexCoordinate.vertex == vertex)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOrderedClockwise(Vertex vert1, Vertex vert2)
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                if (vertices[i].vertex == vert1)
                {
                    if (vertices[(i+1) % vertices.Count].vertex == vert2)
                    {
                        return true;
                    } else if (vertices[Math.Abs((i - 1) % vertices.Count)].vertex == vert2)
                    {
                        return false;
                    }
                }
            }
            throw new ArgumentException("vertices should be in the face");
        }
        

        public void MoveRelative(float3 relative)
        {
            // add a more robust transform API later
            foreach (var vert in vertices)
            {
                vert.vertex.Position += relative;
            }
            Position += relative;
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
                Normal = -Normal;
            }
        }

        internal void AddVertex()
        {
            throw new NotImplementedException { };
        }

        internal bool RemoveVertexUnchecked(Vertex vertex)
        {
            for (int i = 0; i < vertices.Count; ++i)
            {
                if (vertices[i].vertex == vertex)
                {
                    vertices.RemoveAt(i);
                    return true;
                }
            }
            return false;
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

        public void InsertVertexBetweenUnchecked(Vertex toInsert, Vertex before, Vertex after)
        {
            int beforeIndex = -1;
            int afterIndex = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].vertex == before)
                    beforeIndex = i;
                else if (vertices[i].vertex == after)
                    afterIndex = i;
            }
            if ((beforeIndex == -1) || afterIndex == -1)
                throw new ArgumentException("before and after must be inside the face");

            VertexCoordinate insertElement = new VertexCoordinate
            {
                vertex = toInsert,
                uv0 = (vertices[beforeIndex].uv0 + vertices[afterIndex].uv0) / 2f,
            };

            if (afterIndex > beforeIndex)
            {
                vertices.Insert(afterIndex, insertElement);
            }
            else if (beforeIndex > afterIndex)
            {
                vertices.Insert(beforeIndex, insertElement);
            }
            else
            {
                throw new ArgumentException("before and after must not be the same");
            }

        }

        public List<Face> GetAdjacentFaces()
        {
            throw new NotImplementedException { };
        }

        public void GetAdjacentFaces(List<Face> faces)
        {
            throw new NotImplementedException { };
        }


        internal void Delete()
        {
            edges.Clear();
            vertices.Clear();
            Alive = false;
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
            var str = $"{base.ToString()}: verts: {vertices.Count}, position: {Position}, normal: {Normal}\n\tindices:";
            foreach (var vertex in vertices)
            {
                str = $"{str} {vertex.vertex.GetIndex(this)}";
            }
            return str;
        }

        internal Vertex GetOtherVertexToTheSideOf(Vertex middle, Vertex oneSide)
        {
            if (vertices.Count < 3)
            {
                throw new ArgumentException("Not enough vertices");
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].vertex == middle && vertices[(i + 1) % vertices.Count].vertex == oneSide)
                {
                    if (i == 0)
                    {
                        return vertices[vertices.Count - 1].vertex;
                    }
                    else
                    {
                        return vertices[i - 1].vertex;
                    }
                }
                else if (vertices[i].vertex == oneSide && vertices[(i + 1) % vertices.Count].vertex == middle)
                {
                    return vertices[(i + 2) % vertices.Count].vertex;
                }

            }
            throw new ArgumentException("Can't find either vertex");
        }

        private int GetVertexIndex(Vertex vertex)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].vertex == vertex)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void ExchangeVertexUnchecked(Vertex current, Vertex replacement)
        {
            int index = GetVertexIndex(current);
            if (index == -1)
            {
                throw new ArgumentException("vertex is not inside the face");
            }
            vertices[index] = new VertexCoordinate
            {
                vertex = replacement,
                uv0 = vertices[index].uv0
            };

            current.RemoveFaceUnchecked(this);
            replacement.AddFaceChecked(this);
        }
    }
}