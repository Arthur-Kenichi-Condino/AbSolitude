#include"UnityPBSLighting.cginc"
#include"UnityCG.cginc"
#include"AutoLight.cginc"
#include"VoxelTerrainSurfaceData.cginc"
float _columns;
float _rows;
 UNITY_DECLARE_TEX2DARRAY(_albedos);
 UNITY_DECLARE_TEX2DARRAY(_bumps);
 UNITY_DECLARE_TEX2DARRAY(_heights);
  float _heightDistortion;
float _scale;
float _sharpness;
float _fadeStartDis;
 float _fadeEndDis;
#if !defined(MY_LIGHTING_INPUT_INCLUDED)
#define MY_LIGHTING_INPUT_INCLUDED

#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "VoxelTerrainSurfaceData.cginc"

#if defined(_NORMAL_MAP) || defined(_DETAIL_NORMAL_MAP) || defined(_PARALLAX_MAP)
	#define REQUIRES_TANGENT_SPACE 1
	#define TESSELLATION_TANGENT 1
#endif
#define TESSELLATION_UV1 1
#define TESSELLATION_UV2 1

#if defined(_PARALLAX_MAP) && defined(VERTEX_DISPLACEMENT_INSTEAD_OF_PARALLAX)
	#undef _PARALLAX_MAP
	#define VERTEX_DISPLACEMENT 1
	#define _DisplacementMap _ParallaxMap
	#define _DisplacementStrength _ParallaxStrength
#endif

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	#if !defined(FOG_DISTANCE)
		#define FOG_DEPTH 1
	#endif
	#define FOG_ON 1
#endif

#if !defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN)
	#if defined(SHADOWS_SHADOWMASK) && !defined(UNITY_NO_SCREENSPACE_SHADOWS)
//		#define ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS 1
	#endif
#endif

#if defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN)
	#if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
		#define SUBTRACTIVE_LIGHTING 1
	#endif
#endif

UNITY_INSTANCING_BUFFER_START(InstanceProperties)
	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(InstanceProperties)

sampler2D _MainTex, _DetailTex, _DetailMask;
float4 _MainTex_ST, _DetailTex_ST;

sampler2D _NormalMap, _DetailNormalMap;
float _BumpScale, _DetailBumpScale;

sampler2D _MetallicMap;
float _Metallic;
float _Smoothness;

sampler2D _ParallaxMap;
float _ParallaxStrength;

sampler2D _OcclusionMap;
float _OcclusionStrength;

sampler2D _EmissionMap;
float3 _Emission;

float _Cutoff;

struct VertexData {
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

struct InterpolatorsVertex {
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 pos : SV_POSITION;

	#if !defined(NO_DEFAULT_UV)
		float4 uv : TEXCOORD0;
	#endif

	float3 normal : TEXCOORD1;

	#if REQUIRES_TANGENT_SPACE
		#if defined(BINORMAL_PER_FRAGMENT)
			float4 tangent : TEXCOORD2;
		#else
			float3 tangent : TEXCOORD2;
			float3 binormal : TEXCOORD3;
		#endif
	#endif

	#if FOG_DEPTH
		float4 worldPos : TEXCOORD4;
	#else
		float3 worldPos : TEXCOORD4;
	#endif

	UNITY_SHADOW_COORDS(5)

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif

	#if defined(LIGHTMAP_ON) || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
		float2 lightmapUV : TEXCOORD6;
	#endif

	#if defined(DYNAMICLIGHTMAP_ON)
		float2 dynamicLightmapUV : TEXCOORD7;
	#endif

	#if defined(_PARALLAX_MAP)
		float3 tangentViewDir : TEXCOORD8;
	#endif
     float4 uv0:TEXCOORD10;
     float4 uv1:TEXCOORD11;
     float4 uv2:TEXCOORD12;
     float4 uv3:TEXCOORD13;
     float4 uv4:TEXCOORD14;
     float4 uv5:TEXCOORD15;
     float4 uv6:TEXCOORD16;
     float4 uv7:TEXCOORD17;
     float3 vNormal:TEXCOORD18;
};

struct Interpolators {
	UNITY_VERTEX_INPUT_INSTANCE_ID
	#if defined(LOD_FADE_CROSSFADE)
		UNITY_VPOS_TYPE vpos : VPOS;
	#else
		float4 pos : SV_POSITION;
	#endif

	#if !defined(NO_DEFAULT_UV)
		float4 uv : TEXCOORD0;
	#endif

	float3 normal : TEXCOORD1;

	#if REQUIRES_TANGENT_SPACE
		#if defined(BINORMAL_PER_FRAGMENT)
			float4 tangent : TEXCOORD2;
		#else
			float3 tangent : TEXCOORD2;
			float3 binormal : TEXCOORD3;
		#endif
	#endif

	#if FOG_DEPTH
		float4 worldPos : TEXCOORD4;
	#else
		float3 worldPos : TEXCOORD4;
	#endif

	UNITY_SHADOW_COORDS(5)

	#if defined(VERTEXLIGHT_ON)
		float3 vertexLightColor : TEXCOORD6;
	#endif

