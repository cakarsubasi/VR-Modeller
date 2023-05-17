using System;
using System.Collections.Generic;
using Unity.Mathematics;


using static Unity.Mathematics.math;

namespace Meshes
{
    partial struct UMesh
    {
        /// <summary>
        /// Delete a vertex but leave the connecting faces intact.
        /// <br></br>
        /// Generally speaking, this is not what you want.
        /// </summary>
        /// <param name="vertex">Vertex to dissolve</param>
        public void DissolveVertex(Vertex vertex)
        {
            Face face = MergeFaces(vertex.GetFaces(), vertex);
            if (face.TriangleCount == 0)
            {
                face.Delete();
            }
            ExecuteDeletion();
        }

        /// <summary>
        /// A somewhat questionable method to dissolve a vertex
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="mergeOn"></param>
        /// <returns></returns>
        private Face MergeFaces(List<Face> faces, Vertex mergeOn)
        {
            List<Vertex> newFaceVerts = new List<Vertex>(9);
            List<float2> newUVs = new List<float2>(9);
            foreach (Face face in faces)
            {
                var verts = face.Vertices;
                if (newFaceVerts.Count == 0)
                {
                    foreach(Vertex vert in LoopAroundVertex(verts, mergeOn))
                    {
                        newFaceVerts.Add(vert);
                        newUVs.Add(face.GetUVof(vert));
                    }
                } else
                {
                    var iter = LoopAroundVertex(verts, mergeOn).GetEnumerator();
                    iter.MoveNext();
                    while (iter.MoveNext())
                    {
                        Vertex vert = iter.Current;
                        newFaceVerts.Add(vert);
                        newUVs.Add(face.GetUVof(vert));
                    }
                }
            }
            mergeOn.Delete();
            return CreateNGon(newFaceVerts, newUVs);
        }

        /// <summary>
        /// Utility method to visit vertices in a list in a particular order
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="beginning"></param>
        /// <returns></returns>
        internal IEnumerable<Vertex> LoopAroundVertex(List<Vertex> verts, Vertex beginning)
        {
            int length = verts.Count;
            int begin = verts.IndexOf(beginning);

            if (begin == -1)
            {
                yield break;
            }

            for (int i = 1; i < length; ++i)
            {
                yield return verts[(begin + i) % length];
            }
        }

        /// <summary>
        /// Delete Vertex and its surrounding geometry
        /// </summary>
        /// <param name="vertex"></param>
        public void DeleteGeometry(Vertex vertex)
        {
            DeleteVertexSingle(vertex);
            ExecuteDeletion();
        }

        /// <summary>
        /// Delete given vertices and the surrounding geometry
        /// </summary>
        /// <param name="vertices"></param>
        public void DeleteGeometry(IEnumerable<Vertex> vertices)
        {
            foreach (Vertex vertex in vertices)
            {
                DeleteVertexSingle(vertex);
            }
            ExecuteDeletion();
        }

        public void DeleteGeometry(params Vertex[] vertices)
        {
            DeleteGeometry((IEnumerable<Vertex>) vertices);
        }

        /// <summary>
        /// Delete Edge and the faces it comprises
        /// </summary>
        /// <param name="edge"></param>
        public void DeleteGeometry(Edge edge)
        {
            DeleteEdgeSingle(edge);
            ExecuteDeletion();
        }

        /// <summary>
        /// Delete edges and the faces they comprise
        /// </summary>
        /// <param name="edges"></param>
        public void DeleteGeometry(IEnumerable<Edge> edges)
        {
            foreach (Edge edge in edges)
            {
                DeleteEdgeSingle(edge);
            }
            ExecuteDeletion();
        }

        /// <summary>
        /// Delete the face
        /// </summary>
        /// <param name="face"></param>
        public void DeleteGeometry(Face face)
        {
            DeleteFaceSingle(face);
            ExecuteDeletion();
        }

