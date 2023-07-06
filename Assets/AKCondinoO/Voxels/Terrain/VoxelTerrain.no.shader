Shader"Voxels/VoxelTerrain2"{
 Properties{
  //  leave these here or Unity doesn't fill struct Input with correct values:
  _MainTex ("do not add texture",2D)="white"{}
  _MainTex1("do not add texture",2D)="white"{}
  _MainTex2("do not add texture",2D)="white"{}
  _MainTex3("do not add texture",2D)="white"{}
  //  atlas:
  _columns("atlas columns",float)=2
  _rows   ("atlas rows"   ,float)=2
  _materials("materials"       ,2DArray)="white"{}
  _bumps    ("material bumps"  ,2DArray)="bump" {}
  _heights  ("material heights",2DArray)="white"{}
   _height("height",Range(0,.125))=.05//  "distortion" level
  _scale("scale",float)=1 	  
  _sharpness("triplanar blend sharpness",float)=1
  _fadeStartDis("fade start distance",float)=32
  _fadeEndDis("fade end distance",float)=40
 }
 SubShader{
  Tags{"Queue"="AlphaTest" "RenderType"="Transparent" "IgnoreProjector"="True" "DisableBatching"="True"}
  LOD 200
  Pass{
   ZWrite On
   ColorMask 0
   CGPROGRAM
    #pragma   vertex vert
    #pragma fragment frag
    #pragma require 2darray
    #pragma multi_compile_fog
    #include "UnityCG.cginc"
    struct v2f{
     float4 pos:SV_POSITION;
    };
    v2f vert(appdata_base v){
        v2f o;
            o.pos=UnityObjectToClipPos(v.vertex);
     return o;
    }
    half4 frag(v2f i):COLOR{
     return half4(0,0,0,0); 
    }
   ENDCG  
  }
  ZWrite On
  Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
   //  Physically based Standard lighting model, and enable shadows on all light types
   #pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 3.0 target, to get nicer looking lighting
   #pragma target 3.0
   //  Add fog and make it work
   #pragma multi_compile_fog
   #pragma instancing_options assumeuniformscaling
   UNITY_INSTANCING_BUFFER_START(Props)
    //  Put more per-instance properties here
   UNITY_INSTANCING_BUFFER_END  (Props)        
   //  atlas:
   float _columns;
   float _rows;
   UNITY_DECLARE_TEX2DARRAY(_materials);
   UNITY_DECLARE_TEX2DARRAY(_bumps);
   UNITY_DECLARE_TEX2DARRAY(_heights);
    float _height;
   float _scale;
   float _sharpness;
   float _fadeStartDis;
   float _fadeEndDis;
   struct Input{
    float4 position:SV_POSITION;
    float3 worldPos:POSITION;
    float3 worldNormal:NORMAL;
    float3 viewDir;
    float4 color:COLOR;
    float2 uv_MainTex:TEXCOORD0;
    float2 uv2_MainTex1:TEXCOORD1;
    float2 uv3_MainTex2:TEXCOORD2;
    float2 uv4_MainTex3:TEXCOORD3;
    INTERNAL_DATA
   };
   Input vert(inout appdata_full v){
     Input o;
    UNITY_INITIALIZE_OUTPUT(Input,o);
           o.position=UnityObjectToClipPos(v.vertex);
    return o;
   }
   half2 uv_x;
   half2 uv_y;
   half2 uv_z;
   half3 blendWeights;
   struct sampledHeight{
    float2 texOffset;
   };
   sampledHeight sampleHeight(float strenght,float index,float3 viewDir){
    sampledHeight o;
    fixed4 height_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_x),index));
    fixed4 height_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_y),index));
    fixed4 height_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(uv_z),index));
    fixed4 h=(height_axis_x)*blendWeights.x
            +(height_axis_y)*blendWeights.y
            +(height_axis_z)*blendWeights.z;
           o.texOffset=ParallaxOffset(h.r,_height,viewDir);
    return o;
   }
   struct sampledColorNBump{
    fixed4 tex_axis_x;
    fixed4 tex_axis_y;
    fixed4 tex_axis_z;
    fixed4 bump_axis_x;
    fixed4 bump_axis_y;
    fixed4 bump_axis_z;
   };
   sampledColorNBump sampleColorNBump(float2 texOffset,float strenght,float index){
    sampledColorNBump o;
    o.tex_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_materials,float3(frac(uv_x)+texOffset,index));
    o.tex_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_materials,float3(frac(uv_y)+texOffset,index));
    o.tex_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_materials,float3(frac(uv_z)+texOffset,index));
    o.bump_axis_x=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_x)+texOffset,index));
    o.bump_axis_y=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_y)+texOffset,index));
    o.bump_axis_z=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(uv_z)+texOffset,index));
    return o;
   }
   void surf(Input input,inout SurfaceOutputStandard o){
    uv_x=input.worldPos.yz*_scale;
    uv_y=input.worldPos.xz*_scale;
    uv_z=input.worldPos.xy*_scale;
    blendWeights=pow(abs(WorldNormalVector(input,o.Normal)),_sharpness);
    blendWeights=blendWeights/(blendWeights.x+blendWeights.y+blendWeights.z);
    fixed4 c_x=fixed4(0,0,0,0);
    fixed4 c_y=fixed4(0,0,0,0);
    fixed4 c_z=fixed4(0,0,0,0);
    fixed4 b_x=fixed4(0,0,0,0);
    fixed4 b_y=fixed4(0,0,0,0);
    fixed4 b_z=fixed4(0,0,0,0);
    float index_r=input.uv_MainTex.x+_columns*input.uv_MainTex.y;
     sampledHeight height_r=sampleHeight(input.color.r,index_r,input.viewDir);
     sampledColorNBump colorNBump_r=sampleColorNBump(height_r.texOffset,input.color.r,index_r);
     c_x+=colorNBump_r.tex_axis_x;
     c_y+=colorNBump_r.tex_axis_y;
     c_z+=colorNBump_r.tex_axis_z;
     b_x+=colorNBump_r.bump_axis_x;
     b_y+=colorNBump_r.bump_axis_y;
     b_z+=colorNBump_r.bump_axis_z;
    if(input.uv2_MainTex1.x>=0){
     float index_g=input.uv2_MainTex1.x+_columns*input.uv2_MainTex1.y;
      sampledHeight height_g=sampleHeight(input.color.g,index_g,input.viewDir);
      sampledColorNBump colorNBump_g=sampleColorNBump(height_g.texOffset,input.color.g,index_g);
      c_x+=colorNBump_g.tex_axis_x;
      c_y+=colorNBump_g.tex_axis_y;
      c_z+=colorNBump_g.tex_axis_z;
      b_x+=colorNBump_g.bump_axis_x;
      b_y+=colorNBump_g.bump_axis_y;
      b_z+=colorNBump_g.bump_axis_z;
    }
    if(input.uv3_MainTex2.x>=0){
     float index_b=input.uv3_MainTex2.x+_columns*input.uv3_MainTex2.y;
      sampledHeight height_b=sampleHeight(input.color.b,index_b,input.viewDir);
      sampledColorNBump colorNBump_b=sampleColorNBump(height_b.texOffset,input.color.b,index_b);
      c_x+=colorNBump_b.tex_axis_x;
      c_y+=colorNBump_b.tex_axis_y;
      c_z+=colorNBump_b.tex_axis_z;
      b_x+=colorNBump_b.bump_axis_x;
      b_y+=colorNBump_b.bump_axis_y;
      b_z+=colorNBump_b.bump_axis_z;
    }
    if(input.uv4_MainTex3.x>=0){
     float index_a=input.uv4_MainTex3.x+_columns*input.uv4_MainTex3.y;
      sampledHeight height_a=sampleHeight(input.color.a,index_a,input.viewDir);
      sampledColorNBump colorNBump_a=sampleColorNBump(height_a.texOffset,input.color.a,index_a);
      c_x+=colorNBump_a.tex_axis_x;
      c_y+=colorNBump_a.tex_axis_y;
      c_z+=colorNBump_a.tex_axis_z;
      b_x+=colorNBump_a.bump_axis_x;
      b_y+=colorNBump_a.bump_axis_y;
      b_z+=colorNBump_a.bump_axis_z;
    }
    fixed4 c=(c_x)*blendWeights.x
            +(c_y)*blendWeights.y
            +(c_z)*blendWeights.z;
    fixed4 b=(b_x)*blendWeights.x
            +(b_y)*blendWeights.y
            +(b_z)*blendWeights.z;
    o.Albedo=(c.rgb);
    o.Normal=UnpackNormal(b);
    float alpha=c.a;
    float viewDistance=length(_WorldSpaceCameraPos-input.worldPos);
    float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
    clip(opacity);
    alpha=alpha*saturate(opacity);
    o.Alpha=(alpha);
   }
   void applyFixedFog(Input input,SurfaceOutputStandard o,inout fixed4 color){
    float viewDistance=length(_WorldSpaceCameraPos-input.worldPos);
    UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
    if(unityFogFactor>0){
	    color.rgb=lerp(unity_FogColor.rgb,color.rgb,saturate(unityFogFactor));
    }
   }
  ENDCG
 }
 FallBack"Diffuse"
}