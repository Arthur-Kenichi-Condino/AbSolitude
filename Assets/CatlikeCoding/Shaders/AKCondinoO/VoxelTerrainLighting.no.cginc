




#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include"VoxelTerrainUtil.cginc"
//#include "My Lighting Input.cginc"


#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	#if !defined(FOG_DISTANCE)
		#define FOG_DEPTH 1
	#endif
	#define FOG_ON 1
#endif

#if !defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN)
	#if defined(SHADOWS_SHADOWMASK) && !defined(UNITY_NO_SCREENSPACE_SHADOWS)
		#define ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS 1
	#endif
#endif

#if defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN)
	#if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
		#define SUBTRACTIVE_LIGHTING 1
	#endif
#endif

//UNITY_INSTANCING_BUFFER_START(InstanceProperties)
//	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
//#define _Color_arr InstanceProperties
//UNITY_INSTANCING_BUFFER_END(InstanceProperties)

//sampler2D _MainTex, _DetailTex, _DetailMask;
//float4 _MainTex_ST, _DetailTex_ST;

//sampler2D _NormalMap, _DetailNormalMap;
//float _BumpScale, _DetailBumpScale;

//sampler2D _MetallicMap;
//float _Metallic;
//float _Smoothness;

//sampler2D _ParallaxMap;
//float _ParallaxStrength;

//sampler2D _OcclusionMap;
//float _OcclusionStrength;

//sampler2D _EmissionMap;
//float3 _Emission;

//float _Cutoff;

    //struct VertexData{
    // UNITY_VERTEX_INPUT_INSTANCE_ID
    // float4 position:POSITION;
    // float3 normal:NORMAL;
    // fixed4 color:COLOR;
    // float4 texCoord0:TEXCOORD0;
    // float4 texCoord1:TEXCOORD1;
    // float4 texCoord2:TEXCOORD2;
    // float4 texCoord3:TEXCOORD3;
    // float4 texCoord4:TEXCOORD4;
    // float4 texCoord5:TEXCOORD5;
    // float4 texCoord6:TEXCOORD6;
    // float4 texCoord7:TEXCOORD7;
    // float4 tangent:TANGENT;
    //};
    struct Interpolatorerror{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     float4 pos:SV_POSITION;
     float4 uv:TEXCOORD0;
     float3 normal:TEXCOORD1;
     #if defined(BINORMAL_PER_FRAGMENT)
      float4 tangent:TEXCOORD2;
     #else
      float3 tangent:TEXCOORD2;
      float3 binormal:TEXCOORD3;
     #endif
     #if FOG_DEPTH
      float4 worldPos:TEXCOORD4;
     #else
      float3 worldPos:TEXCOORD4;
     #endif
     UNITY_SHADOW_COORDS(5)
     #if defined(VERTEXLIGHT_ON)
      float3 vertexLightColor:TEXCOORD6;
     #endif
     #if defined(LIGHTMAP_ON)||ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
      float2 lightmapUV:TEXCOORD6;
     #endif
     #if defined(DYNAMICLIGHTMAP_ON)
      float2 dynamicLightmapUV:TEXCOORD7;
     #endif
     #if defined(_PARALLAX_MAP)
      float3 tangentViewDir:TEXCOORD8;
     #endif
    };


