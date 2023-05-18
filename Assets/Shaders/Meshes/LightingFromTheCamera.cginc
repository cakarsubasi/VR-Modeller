#if !defined(LIGHTING_FROM_THE_CAMERA)
#define LIGHTING_FROM_THE_CAMERA
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
    float3 cameraVec : TEXCOORD3;
};

UnityLight CreateLight (Interpolators i) {
    UnityLight light;
    //light.dir = _W
    return light;
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

    i.cameraVec = mul((float3x3)unity_CameraToWorld, float3(0,0,1));
    return i;
}

float4 MyFragmentProgram(Interpolators i) : SV_TARGET {
    // prep
    i.normal = normalize(i.normal);
    //float3 lightDir = _WorldSpaceLightPos0.xyz;
    float3 lightDir = - i.cameraVec;
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
    float3 lightColor = _LightColor0.rgb;
    float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
    float3 specularTint;
    float oneMinusReflectivity;
    // calculate diffuse contribution
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
    float3 diffuse = albedo *  lightColor * DotClamped(lightDir, i.normal);
    // calculate specular contribution
    float3 reflectionDir = reflect(-lightDir, i.normal);
    float3 halfVector = normalize(lightDir + viewDir);
    float3 specular = specularTint * 
        lightColor * pow(DotClamped(halfVector, i.normal), (_Smoothness) * 100);
    // setup direct lights
    UnityLight light;
    light.color = lightColor;
    light.dir = lightDir;
    light.ndotl = DotClamped(i.normal, lightDir);
    // setup indirect lights
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    //float3 test = normalize(cross(i.normal, i.cameraVec));
    //return float4(test.r, test.r, test.r ,1);
    // apply the BRDF
    return UNITY_BRDF_PBS(
        albedo, specularTint,
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        light, indirectLight
        );
}

#endif