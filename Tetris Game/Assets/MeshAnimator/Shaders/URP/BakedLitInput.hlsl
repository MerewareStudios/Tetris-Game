#ifndef UNIVERSAL_BAKEDLIT_INPUT_INCLUDED
#define UNIVERSAL_BAKEDLIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
// BEGIN GENERATED MESH ANIMATOR CODE
TEXTURE2D_ARRAY(_AnimTextures);
SAMPLER(sampler_AnimTextures);
UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float, _AnimTextureIndex)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimTimeInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _AnimScalar)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeAnimTextureIndex)
	UNITY_DEFINE_INSTANCED_PROP(float4, _CrossfadeAnimInfo)
	UNITY_DEFINE_INSTANCED_PROP(float4, _CrossfadeAnimScalar)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeStartTime)
	UNITY_DEFINE_INSTANCED_PROP(float, _CrossfadeEndTime)
UNITY_INSTANCING_BUFFER_END(Props)
// END GENERATED MESH ANIMATOR CODE

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    half _Glossiness;
    half _Metallic;
    half _Surface;
CBUFFER_END

#ifdef UNITY_DOTS_INSTANCING_ENABLED

UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DOTS_INSTANCED_PROP(float , _Cutoff)
    UNITY_DOTS_INSTANCED_PROP(float , _Glossiness)
    UNITY_DOTS_INSTANCED_PROP(float , _Metallic)
    UNITY_DOTS_INSTANCED_PROP(float , _Surface)
UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)

static float4 unity_DOTS_Sampled_BaseColor;
static float  unity_DOTS_Sampled_Cutoff;
static float  unity_DOTS_Sampled_Glossiness;
static float  unity_DOTS_Sampled_Metallic;
static float  unity_DOTS_Sampled_Surface;

void SetupDOTSBakedLitMaterialPropertyCaches()
{
    unity_DOTS_Sampled_BaseColor  = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float4, _BaseColor);
    unity_DOTS_Sampled_Cutoff     = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Cutoff);
    unity_DOTS_Sampled_Glossiness = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Glossiness);
    unity_DOTS_Sampled_Metallic   = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Metallic);
    unity_DOTS_Sampled_Surface    = UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(float , _Surface);
}

#undef UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES
#define UNITY_SETUP_DOTS_MATERIAL_PROPERTY_CACHES() SetupDOTSBakedLitMaterialPropertyCaches()

#define _BaseColor          unity_DOTS_Sampled_BaseColor
#define _Cutoff             unity_DOTS_Sampled_Cutoff
#define _Glossiness         unity_DOTS_Sampled_Glossiness
#define _Metallic           unity_DOTS_Sampled_Metallic
#define _Surface            unity_DOTS_Sampled_Surface

#endif

#endif