//float GetDetailMask (Interpolators i) {
//	#if defined (_DETAIL_MASK)
//		return tex2D(_DetailMask, i.uv.xy).a;
//	#else
//		return 1;
//	#endif
//}
    struct SamplerValues{
     float3 viewDir;
     half2 sample_x;
     half2 sample_y;
     half2 sample_z;
     half3 blend;
    };
    struct Texture2DArraySamplerData{
     float index;
     float strenght;
    };
    Texture2DArraySamplerData GetTexture2DArraySamplerData(float x,float y,float strenght){
     Texture2DArraySamplerData o;
     o.index=x+_columns*y;
     o.strenght=strenght;
     return o;
    }
    struct HeightSample{
     float2 heightDistortionTexOffset;
    };
    HeightSample SampleHeight(Texture2DArraySamplerData texture2DArraySamplerData,SamplerValues samplerValues){
     HeightSample o;
     fixed4 heightAxis_x=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(samplerValues.sample_x),texture2DArraySamplerData.index));
     fixed4 heightAxis_y=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(samplerValues.sample_y),texture2DArraySamplerData.index));
     fixed4 heightAxis_z=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(samplerValues.sample_z),texture2DArraySamplerData.index));
     fixed4 h=(heightAxis_x)*samplerValues.blend.x
             +(heightAxis_y)*samplerValues.blend.y
             +(heightAxis_z)*samplerValues.blend.z;
     o.heightDistortionTexOffset=ParallaxOffset(h.rgb,_heightDistortion,samplerValues.viewDir);
     return o;
    }
    struct ColorSample{
     fixed4 colorAxis_x;
     fixed4 colorAxis_y;
     fixed4 colorAxis_z;
    };
    void SampleColor(Texture2DArraySamplerData texture2DArraySamplerData,HeightSample heightSample,SamplerValues samplerValues,inout ColorSample colorSample){
     colorSample.colorAxis_x+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(samplerValues.sample_x)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
     colorSample.colorAxis_y+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(samplerValues.sample_y)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
     colorSample.colorAxis_z+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(samplerValues.sample_z)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
    }
    struct BumpSample{
     fixed4 bumpAxis_x;
     fixed4 bumpAxis_y;
     fixed4 bumpAxis_z;
    };
    void SampleBump(Texture2DArraySamplerData texture2DArraySamplerData,HeightSample heightSample,SamplerValues samplerValues,inout BumpSample bumpSample){
     bumpSample.bumpAxis_x+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(samplerValues.sample_x)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
     bumpSample.bumpAxis_y+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(samplerValues.sample_y)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
     bumpSample.bumpAxis_z+=texture2DArraySamplerData.strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(samplerValues.sample_z)+heightSample.heightDistortionTexOffset,texture2DArraySamplerData.index));
    }
    float3 GetAlbedo(Interpolators i,SamplerValues samplerValues){
     ColorSample colorSample;
     colorSample.colorAxis_x=.5;
     colorSample.colorAxis_y=.5;
     colorSample.colorAxis_z=.5;
     //Texture2DArraySamplerData _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv0.x,i.uv0.y,i.uv6.x);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv0.z,i.uv0.w,i.uv6.y);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv1.x,i.uv1.y,i.uv6.z);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv1.z,i.uv1.w,i.uv6.w);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv2.x,i.uv2.y,i.uv7.x);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv2.z,i.uv2.w,i.uv7.y);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv3.x,i.uv3.y,i.uv7.z);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv3.z,i.uv3.w,i.uv7.w);SampleColor(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,colorSample);
     fixed4 color=colorSample.colorAxis_x*samplerValues.blend.x
                 +colorSample.colorAxis_y*samplerValues.blend.y
                 +colorSample.colorAxis_z*samplerValues.blend.z;
	float3 albedo =
		color.rgb/*tex2D(_MainTex, i.uv.xy).rgb*/ * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).rgb;
	#if defined (_DETAIL_ALBEDO_MAP)
		float3 details = tex2D(_DetailTex, i.uv.zw) * unity_ColorSpaceDouble;
		albedo = lerp(albedo, albedo * details, GetDetailMask(i));
	#endif
	return albedo;
}

//float GetAlpha (Interpolators i) {
//	float alpha = UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).a;
//	#if !defined(_SMOOTHNESS_ALBEDO)
//		alpha *= tex2D(_MainTex, i.uv.xy).a;
//	#endif
//	return alpha;
//}

//    float3 GetTangentSpaceNormal(Interpolators i){
//	float3 normal = float3(0, 0, 1);
 
