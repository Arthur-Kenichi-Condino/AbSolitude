#include"CatlikeCodingVoxelTerrainUtil.cginc"
    InterpolatorsVertex VertexToFragmentProgram(VertexData v){
     InterpolatorsVertex i;
     UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex,i);
     UNITY_SETUP_INSTANCE_ID(v);
     UNITY_TRANSFER_INSTANCE_ID(v,i);
     i.pos=UnityObjectToClipPos(v.vertex);
     i.worldPos.xyz=mul(unity_ObjectToWorld,v.vertex);
     i.worldPos.w=i.pos.z;
     i.normal=UnityObjectToWorldNormal(v.normal);
     i.tangent=float4(UnityObjectToWorldDir(v.tangent.xyz),v.tangent.w);
     i.binormal=DoBinormal(i.normal,i.tangent.xyz,v.tangent.w);
     UNITY_TRANSFER_SHADOW(i,float2(0,0));
     ComputeVertexLightColor(i);
     i.color=v.color;
     i.uv0=v.texCoord0;
     i.uv1=v.texCoord1;
     i.uv2=v.texCoord2;
     i.uv3=v.texCoord3;
     i.uv4=v.texCoord4;
     i.uv5=v.texCoord5;
     i.uv6=v.texCoord6;
     i.uv7=v.texCoord7;
     i.vNormal=v.normal;
     i.tSpace0=float4(i.tangent.x,i.binormal.x,i.normal.x,i.worldPos.x);
     i.tSpace1=float4(i.tangent.y,i.binormal.y,i.normal.y,i.worldPos.y);
     i.tSpace2=float4(i.tangent.z,i.binormal.z,i.normal.z,i.worldPos.z);
     float3x3 objectToTangent=float3x3(
      v.tangent.xyz,
      cross(v.normal,v.tangent.xyz)*v.tangent.w,
      v.normal
     );
     i.tangentViewDir=mul(objectToTangent,ObjSpaceViewDir(v.vertex));
     return i;
    }
    float FadeShadows(Interpolators i,float attenuation){
     return attenuation;
    }
    UnityLight CreateLight(Interpolators i){
     UnityLight light;
        #if defined(POINT)||defined(POINT_COOKIE)||defined(SPOT)
         light.dir=normalize(_WorldSpaceLightPos0.xyz-i.worldPos.xyz);
        #else
         light.dir=_WorldSpaceLightPos0.xyz;
        #endif
     UNITY_LIGHT_ATTENUATION(attenuation,i,i.worldPos.xyz);
     attenuation=FadeShadows(i,attenuation);
     light.color=_LightColor0.rgb*attenuation;
     return light;
    }

