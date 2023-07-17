Shader"Voxels/Terrain/CatlikeCodingVoxelTerrain"{
    Properties{
     [HideInInspector]_Color("Tint",Color)=(1,1,1,1)
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
     _Cutoff("Alpha Cutoff",Range(0,1))=0.0
     [HideInInspector]_SrcBlend("_SrcBlend",Float)=1
     [HideInInspector]_DstBlend("_DstBlend",Float)=0
     [HideInInspector]_ZWrite("_ZWrite",Float)=1
    }
    CGINCLUDE
     #define FOG_DISTANCE
    ENDCG
    SubShader{
        Pass{
         Tags{"LightMode"="ForwardBase"}
         Blend[_SrcBlend][_DstBlend]
         ZWrite[_ZWrite]
            CGPROGRAM
             #pragma target 5.0
             #pragma multi_compile_fwdbase
             #pragma multi_compile_fog
             #define FORWARD_BASE_PASS
             #pragma vertex VertexToFragmentProgram
             #pragma fragment FragmentToColorProgram
             #include"CatlikeCodingVoxelTerrainLighting.cginc"
            ENDCG
        }
        Pass{
         Tags{"LightMode"="ForwardAdd"}
         Blend[_SrcBlend]One
         ZWrite Off
            CGPROGRAM
             #pragma target 5.0
             #pragma multi_compile_fwdadd_fullshadows
             #pragma multi_compile_fog
             #pragma vertex VertexToFragmentProgram
             #pragma fragment FragmentToColorProgram
             #include"CatlikeCodingVoxelTerrainLighting.cginc"
            ENDCG
        }
        Pass{
         Tags{"LightMode"="ShadowCaster"}
            CGPROGRAM
             #pragma target 5.0
             #pragma multi_compile_shadowcaster
             #pragma vertex ShadowVertexToFragmentProgram
             #pragma fragment ShadowFragmentToColorProgram
             #include"CatlikeCodingVoxelTerrainShadows.cginc"
            ENDCG
        }
    }
    CustomEditor"CatlikeCodingVoxelTerrainLightingShaderGUI"
}