//     //BumpSample bumpSample;
//     //bumpSample.bumpAxis_x=0;
//     //bumpSample.bumpAxis_y=0;
//     //bumpSample.bumpAxis_z=0;
//     //Texture2DArraySamplerData _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv0.x,i.uv0.y,i.uv6.x);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv0.z,i.uv0.w,i.uv6.y);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv1.x,i.uv1.y,i.uv6.z);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv1.z,i.uv1.w,i.uv6.w);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv2.x,i.uv2.y,i.uv7.x);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv2.z,i.uv2.w,i.uv7.y);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv3.x,i.uv3.y,i.uv7.z);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //                          _2DArraySamplerData=GetTexture2DArraySamplerData(i.uv3.z,i.uv3.w,i.uv7.w);SampleBump(_2DArraySamplerData,SampleHeight(_2DArraySamplerData,samplerValues),samplerValues,bumpSample);
//     //fixed4 bump=(bumpSample.bumpAxis_x)*samplerValues.blend.x
//     //           +(bumpSample.bumpAxis_y)*samplerValues.blend.y
//     //           +(bumpSample.bumpAxis_z)*samplerValues.blend.z;
//     //normal=UnpackNormal(bump);
 
//	#if defined(_NORMAL_MAP)
//		normal = UnpackScaleNormal(tex2D(_NormalMap, i.uv.xy), _BumpScale);
//	#endif
//	#if defined(_DETAIL_NORMAL_MAP)
//		float3 detailNormal =
//			UnpackScaleNormal(
//				tex2D(_DetailNormalMap, i.uv.zw), _DetailBumpScale
//			);
//		detailNormal = lerp(float3(0, 0, 1), detailNormal, GetDetailMask(i));
//		normal = BlendNormals(normal, detailNormal);
//	#endif
//	return normal;
//}

//float GetMetallic (Interpolators i) {
//	#if defined(_METALLIC_MAP)
//		return tex2D(_MetallicMap, i.uv.xy).r;
//	#else
//		return _Metallic;
//	#endif
//}

//float GetSmoothness (Interpolators i) {
//	float smoothness = 1;
//	#if defined(_SMOOTHNESS_ALBEDO)
//		smoothness = tex2D(_MainTex, i.uv.xy).a;
//	#elif defined(_SMOOTHNESS_METALLIC) && defined(_METALLIC_MAP)
//		smoothness = tex2D(_MetallicMap, i.uv.xy).a;
//	#endif
//	return smoothness * _Smoothness;
//}

//float GetOcclusion (Interpolators i) {
//	#if defined(_OCCLUSION_MAP)
//		return lerp(1, tex2D(_OcclusionMap, i.uv.xy).g, _OcclusionStrength);
//	#else
//		return 1;
//	#endif
//}

//float3 GetEmission (Interpolators i) {
//	#if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)
//		#if defined(_EMISSION_MAP)
//			return tex2D(_EmissionMap, i.uv.xy) * _Emission;
//		#else
//			return _Emission;
//		#endif
//	#else
//		return 0;
//	#endif
//}

void ComputeVertexLightColor (inout Interpolators i) {
	#if defined(VERTEXLIGHT_ON)
		i.vertexLightColor = Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb,
			unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, i.worldPos.xyz, i.normal
		);
	#endif
}

float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}

