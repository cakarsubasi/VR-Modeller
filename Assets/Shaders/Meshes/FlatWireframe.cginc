#if !defined(FLAT_WIREFRAME_INCLUDED)
#define FLAT_WIREFRAME_INCLUDED

#define CUSTOM_GEOMETRY_INTERPOLATORS \
    float2 barycentricCoordinates : TEXCOORD9;

#include "LightingInput.cginc"

float3 GetAlbedoWithWireframe (Interpolators i) {
    
    float3 albedo = GetAlbedo(i);
    float3 barys;
    barys.xy = i.barycentricCoordinates;
    barys.z = 1 - barys.x - barys.y;
    float minBary = min(barys.x, min(barys.y, barys.z));
    float delta = abs(ddx(minBary) + abs(ddy(minBary)));
    minBary = smoothstep(0, delta, minBary);
    return albedo * minBary;
}

float3 GetAlbedoWithSelection(Interpolators i) {
    float3 albedo = GetAlbedo(i);
    #if defined(MESHES_SELECTION_TEXCOORD1)
        //float multiplier = (i.selected.x + i.selected.y + i.selected.z) / 3.0; // > 2.4 ? 1.0 : 0.0;
        float both = i.selected.y;
        float edges = (i.selected.x + i.selected.z) / 2;
        float multiplier = 1 - (both + edges);
        //albedo = albedo * smoothstep(0, 0.9, albedo * multiplier);
        albedo = lerp(0, albedo, multiplier);
    #endif

    return albedo;
}

#define ALBEDO_FUNCTION GetAlbedo

#include "Lighting.cginc"

struct InterpolatorsGeometry {
    InterpolatorsVertex data;
    CUSTOM_GEOMETRY_INTERPOLATORS
};

[maxvertexcount(3)]
void GeometryProgram (triangle InterpolatorsVertex i[3],
                      inout TriangleStream<InterpolatorsGeometry> stream
) {
    float3 p0 = i[0].worldPos.xyz;
    float3 p1 = i[1].worldPos.xyz;
    float3 p2 = i[2].worldPos.xyz;

    float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));

    #if defined(FACE_NORMALS)
    i[0].normal = triangleNormal;
    i[1].normal = triangleNormal;
    i[2].normal = triangleNormal;
    #endif

    InterpolatorsGeometry g0, g1, g2;
    g0.data = i[0];
    g1.data = i[1];
    g2.data = i[2];

    g0.barycentricCoordinates = float2(1,0);
    g1.barycentricCoordinates = float2(0,1);
    g2.barycentricCoordinates = float2(0,0);

    stream.Append(g0);
    stream.Append(g1);
    stream.Append(g2);
}

#endif // FLAT_WIREFRAME_INCLUDED