using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;

using static Unity.Mathematics.math;

namespace Meshes
{
    public interface IMeshGenerator
    {
        void Execute<S>(int i, S streams) where S : struct, IMeshStreams;

        int VertexCount { get; }
        int IndexCount { get; }
        int JobLength { get; }
        Bounds Bounds { get; }
    }

    public struct EmptyMesh : IMeshGenerator
    {
        public int VertexCount => 0;
        public int IndexCount => 0;
        public int JobLength => 0;
        public Bounds Bounds => new Bounds(Vector3.zero, Vector3.zero);

        public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
        {

        }
    }

    public struct QuadMesh : IMeshGenerator
    {
        public int VertexCount => 4;
        public int IndexCount => 6;
        public int JobLength => 1;

        public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.normal.z = -1f;
            vertex.tangent.xw = float2(1f, -1f);
            streams.SetVertex(0, vertex);

            vertex.position = right();
            vertex.texCoord0 = float2(1f, 0f);
            streams.SetVertex(1, vertex);

            vertex.position = up();
            vertex.texCoord0 = float2(0f, 1f);
            streams.SetVertex(2, vertex);

            vertex.position = float3(1f, 1f, 0f);
            vertex.texCoord0 = 1f;
            streams.SetVertex(3, vertex);

            streams.SetTriangle(0, int3(0, 2, 1));
            streams.SetTriangle(1, int3(1, 2, 3));
        }

    }

    public struct MeshJob<G, S> : IJobFor
        where G : struct, IMeshGenerator
        where S : struct, IMeshStreams
    {
        G generator;

        [WriteOnly]
        S streams;

        public void Execute(int i) => generator.Execute(i, streams);

        public static JobHandle ScheduleParallel(
            Mesh mesh, Mesh.MeshData meshData, JobHandle dependency)
        {
            var job = new MeshJob<G, S>();
            mesh.bounds = job.generator.Bounds;
            job.streams.Setup(
                meshData, job.generator.VertexCount, job.generator.IndexCount);
            return job.ScheduleParallel(job.generator.JobLength, 1, dependency);
        }
    }

    public delegate JobHandle MeshJobScheduleDelegate(
        Mesh mesh, Mesh.MeshData meshData, JobHandle dependency);
}