InterpolatorsVertex MyVertexProgram (VertexData v) {
	InterpolatorsVertex i;
 float4 vPos=float4(v.vertex.xyz,1);
	UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, i);
	i.pos = UnityObjectToClipPos(vPos);
	i.worldPos.xyz = mul(unity_ObjectToWorld, vPos);
	#if FOG_DEPTH
		i.worldPos.w = i.pos.z;
	#endif
	i.normal = UnityObjectToWorldNormal(v.normal);

	#if defined(BINORMAL_PER_FRAGMENT)
		i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
	#else
		i.tangent = UnityObjectToWorldDir(v.tangent.xyz);
		i.binormal = CreateBinormal(i.normal, i.tangent, v.tangent.w);
	#endif

	//i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
	//i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

	#if defined(LIGHTMAP_ON) || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
		i.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif

	#if defined(DYNAMICLIGHTMAP_ON)
		i.dynamicLightmapUV =
			v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

	UNITY_TRANSFER_SHADOW(i, v.uv1);

	ComputeVertexLightColor(i);

	#if defined (_PARALLAX_MAP)
		#if defined(PARALLAX_SUPPORT_SCALED_DYNAMIC_BATCHING)
			v.tangent.xyz = normalize(v.tangent.xyz);
			v.normal = normalize(v.normal);
		#endif
		float3x3 objectToTangent = float3x3(
			v.tangent.xyz,
			cross(v.normal, v.tangent.xyz) * v.tangent.w,
			v.normal
		);
		i.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
	#endif
                 //i.uv0=v.texCoord0;
                 //i.uv1=v.texCoord1;
                 //i.uv2=v.texCoord2;
                 //i.uv3=v.texCoord3;
                 //i.uv4=v.texCoord4;
                 //i.uv5=v.texCoord5;
                 //i.uv6=v.texCoord6;
                 //i.uv7=v.texCoord7;
                 i.vNormal=v.normal;

	return i;
}

float FadeShadows (Interpolators i, float attenuation) {
	#if HANDLE_SHADOWS_BLENDING_IN_GI || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
		// UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
		#if ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
			attenuation = SHADOW_ATTENUATION(i);
		#endif
		float viewZ =
			dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
		float shadowFadeDistance =
			UnityComputeShadowFadeDistance(i.worldPos, viewZ);
		float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
		float bakedAttenuation =
			UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos);
		attenuation = UnityMixRealtimeAndBakedShadows(
			attenuation, bakedAttenuation, shadowFade
		);
	#endif

	return attenuation;
}

UnityLight CreateLight (Interpolators i) {
	UnityLight light;

	#if defined(DEFERRED_PASS) || SUBTRACTIVE_LIGHTING
		light.dir = float3(0, 1, 0);
		light.color = 0;
	#else
		#if defined(POINT) || defined(POINT_COOKIE) || defined(SPOT)
			light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
		#else
			light.dir = _WorldSpaceLightPos0.xyz;
		#endif

		UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
		attenuation = FadeShadows(i, attenuation);

		light.color = _LightColor0.rgb * attenuation;
	#endif

	return light;
}

float3 BoxProjection (
	float3 direction, float3 position,
	float4 cubemapPosition, float3 boxMin, float3 boxMax
) {
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubemapPosition.w > 0) {
			float3 factors =
				((direction > 0 ? boxMax : boxMin) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemapPosition);
		}
	#endif
	return direction;
}

void ApplySubtractiveLighting (
	Interpolators i, inout UnityIndirect indirectLight
) {
	#if SUBTRACTIVE_LIGHTING
		UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz);
		attenuation = FadeShadows(i, attenuation);

		float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
		float3 shadowedLightEstimate =
			ndotl * (1 - attenuation) * _LightColor0.rgb;
		float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
		subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
		subtractedLight =
			lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
		indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
	#endif
}

