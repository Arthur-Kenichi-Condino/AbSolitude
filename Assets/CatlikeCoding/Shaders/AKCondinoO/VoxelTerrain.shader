Shader"Voxels/VoxelTerrainFloodFillLightingTextureBlend"{
	Properties {
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo", 2D) = "white" {}

		[NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
		_BumpScale ("Bump Scale", Float) = 1

		[NoScaleOffset] _MetallicMap ("Metallic", 2D) = "white" {}
		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.1

		[NoScaleOffset] _ParallaxMap ("Parallax", 2D) = "black" {}
		_ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0

		[NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}
		_OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1

		[NoScaleOffset] _EmissionMap ("Emission", 2D) = "black" {}
		_Emission ("Emission", Color) = (0, 0, 0)

		[NoScaleOffset] _DetailMask ("Detail Mask", 2D) = "white" {}
		_DetailTex ("Detail Albedo", 2D) = "gray" {}
		[NoScaleOffset] _DetailNormalMap ("Detail Normals", 2D) = "bump" {}
		_DetailBumpScale ("Detail Bump Scale", Float) = 1

		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1
	}

	CGINCLUDE

	#define BINORMAL_PER_FRAGMENT
	#define FOG_DISTANCE

	#define PARALLAX_BIAS 0
//	#define PARALLAX_OFFSET_LIMITING
	#define PARALLAX_RAYMARCHING_STEPS 10
	#define PARALLAX_RAYMARCHING_INTERPOLATE
//	#define PARALLAX_RAYMARCHING_SEARCH_STEPS 3
	#define PARALLAX_FUNCTION ParallaxRaymarching
	#define PARALLAX_SUPPORT_SCALED_DYNAMIC_BATCHING

	ENDCG

	SubShader {

		Pass {
			Tags {
				"LightMode" = "ForwardBase"
			}
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM

			#pragma target 5.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _PARALLAX_MAP
			#pragma shader_feature _OCCLUSION_MAP
			#pragma shader_feature _EMISSION_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP
			#pragma shader_feature _DETAIL_NORMAL_MAP

			#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma instancing_options lodfade force_same_maxcount_for_gl

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#define FORWARD_BASE_PASS

			#include "VoxelTerrainLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ForwardAdd"
			}

			Blend [_SrcBlend] One
			ZWrite Off

			CGPROGRAM

			#pragma target 5.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _PARALLAX_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP
			#pragma shader_feature _DETAIL_NORMAL_MAP

			#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "VoxelTerrainLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "Deferred"
			}

			CGPROGRAM

			#pragma target 5.0
			#pragma exclude_renderers nomrt

			#pragma shader_feature _ _RENDERING_CUTOUT
			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
			#pragma shader_feature _NORMAL_MAP
			#pragma shader_feature _PARALLAX_MAP
			#pragma shader_feature _OCCLUSION_MAP
			#pragma shader_feature _EMISSION_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP
			#pragma shader_feature _DETAIL_NORMAL_MAP

			#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile_prepassfinal
			#pragma multi_compile_instancing
			#pragma instancing_options lodfade

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#define DEFERRED_PASS

			#include "VoxelTerrainLighting.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 5.0

			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature _SEMITRANSPARENT_SHADOWS
			#pragma shader_feature _SMOOTHNESS_ALBEDO

			#pragma multi_compile _ LOD_FADE_CROSSFADE

			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing
			#pragma instancing_options lodfade force_same_maxcount_for_gl

			#pragma vertex MyShadowVertexProgram
			#pragma fragment MyShadowFragmentProgram

			#include "VoxelTerrainShadows.cginc"

			ENDCG
		}

		Pass {
			Tags {
				"LightMode" = "Meta"
			}

			Cull Off

			CGPROGRAM

			#pragma vertex MyLightmappingVertexProgram
			#pragma fragment MyLightmappingFragmentProgram

			#pragma shader_feature _METALLIC_MAP
			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
			#pragma shader_feature _EMISSION_MAP
			#pragma shader_feature _DETAIL_MASK
			#pragma shader_feature _DETAIL_ALBEDO_MAP

			#include "VoxelTerrainLightmapping.cginc"

			ENDCG
		}
	}

	CustomEditor "CatlikeCodingLightingShaderGUI"
}



















  //  Properties{
  //   _columns("Atlas columns",float)=3
  //   _rows   ("Atlas rows"   ,float)=3
  //    _albedos("Albedos",2DArray)="white"{}
  //    _bumps  ("Bumps"  ,2DArray)="bump" {}
  //    _heights("Heights",2DArray)="white"{}
  //     _heightDistortion("Height map distortion",Range(0,.125))=.05//  "distortion" level
  //   _scale("Scale",float)=1
  //   _sharpness("Triplanar blend sharpness",float)=1
  //   _fadeStartDis("Fade start distance",float)=32
  //    _fadeEndDis("Fade end distance",float)=40
  //  }
  //  SubShader{
  //   Tags{"Queue"="AlphaTest""RenderType"="Transparent""IgnoreProjector"="True""DisableBatching"="True"}
  //   LOD 200
  //   ZWrite On
  //   Blend SrcAlpha OneMinusSrcAlpha
  //      Pass{
  //       Name"FORWARD"
  //       Tags{"LightMode"="ForwardBase"}
  //          CGPROGRAM
  //           #pragma target 5.0
  //           #pragma require 2darray
  //           #pragma multi_compile_fog//  make fog work
  //           #include"UnityCG.cginc"
  //           #include"UnityLightingCommon.cginc"//  for _LightColor0
  //           #include"Lighting.cginc"
  //           #pragma multi_compile_fwdbase
  //           #include"AutoLight.cginc"
  //           #pragma vertex VertexToFragment
  //           #pragma fragment FragmentToColor
  //           float _columns;
  //           float _rows;
  //            UNITY_DECLARE_TEX2DARRAY(_albedos);
  //            UNITY_DECLARE_TEX2DARRAY(_bumps);
  //            UNITY_DECLARE_TEX2DARRAY(_heights);
  //             float _heightDistortion;
  //           float _scale;
  //           float _sharpness;
  //           float _fadeStartDis;
  //            float _fadeEndDis;
  //              struct AppVertexData{
  //               float4 pos:POSITION;
  //               float3 normal:NORMAL;
  //               fixed4 color:COLOR;
  //               float4 texCoord0:TEXCOORD0;
  //               float4 texCoord1:TEXCOORD1;
  //               float4 texCoord2:TEXCOORD2;
  //               float4 texCoord3:TEXCOORD3;
  //               float4 texCoord4:TEXCOORD4;
  //               float4 texCoord5:TEXCOORD5;
  //               float4 texCoord6:TEXCOORD6;
  //               float4 texCoord7:TEXCOORD7;
  //               float4 tangent:TANGENT;
  //              };
  //              struct FragmentData{
  //               float4 pos:SV_POSITION;
  //               float4 vWorldPos:POSITION1;
  //               float4 uv0:TEXCOORD0;
  //               float4 uv1:TEXCOORD1;
  //               float4 uv2:TEXCOORD2;
  //               float4 uv3:TEXCOORD3;
  //               float4 uv4:TEXCOORD4;
  //               float4 uv5:TEXCOORD5;
  //               float4 uv6:TEXCOORD6;
  //               float4 uv7:TEXCOORD7;
  //               float4 tSpace0:TEXCOORD8;
  //               float4 tSpace1:TEXCOORD9;
  //               float4 tSpace2:TEXCOORD10;
  //               fixed4 diffuse:COLOR1;//  diffuse lighting color
  //               fixed3 ambient:COLOR2;
  //               SHADOW_COORDS(11)//  put shadows data into TEXCOORD11
  //              };
  //              FragmentData VertexToFragment(AppVertexData v){
  //               FragmentData o;
  //               o.pos=UnityObjectToClipPos(v.pos);
  //               float4 worldPos=mul(unity_ObjectToWorld,float4(v.pos.xyz,1));
  //               o.vWorldPos=worldPos;
  //               o.uv0=v.texCoord0;
  //               o.uv1=v.texCoord1;
  //               o.uv2=v.texCoord2;
  //               o.uv3=v.texCoord3;
  //               o.uv4=v.texCoord4;
  //               o.uv5=v.texCoord5;
  //               o.uv6=v.texCoord6;
  //               o.uv7=v.texCoord7;
  //               float3 worldNormal=UnityObjectToWorldNormal(v.normal);
  //               fixed3 worldTangent=UnityObjectToWorldDir(v.tangent.xyz);
  //               fixed tangentSign=v.tangent.w*unity_WorldTransformParams.w;
  //               fixed3 worldBinormal=cross(worldNormal,worldTangent)*tangentSign;
  //               o.tSpace0=float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
  //               o.tSpace1=float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
  //               o.tSpace2=float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
  //               half nl=max(0,dot(worldNormal,_WorldSpaceLightPos0.xyz));
  //               o.diffuse=nl*_LightColor0;
  //               o.ambient=ShadeSH9(half4(worldNormal,1));
  //               TRANSFER_SHADOW(o);
  //               return o;
  //              }
  //              fixed4 FragmentToColor(FragmentData i):SV_Target{
  //               UNITY_EXTRACT_TBN(i);
  //               fixed4 o=fixed4(.5,.5,.5,1);
  //               fixed shadow=SHADOW_ATTENUATION(i);
  //               fixed3 lighting=i.diffuse*shadow+i.ambient;
  //               o.rgb=o.rgb*lighting;
  //               float alpha=o.a;
  //               float viewDistance=distance(_WorldSpaceCameraPos,i.vWorldPos);
  //               float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
  //               clip(opacity);
  //               alpha=alpha*saturate(opacity);
  //               o.a=alpha;
  //               return o;
  //              }
  //          ENDCG
  //      }
		//Pass {
		//	Tags {
		//		"LightMode" = "ShadowCaster"
		//	}

		//	CGPROGRAM

		//	#pragma target 5.0

		//	#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
		//	#pragma shader_feature _SEMITRANSPARENT_SHADOWS
		//	#pragma shader_feature _SMOOTHNESS_ALBEDO

		//	#pragma multi_compile _ LOD_FADE_CROSSFADE

		//	#pragma multi_compile_shadowcaster
		//	#pragma multi_compile_instancing
		//	#pragma instancing_options lodfade force_same_maxcount_for_gl

		//	#pragma vertex MyShadowVertexProgram
		//	#pragma fragment MyShadowFragmentProgram

  //           float _fadeStartDis;
  //            float _fadeEndDis;
		//	#include "Shadows.cginc"

		//	ENDCG
		//}
  //  }
  //  FallBack"Diffuse"






























