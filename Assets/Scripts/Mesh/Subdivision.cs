using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meshes
{
    public partial struct UMesh
    {

        /// <summary>
        /// Basic subdivision
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="faces"></param>
        private void Subdivide(HashSet<Edge> edges, HashSet<Face> faces)
        {
            // step one
            foreach(Edge edge in edges)
            {
                EulerSplitEdgeCreateVertex(edge.one, edge, out _);
            }

            // step two
            foreach(Face face in faces)
            {
                if (face.VertexCount == 6)
                {
                    SubdivideTriangle(face);
                } else if (face.VertexCount == 8)
                {
                    SubdivideQuad(face);
                }
            }
        }

        private void SubdivideTriangle(Face face)
        {
            throw new NotImplementedException { };
        }

        private void SubdivideQuad(Face face)
        {
            throw new NotImplementedException { };
        }


    }
}