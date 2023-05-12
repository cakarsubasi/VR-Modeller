#if !defined(LIGHTING_INCLUDED)
#define LIGHTING_INCLUDED
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

float4 _Tint;
float _Metallic;
sampler2D _MainTex;
float4 _MainTex_ST;
float _Smoothness;

struct VertexData {
    float4 position: POSITION;
    float3 normal : NORMAL;
    float2 uv: TEXCOORD0;
};

struct Interpolators {
    float4 position: SV_POSITION;
    float2 uv: TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 worldPos : TEXCOORD2;

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD3;
    #endif
};

UnityLight CreateLight (Interpolators i) {
    UnityLight light;
    float3 lightVec = _WorldSpaceLightPos0.xyz - i.worldPos;
    //float attenuation = 1 / (1 + dot(lightVec, lightVec));
    #if defined(POINT) || defined(SPOT)
        light.dir = normalize(lightVec);
    #else
        light.dir = _WorldSpaceLightPos0.xyz;
    #endif

    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);
    light.color = _LightColor0.rgb * attenuation;
    light.ndotl = DotClamped(i.normal, light.dir);
    return light;
}

UnityIndirect CreateIndirectLight(Interpolators i) {
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;

    #if defined(VERTEXLIGHT_ON)
        indirectLight.diffuse = i.vertexLightColor;
    #endif

    #if defined(FORWARD_BASE_PASS)
        indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
    #endif
    return indirectLight;
}

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

Interpolators MyVertexProgram(VertexData v) {
    Interpolators i;
    // transform UVs
    i.uv = TRANSFORM_TEX(v.uv, _MainTex);
    // local space to world space
    i.position = UnityObjectToClipPos(v.position);
    // world position from transformation matrix
    i.worldPos = mul(unity_ObjectToWorld, v.position);
    // object space normals to world space normals
    i.normal = UnityObjectToWorldNormal(v.normal);
    ComputeVertexLightColor(i);
    return i;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
    // prep
    i.normal = normalize(i.normal);
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
    float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
    float3 specularTint;
    float oneMinusReflectivity;
    // calculate diffuse contribution
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
    // setup direct lights
    UnityLight light = CreateLight(i);
    // setup indirect lights
    UnityIndirect indirectLight = CreateIndirectLight(i);

    // apply the BRDF
    return UNITY_BRDF_PBS(
        albedo, specularTint,
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        light, indirectLight
        );
}

#endif