//Shader"Voxels/VoxelTerrainFloodFillLightingTextureBlend"{

//	Properties {
//		_Color ("Tint", Color) = (1, 1, 1, 1)
//		_MainTex ("Albedo", 2D) = "white" {}

//		[NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
//		_BumpScale ("Bump Scale", Float) = 1

//		[NoScaleOffset] _MetallicMap ("Metallic", 2D) = "white" {}
//		[Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
//		_Smoothness ("Smoothness", Range(0, 1)) = 0.1

//		[NoScaleOffset] _ParallaxMap ("Parallax", 2D) = "black" {}
//		_ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0

//		[NoScaleOffset] _OcclusionMap ("Occlusion", 2D) = "white" {}
//		_OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1

//		[NoScaleOffset] _EmissionMap ("Emission", 2D) = "black" {}
//		_Emission ("Emission", Color) = (0, 0, 0)

//		[NoScaleOffset] _DetailMask ("Detail Mask", 2D) = "white" {}
//		_DetailTex ("Detail Albedo", 2D) = "gray" {}
//		[NoScaleOffset] _DetailNormalMap ("Detail Normals", 2D) = "bump" {}
//		_DetailBumpScale ("Detail Bump Scale", Float) = 1

//		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

//		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
//		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
//		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1
//	}

//	CGINCLUDE