UnityIndirect CreateIndirectLight (Interpolators i, float3 viewDir) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

	#if defined(VERTEXLIGHT_ON)
		indirectLight.diffuse = i.vertexLightColor;
	#endif

	#if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)
		#if defined(LIGHTMAP_ON)
			indirectLight.diffuse =
				DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
			
			#if defined(DIRLIGHTMAP_COMBINED)
				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
					unity_LightmapInd, unity_Lightmap, i.lightmapUV
				);
				indirectLight.diffuse = DecodeDirectionalLightmap(
					indirectLight.diffuse, lightmapDirection, i.normal
				);
			#endif

			ApplySubtractiveLighting(i, indirectLight);
		#endif

		#if defined(DYNAMICLIGHTMAP_ON)
			float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
				UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, i.dynamicLightmapUV)
			);

			#if defined(DIRLIGHTMAP_COMBINED)
				float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
					unity_DynamicDirectionality, unity_DynamicLightmap,
					i.dynamicLightmapUV
				);
            	indirectLight.diffuse += DecodeDirectionalLightmap(
            		dynamicLightDiffuse, dynamicLightmapDirection, i.normal
            	);
			#else
				indirectLight.diffuse += dynamicLightDiffuse;
			#endif
		#endif

		#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
			#if UNITY_LIGHT_PROBE_PROXY_VOLUME
				if (unity_ProbeVolumeParams.x == 1) {
					indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
						float4(i.normal, 1), i.worldPos
					);
					indirectLight.diffuse = max(0, indirectLight.diffuse);
					#if defined(UNITY_COLORSPACE_GAMMA)
			            indirectLight.diffuse =
			            	LinearToGammaSpace(indirectLight.diffuse);
			        #endif
				}
				else {
					indirectLight.diffuse +=
						max(0, ShadeSH9(float4(i.normal, 1)));
				}
			#else
				indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
			#endif
		#endif

		float3 reflectionDir = reflect(-viewDir, i.normal);
		Unity_GlossyEnvironmentData envData;
		envData.roughness = 1 - GetSmoothness(i);
		envData.reflUVW = BoxProjection(
			reflectionDir, i.worldPos.xyz,
			unity_SpecCube0_ProbePosition,
			unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
		);
		float3 probe0 = Unity_GlossyEnvironment(
			UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
		);
		envData.reflUVW = BoxProjection(
			reflectionDir, i.worldPos.xyz,
			unity_SpecCube1_ProbePosition,
			unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
		);
		#if UNITY_SPECCUBE_BLENDING
			float interpolator = unity_SpecCube0_BoxMin.w;
			UNITY_BRANCH
			if (interpolator < 0.99999) {
				float3 probe1 = Unity_GlossyEnvironment(
					UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
					unity_SpecCube0_HDR, envData
				);
				indirectLight.specular = lerp(probe1, probe0, interpolator);
			}
			else {
				indirectLight.specular = probe0;
			}
		#else
			indirectLight.specular = probe0;
		#endif

		float occlusion = GetOcclusion(i);
		indirectLight.diffuse *= occlusion;
		indirectLight.specular *= occlusion;

		#if defined(DEFERRED_PASS) && UNITY_ENABLE_REFLECTION_BUFFERS
			indirectLight.specular = 0;
		#endif
	#endif

	return indirectLight;
}

    void InitializeFragmentNormal(inout Interpolators i){
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

float4 ApplyFog (float4 color, Interpolators i) {
	#if FOG_ON
		float viewDistance = length(_WorldSpaceCameraPos - i.worldPos.xyz);
		#if FOG_DEPTH
			viewDistance = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.worldPos.w);
		#endif
		UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
		float3 fogColor = 0;
		#if defined(FORWARD_BASE_PASS)
			fogColor = unity_FogColor.rgb;
		#endif
		color.rgb = lerp(fogColor, color.rgb, saturate(unityFogFactor));
	#endif
	return color;
}

float GetParallaxHeight (float2 uv) {
	return tex2D(_ParallaxMap, uv).g;
}

float2 ParallaxOffset (float2 uv, float2 viewDir) {
	float height = GetParallaxHeight(uv);
	height -= 0.5;
	height *= _ParallaxStrength;
	return viewDir * height;
}

