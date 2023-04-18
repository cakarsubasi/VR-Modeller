using System;
using System.Collections.Generic;

namespace Meshes
{
    partial struct EditableMeshImpl
    {
        /// <summary>
        /// Delete a vertex but leave the connecting faces intact.
        /// <br></br>
        /// Generally speaking, this is not what you want.
        /// </summary>
        /// <param name="vertex">Vertex to dissolve</param>
        public void DissolveVertex(Vertex vertex)
        {
            // delete the vertex

            // shift the vertex buffer left

            // update the index buffer values

            // copy all of the updated values to the 

            throw new NotImplementedException { };
        }

        /// <summary>
        /// Delete a vertex and also delete its connecting faces.
        /// </summary>
        /// <param name="vertex">Vertex to delete</param>
        public void DeleteVertex(Vertex vertex)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Delete a face but leave the surrounding vertices intact.
        /// </summary>
        /// <param name="face">Face to dissolve</param>
        public void DissolveFace(Face face)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Delete a face while also deleting the surrounding vertices as well as 
        /// faces surrounding those vertices
        /// </summary>
        /// <param name="face">Face to delete</param>
        public void DeleteFace(Face face)
        {
            throw new NotImplementedException { };
        }

        public void DeleteVertices(List<Vertex> vertices)
        {
            throw new NotImplementedException { };
        }

        public void DissolveVertices(List<Vertex> vertices)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// To minimize unnecessary iterations, we first plan a deletion, and then
        /// apply it by iterating over the lists
        /// </summary>
        private void ExecuteDeletion()
        {
            //Faces.RemoveAll(face => face.Alive == false);
            //Vertices.RemoveAll(vertex => vertex.Alive == false);

            throw new NotImplementedException { };
        }
    }

}