        /// <summary>
        /// Delete the faces
        /// </summary>
        /// <param name="faces"></param>
        public void DeleteGeometry(IEnumerable<Face> faces)
        {
            foreach (Face face in faces)
            {
                DeleteFaceSingle(face);
            }
            ExecuteDeletion();
        }

        [Obsolete("Use DeleteGeometry family of functions")]
        /// <summary>
        /// Delete a vertex and also delete its connecting faces.
        /// </summary>
        /// <param name="vertex">Vertex to delete</param>
        public void DeleteVertex(Vertex vertex)
        {
            vertex.Delete();
            ExecuteDeletion();
        }

        [Obsolete("Use DeleteGeometry family of functions")]
        /// <summary>
        /// Delete 
        /// </summary>
        /// <param name="vertices"></param>
        public void DeleteVertices(List<Vertex> vertices)
        {
            foreach (Vertex vertex in vertices)
            {
                vertex.Delete();
            }
            ExecuteDeletion();
        }

        public void DissolveVertices(List<Vertex> vertices)
        {
            throw new NotImplementedException { };
        }

        public Vertex MergeVertices(Vertex mergeOnVertex, Vertex otherVertex)
        {
            Vertex vert = MergeTwoVertices(mergeOnVertex, otherVertex);
            ExecuteDeletion();
            return vert;
        }

        public Vertex MergeVertices(params Vertex[] vertices)
        {
            if (vertices.Length < 2)
            {
                throw new ArgumentException("Need at least two vertices to merge");
            }

            Vertex first = vertices[0];
            for (int i = 1; i < vertices.Length; ++i)
            {
                first = MergeTwoVertices(first, vertices[i]);
            }
            ExecuteDeletion();
            return first;
        }

        public Vertex MergeVertices(List<Vertex> vertices)
        {
            if (vertices.Count < 2)
            {
                throw new ArgumentException("Need at least two vertices to merge");
            }

            Vertex first = vertices[0];
            for (int i = 1; i < vertices.Count; ++i)
            {
                first = MergeTwoVertices(first, vertices[i]);
            }
            ExecuteDeletion();
            return first;
        }

        public Vertex MergeVertices(IEnumerable<Vertex> vertices)
        {
            return MergeVertices(new List<Vertex>(vertices));
        }

        internal Vertex MergeTwoVertices(Vertex mergeOnVertex, Vertex otherVertex)
        {
            var sharedFaces = extrusionHelper.facesTemp;
            mergeOnVertex.CommonFaces(otherVertex, sharedFaces);
            var otherUniqueFaces = extrusionHelper.faceSetTemp;
            otherVertex.UniqueFaces(mergeOnVertex, otherUniqueFaces);
            // if there is an edge between, dissolve it
            Edge edgeMaybe = mergeOnVertex.GetEdgeTo(otherVertex);
            if (edgeMaybe != null)
            {
                edgeMaybe.Delete();
            }

            foreach (Edge edge in otherVertex.edges)
            {
                edge.ExchangeVertex(otherVertex, mergeOnVertex);
                mergeOnVertex.AddEdgeUnchecked(edge);
            }
            mergeOnVertex.RemoveDuplicateEdges();

            foreach (Face face in otherUniqueFaces)
            {
                face.ExchangeVertexUnchecked(otherVertex, mergeOnVertex);
            }

            foreach (Face face in sharedFaces)
            {
                face.RemoveVertexUnchecked(otherVertex);
                if (face.VertexCount <= 2)
                {
                    face.Delete();
                }
            }

            foreach (Face face in otherUniqueFaces)
            {
                face.DiscoverEdgesFromVertices();
            }

            foreach (Face face in sharedFaces)
            {
                face.DiscoverEdgesFromVertices();
            }

            otherVertex.Alive = false;

            return mergeOnVertex;
        }

