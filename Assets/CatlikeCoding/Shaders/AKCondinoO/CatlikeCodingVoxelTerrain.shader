Shader"Voxels/CatlikeCodingVoxelTerrainERROR"{
    Properties{
     _columns("Atlas columns",float)=3
     _rows   ("Atlas rows"   ,float)=3
      _albedos("Albedos",2DArray)="white"{}
      _bumps  ("Bumps"  ,2DArray)="bump" {}
      _heights("Heights",2DArray)="white"{}
       _heightDistortion("Height map distortion",Range(0,.125))=.05//  "distortion" level
     _scale("Scale",float)=1  
     _sharpness("Triplanar blend sharpness",float)=1
     _fadeStartDis("Fade start distance",float)=32
      _fadeEndDis("Fade end distance",float)=40
    }
    SubShader{
     Tags{"Queue"="AlphaTest""RenderType"="Transparent"}
     LOD 200
        Pass{
         Tags{"LightMode"="ForwardBase"}
         ZWrite On
         Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
             #pragma target 5.0
             #pragma multi_compile_fwdbase
             #pragma multi_compile_fog//  make fog work
             #define FORWARD_BASE_PASS
             #include"CatlikeCodingVoxelTerrainLighting.cginc"
             #pragma vertex VertexToFragmentProgram
             #pragma fragment FragmentToColorProgram
            ENDCG
        }



		//Pass {
		//	Tags {
		//		"LightMode" = "ForwardAdd"
		//	}

		//	ZWrite Off
		//	Blend SrcAlpha One
		//	CGPROGRAM
  //           #pragma target 5.0
		//	#pragma multi_compile_fwdadd_fullshadows
		//	#pragma multi_compile_fog
  //           #include"CatlikeCodingVoxelTerrainLighting.cginc"
  //           #pragma vertex VertexToFragmentProgram
  //           #pragma fragment FragmentToColorProgram
		//	ENDCG
		//}
		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}

			//CGPROGRAM

			//#pragma target 5.0

			//#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			//#pragma shader_feature _SEMITRANSPARENT_SHADOWS
			//#pragma shader_feature _SMOOTHNESS_ALBEDO

			//#pragma multi_compile _ LOD_FADE_CROSSFADE

			//#pragma multi_compile_shadowcaster
			//#pragma multi_compile_instancing
			//#pragma instancing_options lodfade force_same_maxcount_for_gl

			//#pragma vertex MyShadowVertexProgram
			//#pragma fragment MyShadowFragmentProgram

			//#include "VoxelTerrainShadows.cginc"

			//ENDCG
		}



    }
    FallBack"Diffuse"
}