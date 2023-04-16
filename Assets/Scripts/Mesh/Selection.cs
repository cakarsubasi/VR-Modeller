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
    partial struct EditableMeshImpl
    {

        public void SelectLoop(Vertex vertex, float startingAngle, float maximumAngle)
        {
            throw new NotImplementedException { };
        }

        public void SelectFacesFromVertices(in List<Vertex> vertices, out List<Face> faces)
        {
            throw new NotImplementedException { };
        }

        public void SelectMore(Vertex vertex)
        {
            // much easier when keeping track of edges
            throw new NotImplementedException { };
        }

        public void SelectLess(ArrayList vertex)
        {
            // much easier when keeping track of edges
            throw new NotImplementedException { };
        }

        public void SelectSeam(Vertex vertex, float startingAngle, float maximumAngle)
        {
            // adding this would require also keeping track of edges
            throw new NotImplementedException { };
        }

        private IndexedVertex FindByPosition(float3 position)
        {
            var idx = Vertices.FindIndex(vertex => vertex.Position.Equals(position));
            Vertex? vertex = null;
            if (idx != -1)
                vertex = Vertices[idx];
            return new IndexedVertex
            {
                index = idx,
                vertex = vertex
            };
        }

        private List<Vertex> FindAllByPosition(float3 position)
        {
            return Vertices.FindAll(vert => vert.Position.Equals(position)).ToList();
        }
    }
}