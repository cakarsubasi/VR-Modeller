#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED

#include "LightingInput.cginc"

#if !defined(ALBEDO_FUNCTION)
    #define ALBEDO_FUNCTION GetAlbedo
#endif

/// Handle direct lighting
UnityLight CreateLight (Interpolators i) {
    UnityLight light;
    float3 lightVec = _WorldSpaceLightPos0.xyz - i.worldPos;
    #if defined(POINT) || defined(SPOT)
        light.dir = normalize(lightVec);
    #else
        light.dir = _WorldSpaceLightPos0.xyz;
    #endif

    UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
    // attenuation *= GetOcclusion(i);
    light.color = _LightColor0.rgb * attenuation;
    light.ndotl = DotClamped(i.normal, light.dir);
    return light;
}

/// Perform box projection for certain reflection probes
float3 BoxProjection(
    float3 direction, float3 position,
    float4 cubemapPosition, float3 boxMin, float3 boxMax
) {
    #if UNITY_SPECCUBE_BOX_PROJECTION
    UNITY_BRANCH
    if (cubemapPosition.w > 0) {
        float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
        float scalar = min(min(factors.x, factors.y), factors.z);
        direction = direction * scalar + (position - cubemapPosition);
    }
    #endif
    return direction;
}

/// Handle indirect lighting
UnityIndirect CreateIndirectLight(Interpolators i, float3 viewDir) {
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;

    #if defined(VERTEXLIGHT_ON)
        indirectLight.diffuse = i.vertexLightColor;
    #endif

    #if defined(FORWARD_BASE_PASS)
        indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
        float3 reflectionDir = reflect(-viewDir, i.normal);
        Unity_GlossyEnvironmentData envData;
        envData.roughness = 1 - GetSmoothness(i);
        // calculate the first reflection probe
        envData.reflUVW = BoxProjection(
            reflectionDir, i.worldPos,
            unity_SpecCube0_ProbePosition,
            unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
        );
        float3 probe0 = Unity_GlossyEnvironment(
            UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
        );
        float interpolator = unity_SpecCube0_BoxMin.w;

        // calculate the second reflection probe
        #if UNITY_SPECCUBE_BLENDING
        UNITY_BRANCH
        if (interpolator < 0.99999) {

            envData.reflUVW = BoxProjection(
                reflectionDir, i.worldPos,
                unity_SpecCube1_ProbePosition,
                unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
            );
            float3 probe1 = Unity_GlossyEnvironment(
                UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0),
                unity_SpecCube1_HDR, envData
            );
            // interpolate between probes
            indirectLight.specular = lerp(probe1, probe0, unity_SpecCube0_BoxMin.w);
        } else {
            indirectLight.specular = probe0;
        }
        #else
        indirectLight.specular = probe0;
        #endif

        // apply occlusion
        float occlusion = GetOcclusion(i);
        indirectLight.diffuse *= occlusion;
        indirectLight.specular *= occlusion;
    #endif
    return indirectLight;
}


/// Compute vertex lights
/// Vertex lights are up to four lights on forward shading
/// that is calculated per vertex in addition to per pixel
/// lighting.
void ComputeVertexLightColor(inout Interpolators i) {
    #if defined(VERTEXLIGHT_ON)
    	i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos, i.normal
		);
    #endif
}

/// Calculate the binormal from the normal and tangent for correct lighting
float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
    return cross(normal, tangent.xyz) * 
        (binormalSign * unity_WorldTransformParams.w);
}

void InitializeFragmentNormal(inout Interpolators i) {
    float3 tangentSpaceNormal = GetTangentSpaceNormal(i);

    #if defined(BINORMAL_PER_FRAGMENT)
        float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
    #else
        float3 binormal = i.binormal;
    #endif

    i.normal = normalize(
        tangentSpaceNormal.x * i.tangent + 
        tangentSpaceNormal.y * binormal + 
        tangentSpaceNormal.z * i.normal
    );
}

Interpolators MyVertexProgram(VertexData v) {
    Interpolators i;
    // local space to world space
    i.pos = UnityObjectToClipPos(v.vertex);
    // world position from transformation matrix
    i.worldPos = mul(unity_ObjectToWorld, v.vertex);
    // object space normals to world space normals
    i.normal = UnityObjectToWorldNormal(v.normal);
    // tangent space to world space
    #if defined(BINORMAL_PER_FRAGMENT)
        i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
    #else
        i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
        i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
    #endif
    // transform UVs
    i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
    i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

    #if defined(MESHES_SELECTION_TEXCOORD1)
        i.selected = v.selected;
    #endif

    // screen space shadow coordinates
    TRANSFER_SHADOW(i);

    // Vertex lights (up to 4)
    #if defined(VERTEXLIGHT_ON)
        i.vertexLightColor = float3(0, 0, 0);
        ComputeVertexLightColor(i);
    #endif
    return i;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
    // Cutout transparency
    float alpha = GetAlpha(i);
    #if defined(_RENDERING_CUTOUT)
        clip(alpha - _AlphaCutoff);
    #endif

    // Initialize normals
    InitializeFragmentNormal(i);
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    float3 specularTint;
    float oneMinusReflectivity;
    // calculate diffuse contribution
    float3 albedo = DiffuseAndSpecularFromMetallic(
        ALBEDO_FUNCTION(i), GetMetallic(i), specularTint, oneMinusReflectivity);
    // premultiply alpha
    #if defined(_RENDERING_TRANSPARENT)
        albedo *= alpha;
        alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
    #endif

    // apply the BRDF
    float4 target = UNITY_BRDF_PBS(
        albedo, specularTint,
        oneMinusReflectivity, GetSmoothness(i),
        i.normal, viewDir,
        CreateLight(i), CreateIndirectLight(i, viewDir)
        );

    // apply emission
    target.rgb += GetEmission(i);
    #if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
        target.a = alpha;
    #endif
    return target;
}

#endif