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
     TRANSFER_SHADOW(i);
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
     v.tangent.xyz=normalize(v.tangent.xyz);
     v.normal=normalize(v.normal);
     float3x3 objectToTangent=float3x3(
      v.tangent.xyz,
      cross(v.normal,v.tangent.xyz)*v.tangent.w,
      v.normal
     );
     i.tangentViewDir=mul(objectToTangent,ObjSpaceViewDir(v.vertex));
     return i;
    }
    FragmentOutput FragmentToColorProgram(Interpolators i){
     UNITY_SETUP_INSTANCE_ID(i);
     FragmentOutput output;
     output.color=float4(1,1,1,1);
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
 
 
 
	float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);

	float4 mohsX = 0;//UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.x),3.0));
	float4 mohsY = 0;//UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.y),3.0));
	float4 mohsZ = 0;//UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.z),3.0));
	//float3 triW = GetTriplanarWeights(parameters, 0, 0, 0);
	float3 triW = GetTriplanarWeights(parameters, mohsX.z, mohsY.z, mohsZ.z);
 
 
 UNITY_EXTRACT_TBN(i);
        float3 heightWorldPos=float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
        float3 heightWorldViewDir=normalize(UnityWorldSpaceViewDir(heightWorldPos));
 float3 heightViewDir;
 heightViewDir.x=1.0;
        heightViewDir=_unity_tbn_0*heightWorldViewDir.x+_unity_tbn_1*heightWorldViewDir.y+_unity_tbn_2*heightWorldViewDir.z;
        fixed4 heightX=UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.x),3.0));
        fixed4 heightY=UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.y),3.0));
        fixed4 heightZ=UNITY_SAMPLE_TEX2DARRAY(_heights,float3(frac(triUV.z),3.0));
        fixed4 h=(heightX)*triW.x
                +(heightY)*triW.y
                +(heightZ)*triW.z;
               float2 texOffset=ParallaxOffset(h.r,_heightDistortion,heightViewDir);
 
 
 
	float3 albedoX = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.x)+texOffset,3.0)).rgb;
	float3 albedoY = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.y)+texOffset,3.0)).rgb;
	float3 albedoZ = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.z)+texOffset,3.0)).rgb;
 
	float3 tangentNormalX = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_bumps, float3(frac(triUV.x)+texOffset,3.0)));
	float4 rawNormalY = UNITY_SAMPLE_TEX2DARRAY(_bumps, float3(frac(triUV.y)+texOffset,3.0));
	float3 tangentNormalZ = UnpackNormal(UNITY_SAMPLE_TEX2DARRAY(_bumps, float3(frac(triUV.z)+texOffset,3.0)));
 
	float3 tangentNormalY = UnpackNormal(rawNormalY);
 
	if (parameters.normal.x < 0) {
		tangentNormalX.x = -tangentNormalX.x;
	}
	if (parameters.normal.y < 0) {
		tangentNormalY.x = -tangentNormalY.x;
	}
	if (parameters.normal.z >= 0) {
		tangentNormalZ.x = -tangentNormalZ.x;
	}
 
	float3 worldNormalX =
		BlendTriplanarNormal(tangentNormalX, parameters.normal.zyx).zyx;
	float3 worldNormalY =
		BlendTriplanarNormal(tangentNormalY, parameters.normal.xzy).xzy;
	float3 worldNormalZ =
		BlendTriplanarNormal(tangentNormalZ, parameters.normal);

	//float3 triW = GetTriplanarWeights(parameters, mohsX.z, mohsY.z, mohsZ.z);
 
	surface.albedo = albedoX * triW.x + albedoY * triW.y + albedoZ * triW.z;

	float4 mohs = mohsX * triW.x + mohsY * triW.y + mohsZ * triW.z;
	surface.metallic = mohs.x;
	surface.occlusion = mohs.y;
	surface.smoothness = mohs.a;

	surface.normal = normalize(
		worldNormalX * triW.x + worldNormalY * triW.y + worldNormalZ * triW.z
	);
 
 
 
	i.normal = surface.normal;
 
	float alpha = surface.alpha;
		clip(alpha - 0);
 
 
	float3 specularTint;
	float oneMinusReflectivity;
	float3 albedo = DiffuseAndSpecularFromMetallic(
		surface.albedo, surface.metallic, specularTint, oneMinusReflectivity
	);
		albedo *= alpha;
		alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
 
 
 
	float4 color = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, surface.smoothness,
		i.normal, viewDir,
		CreateLight(i), CreateIndirectLight(i, viewDir, surface)
	);
	color.rgb += surface.emission;
		color.a = alpha;
 //       float3 worldPos=float3(
 //        i.tSpace0.w,
 //        i.tSpace1.w,
 //        i.tSpace2.w
 //       );
 //    half2 sample_x=worldPos.yz*_scale;
 //    half2 sample_y=worldPos.xz*_scale;
 //    half2 sample_z=worldPos.xy*_scale;
 //       fixed4 c_x=fixed4(0,0,0,0);
 //       fixed4 c_y=fixed4(0,0,0,0);
 //       fixed4 c_z=fixed4(0,0,0,0);
 ////fixed4 bump_axis_x=UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(sample_x),1.0));
 ////fixed4 bump_axis_y=UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(sample_y),1.0));
 ////fixed4 bump_axis_z=UNITY_SAMPLE_TEX2DARRAY(_bumps,float3(frac(sample_z),1.0));
	//c_x = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.x),1.0));//tex2D(_MainTex, triUV.x).rgb;
	//c_y = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.y),1.0));//tex2D(_MainTex, triUV.y).rgb;
	//c_z = UNITY_SAMPLE_TEX2DARRAY(_albedos,float3(frac(triUV.z),1.0));;//tex2D(_MainTex, triUV.z).rgb;
 ////       half3 blendingWeights=pow(abs(i.vNormal),_sharpness);
 ////             blendingWeights=blendingWeights/(blendingWeights.x+blendingWeights.y+blendingWeights.z);
 //       fixed4 c=(c_x)*triW.x
 //               +(c_y)*triW.y
 //               +(c_z)*triW.z;
 output.color=color;
 
		//output.color = ApplyFog(color, i);
 
 
     return output;
    }