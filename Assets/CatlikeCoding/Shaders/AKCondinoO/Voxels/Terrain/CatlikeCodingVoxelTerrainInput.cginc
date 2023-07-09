#pragma require 2darray
#include"UnityPBSLighting.cginc"
#include"AutoLight.cginc"
#if defined(FOG_LINEAR)||defined(FOG_EXP)||defined(FOG_EXP2)
    #if!defined(FOG_DISTANCE)
     #define FOG_DEPTH 1
    #endif
 #define FOG_ON 1
#endif
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
     fixed4 color:COLOR;
     float4 texCoord0:TEXCOORD0;
     float4 texCoord1:TEXCOORD1;
     float4 texCoord2:TEXCOORD2;
     float4 texCoord3:TEXCOORD3;
     float4 texCoord4:TEXCOORD4;
     float4 texCoord5:TEXCOORD5;
     float4 texCoord6:TEXCOORD6;
     float4 texCoord7:TEXCOORD7;
     float4 tangent:TANGENT;
    };
    struct InterpolatorsVertex{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     float4 pos:SV_POSITION;
     float4 worldPos:TEXCOORD0;
     float3 normal:TEXCOORD1;
     float4 tangent:TEXCOORD2;
     float3 binormal:TEXCOORD3;
     UNITY_SHADOW_COORDS(4)
     float3 vertexLightColor:TEXCOORD5;
     float3 tangentViewDir:TEXCOORD6;
     fixed4 color:TEXCOORD7;
     float4 uv0:TEXCOORD8;
     float4 uv1:TEXCOORD9;
     float4 uv2:TEXCOORD10;
     float4 uv3:TEXCOORD11;
     float4 uv4:TEXCOORD12;
     float4 uv5:TEXCOORD13;
     float4 uv6:TEXCOORD14;
     float4 uv7:TEXCOORD15;
     float3 vNormal:TEXCOORD16;
     float4 tSpace0:TEXCOORD17;
     float4 tSpace1:TEXCOORD18;
     float4 tSpace2:TEXCOORD19;
    };
    struct Interpolators{
     UNITY_VERTEX_INPUT_INSTANCE_ID
     UNITY_VPOS_TYPE vpos:VPOS;
     float4 worldPos:TEXCOORD0;
     float3 normal:TEXCOORD1;
     float4 tangent:TEXCOORD2;
     float3 binormal:TEXCOORD3;
     UNITY_SHADOW_COORDS(4)
     float3 vertexLightColor:TEXCOORD5;
     float3 tangentViewDir:TEXCOORD6;
     fixed4 color:TEXCOORD7;
     float4 uv0:TEXCOORD8;
     float4 uv1:TEXCOORD9;
     float4 uv2:TEXCOORD10;
     float4 uv3:TEXCOORD11;
     float4 uv4:TEXCOORD12;
     float4 uv5:TEXCOORD13;
     float4 uv6:TEXCOORD14;
     float4 uv7:TEXCOORD15;
     float3 vNormal:TEXCOORD16;
     float4 tSpace0:TEXCOORD17;
     float4 tSpace1:TEXCOORD18;
     float4 tSpace2:TEXCOORD19;
    };
    float4 GetDefaultUV(Interpolators i){
     return float4(0,0,0,0);
    }
#if!defined(UV_FUNCTION)
 #define UV_FUNCTION GetDefaultUV
#endif