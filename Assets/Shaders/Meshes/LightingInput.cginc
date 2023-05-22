#if !defined(LIGHTING_INPUT_INCLUDED)
#define LIGHTING_INPUT_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

float4 _Tint;
sampler2D _MetallicMap;
float _Metallic;
float _Smoothness;

sampler2D _MainTex, _DetailTex, _DetailMask;
float4 _MainTex_ST, _DetailTex_ST;

sampler2D _OcclusionMap;
float _OcclusionStrength;

sampler2D _EmissionMap;
float3 _Emission;

sampler2D _NormalMap, _DetailNormalMap;
float _BumpScale, _DetailBumpScale;

float _AlphaCutoff;

struct VertexData {
    float4 vertex: POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv: TEXCOORD0;
    #if defined(MESHES_SELECTION_TEXCOORD1)
        float3 selected: TEXCOORD1;
    #endif
};

struct InterpolatorsVertex {
    float4 pos: SV_POSITION;
    float4 uv: TEXCOORD0;
    float3 normal : TEXCOORD1;

    #if defined(BINORMAL_PER_FRAGMENT)
        float4 tangent : TEXCOORD2;
    #else
        float3 tangent : TEXCOORD2;
        float3 binormal : TEXCOORD3;
    #endif

    float3 worldPos : TEXCOORD4;

    //#if defined(SHADOWS_SCREEN)
    //    float4 shadowCoordinates : TEXCOORD5;
    //#endif
    SHADOW_COORDS(5)

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD6;
    #endif

    #if defined(MESHES_SELECTION_TEXCOORD1)
        float3 selected: TEXCOORD7;
    #endif
};

struct Interpolators {
    float4 pos: SV_POSITION;
    float4 uv: TEXCOORD0;
    float3 normal : TEXCOORD1;

    #if defined(BINORMAL_PER_FRAGMENT)
        float4 tangent : TEXCOORD2;
    #else
        float3 tangent : TEXCOORD2;
        float3 binormal : TEXCOORD3;
    #endif

    float3 worldPos : TEXCOORD4;

    //#if defined(SHADOWS_SCREEN)
    //    float4 shadowCoordinates : TEXCOORD5;
    //#endif
    SHADOW_COORDS(5)

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD6;
    #endif

    #if defined(MESHES_SELECTION_TEXCOORD1)
        float3 selected: TEXCOORD7;
    #endif

    #if defined(CUSTOM_GEOMETRY_INTERPOLATORS)
        CUSTOM_GEOMETRY_INTERPOLATORS
    #endif
};

float GetMetallic(Interpolators i) {
    #if defined(_METALLIC_MAP)
        return tex2D(_MetallicMap, i.uv.xy).r;
    #else
        return _Metallic;
    #endif
}

float GetSmoothness(Interpolators i) {
    float smoothness = 1;
    #if defined(_SMOOTHNESS_ALBEDO)
        smoothness = tex2D(_MainTex, i.uv.xy).a;
    #elif defined(_SMOOTHNESS_METALLIC) && defined(_METALLIC_MAP)
        smoothness = tex2D(_MetallicMap, i.uv.xy).a;
    #endif
    return smoothness * _Smoothness;
}

float3 GetOcclusion(Interpolators i) {
    #if defined(_OCCLUSION_MAP)
        return lerp(1, tex2D(_OcclusionMap, i.uv.xy).g, _OcclusionStrength);
    #else
        return 1;
    #endif
}

float3 GetEmission(Interpolators i) {
    #if defined(FORWARD_BASE_PASS)
        #if defined(_EMISSION_MAP)
            return tex2D(_EmissionMap, i.uv.xy) * _Emission;
        #else
            return _Emission;
        #endif
    #else
        return 0;
    #endif
}

float3 GetDetailMask(Interpolators i) {
    #if defined (_DETAIL_MASK)
        return tex2D(_DetailMask, i.uv.xy).a;
    #else
        return 1;
    #endif
}

float3 GetAlbedo(Interpolators i) {
    float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Tint.rgb;
    #if defined(_DETAIL_ALBEDO_MAP)
        float3 details = tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;
        albedo = lerp(albedo, albedo * details, GetDetailMask(i));
    #endif
    return albedo;
}

float GetAlpha(Interpolators i) {
    float alpha = _Tint.a;
    #if !defined(_SMOOTHNESS_ALBEDO)
        alpha *= tex2D(_MainTex, i.uv.xy).a;
    #endif
    return alpha;
}

float3 GetTangentSpaceNormal(Interpolators i) {
    float3 normal = float3(0, 0, 1);
    #if defined(_NORMAL_MAP)
        normal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
    #endif
    #if defined(_DETAIL_NORMAL_MAP)
        float3 detailNormal = UnpackScaleNormal(tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale);
        detailNormal = lerp(float3(0, 0, 1), detailNormal, GetDetailMask(i));
        normal = BlendNormals(normal, detailNormal);
    #endif
    return normal;
}

#endif // LIGHTING_INPUT_INCLUDED