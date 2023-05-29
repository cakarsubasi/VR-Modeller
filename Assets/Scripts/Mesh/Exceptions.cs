using System.Collections;
using UnityEngine;
using System;

namespace Meshes
{
    [Serializable]
    public class UMeshException : Exception
    {
        public UMeshException() : base() { }
        public UMeshException(string message) : base(message) { }
        public UMeshException(string message, Exception inner) : base(message, inner) { }

        public override string ToString()
        {
            return $"{base.ToString()} UMesh error";
        }
    }

    [Serializable]
    public sealed class VertexNotInFaceException : UMeshException
    {
        readonly Vertex vertex;
        readonly Face face;
        public VertexNotInFaceException() : base() { }
        public VertexNotInFaceException(string message) : base(message) { }
        public VertexNotInFaceException(string message, Vertex vertex, Face face) : base(message) 
        {
            this.vertex = vertex;
            this.face = face;
        }
        public VertexNotInFaceException(string message, Exception inner) : base(message, inner) { }

        public override string ToString()
        {
            return $"{base.ToString()} {vertex} not found in {face}";
        }
    }

    [Serializable]
    public sealed class VertexNotInEdgeException : UMeshException
    {
        readonly Vertex vertex;
        readonly Edge edge;
        public VertexNotInEdgeException() : base() { }
        public VertexNotInEdgeException(string message) : base(message) { }
        public VertexNotInEdgeException(string message, Vertex vertex, Edge edge) : base(message)
        {
            this.vertex = vertex;
            this.edge = edge;
        }
        public VertexNotInEdgeException(string message, Exception inner) : base(message, inner) { }

        public override string ToString()
        {
            return $"{base.ToString()} {vertex} not found in {edge}";
        }
    }
}