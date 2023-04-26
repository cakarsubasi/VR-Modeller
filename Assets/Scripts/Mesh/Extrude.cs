using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meshes
{
    public partial struct UMesh
    {
        //public struct ExtrusionResult
        //{
        //    public List<Vertex> vertices;
        //    public List<Face> faces;
        //}

        public void Extrude(Vertex input)
        {
            extrusionHelper.Clear();
            extrusionHelper.vertices.Add(input);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        public void Extrude(Edge input)
        {
            extrusionHelper.Clear();
            extrusionHelper.vertices.Add(input.one);
            extrusionHelper.vertices.Add(input.two);
            extrusionHelper.edges.Add(input);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        public void Extrude(Face input)
        {
            extrusionHelper.Clear();
            extrusionHelper.vertices.UnionWith(input.Vertices);
            extrusionHelper.edges.UnionWith(input.edges);
            extrusionHelper.faces.Add(input);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        public void Extrude(List<Vertex> selection)
        {
            extrusionHelper.Load(ref this, selection);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        public void Extrude(List<Edge> selection)
        {
            extrusionHelper.Load(ref this, selection);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        /// <summary>
        /// Extrude faces. Faces follow slightly different rules as it will only extrude parts
        /// belonging to those faces. In other words, edges in between vertices that are part
        /// of the faces will not be extruded.
        /// </summary>
        /// <param name="selection"></param>
        public void ExtrudeFaces(in List<Face> selection)
        {
            extrusionHelper.Load(selection);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        /// <summary>
        /// Extrude exactly the given vertices, edges, faces.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="edges"></param>
        /// <param name="faces"></param>
        public void Extrude(List<Vertex> vertices, List<Edge> edges, List<Face> faces)
        {
            extrusionHelper.Load(vertices, edges, faces);
            Extrude(extrusionHelper.vertices, extrusionHelper.edges, extrusionHelper.faces);
        }

        /// <summary>
        /// The purpose of this struct is to preallocate internal sets to avoid having to reallocate 
        /// </summary>
        private struct ExtrusionHelper
        {
            public HashSet<Vertex> vertices;
            public HashSet<Edge> edges;
            public HashSet<Face> faces;

            public List<Face> facesTemp;
            public List<Edge> edgesTemp;
            public HashSet<Face> faceSetTemp;

            internal void Setup()
            {
                vertices = new(8);
                edges = new(16);
                faces = new(4);
                facesTemp = new(3);
                edgesTemp = new(4);
                faceSetTemp = new(4);
            }

            internal void Load(ref UMesh mesh, List<Vertex> vertexList)
            {
                Clear();
                vertices.UnionWith(vertexList);
                foreach (Vertex vertex in vertices)
                {
                    foreach (Edge edge in vertex.edges)
                    {
                        if (vertices.Contains(edge.Other(vertex)))
                        {
                            edges.Add(edge);
                        }
                    }
                }
                mesh.SelectFacesFromVertices(vertices, faces);
            }

            internal void Load(ref UMesh mesh, List<Edge> edgeList)
            {
                Clear();
                edges.UnionWith(edgeList);
                foreach (Edge edge in edgeList)
                {
                    vertices.Add(edge.one);
                    vertices.Add(edge.two);
                }
                mesh.SelectFacesFromVertices(vertices, faces);
            }

            internal void Load(List<Face> faceList)
            {
                Clear();
                faces.UnionWith(faceList);
                foreach (Face face in faces)
                {
                    edges.UnionWith(face.edges);
                    vertices.UnionWith(face.Vertices);
                }
            }

            internal void Load(List<Vertex> vertexList, List<Edge> edgeList, List<Face> faceList)
            {
                Clear();
                vertices.UnionWith(vertexList);
                edges.UnionWith(edgeList);
                faces.UnionWith(faceList);
            }

            internal void Clear()
            {
                vertices.Clear();
                edges.Clear();
                faces.Clear();
            }
        }

        public void Extrude(HashSet<Vertex> selectedVertices, HashSet<Edge> selectedEdges, HashSet<Face> selectedFaces)
        {

            List<Face> edgeLoop = extrusionHelper.facesTemp;
            List<Edge> edgeList = extrusionHelper.edgesTemp;
            HashSet<Face> facesToFixUp = extrusionHelper.faceSetTemp;

            foreach (Vertex vertex in selectedVertices)
            {
                if (vertex.IsManifold() && selectedFaces.IsSupersetOf(vertex.FacesIter))
                {
                    continue;
                }

                Vertex created = CreateVertexConnectedTo(vertex, out Edge connection);

                edgeList.AddRange(vertex.edges);
                foreach (Edge edge in edgeList)
                {
                    // skip if selected edge
                    if (!selectedEdges.Contains(edge) && edge != connection)
                    {
                        edge.ExchangeVertex(vertex, created);
                        created.AddEdgeUnchecked(edge);
                        vertex.RemoveEdge(edge);
                    }
                }
                edgeList.Clear();

                edgeLoop.AddRange(vertex.FacesIter);
                foreach (Face face in edgeLoop)
                {
                    if (!selectedFaces.Contains(face))
                    {
                        face.ExchangeVertexUnchecked(vertex, created);
                        facesToFixUp.Add(face);
                    }
                }
                edgeLoop.Clear();
            }

            foreach (Edge edge in selectedEdges)
            {

                edge.GetEdgeLoops(edgeLoop);

                // connect the new vertices
                if (edgeLoop.Count >= 2 && selectedFaces.IsSupersetOf(edgeLoop)) {
                    continue;
                } else
                {
                    // get the new vertices
                    Vertex vert1 = edge.one;
                    Vertex vert2 = edge.two;

                    Vertex vert3 = vert1.edges[^1].Other(vert1);
                    Vertex vert4 = vert2.edges[^1].Other(vert2);

                    Edge newEdge = CreateEdge(vert3, vert4);
                    Face newFace = CreateQuad(new QuadVerts(vert2, vert1, vert3, vert4));
                }

                // flip normal of new face as needed TODO
                // internal edge
                if (!selectedFaces.IsSupersetOf(edgeLoop))
                {

                }


            }

            // fix old face if it exists
            foreach (Face face in facesToFixUp)
            {
                face.DiscoverEdgesFromVertices();
            }

        }

        

    }
}