float2 ParallaxRaymarching (float2 uv, float2 viewDir) {
	#if !defined(PARALLAX_RAYMARCHING_STEPS)
		#define PARALLAX_RAYMARCHING_STEPS 10
	#endif
	float2 uvOffset = 0;
	float stepSize = 1.0 / PARALLAX_RAYMARCHING_STEPS;
	float2 uvDelta = viewDir * (stepSize * _ParallaxStrength);

	float stepHeight = 1;
	float surfaceHeight = GetParallaxHeight(uv);

	float2 prevUVOffset = uvOffset;
	float prevStepHeight = stepHeight;
	float prevSurfaceHeight = surfaceHeight;

	for (
		int i = 1;
		i < PARALLAX_RAYMARCHING_STEPS && stepHeight > surfaceHeight;
		i++
	) {
		prevUVOffset = uvOffset;
		prevStepHeight = stepHeight;
		prevSurfaceHeight = surfaceHeight;
		
		uvOffset -= uvDelta;
		stepHeight -= stepSize;
		surfaceHeight = GetParallaxHeight(uv + uvOffset);
	}

	#if !defined(PARALLAX_RAYMARCHING_SEARCH_STEPS)
		#define PARALLAX_RAYMARCHING_SEARCH_STEPS 0
	#endif
	#if PARALLAX_RAYMARCHING_SEARCH_STEPS > 0
		for (int i = 0; i < PARALLAX_RAYMARCHING_SEARCH_STEPS; i++) {
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = GetParallaxHeight(uv + uvOffset);
		}
	#elif defined(PARALLAX_RAYMARCHING_INTERPOLATE)
		float prevDifference = prevStepHeight - prevSurfaceHeight;
		float difference = surfaceHeight - stepHeight;
		float t = prevDifference / (prevDifference + difference);
		uvOffset = prevUVOffset - uvDelta * t;
	#endif

	return uvOffset;
}

void ApplyParallax (inout Interpolators i) {
	#if defined(_PARALLAX_MAP)
		i.tangentViewDir = normalize(i.tangentViewDir);
		#if !defined(PARALLAX_OFFSET_LIMITING)
			#if !defined(PARALLAX_BIAS)
				#define PARALLAX_BIAS 0.42
			#endif
			i.tangentViewDir.xy /= (i.tangentViewDir.z + PARALLAX_BIAS);
		#endif

		#if !defined(PARALLAX_FUNCTION)
			#define PARALLAX_FUNCTION ParallaxOffset
		#endif
		float2 uvOffset = PARALLAX_FUNCTION(i.uv.xy, i.tangentViewDir.xy);
		i.uv.xy += uvOffset;
		i.uv.zw += uvOffset * (_DetailTex_ST.xy / _MainTex_ST.xy);
	#endif
}

struct FragmentOutput {
	#if defined(DEFERRED_PASS)
		float4 gBuffer0 : SV_Target0;
		float4 gBuffer1 : SV_Target1;
		float4 gBuffer2 : SV_Target2;
		float4 gBuffer3 : SV_Target3;

