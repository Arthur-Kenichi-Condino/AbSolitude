Shader"Voxels/OldVoxelTerrain"{
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
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 24 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
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

#LINE 43
  
  }
  ZWrite On
  Blend SrcAlpha OneMinusSrcAlpha
  
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma instancing_options assumeuniformscaling
#pragma multi_compile_instancing
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#define UNITY_ASSUME_UNIFORM_SCALING
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_LINEAR 
#if defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_LINEAR INSTANCING_ON 
#if defined(FOG_LINEAR) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP 
#if defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP INSTANCING_ON 
#if defined(FOG_EXP) && defined(INSTANCING_ON) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP2 
#if defined(FOG_EXP2) && !defined(FOG_EXP) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP2 INSTANCING_ON 
#if defined(FOG_EXP2) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_LIGHTING_COORDS(6,7)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD5; // SH
  #endif
  UNITY_SHADOW_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD5;
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
#endif
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif


ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma instancing_options assumeuniformscaling
#pragma multi_compile_instancing
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd_fullshadows
#include "HLSLSupport.cginc"
#define UNITY_ASSUME_UNIFORM_SCALING
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float3 tSpace0 : TEXCOORD2;
  float3 tSpace1 : TEXCOORD3;
  float3 tSpace2 : TEXCOORD4;
  float3 worldPos : TEXCOORD5;
  fixed4 color : COLOR0;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.color = v.color;

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_LINEAR 
#if defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float3 tSpace0 : TEXCOORD2;
  float3 tSpace1 : TEXCOORD3;
  float3 tSpace2 : TEXCOORD4;
  float3 worldPos : TEXCOORD5;
  fixed4 color : COLOR0;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.color = v.color;

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP 
#if defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float3 tSpace0 : TEXCOORD2;
  float3 tSpace1 : TEXCOORD3;
  float3 tSpace2 : TEXCOORD4;
  float3 worldPos : TEXCOORD5;
  fixed4 color : COLOR0;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.color = v.color;

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif

// -------- variant for: FOG_EXP2 
#if defined(FOG_EXP2) && !defined(FOG_EXP) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float3 tSpace0 : TEXCOORD2;
  float3 tSpace1 : TEXCOORD3;
  float3 tSpace2 : TEXCOORD4;
  float3 worldPos : TEXCOORD5;
  fixed4 color : COLOR0;
  UNITY_LIGHTING_COORDS(6,7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.color = v.color;

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  applyFixedFog (surfIN, o, c);
  return c;
}


#endif


ENDCG

}

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma instancing_options assumeuniformscaling
#pragma multi_compile_instancing
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal
#include "HLSLSupport.cginc"
#define UNITY_ASSUME_UNIFORM_SCALING
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_LINEAR 
#if defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_LINEAR INSTANCING_ON 
#if defined(FOG_LINEAR) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_EXP 
#if defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_EXP INSTANCING_ON 
#if defined(FOG_EXP) && defined(INSTANCING_ON) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_EXP2 
#if defined(FOG_EXP2) && !defined(FOG_EXP) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: FOG_EXP2 INSTANCING_ON 
#if defined(FOG_EXP2) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
  float3 viewDir : TEXCOORD5;
  float4 lmap : TEXCOORD6;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD7; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD7;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  o.color = v.color;
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = normalize(IN.viewDir);
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif


ENDCG

}

	// ---- shadow caster pass:
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On ZTest LEqual

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma instancing_options assumeuniformscaling
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_shadowcaster
#include "HLSLSupport.cginc"
#define UNITY_ASSUME_UNIFORM_SCALING
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_LINEAR 
#if defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_LINEAR INSTANCING_ON 
#if defined(FOG_LINEAR) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_EXP 
#if defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_EXP INSTANCING_ON 
#if defined(FOG_EXP) && defined(INSTANCING_ON) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_EXP2 
#if defined(FOG_EXP2) && !defined(FOG_EXP) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif

// -------- variant for: FOG_EXP2 INSTANCING_ON 
#if defined(FOG_EXP2) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  

// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float4 pack0 : TEXCOORD1; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD2; // _MainTex2 _MainTex3
  float3 worldNormal : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.worldNormal = worldNormal;
  o.color = v.color;
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  fixed3 viewDir = worldViewDir;
  surfIN.worldNormal = IN.worldNormal;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  SHADOW_CASTER_FRAGMENT(IN)
}


#endif