//	#define BINORMAL_PER_FRAGMENT
//	#define FOG_DISTANCE

//	#define PARALLAX_BIAS 0
////	#define PARALLAX_OFFSET_LIMITING
//	#define PARALLAX_RAYMARCHING_STEPS 10
//	#define PARALLAX_RAYMARCHING_INTERPOLATE
////	#define PARALLAX_RAYMARCHING_SEARCH_STEPS 3
//	#define PARALLAX_FUNCTION ParallaxRaymarching
//	#define PARALLAX_SUPPORT_SCALED_DYNAMIC_BATCHING

//	ENDCG

//	SubShader {

//		Pass {
//			Tags {
//				"LightMode" = "ForwardBase"
//			}
//			Blend [_SrcBlend] [_DstBlend]
//			ZWrite [_ZWrite]

//			CGPROGRAM

//			#pragma target 3.0

//			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
//			#pragma shader_feature _METALLIC_MAP
//			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
//			#pragma shader_feature _NORMAL_MAP
//			#pragma shader_feature _PARALLAX_MAP
//			#pragma shader_feature _OCCLUSION_MAP
//			#pragma shader_feature _EMISSION_MAP
//			#pragma shader_feature _DETAIL_MASK
//			#pragma shader_feature _DETAIL_ALBEDO_MAP
//			#pragma shader_feature _DETAIL_NORMAL_MAP

