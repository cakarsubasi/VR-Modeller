using System;

namespace Meshes
{
    partial struct EditableMeshImpl
    {
        /// <summary>
        /// Delete a vertex but leave the connecting faces intact.
        /// <br></br>
        /// Generally speaking, this is not what you want.
        /// </summary>
        /// <param name="index">index of the vertex to delete</param>
        public void DeleteVertex(int index)
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
        /// <param name="index">index of the vertex to delete</param>
        public void DeleteVertexRecursive(int index)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Delete a face but leave the surrounding vertices intact.
        /// </summary>
        /// <param name="index">index of the face to delete</param>
        public void DeleteFace(int index)
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Delete a face while also deleting the surrounding vertices as well as 
        /// faces surrounding those vertices
        /// </summary>
        /// <param name="index">index of the face to delete</param>
        public void DeleteFaceRecursive(int index)
        {
            throw new NotImplementedException { };
        }
    }

}