ENDCG

}

	// ---- meta information extraction pass:
	Pass {
		Name "Meta"
		Tags { "LightMode" = "Meta" }
		Cull Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma instancing_options assumeuniformscaling
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma shader_feature EDITOR_VISUALIZATION

#include "HLSLSupport.cginc"
#define UNITY_ASSUME_UNIFORM_SCALING
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#define UNITY_INSTANCED_RENDERER_BOUNDS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: <when no other keywords are defined>
#if !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: INSTANCING_ON 
#if defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_LINEAR 
#if defined(FOG_LINEAR) && !defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_LINEAR INSTANCING_ON 
#if defined(FOG_LINEAR) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_EXP2)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_EXP 
#if defined(FOG_EXP) && !defined(FOG_EXP2) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_EXP INSTANCING_ON 
#if defined(FOG_EXP) && defined(INSTANCING_ON) && !defined(FOG_EXP2) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_EXP2 
#if defined(FOG_EXP2) && !defined(FOG_EXP) && !defined(FOG_LINEAR) && !defined(INSTANCING_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif

// -------- variant for: FOG_EXP2 INSTANCING_ON 
#if defined(FOG_EXP2) && defined(INSTANCING_ON) && !defined(FOG_EXP) && !defined(FOG_LINEAR)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: YES
// needs screen space position: no
// needs world space position: YES
// needs view direction: YES
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: YES
// needs VFACE: no
// needs SV_IsFrontFace: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: YES
// 4 texcoords actually used
//   float2 _MainTex
//   float2 _MainTex1
//   float2 _MainTex2
//   float2 _MainTex3
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 45 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
   //  Physically based Standard lighting model, and enable shadows on all light types
   //#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
   //  Use shader model 5.0 target, to get nicer looking lighting
   //#pragma target 5.0
   //  Add fog and make it work
   //#pragma multi_compile_fog
   //#pragma instancing_options assumeuniformscaling
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
    float4 texcoord4:TEXCOORD4;
    float4 texcoord5:TEXCOORD5;
    float4 texcoord6:TEXCOORD6;
    float4 texcoord7:TEXCOORD7;
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
    if(input.texcoord4.x>=0){
     float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
      sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
      sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
      c_x+=colorNBump_texcoord4z.tex_axis_x;
      c_y+=colorNBump_texcoord4z.tex_axis_y;
      c_z+=colorNBump_texcoord4z.tex_axis_z;
      b_x+=colorNBump_texcoord4z.bump_axis_x;
      b_y+=colorNBump_texcoord4z.bump_axis_y;
      b_z+=colorNBump_texcoord4z.bump_axis_z;
    }
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
  
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 pack0 : TEXCOORD0; // _MainTex _MainTex1
  float4 pack1 : TEXCOORD1; // _MainTex2 _MainTex3
  float4 tSpace0 : TEXCOORD2;
  float4 tSpace1 : TEXCOORD3;
  float4 tSpace2 : TEXCOORD4;
  fixed4 color : COLOR0;
#ifdef EDITOR_VISUALIZATION
  float2 vizUV : TEXCOORD5;
  float4 lightCoord : TEXCOORD6;
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;
float4 _MainTex1_ST;
float4 _MainTex2_ST;
float4 _MainTex3_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  vert (v);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
#ifdef EDITOR_VISUALIZATION
  o.vizUV = 0;
  o.lightCoord = 0;
  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
    o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
  {
    o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
  }
#endif
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord1, _MainTex1);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord2, _MainTex2);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord3, _MainTex3);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.position.x = 1.0;
  surfIN.worldPos.x = 1.0;
  surfIN.worldNormal.x = 1.0;
  surfIN.viewDir.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv2_MainTex1.x = 1.0;
  surfIN.uv3_MainTex2.x = 1.0;
  surfIN.uv4_MainTex3.x = 1.0;
  surfIN.texcoord4.x = 1.0;
  surfIN.texcoord5.x = 1.0;
  surfIN.texcoord6.x = 1.0;
  surfIN.texcoord7.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  surfIN.uv2_MainTex1 = IN.pack0.zw;
  surfIN.uv3_MainTex2 = IN.pack1.xy;
  surfIN.uv4_MainTex3 = IN.pack1.zw;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  float3 viewDir = _unity_tbn_0 * worldViewDir.x + _unity_tbn_1 * worldViewDir.y  + _unity_tbn_2 * worldViewDir.z;
  surfIN.worldNormal = 0.0;
  surfIN.internalSurfaceTtoW0 = _unity_tbn_0;
  surfIN.internalSurfaceTtoW1 = _unity_tbn_1;
  surfIN.internalSurfaceTtoW2 = _unity_tbn_2;
  surfIN.worldPos = worldPos;
  surfIN.viewDir = viewDir;
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
#ifdef EDITOR_VISUALIZATION
  metaIN.VizUV = IN.vizUV;
  metaIN.LightCoord = IN.lightCoord;
#endif
  return UnityMetaFragment(metaIN);
}


#endif


ENDCG

}

	// ---- end of surface shader generated code

#LINE 214

 }
 FallBack"Diffuse"
}