//			#pragma multi_compile _ LOD_FADE_CROSSFADE

//			#pragma multi_compile_fwdbase
//			#pragma multi_compile_fog
//			#pragma multi_compile_instancing
//			#pragma instancing_options lodfade force_same_maxcount_for_gl

//			#pragma vertex MyVertexProgram
//			#pragma fragment MyFragmentProgram

//			#define FORWARD_BASE_PASS

//			#include "My Lighting.cginc"

//			ENDCG
//		}

//		Pass {
//			Tags {
//				"LightMode" = "ForwardAdd"
//			}

//			Blend [_SrcBlend] One
//			ZWrite Off

//			CGPROGRAM

//			#pragma target 3.0

//			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
//			#pragma shader_feature _METALLIC_MAP
//			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
//			#pragma shader_feature _NORMAL_MAP
//			#pragma shader_feature _PARALLAX_MAP
//			#pragma shader_feature _DETAIL_MASK
//			#pragma shader_feature _DETAIL_ALBEDO_MAP
//			#pragma shader_feature _DETAIL_NORMAL_MAP

//			#pragma multi_compile _ LOD_FADE_CROSSFADE

//			#pragma multi_compile_fwdadd_fullshadows
//			#pragma multi_compile_fog
			
//			#pragma vertex MyVertexProgram
//			#pragma fragment MyFragmentProgram

//			#include "My Lighting.cginc"

//			ENDCG
//		}

//		Pass {
//			Tags {
//				"LightMode" = "Deferred"
//			}

//			CGPROGRAM

//			#pragma target 3.0
//			#pragma exclude_renderers nomrt

//			#pragma shader_feature _ _RENDERING_CUTOUT
//			#pragma shader_feature _METALLIC_MAP
//			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
//			#pragma shader_feature _NORMAL_MAP
//			#pragma shader_feature _PARALLAX_MAP
//			#pragma shader_feature _OCCLUSION_MAP
//			#pragma shader_feature _EMISSION_MAP
//			#pragma shader_feature _DETAIL_MASK
//			#pragma shader_feature _DETAIL_ALBEDO_MAP
//			#pragma shader_feature _DETAIL_NORMAL_MAP

//			#pragma multi_compile _ LOD_FADE_CROSSFADE

//			#pragma multi_compile_prepassfinal
//			#pragma multi_compile_instancing
//			#pragma instancing_options lodfade

//			#pragma vertex MyVertexProgram
//			#pragma fragment MyFragmentProgram

//			#define DEFERRED_PASS

//			#include "My Lighting.cginc"

//			ENDCG
//		}

