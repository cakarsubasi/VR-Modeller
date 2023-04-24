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
            HashSet<Vertex> selectedVertices = new() { input };
            HashSet<Edge> selectedEdges = new();
            HashSet<Face> selectedFaces = new();
            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void Extrude(Edge input)
        {
            HashSet<Vertex> selectedVertices = new() { input.one, input.two };
            HashSet<Edge> selectedEdges = new() { input };
            HashSet<Face> selectedFaces = new();
            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void Extrude(Face face)
        {
            HashSet<Vertex> selectedVertices = new(face.Vertices);
            HashSet<Edge> selectedEdges = new(face.edges);
            HashSet<Face> selectedFaces = new() { face };
            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void Extrude(List<Vertex> selection)
        {
            HashSet<Vertex> selectedVertices = new(selection);
            HashSet<Edge> selectedEdges = new();
            HashSet<Face> selectedFaces = new();
            foreach (Vertex vertex in selectedVertices)
            {
                foreach (Edge edge in vertex.edges)
                {
                    if (selectedVertices.Contains(edge.Other(vertex)))
                    {
                        selectedEdges.Add(edge);
                    }
                }
            }
            SelectFacesFromVertices(selectedVertices, selectedFaces);
            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void Extrude(List<Edge> selection)
        {
            HashSet<Vertex> selectedVertices = new();
            HashSet<Edge> selectedEdges = new(selection);
            HashSet<Face> selectedFaces = new();
            foreach (Edge edge in selectedEdges)
            {
                selectedVertices.Add(edge.one);
                selectedVertices.Add(edge.two);
            }
            SelectFacesFromVertices(selectedVertices, selectedFaces);
            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void ExtrudeFaces(in List<Face> selection)
        {
            throw new NotImplementedException { };
        }

        public void Extrude(List<Vertex> vertices, List<Edge> edges, List<Face> faces)
        {
            HashSet<Vertex> selectedVertices = new(vertices);
            HashSet<Edge> selectedEdges = new(edges);
            HashSet<Face> selectedFaces = new(faces);

            Extrude(selectedVertices, selectedEdges, selectedFaces);
        }

        public void Extrude(HashSet<Vertex> selectedVertices, HashSet<Edge> selectedEdges, HashSet<Face> selectedFaces)
        {

            List<Face> edgeLoop = new List<Face>(2);
            List<Edge> edgeList = new List<Edge>(4);


            HashSet<Face> facesToFixUp = new HashSet<Face>();
            HashSet<Edge> newEdges = new HashSet<Edge>();

            foreach (Vertex vertex in selectedVertices)
            {
                if (vertex.IsManifold() && selectedFaces.IsSupersetOf(vertex.Faces))
                {
                    continue;
                }

                Vertex created = CreateVertexConnectedTo(vertex, out Edge connection);
                newEdges.Add(connection);

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

                edgeLoop.AddRange(vertex.Faces);
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
                // get the new vertices
                Vertex vert1 = edge.one;
                Vertex vert2 = edge.two;

                Vertex vert3 = vert1.edges[^1].Other(vert1);
                Vertex vert4 = vert2.edges[^1].Other(vert2);

                // connect the new vertices
                if (edgeLoop.Count >= 2 && selectedFaces.IsSupersetOf(edgeLoop)) {
                    continue;
                } else
                {
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