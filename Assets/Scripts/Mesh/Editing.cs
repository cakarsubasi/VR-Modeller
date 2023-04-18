using System.Collections;
using Unity.Collections;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Meshes
{
    partial struct EditableMeshImpl
    {
        /// <summary>
        /// Move a vertex of the given index to the specified position
        /// </summary>
        /// <param name="index">index of the vertex to move</param>
        /// <param name="position">position to move to</param>
        public void MoveVertex(int index, float3 position)
        {
            Vertices[index].Position = position;
            Stream0[] stream = Vertices[index].ToStream();
            int[] indices = Vertices[index].Indices();
            // should probably get rid of this
            NativeArray<Stream0> buffer = new(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < indices.Length; ++i)
            {
                buffer[0] = stream[i];
                mesh.SetVertexBufferData<Stream0>(buffer, 0, indices[i], 1);
            }
            buffer.Dispose();
        }

        public enum TransformType
        {
            BoundingBoxCenter,
            IndividualCenter,
            // ...
        }

        public void TransformVertices(List<int> vertices, Matrix4x4 transform, TransformType type)
        {
            throw new NotImplementedException { };
        }

        public void SetFace()
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Recalculate normals
        /// </summary>
        public void RecalculateNormals()
        {
            foreach (Face face in Faces)
            {
                face.RecalculateNormal();
            }
            // vertices derive their normals from the faces
            // so their normals have to be calculated after
            foreach (Vertex vertex in Vertices)
            {
                vertex.RecalculateNormal();
            }
        }

        /// <summary>
        /// Flip normals
        /// </summary>
        public void FlipNormals()
        {
            foreach (Face face in Faces)
            {
                face.FlipFace(true);
            }
            foreach (Vertex vertex in Vertices)
            {
                vertex.Normal = -vertex.Normal;
            }
        }


        public void RecalculateTangents()
        {
            throw new NotImplementedException { };
        }

        public void Triangulate()
        {
            throw new NotImplementedException { };
        }

        public void TrianglesToQuads()
        {
            throw new NotImplementedException { };
        }

    }
}