//		Pass {
//			Tags {
//				"LightMode" = "ShadowCaster"
//			}

//			CGPROGRAM

//			#pragma target 3.0

//			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
//			#pragma shader_feature _SEMITRANSPARENT_SHADOWS
//			#pragma shader_feature _SMOOTHNESS_ALBEDO

//			#pragma multi_compile _ LOD_FADE_CROSSFADE

//			#pragma multi_compile_shadowcaster
//			#pragma multi_compile_instancing
//			#pragma instancing_options lodfade force_same_maxcount_for_gl

//			#pragma vertex MyShadowVertexProgram
//			#pragma fragment MyShadowFragmentProgram

//			#include "My Shadows.cginc"

//			ENDCG
//		}

//		Pass {
//			Tags {
//				"LightMode" = "Meta"
//			}

//			Cull Off

//			CGPROGRAM

//			#pragma vertex MyLightmappingVertexProgram
//			#pragma fragment MyLightmappingFragmentProgram

//			#pragma shader_feature _METALLIC_MAP
//			#pragma shader_feature _ _SMOOTHNESS_ALBEDO _SMOOTHNESS_METALLIC
//			#pragma shader_feature _EMISSION_MAP
//			#pragma shader_feature _DETAIL_MASK
//			#pragma shader_feature _DETAIL_ALBEDO_MAP

//			#include "My Lightmapping.cginc"

//			ENDCG
//		}
//	}

//	CustomEditor "MyLightingShaderGUI"
//}




//// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// //Properties{
// //       _Color ("Main Color", Color) = (1,1,1,1) //note: required but not used
// // _columns("Atlas columns",float)=3
// // _rows   ("Atlas rows"   ,float)=3
// //  _albedos("Albedos",2DArray)="white"{}
// //  _bumps  ("Bumps"  ,2DArray)="bump" {}
// //  _heights("Heights",2DArray)="white"{}
// //   _heightDistortion("Height map distortion",Range(0,.125))=.05//  "distortion" level
// // _scale("Scale",float)=1
// // _sharpness("Triplanar blend sharpness",float)=1
// // _fadeStartDis("Fade start distance",float)=32
// //  _fadeEndDis("Fade end distance",float)=40
// //}
// //SubShader{
// // Tags{"Queue"="AlphaTest""RenderType"="Transparent""IgnoreProjector"="True""DisableBatching"="True"}
// // LOD 100
// // ZWrite On
// // Blend SrcAlpha OneMinusSrcAlpha
// // Pass{
// //  Name"FORWARD"
// //  Tags{"LightMode"="ForwardBase"}
// //  CGPROGRAM
// //   #pragma target 5.0
// //   #pragma require 2darray
// //   #pragma multi_compile_fog//  make fog work
// //   #pragma vertex VertexToFragment
// //   #pragma fragment FragmentToColor
// //   #include"UnityCG.cginc"
// //   #include"UnityLightingCommon.cginc"//  for _LightColor0
// //   #include"Lighting.cginc"
// //   #pragma multi_compile_fwdbase
// //   #include"AutoLight.cginc"
// //   float _columns;
// //   float _rows;
// //    UNITY_DECLARE_TEX2DARRAY(_albedos);
// //    UNITY_DECLARE_TEX2DARRAY(_bumps);
// //    UNITY_DECLARE_TEX2DARRAY(_heights);
// //     float _heightDistortion;
// //   float _scale;
// //   float _sharpness;
// //   float _fadeStartDis;
// //    float _fadeEndDis;
// //      struct AppVertexData{
// //       float4 pos:POSITION;
// //       float3 normal:NORMAL;
// //       fixed4 color:COLOR;
// //       float4 texCoord0:TEXCOORD0;
// //       float4 texCoord1:TEXCOORD1;
// //       float4 texCoord2:TEXCOORD2;
// //       float4 texCoord3:TEXCOORD3;
// //       float4 texCoord4:TEXCOORD4;
// //       float4 texCoord5:TEXCOORD5;
// //       float4 texCoord6:TEXCOORD6;
// //       float4 texCoord7:TEXCOORD7;
// //       float4 tangent:TANGENT;
// //      };
// //      struct FragmentData{
// //       float4 pos:SV_POSITION;
// //       float4 vWorldPos:POSITION1;
// //      };
// //      FragmentData VertexToFragment(AppVertexData v){
// //       FragmentData o;
// //       o.pos=UnityObjectToClipPos(v.pos);
// //       float4 worldPos=mul(unity_ObjectToWorld,float4(v.pos.xyz,1));
// //       o.vWorldPos=worldPos;
// //       return o;
// //      }
// //      fixed4 FragmentToColor(FragmentData i):SV_Target{
// //       fixed4 o=fixed4(1,1,1,1);
// //       float alpha=o.a;
// //       float viewDistance=distance(_WorldSpaceCameraPos,i.vWorldPos);
// //       float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
// //       clip(opacity);
// //       alpha=alpha*saturate(opacity);
// //       o.a=alpha;
// //       return o;
// //      }
// //  ENDCG
// // }
//        //Pass {
//        //    Tags { "LightMode" = "ForwardAdd" }
//        //    Blend One One
//        //    Fog { Color(0,0,0,0) }
 