float3 BoxProjection (
	float3 direction, float3 position,
	float4 cubemapPosition, float3 boxMin, float3 boxMax
) {
	#if UNITY_SPECCUBE_BOX_PROJECTION
		UNITY_BRANCH
		if (cubemapPosition.w > 0) {
			float3 factors =
				((direction > 0 ? boxMax : boxMin) - position) / direction;
			float scalar = min(min(factors.x, factors.y), factors.z);
			direction = direction * scalar + (position - cubemapPosition);
		}
	#endif
	return direction;
}

    UnityIndirect CreateIndirectLight(Interpolators i,float3 viewDir,SurfaceData surface){
     UnityIndirect indirectLight;
     indirectLight.diffuse=0;
     indirectLight.specular=0;
        #if defined(VERTEXLIGHT_ON)
         indirectLight.diffuse=i.vertexLightColor;
        #endif
        #if defined(FORWARD_BASE_PASS)
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
             if(unity_ProbeVolumeParams.x==1){
              indirectLight.diffuse=SHEvalLinearL0L1_SampleProbeVolume(
               float4(i.normal,1),i.worldPos
              );
              indirectLight.diffuse=max(0,indirectLight.diffuse);
                 #if defined(UNITY_COLORSPACE_GAMMA)
                  indirectLight.diffuse=LinearToGammaSpace(indirectLight.diffuse);
                 #endif
             }else{
              indirectLight.diffuse+=max(0,ShadeSH9(float4(i.normal,1)));
             }
            #else
             indirectLight.diffuse+=max(0,ShadeSH9(float4(i.normal,1)));
            #endif
         float3 reflectionDir=reflect(-viewDir,i.normal);
         Unity_GlossyEnvironmentData envData;
         envData.roughness=1-surface.smoothness;
         envData.reflUVW=BoxProjection(
          reflectionDir,
          i.worldPos.xyz,
          unity_SpecCube0_ProbePosition,
          unity_SpecCube0_BoxMin,
          unity_SpecCube0_BoxMax
         );
         float3 probe0=Unity_GlossyEnvironment(
          UNITY_PASS_TEXCUBE(unity_SpecCube0),
          unity_SpecCube0_HDR,
          envData
         );
         envData.reflUVW=BoxProjection(
          reflectionDir,
          i.worldPos.xyz,
          unity_SpecCube1_ProbePosition,
          unity_SpecCube1_BoxMin,
          unity_SpecCube1_BoxMax
         );
            #if UNITY_SPECCUBE_BLENDING
             float interpolator=unity_SpecCube0_BoxMin.w;
             UNITY_BRANCH
             if(interpolator<0.99999){
              float3 probe1=Unity_GlossyEnvironment(
               UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0),
               unity_SpecCube0_HDR,
               envData
              );
              indirectLight.specular=lerp(probe1,probe0,interpolator);
             }else{
              indirectLight.specular=probe0;
             }
            #else
             indirectLight.specular=probe0;
            #endif
         float occlusion=surface.occlusion;
         indirectLight.diffuse*=occlusion;
         indirectLight.specular*=occlusion;
        #endif
     return indirectLight;
    }
    float4 ApplyFog(float4 color,Interpolators i){
     //#if FOG_ON
      float viewDistance=length(_WorldSpaceCameraPos-i.worldPos.xyz);
         //#if FOG_DEPTH
          viewDistance=UNITY_Z_0_FAR_FROM_CLIPSPACE(i.worldPos.w);
         //#endif
      UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
      float3 fogColor=0;
         #if defined(FORWARD_BASE_PASS)
          fogColor=unity_FogColor.rgb;
         #endif
      color.rgb=lerp(fogColor,color.rgb,saturate(unityFogFactor));
     //#endif
     return color;
    }
    void AddSample(float x,float y,float strenght,TriplanarUV triUV,float3 triW,float3 tViewDir,
     inout float3 albedoX,
     inout float3 albedoY,
     inout float3 albedoZ,
     inout float4 bumpX,
     inout float4 bumpY,
     inout float4 bumpZ
    ){
     float textureIndex=x+_columns*y;
     fixed4 heightX=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.x),textureIndex));
     fixed4 heightY=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.y),textureIndex));
     fixed4 heightZ=strenght*UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.z),textureIndex));
     fixed4 h=heightX*triW.x+
              heightY*triW.y+
              heightZ*triW.z;
     float2 texOffset=ParallaxOffset(h.r,_heightDistortion,tViewDir);
     albedoX+=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.x)+texOffset,textureIndex)).rgb;
     albedoY+=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.y)+texOffset,textureIndex)).rgb;
     albedoZ+=strenght*UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.z)+texOffset,textureIndex)).rgb;
     bumpX  +=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(triUV.x)+texOffset,textureIndex));
     bumpY  +=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(triUV.y)+texOffset,textureIndex));
     bumpZ  +=strenght*UNITY_SAMPLE_TEX2DARRAY(_bumps  ,float3(frac(triUV.z)+texOffset,textureIndex));
    }
    float4 FragmentToColorProgram(Interpolators i):SV_Target{
     UNITY_SETUP_INSTANCE_ID(i);
     UNITY_EXTRACT_TBN(i);
     InitializeFragmentNormal(i);
     float3 viewDir=normalize(_WorldSpaceCameraPos-i.worldPos.xyz);
     SurfaceData surface;
     surface.normal=i.normal;
     surface.albedo=1;
     surface.alpha=1;
     surface.emission=0;
     surface.metallic=0;
     surface.occlusion=1;
     surface.smoothness=0.5;
     SurfaceParameters parameters;
     parameters.normal=i.normal;
     parameters.position=i.worldPos.xyz;
     TriplanarUV triUV=GetTriplanarUV(parameters);
     float4 mohsX=0;
     float4 mohsY=0;
     float4 mohsZ=0;
     float3 triW=GetTriplanarWeights(parameters,mohsX.z,mohsY.z,mohsZ.z);
     float3 tWorldPos=float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
     float3 tWorldViewDir=normalize(UnityWorldSpaceViewDir(tWorldPos));
     float3 tViewDir;
     tViewDir.x=1.0;
     tViewDir=_unity_tbn_0*tWorldViewDir.x+_unity_tbn_1*tWorldViewDir.y+_unity_tbn_2*tWorldViewDir.z;
     float3 albedoX=0;
     float3 albedoY=0;
     float3 albedoZ=0;
     float4 bumpX=0;
     float4 bumpY=0;
     float4 bumpZ=0;
     AddSample(i.uv0.x,i.uv0.y,i.uv6.x,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv0.z,i.uv0.w,i.uv6.y,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv1.x,i.uv1.y,i.uv6.z,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv1.z,i.uv1.w,i.uv6.w,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv2.x,i.uv2.y,i.uv7.x,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv2.z,i.uv2.w,i.uv7.y,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv3.x,i.uv3.y,i.uv7.z,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     AddSample(i.uv3.z,i.uv3.w,i.uv7.w,triUV,triW,tViewDir,albedoX,albedoY,albedoZ,bumpX,bumpY,bumpZ);
     float3 tangentNormalX=UnpackNormal(bumpX);
     float4 rawNormalY=bumpY;
     float3 tangentNormalZ=UnpackNormal(bumpZ);
     float3 tangentNormalY=UnpackNormal(rawNormalY);
     if(parameters.normal.x<0 ){tangentNormalX.x=-tangentNormalX.x;}
     if(parameters.normal.y<0 ){tangentNormalY.x=-tangentNormalY.x;}
     if(parameters.normal.z>=0){tangentNormalZ.x=-tangentNormalZ.x;}
     float3 worldNormalX=BlendTriplanarNormal(tangentNormalX,parameters.normal.zyx).zyx;
     float3 worldNormalY=BlendTriplanarNormal(tangentNormalY,parameters.normal.xzy).xzy;
     float3 worldNormalZ=BlendTriplanarNormal(tangentNormalZ,parameters.normal);
     surface.albedo=albedoX*triW.x+
                    albedoY*triW.y+
                    albedoZ*triW.z;
     surface.normal=normalize(
      worldNormalX*triW.x+
      worldNormalY*triW.y+
      worldNormalZ*triW.z
     );
     i.normal=surface.normal;
     float viewDistance=distance(_WorldSpaceCameraPos,i.worldPos);
     float opacity=(_fadeEndDis-viewDistance)/(_fadeEndDis-_fadeStartDis);
     clip(opacity);
     float alpha=surface.alpha;
     alpha=alpha*saturate(opacity);
     clip(alpha-_Cutoff);
     float3 specularTint;
     float oneMinusReflectivity;
     float3 albedo=DiffuseAndSpecularFromMetallic(
      surface.albedo,
      surface.metallic,
      specularTint,
      oneMinusReflectivity
     );
     albedo*=alpha;
     alpha=1-oneMinusReflectivity+alpha*oneMinusReflectivity;
     float4 color=UNITY_BRDF_PBS(
      albedo,
      specularTint,
      oneMinusReflectivity,
      surface.smoothness,
      i.normal,
      viewDir,
      CreateLight(i),
      CreateIndirectLight(i,viewDir,surface)
     );
     color.rgb+=surface.emission;
     color.a=alpha;
     color=ApplyFog(color,i);
     return color;
    }