using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace Meshes
{
    public partial struct UMesh
    {

        private Mesh mesh;

        public Mesh Mesh { get => mesh; set => SetupMesh(value); }
        public string Name;
        public ShadingType Shading;

        public List<Vertex> Vertices;
        public List<Edge> Edges;
        public List<Face> Faces;

        private int internalVertexCount;
        private int internalVertexCountMax;
        private int internalIndexCount;
        private int internalTriangleCount => internalIndexCount / 3;
        private int internalIndexCountMax;

        private static readonly int absoluteMaxVerts = 2 << 19;
        private static int AbsoluteMaxIndices => absoluteMaxTriangles * 3;
        private static readonly int absoluteMaxTriangles = 2 << 19;

        private static readonly int initialMaxVerts = 320;
        private static readonly int initialMaxTriangles = 320;

        public int VertexCount { get => Vertices.Count; }
        public int FaceCount { get => Faces.Count; }
        public int EdgeCount { get => Edges.Count; }

        private ExtrusionHelper extrusionHelper;

        public Vector3[] VertexLocations
        {
            get => Vertices.Select(vertex => (Vector3)vertex.Position).ToArray();
        }

        public Vector3[] FaceLocations
        {
            get => Faces.Select(face => (Vector3)face.Position).ToArray();
        }

        /// <summary>
        /// Create a new UMesh and render into the given mesh filter
        /// </summary>
        /// <param name="mesh">mesh filter to render to</param>
        /// <returns>The UMesh</returns>
        public static UMesh Create(Mesh mesh, String name = "Mesh")
        {
            UMesh uMesh = default;
            uMesh.Name = name;
            uMesh.Shading = ShadingType.Flat;
            uMesh.Setup(mesh);
            return uMesh;
        }

        /// <summary>
        /// Create a new UMesh, and render into a newly created mesh filter.
        /// <br>You can get the mesh filter with the Mesh property</br>
        /// </summary>
        /// <returns>The UMesh</returns>
        public static UMesh Create()
        {
            return UMesh.Create(new Mesh());
        }

        /// <summary>
        /// Create a new EditableMesh from scratch
        /// </summary>
        /// <param name="mesh">Container mesh to write to</param>
        public void Setup(Mesh mesh)
        {
            InitializeMainStructures();
            InitializeHelperStructures();
            SetupMesh(mesh);
        }

        /// <summary>
        /// Create a new EditableMesh from scratch but do not assign a mesh to write to
        /// </summary>
        public void Setup()
        {
            InitializeMainStructures();
            InitializeHelperStructures();
        }

        /// <summary>
        /// Set the correct vertex and intex buffer parameters for the given mesh
        /// </summary>
        /// <param name="mesh"></param>
        public void SetupMesh(Mesh mesh)
        {
            this.mesh = mesh;
            this.mesh.MarkDynamic();

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            Setup(meshData, initialMaxVerts, initialMaxTriangles);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            
        }

        private void Setup(Mesh.MeshData meshData, int vertexCount, int triangleCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
    4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // positions
            vertexAttributes[0] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Position);
            // normals
            vertexAttributes[1] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Normal);
            // tangents
            vertexAttributes[2] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Tangent,
                dimension: 4);
            // uv coordinates
            vertexAttributes[3] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.TexCoord0,
                dimension: 2);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();

            meshData.SetIndexBufferParams(triangleCount * 3, IndexFormat.UInt32);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleCount * 3),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            internalVertexCountMax = vertexCount;
            internalIndexCountMax = triangleCount * 3;

            var stream0 = meshData.GetVertexData<Stream0>();
            var triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);

            for (int i = 0; i < stream0.Length; ++i)
            {
                stream0[i] = Stream0.degenerate;
            }
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i] = Triangle.degenerate;
            }
        }

        private void InitializeMainStructures()
        {
            // initialize safe garbage here
            Vertices = new List<Vertex>(initialMaxVerts / 2);
            Faces = new List<Face>(initialMaxVerts / 2);
            Edges = new List<Edge>(initialMaxVerts);
        }

        private void InitializeHelperStructures()
        {
            extrusionHelper.Setup();
        }

        private void ResizeInternalBuffers(int desiredVertexCount, int desiredTriangleCount)
        {
            if (desiredVertexCount > absoluteMaxVerts || desiredTriangleCount > absoluteMaxTriangles)
            {
                // if the required vertices are too large, give up
                throw new InvalidOperationException(
                    "Number of vertices or triangle indices has exceeded the maximum allowed" +
                    "by the API, this usually indicates that the previous operation has required" +
                    "too many new vertices to be created and the operation was prevented");
            }
            // set up mesh again

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            Setup(meshData, desiredVertexCount, desiredTriangleCount);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        /// <summary>
        /// Resize internal buffers to fit the data exactly
        /// </summary>
        public void OptimizeRendering()
        {
            OptimizeIndices();
            ResizeInternalBuffers(internalVertexCount, internalTriangleCount);
        }

        /// <summary>
        /// Create an editable mesh by copying an existing mesh
        /// The original instance is not touched and a new instance is returned
        /// </summary>
        /// <param name="mesh">Mesh to copy from</param>
        /// <returns>A reference to the new mesh instance</returns>
        public Mesh CopySetup(Mesh mesh)
        {
            this.mesh = new Mesh
            {
                name = mesh.name,
            };
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            Setup(this.mesh);

            Vertex[] vertexmap = new Vertex[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertexmap[i] = CreateVertexOrReturnReferenceIfExists(vertices[i], normals[i], tangents[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                TriangleElement<Vertex> verts = new(vertexmap[triangles[i]], vertexmap[triangles[i + 1]], vertexmap[triangles[i + 2]]);
                TriangleElement<float2> uv0s = new(uvs[triangles[i]], uvs[triangles[i + 1]], uvs[triangles[i + 2]]);
                CreateTriangle(verts, uv0s);
            }
            RecalculateNormals();
            WriteAllToMesh();
            RecalculateBounds();
            InitializeHelperStructures();
            return this.mesh;
        }

        /// <summary>
        /// Combine the other UMesh to this UMesh, consume the other UMesh
        /// </summary>
        /// <param name="other">other UMesh to combine</param>
        public void Combine(UMesh other)
        {
            Vertices.AddRange(other.Vertices);
            Faces.AddRange(other.Faces);
            Edges.AddRange(other.Edges);
            other.Dispose();
        }

        /// <summary>
        /// Make a copy of the selected vertices, edges and faces and return them
        /// Note that an assumption is made that all of the vertices comprising the
        /// edges and faces are given to this method, otherwise the function will likely fail
        /// </summary>
        /// <param name="selectedVertices"></param>
        /// <param name="selectedEdges"></param>
        /// <param name="selectedFaces"></param>
        /// <param name="createdVertices"></param>
        /// <param name="createdEdges"></param>
        /// <param name="createdFaces"></param>
        public void CopySelected(
            in IEnumerable<Vertex> selectedVertices, 
            in IEnumerable<Edge> selectedEdges, 
            in IEnumerable<Face> selectedFaces,
            out List<Vertex> createdVertices,
            out List<Edge> createdEdges,
            out List<Face> createdFaces)
        {
            createdVertices = new();
            createdEdges = new();
            createdFaces = new();
            
            int index = 0;
            foreach (Vertex vert in selectedVertices)
            {
                vert.Index = index++;
                createdVertices.Add(CreateVertex(vert));
            }

            foreach (Edge edge in selectedEdges)
            {
                createdEdges.Add(CreateEdge(createdVertices[edge.one.Index], createdVertices[edge.two.Index]));
            }

            List<Vertex> tempVerts = new(4);
            foreach (Face face in selectedFaces)
            {
               foreach (Vertex vert in face.VerticesIter)
               {
                    tempVerts.Add(createdVertices[vert.Index]);
               }
                createdFaces.Add(CreateNGon(tempVerts));
                tempVerts.Clear();
            }
        }

        /// <summary>
        /// Create a new UMesh with the given selection by copying. The selection is assumed to be complete.
        /// </summary>
        /// <param name="selectedVertices">selected vertices</param>
        /// <param name="selectedEdges">selected edges</param>
        /// <param name="selectedFaces">selected faces</param>
        /// <returns>New UMesh from the selection</returns>
        public UMesh CopySelectionToNewMesh(
            in IEnumerable<Vertex> selectedVertices,
            in IEnumerable<Edge> selectedEdges,
            in IEnumerable<Face> selectedFaces)
        {
            UMesh separation = UMesh.Create();
            separation.Name = new String(Name);

            int index = 0;
            foreach (Vertex vert in selectedVertices)
            {
                vert.Index = index++;
                separation.CreateVertex(vert);
            }

            foreach (Edge edge in selectedEdges)
            {
                separation.CreateEdge(separation.Vertices[edge.one.Index], separation.Vertices[edge.two.Index]);
            }

            List<Vertex> tempVerts = new(4);
            foreach (Face face in selectedFaces)
            {
                                tempVerts.Clear();
                foreach (Vertex vert in face.VerticesIter)
                {
                    tempVerts.Add(separation.Vertices[vert.Index]);
                }
                separation.CreateNGon(tempVerts);

            }

            return separation;
        }


        /// <summary>
        /// Separate the given vertices, edges, and faces to their own UMesh.
        /// <br>The rule is that manifold geometry is moved while non-manifold
        /// geometry is copied and will exist in both UMeshes</br>
        /// </summary>
        /// <param name="selectedVertices"></param>
        /// <param name="selectedEdges"></param>
        /// <param name="selectedFaces"></param>
        /// <param name="createdVertices"></param>
        /// <param name="createdEdges"></param>
        /// <param name="createdFaces"></param>
        /// <returns></returns>
        public UMesh SeparateSelected(
            in IEnumerable<Vertex> selectedVertices,
            in IEnumerable<Edge> selectedEdges,
            in IEnumerable<Face> selectedFaces,
            out List<Vertex> createdVertices,
            out List<Edge> createdEdges,
            out List<Face> createdFaces)
        {
            UMesh separation = UMesh.Create();
            separation.Name = new String(Name);

            throw new NotImplementedException { };
        }

        /// <summary>
        /// Create a complete copy of this mesh.
        /// </summary>
        /// <returns>A new UMesh with the same geometry</returns>
        public UMesh DeepCopy()
        {
            UMesh copy = UMesh.Create();
            copy.Name = new String(Name);

            int index = 0;
            foreach (Vertex vert in Vertices)
            {
                vert.Index = index++;
                copy.CreateVertex(vert);
            }

            foreach (Edge edge in Edges)
            {
                copy.CreateEdge(copy.Vertices[edge.one.Index], copy.Vertices[edge.two.Index]);
            }

            List<Vertex> tempVerts = new(4);
            foreach (Face face in Faces)
            {
                tempVerts.Clear();
                foreach (Vertex vert in face.VerticesIter)
                {
                    tempVerts.Add(copy.Vertices[vert.Index]);
                }
                copy.CreateNGon(tempVerts);
            }

            return copy;
        }

        /// <summary>
        /// Optimize the in memory representation of the mesh for rendering.
        /// <br></br>
        /// Call after particularly complex operations
        /// </summary>
        public void OptimizeIndices()
        {
            int i = 0;
            foreach (Vertex vertex in Vertices)
            {
                i = vertex.OptimizeIndices(i);
            }
            internalVertexCount = i;

            i = 0;
            foreach (Face face in Faces)
            {
                i += face.TriangleCount;
            }
            internalIndexCount = i * 3;
        }

        /// <summary>
        /// Call this method after modifying the mesh to update indices and display all of the changes
        /// </summary>
        public void WriteAllToMesh()
        {
            int previousVertexCount = internalVertexCount;
            int previousTriangleCount = internalTriangleCount;
            // figure out the indices
            OptimizeIndices();
            // resize the vertex and index buffers if needed
            if (internalVertexCount > internalVertexCountMax || internalIndexCount > internalIndexCountMax)
            {
                ResizeInternalBuffers(internalVertexCount * 2, internalTriangleCount * 2);
            }
            // write vertices 
            NativeArray<Stream0> vertexStream = new NativeArray<Stream0>(internalVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (Vertex vertex in Vertices)
            {
                vertex.WriteToStream(ref vertexStream);
            }
            mesh.SetVertexBufferData<Stream0>(vertexStream, 0, 0, internalVertexCount,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            vertexStream.Dispose();
            NativeArray<int3> indexStream = new NativeArray<int3>(internalIndexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int i = 0;
            foreach(Face face in Faces)
            {
                i = face.WriteToStream(ref indexStream, i);
            }
            mesh.SetIndexBufferData<int3>(indexStream, 0, 0, internalTriangleCount,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

            // If any deletions have occurred, zero out the remainder
            DeletePadding(internalVertexCount, previousVertexCount, internalTriangleCount, previousTriangleCount);
        }

        public void UnsafeWriteVertexToMesh(Vertex vertex)
        {
            UnsafeWriteVerticesToMesh(new List<Vertex>() { vertex });
        }

        /// <summary>
        /// Write the given vertices to the mesh without validating indices. This is useful for applying
        /// just movement for instance. If called after adding or deleting meshes or faces, it may result
        /// in broken geometry.
        /// </summary>
        /// <param name="verts">vertices to write</param>
        public void UnsafeWriteVerticesToMesh(List<Vertex> verts)
        {
            NativeArray<Stream0> buffer = new(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (var vert in verts)
            {
                Stream0[] stream = vert.ToStream();
                int[] indices = vert.Indices();

                for (int i = 0; i < indices.Length; ++i)
                {
                    buffer[0] = stream[i];
                    Mesh.SetVertexBufferData<Stream0>(buffer, 0, indices[i], 1);
                }
            }
            buffer.Dispose();
        }

        private void DeletePadding(int vertexStart, int vertexEnd, int triangleStart, int triangleEnd)
        {
            Stream0[] degenerateVert = { Stream0.degenerate };
            for (int i = vertexStart; i < vertexEnd; i++)
            {
                mesh.SetVertexBufferData<Stream0>(degenerateVert, 0, i, 1,
                    flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            }
            int3[] degenerateTriangle = { Triangle.degenerate };
            for (int i = triangleStart; i < triangleEnd; i++)
            {
                mesh.SetIndexBufferData<int3>(degenerateTriangle, 0, i, 1, 
                    flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            }
        }

        /// <summary>
        /// Recalculate the bounds for the renderer.
        /// </summary>
        public void RecalculateBounds()
        {
            Mesh.RecalculateBounds();
        }

        public void Dispose()
        {
            Vertices.Clear();
            Edges.Clear();
            Faces.Clear();
        }

        public override string ToString()
        {
            var str = $"{base.ToString()}: vertices: {VertexCount}, faces: {FaceCount}";

            foreach(Vertex vert in Vertices)
            {
                str = $"{str}\n{vert}";
            }
            str = $"{str}\nFaces:";
            foreach (Face face in Faces)
            {
                str = $"{str}\n{face}";
            }
            return str;
        }

    }

}