//        //    CGPROGRAM
//        //    #pragma vertex vert
//        //    #pragma fragment frag
//        //    #pragma multi_compile_fwdadd_fullshadows
//        //    #include "UnityCG.cginc"
//        //    #include "AutoLight.cginc"
 
//        //    struct v2f {
//        //        float4 pos : SV_POSITION;
//        //        LIGHTING_COORDS(0,1)
//        //    };
 
//        //    v2f vert (appdata_full v) {
//        //        v2f o;
//        //        o.pos = UnityObjectToClipPos(v.vertex);
//        //        TRANSFER_VERTEX_TO_FRAGMENT(o);
//        //        return o;
//        //    }
 
//        //    float4 frag (v2f i) : COLOR {
//        //        return LIGHT_ATTENUATION(i);
//        //    }
//        //    ENDCG
//        //} //Pass
//  //  [https://forum.unity.com/threads/using-alphatest-greater-0-5-how-to-cast-shadow.130565/]
////Pass
////        {
////            Name "ShadowCaster"
////            Tags{"LightMode" = "ShadowCaster"}
 
////            ZWrite On
////            ZTest LEqual
////            Cull[_Cull]
 
////            HLSLPROGRAM
////            // Required to compile gles 2.0 with standard srp library
////            #pragma prefer_hlslcc gles
////            #pragma exclude_renderers d3d11_9x
////            #pragma target 2.0
 
////            // -------------------------------------
////            // Material Keywords
////            #pragma shader_feature _ALPHATEST_ON
 
////            //--------------------------------------
////            // GPU Instancing
////            #pragma multi_compile_instancing
////            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 
////            #pragma vertex ShadowPassVertex
////            #pragma fragment ShadowPassFragment
 
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/ShadowCasterPass.hlsl"
////            ENDHLSL
////        }
 
////        Pass
////        {
////            Name "DepthOnly"
////            Tags{"LightMode" = "DepthOnly"}
 
////            ZWrite On
////            ColorMask 0
////            Cull[_Cull]
 
////            HLSLPROGRAM
////            // Required to compile gles 2.0 with standard srp library
////            #pragma prefer_hlslcc gles
////            #pragma exclude_renderers d3d11_9x
////            #pragma target 2.0
 
////            #pragma vertex DepthOnlyVertex
////            #pragma fragment DepthOnlyFragment
 
////            // -------------------------------------
////            // Material Keywords
////            #pragma shader_feature _ALPHATEST_ON
////            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 
////            //--------------------------------------
////            // GPU Instancing
////            #pragma multi_compile_instancing
 
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/DepthOnlyPass.hlsl"
////            ENDHLSL
////        }
 
////        // This pass it not used during regular rendering, only for lightmap baking.
////        Pass
////        {
////            Name "Meta"
////            Tags{"LightMode" = "Meta"}
 
////            Cull Off
 
