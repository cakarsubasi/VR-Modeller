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
        /// Create an unconnected Vertex by copying the parameters of another Vertex
        /// </summary>
        /// <param name="other">Vertex to copy from</param>
        /// <returns>The new Vertex</returns>
        public static Vertex Copy(Vertex other)
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
        /// <returns>True if vertex is manifold geometry</returns>
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

        /// <summary>
        /// Create a new list of faces shared by this vertex. 
        /// If you need an iterator use FacesIter
        /// </summary>
        /// <returns>List of faces</returns>
        public List<Face> GetFaces()
        {
            List<Face> faces = new(FaceCount);
            GetFaces(faces);
            return faces;
        }

        /// <summary>
        /// Get a list of faces without allocations. The list is cleared first.
        /// </summary>
        /// <param name="faces">out list of faces</param>
        public void GetFaces(in List<Face> faces)
        {
            faces.Clear();
            foreach (Face face in FacesIter)
            {
                faces.Add(face);
            }
        }

        /// <summary>
        /// Get a list of vertices connected to this vertex.
        /// </summary>
        /// <returns>list of vertices</returns>
        public List<Vertex> GetConnectedVertices()
        {
            List<Vertex> vertices = new(EdgeCount);
            GetConnectedVertices(vertices);
            return vertices;
        }

        /// <summary>
        /// Get a list of vertices connected to this vertex without allocations.
        /// </summary>
        /// <param name="vertices">out list of vertices</param>
        public void GetConnectedVertices(List<Vertex> vertices)
        {
            vertices.Clear();
            foreach (Edge edge in edges)
            {
                Vertex other = edge.Other(this);
                if (!vertices.Contains(other))
                {
                    vertices.Add(other);
                }
            }
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
                if (ReferenceEquals(edge.Other(this), other))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this vertex is a part of the given edge. It is generally faster to check
        /// this the other way around if the geometry is in a valid state.
        /// </summary>
        /// <param name="edge">Edge to check</param>
        /// <returns>true if connected</returns>
        public bool IsConnected(Edge edge)
        {
            return edges.Contains(edge);
        }

        /// <summary>
        /// Returns true if this vertex is connected to the given face.
        /// </summary>
        /// <param name="face">Face to check</param>
        /// <returns>true if connected</returns>
        public bool IsConnected(Face face)
        {
            foreach (Face candidateFace in FacesIter)
            {
                if (ReferenceEquals(candidateFace, face))
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
                if (ReferenceEquals(edge.Other(this), other))
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
        /// Get the indices for this vertex's stream.
        /// Use WriteToStream() to avoid allocations.
        /// </summary>
        /// <returns>The stream indices</returns>
        public int[] Indices()
        {
            return faces.Select(props => props.index).ToArray();
        }

        /// <summary>
        /// Write the vertex to a vertex buffer without allocations
        /// </summary>
        /// <param name="stream">stream to write into</param>
        internal void WriteToStream(ref NativeArray<Stream0> stream, ShadingType fallbackShading = ShadingType.Smooth)
        {
            Stream0 temp = default;
            temp.position = Position;
            temp.normal = Normal;
            temp.tangent = Tangent;
            //temp.selected = Selected ? 1.0f : 0.0f;
            foreach (FaceIndex faceIndex in faces)
            {
                //temp.uv0 = faceIndex.uv0;
                faceIndex.face.GetUVandSelection(this, out temp.uv0, out temp.selected);
                ShadingType faceShading = faceIndex.face.Shading;
                temp.normal = Normal;
                if (faceShading == ShadingType.Smooth)
                {
                    temp.normal = Normal;
                }
                else if (faceShading == ShadingType.Flat)
                {
                    temp.normal = faceIndex.face.Normal;
                    temp.tangent = faceIndex.face.Tangent;
                } 
                else
                {
                    if (fallbackShading == ShadingType.Smooth)
                    {
                        temp.normal = Normal;
                    }
                    else if (fallbackShading == ShadingType.Flat)
                    {
                        temp.normal = faceIndex.face.Normal;
                        temp.tangent = faceIndex.face.Tangent;
                    }
                }
                stream[faceIndex.index] = temp;
            }
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
                if (uv0next.Equals(uv0) || prop.face.Shading != ShadingType.Inherited)
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

        public int OptimizeIndicesAlt(int beginning)
        {
            for (int i = 0; i < faces.Count; ++i)
            {
                var prop = faces[i];
                prop.uv0 = prop.face.GetUVof(this);
                prop.index = beginning + i;
                faces[i] = prop;
            }
            return beginning + faces.Count;
        }

        /// <summary>
        /// Get the stream index associated with each face. Returns -1 if the face is not found.
        /// </summary>
        /// <param name="face">Face to check</param>
        /// <returns>Stream index or -1 if the face is not connected</returns>
        public int GetIndex(Face face)
        {
            foreach (FaceIndex faceIndex in faces)
            {
                if (ReferenceEquals(faceIndex.face, face))
                {
                    return faceIndex.index;
                }
            }
            return -1;
        }

        public void Transform(Matrix4x4 transform)
        {
            float4 pos = transform * new Vector4(Position.x, Position.y, Position.z, 1);
            Position = pos.xyz;
            float4 norm = transform * new Vector4(Normal.x, Normal.y, Normal.z, 0);
            Normal = norm.xyz;
            Tangent = transform * Tangent;
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
                Normal /= (float) faces.Count;
            }
        }

        /// <summary>
        /// Recalculate the tangents based on the normal in the laziest possible way.
        /// The normal should be calculated before this is called.
        /// </summary>
        public void RecalculateTangent()
        {
            Tangent = float4(-Normal.y, Normal.x, Normal.z, 0f);
        }

        /// <summary>
        /// Add a face to the internal faces list, but check if the face
        /// is already included first
        /// </summary>
        /// <param name="face">face to add</param>
        internal void AddFaceChecked(Face face)
        {
            if (!IsConnected(face))
            {
                faces.Add(new FaceIndex { face = face });
            }
            else
            {
                Debug.Log($"Tried adding existing face: {face}");
            }
        }

        /// <summary>
        /// Add an edge to the internal edges list, but check
        /// if the edge is already in the list first.
        /// </summary>
        /// <param name="edge">edge to add</param>
        internal void AddEdgeChecked(Edge edge)
        {
            if (IsConnected(edge))
            {
                return;
            } else
            {
                AddEdgeUnchecked(edge);
            }
        }

        /// <summary>
        /// Add an edge to the internal edges list, without performing any checks.
        /// </summary>
        /// <param name="edge"></param>
        internal void AddEdgeUnchecked(Edge edge)
        {
            edges.Add(edge);
        }

        internal void Delete()
        {
            // clear edges first

            
            foreach (FaceIndex face in faces)
            {
                face.face.Delete();
            }


            foreach (Edge edge in edges)
            {
                edge.Delete();
                edge.Other(this).RemoveDeadFaces();
                edge.Other(this).RemoveEdge(edge);
            }
            edges.Clear();
            faces.Clear();
            edges.Clear();
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

        internal void RemoveDeadFaces()
        {
            faces.RemoveAll(faceInd => !faceInd.face.Alive);
        }

        internal void RemoveDuplicateEdges(bool delete = false)
        {
            if (edges.Count < 2)
            {
                return;
            }
            for (int i = 0; i < edges.Count - 1; i++)
            {
                for (int j = i + 1;  j < edges.Count; j++)
                {
                    Edge edge = edges[j];
                    if (edges[i].Equals(edge))
                    {
                        edges.RemoveAt(j);
                        j--;
                        if (delete)
                        {
                            edge.Delete();
                        }
                    }
                        
                }
            }
        }

        internal bool RemoveFaceUnchecked(Face face)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                if (ReferenceEquals(faces[i].face, face))
                {
                    faces.RemoveAt(i);
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

        /// <summary>
        /// Fill the collection common with faces that exist in both this vertex and other vertex.
        /// The collection is cleared before it is filled.
        /// </summary>
        /// <param name="other">other vertex</param>
        /// <param name="common">collection to fill</param>
        internal void CommonFaces(Vertex other, ICollection<Face> common)
        {
            common.Clear();
            foreach (Face face in FacesIter)
            {
                if (other.IsConnected(face))
                {
                    common.Add(face);
                }
            }
        }

        /// <summary>
        /// Fill the collection unique with faces of this vertex that are not in the other vertex.
        /// The collection is cleared before it is filled.
        /// </summary>
        /// <param name="other">other vertex</param>
        /// <param name="unique">collection to fill</param>
        internal void UniqueFaces(Vertex other, ICollection<Face> unique)
        {
            unique.Clear();
            foreach (Face face in FacesIter)
            {
                if (!other.IsConnected(face))
                {
                    unique.Add(face);
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
        /// <summary>
        /// Construct an edge between the two vertices.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        internal Edge(Vertex one, Vertex two)
        {
            if (ReferenceEquals(one, two))
            {
                throw new ArgumentException("Vertices must be unique");
            }
            this.one = one;
            this.two = two;
        }

        /// <summary>
        /// Returns true if edge is between vertex1 and vertex2
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        public bool IsBetween(Vertex vertex1, Vertex vertex2)
        {
            return ((ReferenceEquals(one, vertex1) && ReferenceEquals(two, vertex2)) || 
                (ReferenceEquals(one, vertex2) && ReferenceEquals(two, vertex1)));
        }

        /// <summary>
        /// Returns true if vertex is a part of this edge.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Contains(Vertex vertex)
        {
            return ReferenceEquals(one, vertex) || ReferenceEquals(two, vertex);
        }

        /// <summary>
        /// Pass one vertex, get the other vertex, throws if vertex is not part of this edge.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns>The other vertex</returns>
        public Vertex Other(Vertex vertex)
        {
            if (ReferenceEquals(one, vertex))
            {
                return two;
            }
            else if (ReferenceEquals(two, vertex))
            {
                return one;
            }
            else
            {
                throw new VertexNotInEdgeException("ExchangeVertex", vertex, this);
            }
        }

        /// <summary>
        /// Elementwise comparison. Two edges are equal if they are between the same vertices.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Edge? otherEdge = obj as Edge;
            if (otherEdge == null)
            {
                return false;
            }
            // this guards against invalid edges that are between the same vertex
            if (otherEdge.Contains(one))
            {
                if (ReferenceEquals(otherEdge.Other(one), two))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Exchange one vertex in this edge with another vertex, without any other validation.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="replacement"></param>
        public void ExchangeVertex(Vertex current, Vertex replacement)
        {
            if (ReferenceEquals(one, current))
            {
                one = replacement;
            }
            else if (ReferenceEquals(two, current))
            {
                two = replacement;
            }
            else
            {
                throw new VertexNotInEdgeException("ExchangeVertex", current, this);
            }
        }

        /// <summary>
        /// Get the faces that are shared by this edge without allocations.
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

        /// <summary>
        /// Get the faces that are shared by this edge.
        /// </summary>
        /// <returns>Faces of this edge</returns>
        public List<Face> GetEdgeLoops()
        {
            List<Face> ret = new();
            GetEdgeLoops(ret);
            return ret;
        }

        /// <summary>
        /// Get an iterator to the edge loop.
        /// </summary>
        /// <returns>Iterator to the edge loop</returns>
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

        /// <summary>
        /// Move this edge by the relative offset
        /// </summary>
        /// <param name="relative">offset</param>
        public void MoveRelative(float3 relative)
        {
            // need a more robust API later
            one.Position += relative;
            two.Position += relative;
        }

        /// <summary>
        /// Delete the edge, also deletes the faces in the edge loop, but not the vertices.
        /// </summary>
        public void Delete()
        {
            Alive = false;
        }

        /// <summary>
        /// Get the number of faces shared by this edge.
        /// </summary>
        /// <returns></returns>
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
            return EdgeLoopCount == 2;
        }
    }

    partial class Face
    {
        /// <summary>
        /// Construct degenerate face
        /// </summary>
        private Face()
        {
            Normal = Position = Unity.Mathematics.float3.zero;
            vertices = new List<VertexCoordinate>(0);
            edges = new List<Edge>(0);
        }

        /// <summary>
        /// Get a degenerate face. It is probably not what you need, but good for testing.
        /// </summary>
        /// <returns>A face with no vertices or edges</returns>
        public static Face Degenerate()
        {
            return new Face();
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
            RecalculateNormalFast();
            RecalculateTangent();
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
        /// Recalculate the normals, only check three vertices to save time.
        /// </summary>
        public void RecalculateNormalFast()
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

        /// <summary>
        /// Recalculate the tangents based on the normal in the laziest possible way.
        /// The normal should be calculated before this is called.
        /// </summary>
        public void RecalculateTangent()
        {
            Tangent = float4(-Normal.y, Normal.x, Normal.z, 0f);
        }

        /// <summary>
        /// Recalculate the position based on averaging the position of the other vertices.
        /// </summary>
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
        /// Get an iterator to the vertices
        /// </summary>
        /// <returns>An iterator to the vertices</returns>
        internal IEnumerable<Vertex> GetVerticesIter()
        {
            foreach (var vertexIndex in vertices)
            {
                yield return vertexIndex.vertex;
            }
        }

        public IEnumerable<FaceVertexInfo> GetVertexInfo(ShadingType fallback = ShadingType.Flat)
        {
            float3 normal = Normal;
            foreach (VertexCoordinate coor in vertices)
            {
                    yield return new FaceVertexInfo
                    {
                        Index = coor.vertex.Index,
                        uv0 = coor.uv0,
                        normal = (Shading == ShadingType.Smooth || fallback == ShadingType.Smooth) ? coor.vertex.Normal : normal,
                };
            }
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

        /// <summary>
        /// Return true if the vertex is part of this face.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool ContainsVertex(Vertex vertex)
        {
            return VerticesIter.Contains(vertex);
        }

        /// <summary>
        /// This method returns true if vert2 is right after vert1 assuming
        /// the vertices within this face form a circular list. This is useful
        /// mainly for ensuring the normal face the correct direction.
        /// </summary>
        /// <param name="vert1"></param>
        /// <param name="vert2"></param>
        /// <returns></returns>
        internal bool IsOrderedClockwise(Vertex vert1, Vertex vert2)
        {
            int vert1Index = GetVertexIndex(vert1);
            if (vert1Index == -1)
            {
                throw new VertexNotInFaceException("IsOrderedClockwise()", vert1, this);
            }
            int vert2Index = GetVertexIndex(vert2);
            if (vert2Index == -1)
            {
                throw new VertexNotInFaceException("IsOrderedClockwise()", vert2, this);
            }

            if (vert2Index == vert1Index + 1)
            {
                return true;
            } else if (vert1Index == vert2Index + 1)
            {
                return false;
            } else if (vert2Index == 0 && vert1Index == vertices.Count - 1)
            {
                return true;
            } else if (vert1Index == 0 && vert2Index == vertices.Count - 1)
            {
                return false;
            }
            return false;
        }
        
        /// <summary>
        /// Move the face with the given relative offset.
        /// </summary>
        /// <param name="relative">relative offset</param>
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
        public void FlipFace(bool flipNormal = true)
        {
            vertices.Reverse();
            if (flipNormal)
            {
                Normal = -Normal;
            }
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
                if (ReferenceEquals(vert.vertex, vertex))
                {
                    return vert.uv0;
                }
            }
            return float2(0f, 0f);
        }

        public void GetUVandSelection(in Vertex vertex, out float2 uv0, out float3 selection)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (ReferenceEquals(vertex, vertices[i].vertex))
                {
                    uv0 = vertices[i].uv0;
                    int prev = (i - 1) < 0 ? vertices.Count - 1 : i -1;
                    int next = (i + 1) % vertices.Count;
                    
                    selection = float3(
                        vertices[prev].vertex.Selected ? 1f : 0f,
                        vertex.Selected ? 1f : 0f,
                        vertices[next].vertex.Selected ? 1f : 0f
                        );
                    return;
                }
            }
            uv0 = float2(0f, 0f);
            selection = float3(0f,0f,0f);
        }

        internal void RemoveVertexUnchecked(Vertex vertex)
        {
            int index = GetVertexIndex(vertex);
            if (index != -1)
            {
                vertices.RemoveAt(index);
            }
        }

        /// <summary>
        /// Delete this face, but not the edges or the vertices that comprise it.
        /// </summary>
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
            if (triangles == 0)
            {
                return startIndex;
            }
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

        private int GetVertexIndex(Vertex vertex)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (ReferenceEquals(vertices[i].vertex, vertex))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Replace the current vertex with the replacement vertex at the same position.
        /// Check if current vertex exists in the face but do not check if these form a valid simple path.
        /// <br>Also update the vertices if updateVertices is set</br>
        /// </summary>
        /// <param name="current"></param>
        /// <param name="replacement"></param>
        internal void ExchangeVertexUnchecked(Vertex current, Vertex replacement, bool updateVertices = true)
        {
            int index = GetVertexIndex(current);
            if (index == -1)
            {
                throw new VertexNotInFaceException("ExchangeVertexUnchecked()", current, this);
            }
            vertices[index] = new VertexCoordinate
            {
                vertex = replacement,
                uv0 = vertices[index].uv0
            };
            // if we are iterating over the faces in one vertex, updating them will throw
            if (updateVertices)
            {
                current.RemoveFaceUnchecked(this);
                replacement.AddFaceChecked(this);
            }
        }
    }
}