		#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
			float4 gBuffer4 : SV_Target4;
		#endif
	#else
		float4 color : SV_Target;
	#endif
};
FragmentOutput MyFragmentProgram (Interpolators i) {
	UNITY_SETUP_INSTANCE_ID(i);
	#if defined(LOD_FADE_CROSSFADE)
		UnityApplyDitherCrossFade(i.vpos);
	#endif

	ApplyParallax(i);

	InitializeFragmentNormal(i);

	SurfaceData surface;
	#if defined(SURFACE_FUNCTION)
		surface.normal = i.normal;
		surface.albedo = 1;
		surface.alpha = 1;
		surface.emission = 0;
		surface.metallic = 0;
		surface.occlusion = 1;
		surface.smoothness = 0.5;

		SurfaceParameters sp;
		sp.normal = i.normal;
		sp.position = i.worldPos.xyz;
		sp.uv = UV_FUNCTION(i);

		SURFACE_FUNCTION(surface, sp);
	#else
		surface.normal = i.normal;
		surface.albedo = ALBEDO_FUNCTION(i);
		surface.alpha = GetAlpha(i);
		surface.emission = GetEmission(i);
		surface.metallic = GetMetallic(i);
		surface.occlusion = GetOcclusion(i);
		surface.smoothness = GetSmoothness(i);
	#endif
	i.normal = surface.normal;

	float alpha = surface.alpha;
	#if defined(_RENDERING_CUTOUT)
		clip(alpha - _Cutoff);
	#endif

	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);

	float3 specularTint;
	float oneMinusReflectivity;
	float3 albedo = DiffuseAndSpecularFromMetallic(
		surface.albedo, surface.metallic, specularTint, oneMinusReflectivity
	);
	#if defined(_RENDERING_TRANSPARENT)
		albedo *= alpha;
		alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
	#endif

	float4 color = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, surface.smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i, viewDir, surface)
	);
	color.rgb += surface.emission;
	#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
		color.a = alpha;
	#endif

	FragmentOutput output;
	#if defined(DEFERRED_PASS)
		#if !defined(UNITY_HDR_ON)
			color.rgb = exp2(-color.rgb);
		#endif
		output.gBuffer0.rgb = albedo;
		output.gBuffer0.a = surface.occlusion;
		output.gBuffer1.rgb = specularTint;
		output.gBuffer1.a = surface.smoothness;
		output.gBuffer2 = float4(i.normal * 0.5 + 0.5, 1);
		output.gBuffer3 = color;

		#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
			float2 shadowUV = 0;
			#if defined(LIGHTMAP_ON)
				shadowUV = i.lightmapUV;
			#endif
			output.gBuffer4 =
				UnityGetRawBakedOcclusions(shadowUV, i.worldPos.xyz);
		#endif
	#else
		output.color = ApplyFog(color, i);
	#endif
	return output;
}
//    FragmentOutput FragmentToColorProgram(Interpolators i){
//	UNITY_SETUP_INSTANCE_ID(i);
//	#if defined(LOD_FADE_CROSSFADE)
//		UnityApplyDitherCrossFade(i.vpos);
//	#endif
//     float3 viewDir=normalize(_WorldSpaceCameraPos-i.worldPos.xyz);
//     SamplerValues samplerValues;
//     samplerValues.viewDir=viewDir;
//     samplerValues.sample_x=i.worldPos.yz*_scale;
//     samplerValues.sample_y=i.worldPos.xz*_scale;
//     samplerValues.sample_z=i.worldPos.xy*_scale;
//     samplerValues.blend=pow(abs(i.vNormal),_sharpness);
//     samplerValues.blend=samplerValues.blend/(samplerValues.blend.x+samplerValues.blend.y+samplerValues.blend.z);
//     InitializeFragmentNormal(i,samplerValues);
//     //ApplyParallax(i);

//	float alpha = GetAlpha(i);
//	#if defined(_RENDERING_CUTOUT)
//		clip(alpha - _Cutoff);
//	#endif


//	float3 specularTint;
//	float oneMinusReflectivity;
//	float3 albedo = DiffuseAndSpecularFromMetallic(
//		GetAlbedo(i,samplerValues), GetMetallic(i), specularTint, oneMinusReflectivity
//	);
//	#if defined(_RENDERING_TRANSPARENT)
//		albedo *= alpha;
//		alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
//	#endif

//	float4 color = UNITY_BRDF_PBS(
//		albedo, specularTint,
//		oneMinusReflectivity, GetSmoothness(i),
//		i.normal, viewDir,
//		CreateLight(i), CreateIndirectLight(i, viewDir)
//	);
//	color.rgb += GetEmission(i);
//	#if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
//		color.a = alpha;
//	#endif

//	FragmentOutput output;
//	#if defined(DEFERRED_PASS)
//		#if !defined(UNITY_HDR_ON)
//			color.rgb = exp2(-color.rgb);
//		#endif
//		output.gBuffer0.rgb = albedo;
//		output.gBuffer0.a = GetOcclusion(i);
//		output.gBuffer1.rgb = specularTint;
//		output.gBuffer1.a = GetSmoothness(i);
//		output.gBuffer2 = float4(i.normal * 0.5 + 0.5, 1);
//		output.gBuffer3 = color;

//		#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
//			float2 shadowUV = 0;
//			#if defined(LIGHTMAP_ON)
//				shadowUV = i.lightmapUV;
//			#endif
//			output.gBuffer4 =
//				UnityGetRawBakedOcclusions(shadowUV, i.worldPos.xyz);
//		#endif
//	#else
//		output.color = ApplyFog(color, i);
//	#endif
//	return output;
//}

#endif











