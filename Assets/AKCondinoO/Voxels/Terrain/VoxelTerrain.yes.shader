Shader"Voxels/VoxelTerrainFloodFillLightingTextureBlend"{
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
  Tags{"Queue"="AlphaTest""RenderType"="Transparent""IgnoreProjector"="True""DisableBatching"="True""LightMode"="ForwardBase"}
  LOD 200
  Pass{
   ZWrite On
   ColorMask 0
   CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
       struct AppVertexData{
        float4 pos:POSITION;
        float3 normal:NORMAL;
        float4 tangent:TANGENT;
        fixed4 color:COLOR;
        float4 uv0:TEXCOORD0;
        float4 uv1:TEXCOORD1;
        float4 uv2:TEXCOORD2;
        float4 uv3:TEXCOORD3;
        float4 uv4:TEXCOORD4;
        float4 uv5:TEXCOORD5;
        float4 uv6:TEXCOORD6;
        float4 uv7:TEXCOORD7;
       };
       struct v2f{
        float4 pos:SV_POSITION;
       };
       v2f vert(AppVertexData v){
        v2f o;
        o.pos=UnityObjectToClipPos(v.pos);
        return o;
       }
       fixed4 frag(v2f i):COLOR{
        return fixed4(0,0,0,0);
       }
   ENDCG
  }
  Pass{
   ZWrite On
   Blend SrcAlpha OneMinusSrcAlpha
   CGPROGRAM
    #pragma target 5.0
    #pragma require 2darray
    #pragma multi_compile_fog//  make fog work
    #pragma instancing_options assumeuniformscaling
    #pragma vertex VertexToFragment alpha
    #pragma fragment FragmentToColor alpha
    #include "UnityCG.cginc"
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
        float4 tangent:TANGENT;
        fixed4 color:COLOR;
        float4 uv0:TEXCOORD0;
        float4 uv1:TEXCOORD1;
        float4 uv2:TEXCOORD2;
        float4 uv3:TEXCOORD3;
        float4 uv4:TEXCOORD4;
        float4 uv5:TEXCOORD5;
        float4 uv6:TEXCOORD6;
        float4 uv7:TEXCOORD7;
       };
       struct FragmentData{
        float4 pos:SV_POSITION;
        fixed4 color:COLOR0;
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
        float3 vertexWorldPos:POSITION1;
        float3 vertexNormal:NORMAL0;
        float3 vertexWorldNormal:NORMAL1;
        UNITY_FOG_COORDS(11)
       };
       FragmentData VertexToFragment(AppVertexData appVertexData){
        float3 worldPos=mul(unity_ObjectToWorld,appVertexData.pos).xyz;
        float3 worldNormal=UnityObjectToWorldNormal(appVertexData.normal);
        fixed3 worldTangent=UnityObjectToWorldDir(appVertexData.tangent.xyz);
        fixed tangentSign=appVertexData.tangent.w*unity_WorldTransformParams.w;
        fixed3 worldBinormal=cross(worldNormal,worldTangent)*tangentSign;
        FragmentData fragmentData;
        fragmentData.pos=UnityObjectToClipPos(appVertexData.pos);
        fragmentData.color=appVertexData.color;
        fragmentData.uv0=appVertexData.uv0;
        fragmentData.uv1=appVertexData.uv1;
        fragmentData.uv2=appVertexData.uv2;
        fragmentData.uv3=appVertexData.uv3;
        fragmentData.uv4=appVertexData.uv4;
        fragmentData.uv5=appVertexData.uv5;
        fragmentData.uv6=appVertexData.uv6;
        fragmentData.uv7=appVertexData.uv7;
        fragmentData.tSpace0=float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
        fragmentData.tSpace1=float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
        fragmentData.tSpace2=float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
        fragmentData.vertexWorldPos=worldPos;
        fragmentData.vertexNormal=appVertexData.normal;
        fragmentData.vertexWorldNormal=worldNormal;
        UNITY_TRANSFER_FOG(fragmentData,fragmentData.pos);
        return fragmentData;
       }
       struct HeightSample{
        float2 texOffset;
       };
       HeightSample SampleHeight(float strenght,float index,float3 viewDir,half2 uv_x,half2 uv_y,half2 uv_z,half3 blendingWeights){
        HeightSample output;
        fixed4 height_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_x),index));
        fixed4 height_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_y),index));
        fixed4 height_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_z),index));
        fixed4 h=(height_axis_x)*blendingWeights.x
                +(height_axis_y)*blendingWeights.y
                +(height_axis_z)*blendingWeights.z;
               output.texOffset=ParallaxOffset(h.rgb,_heightDistortion,viewDir);
        return output;
       }
       struct ColorAndBumpSample{
        fixed4 tex_axis_x;
        fixed4 tex_axis_y;
        fixed4 tex_axis_z;
        fixed4 bump_axis_x;
        fixed4 bump_axis_y;
        fixed4 bump_axis_z;
       };
       ColorAndBumpSample SampleColorAndBump(float2 texOffset,float strenght,float index,half2 uv_x,half2 uv_y,half2 uv_z){
        ColorAndBumpSample output;
        output.tex_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(uv_x)+texOffset,index));
        output.tex_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(uv_y)+texOffset,index));
        output.tex_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(uv_z)+texOffset,index));
        output.bump_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_x)+texOffset,index));
        output.bump_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_y)+texOffset,index));
        output.bump_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_z)+texOffset,index));
        return output;
       }
       fixed4 FragmentToColor(FragmentData fragmentData):SV_Target{
        UNITY_EXTRACT_TBN(fragmentData);
        float3 worldPos=float3(
         fragmentData.tSpace0.w,
         fragmentData.tSpace1.w,
         fragmentData.tSpace2.w
        );
        fixed3 lightDir=normalize(UnityWorldSpaceLightDir(worldPos));
        float3 worldViewDir=normalize(UnityWorldSpaceViewDir(worldPos));
        float3 viewDir=_unity_tbn_0*worldViewDir.x+
                       _unity_tbn_1*worldViewDir.y+
                       _unity_tbn_2*worldViewDir.z;
        //
        half2 uv_x=worldPos.yz*_scale;
        half2 uv_y=worldPos.xz*_scale;
        half2 uv_z=worldPos.xy*_scale;
        half3 blendingWeights=pow(abs(fragmentData.vertexNormal),_sharpness);
              blendingWeights=blendingWeights/(blendingWeights.x+blendingWeights.y+blendingWeights.z);
        fixed4 color_x=fixed4(0,0,0,0);
        fixed4 color_y=fixed4(0,0,0,0);
        fixed4 color_z=fixed4(0,0,0,0);
        fixed4 bump_x=fixed4(0,0,0,0);
        fixed4 bump_y=fixed4(0,0,0,0);
        fixed4 bump_z=fixed4(0,0,0,0);
        if(fragmentData.uv1.z>=0){
         float index1xy=fragmentData.uv1.z+_columns*fragmentData.uv1.w;
          HeightSample height1zw=SampleHeight(fragmentData.uv6.w,index1xy,viewDir,uv_x,uv_y,uv_z,blendingWeights);
          ColorAndBumpSample colorAndBump1zw=SampleColorAndBump(height1zw.texOffset,fragmentData.uv6.w,index1xy,uv_x,uv_y,uv_z);
           color_x+=colorAndBump1zw.tex_axis_x;
           color_y+=colorAndBump1zw.tex_axis_y;
           color_z+=colorAndBump1zw.tex_axis_z;
           bump_x+=colorAndBump1zw.bump_axis_x;
           bump_y+=colorAndBump1zw.bump_axis_y;
           bump_z+=colorAndBump1zw.bump_axis_z;
        }
        if(fragmentData.uv1.x>=0){
         float index1xy=fragmentData.uv1.x+_columns*fragmentData.uv1.y;
          HeightSample height1xy=SampleHeight(fragmentData.uv6.z,index1xy,viewDir,uv_x,uv_y,uv_z,blendingWeights);
          ColorAndBumpSample colorAndBump1xy=SampleColorAndBump(height1xy.texOffset,fragmentData.uv6.z,index1xy,uv_x,uv_y,uv_z);
           color_x+=colorAndBump1xy.tex_axis_x;
           color_y+=colorAndBump1xy.tex_axis_y;
           color_z+=colorAndBump1xy.tex_axis_z;
           bump_x+=colorAndBump1xy.bump_axis_x;
           bump_y+=colorAndBump1xy.bump_axis_y;
           bump_z+=colorAndBump1xy.bump_axis_z;
        }
        if(fragmentData.uv0.z>=0){
         float index0zw=fragmentData.uv0.z+_columns*fragmentData.uv0.w;
          HeightSample height0zw=SampleHeight(fragmentData.uv6.y,index0zw,viewDir,uv_x,uv_y,uv_z,blendingWeights);
          ColorAndBumpSample colorAndBump0zw=SampleColorAndBump(height0zw.texOffset,fragmentData.uv6.y,index0zw,uv_x,uv_y,uv_z);
           color_x+=colorAndBump0zw.tex_axis_x;
           color_y+=colorAndBump0zw.tex_axis_y;
           color_z+=colorAndBump0zw.tex_axis_z;
           bump_x+=colorAndBump0zw.bump_axis_x;
           bump_y+=colorAndBump0zw.bump_axis_y;
           bump_z+=colorAndBump0zw.bump_axis_z;
        }
         float index0xy=fragmentData.uv0.x+_columns*fragmentData.uv0.y;
          HeightSample height0xy=SampleHeight(fragmentData.uv6.x,index0xy,viewDir,uv_x,uv_y,uv_z,blendingWeights);
          ColorAndBumpSample colorAndBump0xy=SampleColorAndBump(height0xy.texOffset,fragmentData.uv6.x,index0xy,uv_x,uv_y,uv_z);
           color_x+=colorAndBump0xy.tex_axis_x;
           color_y+=colorAndBump0xy.tex_axis_y;
           color_z+=colorAndBump0xy.tex_axis_z;
           bump_x+=colorAndBump0xy.bump_axis_x;
           bump_y+=colorAndBump0xy.bump_axis_y;
           bump_z+=colorAndBump0xy.bump_axis_z;
        fixed4 Albedo=color_x*blendingWeights.x
                     +color_y*blendingWeights.y
                     +color_z*blendingWeights.z;
        fixed4 bump=(bump_x)*blendingWeights.x
                   +(bump_y)*blendingWeights.y
                   +(bump_z)*blendingWeights.z;
        fixed3 Normal=UnpackNormal(bump);
 
 
 
        //fixed4 c=fixed4(fragmentData.uv6.x,fragmentData.uv6.x,fragmentData.uv6.x,1.0);
        fixed4 c=Albedo;
 
 
 
        //  apply fog
        UNITY_APPLY_FOG(fragmentData.fogCoord,c);
        //c.a=1.0;
        return c;
 
 
 
       }
   ENDCG
  }
        Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
 }
}