using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
// get rid of this TODO
using System.Linq;

using static Unity.Mathematics.math;
#nullable enable

namespace Meshes
{
    partial struct UMesh
    {
        [Obsolete]
        public void SelectFromIndices(List<int> indices, List<Vertex> selection)
        {
            selection.Clear();
            ResizeListAsNeeded(selection, indices.Count);
            
            foreach (int index in indices)
            {
                selection.Add(Vertices[index]);
            }
        }


        public void ExtendSelectionWith(ICollection<Vertex> vertices, ICollection<Edge> edges, ICollection<Face> faces, Vertex extension)
        {
            if (!vertices.Contains(extension))
            {
                vertices.Add(extension);
            }

            foreach (Edge edge in extension.edges)
            {
                if (!edges.Contains(edge))
                {
                    edges.Add(edge);
                }
            }

            foreach (Face face in extension.FacesIter)
            {
                if (!faces.Contains(face))
                {
                    faces.Add(face);
                }
            }
        }

        public void SelectLoop(Vertex vertex, List<Vertex> selection, float startingAngle = 0f, float maximumAngle = 90f)
        {
            throw new NotImplementedException { };
        }

        public void SelectShortestPath(Vertex vertex1, Vertex vertex2, List<Vertex> selection)
        {
            throw new NotImplementedException { };
        }

        public void SelectEdgesFromVertices(IEnumerable<Vertex> vertices, ICollection<Edge> selection)
        {
            selection.Clear();
            foreach (Vertex vertex in vertices)
            {
                foreach (Edge edge in vertex.Edges)
                {
                    if (vertices.Contains(edge.Other(vertex)))
                    {
                        if (!selection.Contains(edge))
                        {
                            selection.Add(edge);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find selected faces from vertices, careful with this operation as it is V*F on mesh size
        /// (not input size)
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="selection"></param>
        public void SelectFacesFromVertices(IEnumerable<Vertex> vertices, ICollection<Face> selection)
        {
            selection.Clear();
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
                    selection.Add(face);
                }
            }
        }

        public void SelectMore(List<Vertex> selection)
        {
            // much easier when keeping track of edges
            throw new NotImplementedException { };
        }

        public void SelectLess(List<Vertex> selection)
        {
            // much easier when keeping track of edges
            throw new NotImplementedException { };
        }

        public void SelectSeam(Vertex vertex, List<Vertex> selection, float startingAngle, float maximumAngle)
        {
            // adding this would require also keeping track of edges
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Find a Vertex in the given position and return the first one if it exists
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>The Vertex</returns>
        public Vertex? FindByPosition(float3 position, float eps = 0.01f)
        {
            foreach (Vertex vertex in Vertices)
            {
                if (math.length(vertex.Position - position) < eps)
                {
                    return vertex;
                }
            }
            return null;
        }

        /// <summary>
        /// Find all Vertices in a given position and return them in a list. May be useful while
        /// eliminating duplicates.
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>List of vertices</returns>
        public List<Vertex> FindAllByPosition(float3 position, float eps = 0.01f)
        {
            List<Vertex> output = new();
            FindAllByPosition(output, position, eps);
            return output;
        }

        public void FindAllByPosition(ICollection<Vertex> output, float3 position, float eps = 0.01f)
        {
            output.Clear();
            foreach (Vertex vert in Vertices)
            {
                if (math.length(vert.Position - position) < eps)
                {
                    output.Add(vert);
                }
            }
        }


        private static void ResizeListAsNeeded<T>(List<T> list, int size)
        {
            if (size > 1e19)
            {
                throw new ArgumentException("Requested size is too large");
            } 
            if (size > list.Capacity)
            {
                list.Capacity = 2 * size;
            }
        }
    }
}