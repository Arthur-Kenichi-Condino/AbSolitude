Shader"Voxels/VoxelTerrain"{
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
  Tags{"Queue"="AlphaTest""RenderType"="Transparent""IgnoreProjector"="True""DisableBatching"="True"}
  LOD 100
  ZWrite On
  Blend SrcAlpha OneMinusSrcAlpha
  Pass{
   Name"FORWARD"
   Tags{"LightMode"="ForwardBase"}
   CGPROGRAM
    #pragma target 5.0
    #pragma require 2darray
    #pragma multi_compile_fog//  make fog work
    #pragma vertex VertexToFragment
    #pragma fragment FragmentToColor
    #include"UnityCG.cginc"
    #include"UnityLightingCommon.cginc"//  for _LightColor0
    #include"Lighting.cginc"
    #pragma multi_compile_fwdbase
    #include"AutoLight.cginc"
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
       struct AppVertexData{
        float4 pos:POSITION;
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
       struct FragmentData{
        float4 pos:SV_POSITION;
        float4 uv0:TEXCOORD0;
        float4 uv1:TEXCOORD1;
        float4 uv2:TEXCOORD2;
        float4 uv3:TEXCOORD3;
        float4 uv4:TEXCOORD4;
        float4 uv5:TEXCOORD5;
        float4 uv6:TEXCOORD6;
        float4 uv7:TEXCOORD7;
        float4 tSpace0:TEXCOORD8;
        float4 tSpace1:TEXCOORD9;
        float4 tSpace2:TEXCOORD10;
        fixed4 color:COLOR0;
        float3 vPos:POSITION1;
        float3 vWorldPos:POSITION2;
        float3 vNormal:NORMAL0;
        fixed4 diffuse:COLOR1;//  diffuse lighting color
        fixed3 ambient:COLOR2;
        SHADOW_COORDS(11)//  put shadows data into TEXCOORD11
        UNITY_FOG_COORDS(12)
       };
       FragmentData VertexToFragment(AppVertexData v){
        FragmentData o;
        o.vPos=v.pos;
        o.pos=UnityObjectToClipPos(v.pos);
        o.uv0=v.texCoord0;
        o.uv1=v.texCoord1;
        o.uv2=v.texCoord2;
        o.uv3=v.texCoord3;
        o.uv4=v.texCoord4;
        o.uv5=v.texCoord5;
        o.uv6=v.texCoord6;
        o.uv7=v.texCoord7;
        float3 worldPos=mul(unity_ObjectToWorld,v.pos).xyz;
        o.vWorldPos=worldPos;
        float3 worldNormal=UnityObjectToWorldNormal(v.normal);
        fixed3 worldTangent=UnityObjectToWorldDir(v.tangent.xyz);
        fixed tangentSign=v.tangent.w*unity_WorldTransformParams.w;
        fixed3 worldBinormal=cross(worldNormal,worldTangent)*tangentSign;
        o.tSpace0=float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
        o.tSpace1=float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
        o.tSpace2=float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
        o.color=v.color;
        o.vNormal=v.normal;
        half nl=max(0,dot(worldNormal,_WorldSpaceLightPos0.xyz));
        o.diffuse=nl*_LightColor0;
        o.ambient=ShadeSH9(half4(worldNormal,1));
        TRANSFER_SHADOW(o);
        UNITY_TRANSFER_FOG(o,o.pos);
        return o;
       }
       struct HeightSample{
        float2 heightDistortionTexOffset;
       };
       HeightSample SampleHeight(half2 sample_x,half2 sample_y,half2 sample_z,float heightTextureIndex,float strenght,half3 blend,float3 viewDir){
        HeightSample o;
        fixed4 heightAxis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(sample_x),heightTextureIndex));
        fixed4 heightAxis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(sample_y),heightTextureIndex));
        fixed4 heightAxis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(sample_z),heightTextureIndex));
        fixed4 h=(heightAxis_x)*blend.x
                +(heightAxis_y)*blend.y
                +(heightAxis_z)*blend.z;
        o.heightDistortionTexOffset=ParallaxOffset(h.rgb,_heightDistortion,viewDir);
        return o;
       }
       struct ColorAndBumpSample{
        fixed4 colorAxis_x;
        fixed4 colorAxis_y;
        fixed4 colorAxis_z;
        fixed4 bumpAxis_x;
        fixed4 bumpAxis_y;
        fixed4 bumpAxis_z;
       };
       ColorAndBumpSample SampleColorAndBump(half2 sample_x,half2 sample_y,half2 sample_z,float colorAndBumpTextureIndex,float strenght,float2 texOffset){
        ColorAndBumpSample o;
        o.colorAxis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(sample_x)+texOffset,colorAndBumpTextureIndex));
        o.colorAxis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(sample_y)+texOffset,colorAndBumpTextureIndex));
        o.colorAxis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(sample_z)+texOffset,colorAndBumpTextureIndex));
        o.bumpAxis_x =strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(sample_x)+texOffset,colorAndBumpTextureIndex));
        o.bumpAxis_y =strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(sample_y)+texOffset,colorAndBumpTextureIndex));
        o.bumpAxis_z =strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(sample_z)+texOffset,colorAndBumpTextureIndex));
        return o;
       }
       void AddSample(float x,float y,float strenght,float3 viewDir,half2 sample_x,half2 sample_y,half2 sample_z,half3 blend,inout fixed4 color_x,inout fixed4 color_y,inout fixed4 color_z,inout fixed4 bump_x,inout fixed4 bump_y,inout fixed4 bump_z){
        if(x>=0){
         float textureIndex=x+_columns*y;
          HeightSample heightSample=SampleHeight(sample_x,sample_y,sample_z,textureIndex,strenght,blend,viewDir);
          ColorAndBumpSample colorAndBumpSample=SampleColorAndBump(sample_x,sample_y,sample_z,textureIndex,strenght,heightSample.heightDistortionTexOffset);
           color_x+=colorAndBumpSample.colorAxis_x;
           color_y+=colorAndBumpSample.colorAxis_y;
           color_z+=colorAndBumpSample.colorAxis_z;
            bump_x+=colorAndBumpSample.bumpAxis_x;
            bump_y+=colorAndBumpSample.bumpAxis_y;
            bump_z+=colorAndBumpSample.bumpAxis_z;
        }
       }
       fixed4 FragmentToColor(FragmentData i):SV_Target{
        UNITY_EXTRACT_TBN(i);
        float3 worldPos=float3(
         i.tSpace0.w,
         i.tSpace1.w,
         i.tSpace2.w
        );
        fixed3 lightDir=normalize(UnityWorldSpaceLightDir(worldPos));
        float3 worldViewDir=normalize(UnityWorldSpaceViewDir(worldPos));
        float3 viewDir=_unity_tbn_0*worldViewDir.x+
                       _unity_tbn_1*worldViewDir.y+
                       _unity_tbn_2*worldViewDir.z;
        half2 sample_x=i.vWorldPos.yz*_scale;
        half2 sample_y=i.vWorldPos.xz*_scale;
        half2 sample_z=i.vWorldPos.xy*_scale;
        half3 blend=pow(abs(i.vNormal),_sharpness);
              blend=blend/(blend.x+blend.y+blend.z);
        fixed4 color_x=fixed4(0,0,0,0);
        fixed4 color_y=fixed4(0,0,0,0);
        fixed4 color_z=fixed4(0,0,0,0);
        fixed4  bump_x=fixed4(0,0,0,0);
        fixed4  bump_y=fixed4(0,0,0,0);
        fixed4  bump_z=fixed4(0,0,0,0);
        AddSample(i.uv0.x,i.uv0.y,i.uv6.x,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv0.z,i.uv0.w,i.uv6.y,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv1.x,i.uv1.y,i.uv6.z,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv1.z,i.uv1.w,i.uv6.w,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv2.x,i.uv2.y,i.uv7.x,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv2.z,i.uv2.w,i.uv7.y,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv3.x,i.uv3.y,i.uv7.z,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        AddSample(i.uv3.z,i.uv3.w,i.uv7.w,viewDir,sample_x,sample_y,sample_z,blend,color_x,color_y,color_z,bump_x,bump_y,bump_z);
        fixed4 color=color_x*blend.x
                    +color_y*blend.y
                    +color_z*blend.z;
        color=fixed4(1,1,1,1);
        fixed4 o=color;
        fixed shadow=SHADOW_ATTENUATION(i);
        fixed3 lighting=i.diffuse*shadow+i.ambient;
        o.rgb=o.rgb*lighting;
        float alpha=o.a;
        float viewDistance=distance(_WorldSpaceCameraPos.xyz,i.vWorldPos);
        float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
        clip(opacity);
        alpha=alpha*saturate(opacity);
        o.a=alpha;
        UNITY_APPLY_FOG(i.fogCoord,o);
        return o;
       }
   ENDCG
  }
  //  [https://forum.unity.com/threads/using-alphatest-greater-0-5-how-to-cast-shadow.130565/]
  Pass{
   Name"Caster"
   Tags{"LightMode"="ShadowCaster"}
   Offset 1,1
   Fog{Mode Off}
   ZWrite On ZTest LEqual Cull Off
   CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_shadowcaster
    #pragma fragmentoption ARB_precision_hint_fastest
    #include"UnityCG.cginc"
       struct v2f{
        V2F_SHADOW_CASTER;
       };
       v2f vert(appdata_base v){
        v2f o;
        TRANSFER_SHADOW_CASTER(o)
        return o;
       }
    uniform fixed _Cutoff;
    uniform fixed4 _Color;
       float4 frag(v2f i):COLOR{
        fixed4 texcol=fixed4(0,0,0,0);
        clip(texcol.a*_Color.a-_Cutoff);
        SHADOW_CASTER_FRAGMENT(i)
       }
   ENDCG
  }
  //  Pass to render object as a shadow collector
  Pass{
   Name"ShadowCollector"
   Tags{"LightMode"="ShadowCollector"}
   Fog{Mode Off}
   ZWrite On ZTest LEqual
   CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #pragma fragmentoption ARB_precision_hint_fastest
    #pragma multi_compile_shadowcollector
    #define SHADOW_COLLECTOR_PASS
    #include"UnityCG.cginc"
       struct v2f{
        V2F_SHADOW_COLLECTOR;
       };
       v2f vert(appdata_base v){
        v2f o;
        TRANSFER_SHADOW_COLLECTOR(o)
        return o;
       }
    uniform fixed _Cutoff;
    uniform fixed4 _Color;
       fixed4 frag(v2f i):COLOR{
        fixed4 texcol=fixed4(0,0,0,0);
        clip(texcol.a*_Color.a-_Cutoff);
        SHADOW_COLLECTOR_FRAGMENT(i)
       }
   ENDCG
  }
 }
 FallBack"Transparent/VertexLit"
}