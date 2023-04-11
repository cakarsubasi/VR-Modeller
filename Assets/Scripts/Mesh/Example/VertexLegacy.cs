using System.Collections;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace Meshes
{
    public struct VertexLegacy
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
        public float2 texCoord0;
    }
}