////            HLSLPROGRAM
////            // Required to compile gles 2.0 with standard srp library
////            #pragma prefer_hlslcc gles
////            #pragma exclude_renderers d3d11_9x
 
////            #pragma vertex LightweightVertexMeta
////            #pragma fragment LightweightFragmentMeta
 
////            #pragma shader_feature _SPECULAR_SETUP
////            #pragma shader_feature _EMISSION
////            #pragma shader_feature _METALLICSPECGLOSSMAP
////            #pragma shader_feature _ALPHATEST_ON
////            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
 
////            #pragma shader_feature _SPECGLOSSMAP
 
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
////            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitMetaPass.hlsl"
 
////            ENDHLSL
////        }
//  //Pass{
//  // Name"Caster"
//  // Tags{"LightMode"="ShadowCaster"}
//  // Offset 1,1
//  // Fog{Mode Off}
//  // ZWrite On ZTest LEqual Cull Off
//  // CGPROGRAM
//  //  #pragma vertex vert
//  //  #pragma fragment frag
//  //          #pragma shader_feature _ALPHATEST_ON
//  //          #pragma shader_feature _ALPHAPREMULTIPLY_ON
//  //  #pragma multi_compile_shadowcaster
//  //  #pragma fragmentoption ARB_precision_hint_fastest
//  //  #include"UnityCG.cginc"
//  //     struct v2f{
//  //      V2F_SHADOW_CASTER;
//  //     };
//  //     v2f vert(appdata_base v){
//  //      v2f o;
//  //      //float4 worldPos=mul(unity_ObjectToWorld,float4(v.vertex.xyz,1));
//  //      float viewDistance=distance(_WorldSpaceCameraPos,fixed4(0,0,0,1));
//  //      if(viewDistance<=8){
//  //       TRANSFER_SHADOW_CASTER(o)
//  //      }
//  //      return o;
//  //     }
//  //  uniform fixed _Cutoff;
//  //  uniform fixed4 _Color;
//  //     float4 frag(v2f i):COLOR{
//  //      fixed4 texcol=fixed4(1,1,1,1);
//  //      clip(texcol.a*_Color.a-_Cutoff);
//  //      //SHADOW_CASTER_FRAGMENT(i)
//  //      return texcol;
//  //     }
//  // ENDCG
//  //}
//  //  Pass to render object as a shadow collector
//  //Pass{
//  // Name"ShadowCollector"
//  // Tags{"LightMode"="ShadowCollector"}
//  // Fog{Mode Off}
//  // ZWrite On ZTest LEqual
//  // CGPROGRAM
//  //  #pragma vertex vert
//  //  #pragma fragment frag
//  //  #pragma fragmentoption ARB_precision_hint_fastest
//  //  #pragma multi_compile_shadowcollector
//  //  #define SHADOW_COLLECTOR_PASS
//  //  #include"UnityCG.cginc"
//  //     struct v2f{
//  //      //V2F_SHADOW_COLLECTOR;
//  //     };
//  //     v2f vert(appdata_base v){
//  //      v2f o;
//  //      //float4 worldPos=mul(unity_ObjectToWorld,float4(v.vertex.xyz,1));
//  //      //float viewDistance=distance(_WorldSpaceCameraPos,worldPos);
//  //      //if(viewDistance<=16){
//  //      // TRANSFER_SHADOW_COLLECTOR(o)
//  //      //}
//  //      return o;
//  //     }
//  //  uniform fixed _Cutoff;
//  //  uniform fixed4 _Color;
//  //     fixed4 frag(v2f i):COLOR{
//  //      fixed4 texcol=fixed4(1,1,1,1);
//  //      clip(texcol.a*_Color.a-_Cutoff);
//  //      //SHADOW_COLLECTOR_FRAGMENT(i)
//  //      return texcol;
//  //     }
//  // ENDCG
//  //}
// //}
//    //FallBack "Diffuse" //note: for passes: ForwardBase, ShadowCaster, ShadowCollector
// //FallBack"Transparent/Cutout/VertexLit"