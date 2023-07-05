Shader"Voxels/VoxelTerrainFloodFillLightingTextureBlendError"{
 Properties{
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
  Tags{"Queue"="AlphaTest""RenderType"="Transparent""IgnoreProjector"="True""DisableBatching"="True"}
  LOD 200
  ZWrite On
  ColorMask 0
  Blend SrcAlpha OneMinusSrcAlpha
  Pass{
   Name"FORWARD"
   Tags{"LightMode"="ForwardBase"}
   CGPROGRAM
   #pragma vertex vert_surf
   #pragma fragment frag_surf
   #pragma target 5.0
   #pragma multi_compile_fog
   #pragma instancing_options assumeuniformscaling
   #pragma multi_compile_instancing
   #pragma multi_compile_fwdbase
   #include"HLSLSupport.cginc"
   #define UNITY_ASSUME_UNIFORM_SCALING
   #define UNITY_INSTANCED_LOD_FADE
   #define UNITY_INSTANCED_SH
   #define UNITY_INSTANCED_LIGHTMAPSTS
   #define UNITY_INSTANCED_RENDERER_BOUNDS
   #include"UnityShaderVariables.cginc"
   #include"UnityShaderUtilities.cginc"
   //#if!defined(FOG_EXP)&&!defined(FOG_EXP2)&&!defined(FOG_LINEAR)&&!defined(INSTANCING_ON)
       #include"UnityCG.cginc"
       #include"Lighting.cginc"
       #include"UnityPBSLighting.cginc"
       #include"AutoLight.cginc"
       #define INTERNAL_DATA half3 internalSurfaceTtoW0;half3 internalSurfaceTtoW1;half3 internalSurfaceTtoW2;
       #define WorldReflectionVector(data,normal)reflect(data.worldRefl,half3(dot(data.internalSurfaceTtoW0,normal),dot(data.internalSurfaceTtoW1,normal),dot(data.internalSurfaceTtoW2,normal)))
       #define WorldNormalVector(data,normal)fixed3(dot(data.internalSurfaceTtoW0,normal),dot(data.internalSurfaceTtoW1,normal),dot(data.internalSurfaceTtoW2,normal))
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
       struct appdata{
        float4 vertex:POSITION;
        float4 tangent:TANGENT;
        float3 normal:NORMAL;
        float4 texcoord:TEXCOORD0;
        float4 texcoord1:TEXCOORD1;
        float4 texcoord2:TEXCOORD2;
        fixed4 color:COLOR;
        float4 texcoord3:TEXCOORD3;
        float4 texcoord4:TEXCOORD4;
        float4 texcoord5:TEXCOORD5;
        float4 texcoord6:TEXCOORD6;
        float4 texcoord7:TEXCOORD7;
       };
       //  vertex-to-fragment interpolation data
       //  no lightmaps:
       #ifndef LIGHTMAP_ON
           //  half-precision fragment shader registers:
           #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
               struct v2f_surf{
               };
           #endif
           //  high-precision fragment shader registers:
           #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
               struct v2f_surf{
                UNITY_POSITION(pos);
                float4 texcoord_pack_0_1:TEXCOORD0;
                float4 texcoord_pack_2_3:TEXCOORD1;
                float4 tSpace0:TEXCOORD2;
                float4 tSpace1:TEXCOORD3;
                float4 tSpace2:TEXCOORD4;
                fixed4 color:COLOR0;
                #if UNITY_SHOULD_SAMPLE_SH
                    half3 sh:TEXCOORD5;//  SH
                #endif
                UNITY_SHADOW_COORDS(6)
                #if SHADER_TARGET>=30
                    float4 lmap:TEXCOORD7;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
               };
           #endif
       #endif
       //  with lightmaps:
       #ifdef LIGHTMAP_ON
           // half-precision fragment shader registers:
           #ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
               struct v2f_surf{
               };
           #endif
           // high-precision fragment shader registers:
           #ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
               struct v2f_surf{
               };
           #endif
       #endif
       float4 _MainTex_ST;
       //  vertex shader
       v2f_surf vert_surf(appdata input){
        UNITY_SETUP_INSTANCE_ID(input);
        v2f_surf output;
        UNITY_INITIALIZE_OUTPUT(v2f_surf,output);
        UNITY_TRANSFER_INSTANCE_ID(input,output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.pos=UnityObjectToClipPos(input.vertex);
        output.texcoord_pack_0_1.xy=TRANSFORM_TEX(input.texcoord ,_MainTex);
        output.texcoord_pack_0_1.zw=TRANSFORM_TEX(input.texcoord1,_MainTex);
        output.texcoord_pack_2_3.xy=TRANSFORM_TEX(input.texcoord2,_MainTex);
        output.texcoord_pack_2_3.zw=TRANSFORM_TEX(input.texcoord3,_MainTex);
        float3 worldPos=mul(unity_ObjectToWorld,input.vertex).xyz;
        float3 worldNormal=UnityObjectToWorldNormal(input.normal);
        fixed3 worldTangent=UnityObjectToWorldDir(input.tangent.xyz);
        fixed tangentSign=input.tangent.w*unity_WorldTransformParams.w;
        fixed3 worldBinormal=cross(worldNormal,worldTangent)*tangentSign;
        output.tSpace0=float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
        output.tSpace1=float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
        output.tSpace2=float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
        output.color=input.color;
        #ifdef DYNAMICLIGHTMAP_ON
            output.lmap.zw=input.texcoord2.xy*unity_DynamicLightmapST.xy+unity_DynamicLightmapST.zw;
        #endif
        #ifdef LIGHTMAP_ON
            output.lmap.xy=input.texcoord1.xy*unity_LightmapST.xy+unity_LightmapST.zw;
        #endif
        //  SH / ambient and vertex lights
        #ifndef LIGHTMAP_ON
            #if UNITY_SHOULD_SAMPLE_SH&&!UNITY_SAMPLE_FULL_SH_PER_PIXEL
                output.sh=0;
                //  Approximated illumination from non-important point lights
                #ifdef VERTEXLIGHT_ON
                    output.sh+=Shade4PointLights(
                     unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
                     unity_LightColor[0].rgb,unity_LightColor[1].rgb,unity_LightColor[2].rgb,unity_LightColor[3].rgb,
                     unity_4LightAtten0,worldPos,worldNormal
                    );
                #endif
                output.sh=ShadeSHPerVertex(worldNormal,output.sh);
            #endif
        #endif//  !LIGHTMAP_ON
        UNITY_TRANSFER_LIGHTING(output,input.texcoord1.xy);//  pass shadow and, possibly, light cookie coordinates to pixel shader
        return output;
       }
       struct frag_surf_surface{
        float3 worldPos:POSITION;
        float3 worldNormal:NORMAL;
        float4 pos:SV_POSITION;
        fixed4 color:COLOR;
        float3 viewDir;
        float2 uv0:TEXCOORD0;
        float2 uv1:TEXCOORD1;
        float2 uv2:TEXCOORD2;
        float2 uv3:TEXCOORD3;
        float2 uv4:TEXCOORD4;
        float2 uv5:TEXCOORD5;
        float2 uv6:TEXCOORD6;
        float2 uv7:TEXCOORD7;
        INTERNAL_DATA
       };
       //  fragment shader
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
       void surface(frag_surf_surface input,inout SurfaceOutputStandard output){
        uv_x=input.worldPos.yz*_scale;
        uv_y=input.worldPos.xz*_scale;
        uv_z=input.worldPos.xy*_scale;
        blendWeights=pow(abs(WorldNormalVector(input,output.Normal)),_sharpness);
        blendWeights=blendWeights/(blendWeights.x+blendWeights.y+blendWeights.z);
        fixed4 c_x=fixed4(0,0,0,0);
        fixed4 c_y=fixed4(0,0,0,0);
        fixed4 c_z=fixed4(0,0,0,0);
        fixed4 b_x=fixed4(0,0,0,0);
        fixed4 b_y=fixed4(0,0,0,0);
        fixed4 b_z=fixed4(0,0,0,0);
        float index_r=input.uv0.x+_columns*input.uv0.y;
         sampledHeight height_r=sampleHeight(input.color.r,index_r,input.viewDir);
         sampledColorNBump colorNBump_r=sampleColorNBump(height_r.texOffset,input.color.r,index_r);
         c_x+=colorNBump_r.tex_axis_x;
         c_y+=colorNBump_r.tex_axis_y;
         c_z+=colorNBump_r.tex_axis_z;
         b_x+=colorNBump_r.bump_axis_x;
         b_y+=colorNBump_r.bump_axis_y;
         b_z+=colorNBump_r.bump_axis_z;
        //if(input.texcoord4.x>=0){
        // float index_texcoord4z=input.texcoord4.x+_columns*input.texcoord4.y;
        //  sampledHeight height_texcoord4z=sampleHeight(input.texcoord4.z,index_texcoord4z,input.viewDir);
        //  sampledColorNBump colorNBump_texcoord4z=sampleColorNBump(height_texcoord4z.texOffset,input.texcoord4.z,index_texcoord4z);
        //  c_x+=colorNBump_texcoord4z.tex_axis_x;
        //  c_y+=colorNBump_texcoord4z.tex_axis_y;
        //  c_z+=colorNBump_texcoord4z.tex_axis_z;
        //  b_x+=colorNBump_texcoord4z.bump_axis_x;
        //  b_y+=colorNBump_texcoord4z.bump_axis_y;
        //  b_z+=colorNBump_texcoord4z.bump_axis_z;
        //}
        if(input.uv1.x>=0){
         float index_g=input.uv1.x+_columns*input.uv1.y;
          sampledHeight height_g=sampleHeight(input.color.g,index_g,input.viewDir);
          sampledColorNBump colorNBump_g=sampleColorNBump(height_g.texOffset,input.color.g,index_g);
          c_x+=colorNBump_g.tex_axis_x;
          c_y+=colorNBump_g.tex_axis_y;
          c_z+=colorNBump_g.tex_axis_z;
          b_x+=colorNBump_g.bump_axis_x;
          b_y+=colorNBump_g.bump_axis_y;
          b_z+=colorNBump_g.bump_axis_z;
        }
        if(input.uv2.x>=0){
         float index_b=input.uv2.x+_columns*input.uv2.y;
          sampledHeight height_b=sampleHeight(input.color.b,index_b,input.viewDir);
          sampledColorNBump colorNBump_b=sampleColorNBump(height_b.texOffset,input.color.b,index_b);
          c_x+=colorNBump_b.tex_axis_x;
          c_y+=colorNBump_b.tex_axis_y;
          c_z+=colorNBump_b.tex_axis_z;
          b_x+=colorNBump_b.bump_axis_x;
          b_y+=colorNBump_b.bump_axis_y;
          b_z+=colorNBump_b.bump_axis_z;
        }
        if(input.uv3.x>=0){
         float index_a=input.uv3.x+_columns*input.uv3.y;
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
        output.Albedo=(c.rgb);
        output.Normal=UnpackNormal(b);
        float alpha=c.a;
        float viewDistance=length(_WorldSpaceCameraPos-input.worldPos);
        float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
        clip(opacity);
        alpha=alpha*saturate(opacity);
        output.Alpha=(alpha);
       }
       void applyFixedFog(frag_surf_surface input,SurfaceOutputStandard output,inout fixed4 color){
        float viewDistance=length(_WorldSpaceCameraPos-input.worldPos);
        UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
        if(unityFogFactor>0){
	        color.rgb=lerp(unity_FogColor.rgb,color.rgb,saturate(unityFogFactor));
        }
       }
       fixed4 frag_surf(v2f_surf input):SV_Target{
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        //  prepare and unpack data
        frag_surf_surface surfaceFunInput;
        #ifdef FOG_COMBINED_WITH_TSPACE
            UNITY_RECONSTRUCT_TBN(input);
        #else
            UNITY_EXTRACT_TBN(input);
        #endif
        UNITY_INITIALIZE_OUTPUT(frag_surf_surface,surfaceFunInput);
        surfaceFunInput.worldPos.x=1.0;
        surfaceFunInput.worldNormal.x=1.0;
        surfaceFunInput.pos.x=1.0;
        surfaceFunInput.viewDir.x=1.0;
        surfaceFunInput.color.x=1.0;
        surfaceFunInput.uv0.x=1.0;
        surfaceFunInput.uv1.x=1.0;
        surfaceFunInput.uv2.x=1.0;
        surfaceFunInput.uv3.x=1.0;
        surfaceFunInput.uv4.x=1.0;
        surfaceFunInput.uv5.x=1.0;
        surfaceFunInput.uv6.x=1.0;
        surfaceFunInput.uv7.x=1.0;
        surfaceFunInput.uv0=input.texcoord_pack_0_1.xy;
        surfaceFunInput.uv1=input.texcoord_pack_0_1.zw;
        surfaceFunInput.uv2=input.texcoord_pack_2_3.xy;
        surfaceFunInput.uv3=input.texcoord_pack_2_3.zw;
        float3 worldPos=float3(input.tSpace0.w,input.tSpace1.w,input.tSpace2.w);
        #ifndef USING_DIRECTIONAL_LIGHT
            fixed3 lightDir=normalize(UnityWorldSpaceLightDir(worldPos));
        #else
            fixed3 lightDir=_WorldSpaceLightPos0.xyz;
        #endif
        float3 worldViewDir=normalize(UnityWorldSpaceViewDir(worldPos));
        float3 viewDir=_unity_tbn_0*worldViewDir.x+_unity_tbn_1*worldViewDir.y+_unity_tbn_2*worldViewDir.z;
        surfaceFunInput.worldNormal=0.0;
        surfaceFunInput.internalSurfaceTtoW0=_unity_tbn_0;
        surfaceFunInput.internalSurfaceTtoW1=_unity_tbn_1;
        surfaceFunInput.internalSurfaceTtoW2=_unity_tbn_2;
        surfaceFunInput.worldPos=worldPos;
        surfaceFunInput.viewDir=viewDir;
        surfaceFunInput.color=input.color;
        #ifdef UNITY_COMPILER_HLSL
            SurfaceOutputStandard output=(SurfaceOutputStandard)0;
        #else
            SurfaceOutputStandard output;
        #endif
        output.Albedo=0.0;
        output.Emission=0.0;
        output.Alpha=0.0;
        output.Occlusion=1.0;
        fixed3 normalWorldVertex=fixed3(0,0,1);
        output.Normal=fixed3(0,0,1);
        //  surface function
        surface(surfaceFunInput,output);
        //  compute lighting & shadowing factor
        UNITY_LIGHT_ATTENUATION(atten,input,worldPos)
        fixed4 c=0;
        float3 worldN;
        worldN.x=dot(_unity_tbn_0,output.Normal);
        worldN.y=dot(_unity_tbn_1,output.Normal);
        worldN.z=dot(_unity_tbn_2,output.Normal);
        worldN=normalize(worldN);
        output.Normal=worldN;
        //  Setup lighting environment
        UnityGI gi;
        UNITY_INITIALIZE_OUTPUT(UnityGI,gi);
        gi.indirect.diffuse=0;
        gi.indirect.specular=0;
        gi.light.color=_LightColor0.rgb;
        gi.light.dir=lightDir;
        //  Call GI (lightmaps/SH/reflections) lighting function
        UnityGIInput giInput;
        UNITY_INITIALIZE_OUTPUT(UnityGIInput,giInput);
        giInput.light=gi.light;
        giInput.worldPos=worldPos;
        giInput.worldViewDir=worldViewDir;
        giInput.atten=atten;
        #if defined(LIGHTMAP_ON)||defined(DYNAMICLIGHTMAP_ON)
            giInput.lightmapUV=input.lmap;
        #else
            giInput.lightmapUV=0.0;
        #endif
        #if UNITY_SHOULD_SAMPLE_SH&&!UNITY_SAMPLE_FULL_SH_PER_PIXEL
            giInput.ambient=input.sh;
        #else
            giInput.ambient.rgb=0.0;
        #endif
        giInput.probeHDR[0]=unity_SpecCube0_HDR;
        giInput.probeHDR[1]=unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING)||defined(UNITY_SPECCUBE_BOX_PROJECTION)
            giInput.boxMin[0]=unity_SpecCube0_BoxMin;//  .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            giInput.boxMax[0]=unity_SpecCube0_BoxMax;
            giInput.probePosition[0]=unity_SpecCube0_ProbePosition;
            giInput.boxMax[1]=unity_SpecCube1_BoxMax;
            giInput.boxMin[1]=unity_SpecCube1_BoxMin;
            giInput.probePosition[1]=unity_SpecCube1_ProbePosition;
        #endif
        LightingStandard_GI(output,giInput,gi);
        //  realtime lighting: call lighting function
        c+=LightingStandard(output,worldViewDir,gi);
        applyFixedFog(surfaceFunInput,output,c);
        return c;
       }
   //#endif
   ENDCG
  }
 }
}