	#if defined(LIGHTMAP_ON) || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
		float2 lightmapUV : TEXCOORD6;
	#endif

	#if defined(DYNAMICLIGHTMAP_ON)
		float2 dynamicLightmapUV : TEXCOORD7;
	#endif

	#if defined(_PARALLAX_MAP)
		float3 tangentViewDir : TEXCOORD8;
	#endif

	#if defined (CUSTOM_GEOMETRY_INTERPOLATORS)
		CUSTOM_GEOMETRY_INTERPOLATORS
	#endif
     float4 uv0:TEXCOORD10;
     float4 uv1:TEXCOORD11;
     float4 uv2:TEXCOORD12;
     float4 uv3:TEXCOORD13;
     float4 uv4:TEXCOORD14;
     float4 uv5:TEXCOORD15;
     float4 uv6:TEXCOORD16;
     float4 uv7:TEXCOORD17;
     float3 vNormal:TEXCOORD18;
};

float4 GetDefaultUV (Interpolators i) {
	#if defined(NO_DEFAULT_UV)
		return float4(0, 0, 0, 0);
	#else
		return i.uv;
	#endif
}

#if !defined(UV_FUNCTION)
	#define UV_FUNCTION GetDefaultUV
#endif

float GetDetailMask (Interpolators i) {
	#if defined (_DETAIL_MASK)
		return tex2D(_DetailMask, UV_FUNCTION(i).xy).a;
	#else
		return 1;
	#endif
}

float3 GetAlbedo (Interpolators i) {
	float3 albedo =
		tex2D(_MainTex, UV_FUNCTION(i).xy).rgb * UNITY_ACCESS_INSTANCED_PROP(InstanceProperties, _Color).rgb;
	#if defined (_DETAIL_ALBEDO_MAP)
		float3 details = tex2D(_DetailTex, UV_FUNCTION(i).zw) * unity_ColorSpaceDouble;
		albedo = lerp(albedo, albedo * details, GetDetailMask(i));
	#endif
	return albedo;
}

float GetAlpha (Interpolators i) {
	float alpha = UNITY_ACCESS_INSTANCED_PROP(InstanceProperties, _Color).a;
	#if !defined(_SMOOTHNESS_ALBEDO)
		alpha *= tex2D(_MainTex, UV_FUNCTION(i).xy).a;
	#endif
	return alpha;
}

float3 GetTangentSpaceNormal (Interpolators i) {
	float3 normal = float3(0, 0, 1);
	#if defined(_NORMAL_MAP)
		normal = UnpackScaleNormal(tex2D(_NormalMap, UV_FUNCTION(i).xy), _BumpScale);
	#endif
	#if defined(_DETAIL_NORMAL_MAP)
		float3 detailNormal =
			UnpackScaleNormal(
				tex2D(_DetailNormalMap, UV_FUNCTION(i).zw), _DetailBumpScale
			);
		detailNormal = lerp(float3(0, 0, 1), detailNormal, GetDetailMask(i));
		normal = BlendNormals(normal, detailNormal);
	#endif
	return normal;
}

float GetMetallic (Interpolators i) {
	#if defined(_METALLIC_MAP)
		return tex2D(_MetallicMap, UV_FUNCTION(i).xy).r;
	#else
		return _Metallic;
	#endif
}

float GetSmoothness (Interpolators i) {
	float smoothness = 1;
	#if defined(_SMOOTHNESS_ALBEDO)
		smoothness = tex2D(_MainTex, UV_FUNCTION(i).xy).a;
	#elif defined(_SMOOTHNESS_METALLIC) && defined(_METALLIC_MAP)
		smoothness = tex2D(_MetallicMap, UV_FUNCTION(i).xy).a;
	#endif
	return smoothness * _Smoothness;
}

float GetOcclusion (Interpolators i) {
	#if defined(_OCCLUSION_MAP)
		return lerp(1, tex2D(_OcclusionMap, UV_FUNCTION(i).xy).g, _OcclusionStrength);
	#else
		return 1;
	#endif
}

float3 GetEmission (Interpolators i) {
	#if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)
		#if defined(_EMISSION_MAP)
			return tex2D(_EmissionMap, UV_FUNCTION(i).xy) * _Emission;
		#else
			return _Emission;
		#endif
	#else
		return 0;
	#endif
}

#endif
//struct Interpolators {
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//	#if defined(LOD_FADE_CROSSFADE)
//		UNITY_VPOS_TYPE vpos : VPOS;
//	#else
//		float4 pos : SV_POSITION;
//	#endif

//	float4 uv : TEXCOORD0;
//	float3 normal : TEXCOORD1;

//	#if defined(BINORMAL_PER_FRAGMENT)
//		float4 tangent : TEXCOORD2;
//	#else
//		float3 tangent : TEXCOORD2;
//		float3 binormal : TEXCOORD3;
//	#endif

//	#if FOG_DEPTH
//		float4 worldPos : TEXCOORD4;
//	#else
//		float3 worldPos : TEXCOORD4;
//	#endif

//	UNITY_SHADOW_COORDS(5)

//	#if defined(VERTEXLIGHT_ON)
//		float3 vertexLightColor : TEXCOORD6;
//	#endif

//	#if defined(LIGHTMAP_ON) || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
//		float2 lightmapUV : TEXCOORD6;
//	#endif

//	#if defined(DYNAMICLIGHTMAP_ON)
//		float2 dynamicLightmapUV : TEXCOORD7;
//	#endif

//	#if defined(_PARALLAX_MAP)
//		float3 tangentViewDir : TEXCOORD8;
//	#endif
//     float4 uv0:TEXCOORD10;
//     float4 uv1:TEXCOORD11;
//     float4 uv2:TEXCOORD12;
//     float4 uv3:TEXCOORD13;
//     float4 uv4:TEXCOORD14;
//     float4 uv5:TEXCOORD15;
//     float4 uv6:TEXCOORD16;
//     float4 uv7:TEXCOORD17;
//     float3 vNormal:TEXCOORD18;
//};
    struct TriplanarUV{
     float2 x,y,z;
    };

TriplanarUV GetTriplanarUV (SurfaceParameters parameters) {
	TriplanarUV triUV;
	float3 p = parameters.position * _scale;
	triUV.x = p.zy;
	triUV.y = p.xz;
	triUV.z = p.xy;
	if (parameters.normal.x < 0) {
		triUV.x.x = -triUV.x.x;
	}
	if (parameters.normal.y < 0) {
		triUV.y.x = -triUV.y.x;
	}
	if (parameters.normal.z >= 0) {
		triUV.z.x = -triUV.z.x;
	}
	triUV.x.y += 0.5;
	triUV.z.x += 0.5;
	return triUV;
}

float3 GetTriplanarWeights (
	SurfaceParameters parameters, float heightX, float heightY, float heightZ
) {
	float3 triW = abs(parameters.normal);
	triW = saturate(triW - .25);
	triW *= lerp(1, float3(heightX, heightY, heightZ), _heightDistortion);
	triW = pow(triW, 3);
	return triW / (triW.x + triW.y + triW.z);
}

float3 BlendTriplanarNormal (float3 mappedNormal, float3 surfaceNormal) {
	float3 n;
	n.xy = mappedNormal.xy + surfaceNormal.xy;
	n.z = mappedNormal.z * surfaceNormal.z;
	return n;
}

void MyTriPlanarSurfaceFunction (
	inout SurfaceData surface, SurfaceParameters parameters
) {
	TriplanarUV triUV = GetTriplanarUV(parameters);
	
	float3 albedoX = 0;//tex2D(_MainTex, triUV.x).rgb;
	float3 albedoY = 0;//tex2D(_MainTex, triUV.y).rgb;
	float3 albedoZ = 0;//tex2D(_MainTex, triUV.z).rgb;

	float4 mohsX = 0;//tex2D(_MOHSMap, triUV.x);
	float4 mohsY = 0;//tex2D(_MOHSMap, triUV.y);
	float4 mohsZ = 0;//tex2D(_MOHSMap, triUV.z);

	float3 tangentNormalX = 0;//UnpackNormal(tex2D(_NormalMap, triUV.x));
	float4 rawNormalY = 0;//tex2D(_NormalMap, triUV.y);
	float3 tangentNormalZ = 1;//UnpackNormal(tex2D(_NormalMap, triUV.z));

	//#if defined(_SEPARATE_TOP_MAPS)
	//	if (parameters.normal.y > 0) {
	//		albedoY = tex2D(_TopMainTex, triUV.y).rgb;
	//		mohsY = tex2D(_TopMOHSMap, triUV.y);
	//		rawNormalY = tex2D(_TopNormalMap, triUV.y);
	//	}
	//#endif
	float3 tangentNormalY = UnpackNormal(rawNormalY);

	if (parameters.normal.x < 0) {
		tangentNormalX.x = -tangentNormalX.x;
	}
	if (parameters.normal.y < 0) {
		tangentNormalY.x = -tangentNormalY.x;
	}
	if (parameters.normal.z >= 0) {
		tangentNormalZ.x = -tangentNormalZ.x;
	}

	float3 worldNormalX =
		BlendTriplanarNormal(tangentNormalX, parameters.normal.zyx).zyx;
	float3 worldNormalY =
		BlendTriplanarNormal(tangentNormalY, parameters.normal.xzy).xzy;
	float3 worldNormalZ =
		BlendTriplanarNormal(tangentNormalZ, parameters.normal);

	float3 triW = GetTriplanarWeights(parameters, mohsX.z, mohsY.z, mohsZ.z);

	surface.albedo = albedoX * triW.x + albedoY * triW.y + albedoZ * triW.z;

	float4 mohs = mohsX * triW.x + mohsY * triW.y + mohsZ * triW.z;
	surface.metallic = mohs.x;
	surface.occlusion = mohs.y;
	surface.smoothness = mohs.a;

	surface.normal = normalize(
		worldNormalX * triW.x + worldNormalY * triW.y + worldNormalZ * triW.z
	);
}

#define SURFACE_FUNCTION MyTriPlanarSurfaceFunction