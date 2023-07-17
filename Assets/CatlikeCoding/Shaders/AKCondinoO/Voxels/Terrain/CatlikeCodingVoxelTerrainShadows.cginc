#pragma require 2darray
#include"UnityCG.cginc"
UNITY_INSTANCING_BUFFER_START(InstanceProperties)
 UNITY_DEFINE_INSTANCED_PROP(float4,_Color)
UNITY_INSTANCING_BUFFER_END  (InstanceProperties)
float _columns;
float _rows;
 UNITY_DECLARE_TEX2DARRAY(_albedos);
 UNITY_DECLARE_TEX2DARRAY(_bumps  );
 UNITY_DECLARE_TEX2DARRAY(_heights);
  float _heightDistortion;
float _scale;
float _sharpness;
float _fadeStartDis;
 float _fadeEndDis;
float _Cutoff;
    struct VertexData{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     float4 vertex:POSITION;
     float3 normal:NORMAL;
    };
    struct InterpolatorsVertex{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     float4 position:SV_POSITION;
        #if defined(SHADOWS_CUBE)
         float3 lightVec:TEXCOORD0;
        #endif
     float4 worldPos:TEXCOORD1;
    };
    struct Interpolators{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     UNITY_VPOS_TYPE vpos:VPOS;
        #if defined(SHADOWS_CUBE)
         float3 lightVec:TEXCOORD0;
        #endif
     float4 worldPos:TEXCOORD1;
    };
#define VertexToFragmentProgram ShadowVertexToFragmentProgram
    InterpolatorsVertex ShadowVertexToFragmentProgram(VertexData v){
     InterpolatorsVertex i;
     UNITY_SETUP_INSTANCE_ID(v);
     UNITY_TRANSFER_INSTANCE_ID(v,i);
        #if defined(GetTangentSpaceNormal)
         i.position=UnityObjectToClipPos(v.vertex);
         i.lightVec=mul(unity_ObjectToWorld,v.vertex).xyz-_LightPositionRange.xyz;
        #else
         i.position=UnityClipSpaceShadowCasterPos(v.vertex.xyz,v.normal);
         i.position=UnityApplyLinearShadowBias(i.position);
        #endif
     i.worldPos.xyz=mul(unity_ObjectToWorld,v.vertex);
     i.worldPos.w=i.position.z;
     return i;
    }
    float4 ShadowFragmentToColorProgram(Interpolators i):SV_TARGET{
     UNITY_SETUP_INSTANCE_ID(i);
     float viewDistance=distance(_WorldSpaceCameraPos,i.worldPos);
     float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
     clip(opacity);
     float alpha=1;
     alpha=alpha*saturate(opacity);
     clip(alpha-_Cutoff);
        #if defined(SHADOWS_CUBE)
         float depth=length(i.lightVec)+unity_LightShadowBias.x;
         depth*=_LightPositionRange.w;
         return UnityEncodeCubeShadowDepth(depth);
        #else
         return 0;
        #endif
    }