        /// <summary>
        /// Merge vertices by distance, should run at O(V3) time.
        /// </summary>
        /// <param name="eps">Maximum distance to consider the same position</param>
        public void MergeByDistance(float eps = 0.01f)
        {
            HashSet<Vertex> verts = extrusionHelper.vertices;
            // in theory, iterating like this should be safe because we should never remove
            for (int i = 0; i < Vertices.Count; i++)
            {
                FindAllByPosition(verts, Vertices[i].Position, eps);
                MergeVertices(verts);
            }
        }

        private Edge DeleteDuplicateEdge(Edge edge1, Edge edge2)
        {
            // same edge, done
            if (edge1 == edge2)
            {
                return edge1;
            }
            //
            if (!edge1.Equals(edge2))
            {
                throw new ArgumentException("Edges must be between the same vertices");
            }

            edge1.one.RemoveEdge(edge2);
            edge1.two.RemoveEdge(edge2);

            foreach (Face face in edge1.one.FacesIter)
            {
                face.DiscoverEdgesFromVertices();
            }
            foreach (Face face in edge1.two.FacesIter)
            {
                face.DiscoverEdgesFromVertices();
            }

            edge2.Delete();

            return edge1;
        }

        public Face MergeFaces(Edge edge)
        {
            throw new NotImplementedException { };
        }

        [Obsolete("Use DeleteGeometry family of functions")]
        /// <summary>
        /// Delete a face but leave the surrounding vertices intact.
        /// </summary>
        /// <param name="face">Face to delete</param>
        public void DeleteFace(Face face)
        {
            DeleteFaceSingle(face);
            ExecuteDeletion();
        }

        [Obsolete("Use DeleteGeometry family of functions")]
        /// <summary>
        /// Delete multiple faces but leave surrounding vertices intact
        /// </summary>
        /// <param name="faces">Faces to delete</param>
        public void DeleteOnlyFaces(List<Face> faces)
        {
            foreach (Face face in faces)
            {
                DeleteFaceSingle(face);
            }
            ExecuteDeletion();
        }

        [Obsolete("Use DeleteGeometry family of functions")]
        /// <summary>
        /// Delete multiple faces leaving surrounding vertices intact.
        /// Only faces that are fully selected by surrounding vertices will be deleted.
        /// This operation is O(V*F)
        /// </summary>
        /// <param name="vertices"></param>
        public void DeleteOnlyFaces(List<Vertex> vertices)
        {
            foreach (Face face in Faces)
            {
                int i = 0;
                foreach (Vertex vertex in vertices)
                {
                    if (face.ContainsVertex(vertex))
                        i++;
                }
                if (i == face.VertexCount)
                {
                    face.Delete();
                }
            }
            ExecuteDeletion();
        }

        /// <summary>
        /// Delete surrounding faces, delete surrounding edges, delete the vertex
        /// </summary>
        /// <param name="vertex"></param>
        private void DeleteVertexSingle(Vertex vertex)
        {
            vertex.Delete();
        }

        /// <summary>
        /// Remove the edge from both vertices, delete surrounding faces, then delete the edge
        /// </summary>
        /// <param name="edge"></param>
        private void DeleteEdgeSingle(Edge edge)
        {
            foreach (Face face in edge.GetEdgeLoopsIter())
            {
                DeleteFaceSingle(face);
            }
            edge.one.RemoveEdge(edge);
            edge.two.RemoveEdge(edge);
            edge.Delete();
        }

        /// <summary>
        /// Remove the face from the surrounding vertices, then delete the face
        /// </summary>
        /// <param name="face"></param>
        private void DeleteFaceSingle(Face face)
        {
            foreach (Vertex vert in face.VerticesIter)
            {
                vert.RemoveFaceUnchecked(face);
            }
            face.Delete();
        }

        /// <summary>
        /// To minimize unnecessary iterations, we first plan a deletion, and then
        /// apply it by iterating over the lists
        /// </summary>
        private void ExecuteDeletion()
        {
            Faces.RemoveAll(face => face.Alive == false);
            Vertices.RemoveAll(vertex => vertex.Alive == false);
            Edges.RemoveAll(edge => edge.Alive == false);
        }
    }

}