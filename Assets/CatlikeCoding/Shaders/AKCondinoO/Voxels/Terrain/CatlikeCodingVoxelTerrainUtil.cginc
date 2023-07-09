#include"CatlikeCodingVoxelTerrainOutput.cginc"
    float3 DoBinormal(float3 normal,float3 tangent,float binormalSign){
     return cross(normal,tangent.xyz)*(binormalSign*unity_WorldTransformParams.w);
    }
    void ComputeVertexLightColor(inout InterpolatorsVertex i){
     i.vertexLightColor=Shade4PointLights(
      unity_4LightPosX0,unity_4LightPosY0,unity_4LightPosZ0,
      unity_LightColor[0].rgb,unity_LightColor[1].rgb,
      unity_LightColor[2].rgb,unity_LightColor[3].rgb,
      unity_4LightAtten0,i.worldPos.xyz,i.normal
     );
    }
    void InitializeFragmentNormal(inout Interpolators i){
     i.normal=normalize(i.normal);
    }
    struct TriplanarUV{
     float2 x,y,z;
    };
    TriplanarUV GetTriplanarUV(SurfaceParameters parameters){
     TriplanarUV triUV;
     float3 p=parameters.position*_scale;
     triUV.x=p.zy;
     triUV.y=p.xz;
     triUV.z=p.xy;
     if(parameters.normal.x<0 ){triUV.x.x=-triUV.x.x;}
     if(parameters.normal.y<0 ){triUV.y.x=-triUV.y.x;}
     if(parameters.normal.z>=0){triUV.z.x=-triUV.z.x;}
     triUV.x.y+=0.5;
     triUV.z.x+=0.5;
     return triUV;
    }
float3 GetTriplanarWeights (
	SurfaceParameters parameters, float heightX, float heightY, float heightZ
) {
	float3 triW = abs(parameters.normal);
	triW = saturate(triW - .5);
	triW *= lerp(1, float3(heightX, heightY, heightZ), .5);
	triW = pow(triW, 8);
	return triW / (triW.x + triW.y + triW.z);
}
float3 BlendTriplanarNormal (float3 mappedNormal, float3 surfaceNormal) {
	float3 n;
	n.xy = mappedNormal.xy + surfaceNormal.xy;
	n.z = mappedNormal.z * surfaceNormal.z;
	return n;
}