//#define MY_SHADOWS_INCLUDED
//#include"UnityCG.cginc"
//    struct VertexData{
//     float4 position:POSITION;
//     float3 normal:NORMAL;
//     fixed4 color:COLOR;
//     float4 texCoord0:TEXCOORD0;
//     float4 texCoord1:TEXCOORD1;
//     float4 texCoord2:TEXCOORD2;
//     float4 texCoord3:TEXCOORD3;
//     float4 texCoord4:TEXCOORD4;
//     float4 texCoord5:TEXCOORD5;
//     float4 texCoord6:TEXCOORD6;
//     float4 texCoord7:TEXCOORD7;
//     float4 tangent:TANGENT;
//    };

//struct InterpolatorsVertex {
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//	float4 position : SV_POSITION;
//	#if SHADOWS_NEED_UV
//		float2 uv : TEXCOORD0;
//	#endif
//	#if defined(SHADOWS_CUBE)
//		float3 lightVec : TEXCOORD1;
//	#endif
//  float4 vWorldPos:TEXCOORD2;
//};

//struct Interpolators {
//	UNITY_VERTEX_INPUT_INSTANCE_ID
//	#if SHADOWS_SEMITRANSPARENT || defined(LOD_FADE_CROSSFADE)
//		UNITY_VPOS_TYPE vpos : VPOS;
//	#else
//		float4 positions : SV_POSITION;
//	#endif

//	#if SHADOWS_NEED_UV
//		float2 uv : TEXCOORD0;
//	#endif
//	#if defined(SHADOWS_CUBE)
//		float3 lightVec : TEXCOORD1;
//	#endif
//  float4 vWorldPos:TEXCOORD2;
//};

//float GetAlpha (Interpolators i) {
//	float alpha = 1;//UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).a;
//	#if SHADOWS_NEED_UV
//		alpha *= tex2D(_MainTex, i.uv.xy).a;
//	#endif
//	return alpha;
//}

//InterpolatorsVertex MyShadowVertexProgram (VertexData v) {
//	InterpolatorsVertex i;
//	UNITY_SETUP_INSTANCE_ID(v);
//	UNITY_TRANSFER_INSTANCE_ID(v, i);
//	#if defined(SHADOWS_CUBE)
//		i.position = UnityObjectToClipPos(v.position);
//		i.lightVec =
//			mul(unity_ObjectToWorld, v.position).xyz - _LightPositionRange.xyz;
//	#else
//		i.position = UnityClipSpaceShadowCasterPos(v.position.xyz, v.normal);
//		i.position = UnityApplyLinearShadowBias(i.position);
//	#endif

//	#if SHADOWS_NEED_UV
//		i.uv = TRANSFORM_TEX(v.uv, _MainTex);
//	#endif
//                 float4 worldPos=mul(unity_ObjectToWorld,float4(v.position.xyz,1));
// i.vWorldPos=worldPos;
//	return i;
//}

//float4 MyShadowFragmentProgram (Interpolators i) : SV_TARGET {
//	UNITY_SETUP_INSTANCE_ID(i);
//	#if defined(LOD_FADE_CROSSFADE)
//		UnityApplyDitherCrossFade(i.vpos);
//	#endif
 
//                 float viewDistance=distance(_WorldSpaceCameraPos,i.vWorldPos);
// float fadeStart=_fadeStartDis+8;
// float fadeEnd=_fadeEndDis+8;
//	float alpha = (fadeEnd-viewDistance)/(fadeEnd-fadeStart);//(32-viewDistance)/(32-16);//GetAlpha(i);
//		clip(alpha - .25);

//	#if SHADOWS_SEMITRANSPARENT
//		float dither =
//			tex3D(_DitherMaskLOD, float3(i.vpos.xy * 0.25, alpha * 0.9375)).a;
//		clip(dither - 0.01);
//	#endif
	
//	#if defined(SHADOWS_CUBE)
//		float depth = length(i.lightVec) + unity_LightShadowBias.x;
//		depth *= _LightPositionRange.w;
//		return UnityEncodeCubeShadowDepth(depth);
//	#else
//		return 0;
//	#endif
//}