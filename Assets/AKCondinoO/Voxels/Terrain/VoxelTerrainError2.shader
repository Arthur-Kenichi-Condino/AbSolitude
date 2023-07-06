Shader"Voxels/VoxelTerrainError2"{
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
  LOD 200
  ZWrite On
  Blend SrcAlpha OneMinusSrcAlpha
  Pass{
   Name"FORWARD"
   Tags{"LightMode"="ForwardBase"}
   CGPROGRAM
    #pragma target 5.0
    #pragma require 2darray
    #pragma multi_compile_fog//  make fog work
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
    #pragma vertex VertexToFragment
    #pragma fragment FragmentToColor
    #include"UnityCG.cginc"
    #include"Lighting.cginc"
    #include"UnityPBSLighting.cginc"
    #include"AutoLight.cginc"
    #define INTERNAL_DATA half3 internalSurfaceTtoW0;half3 internalSurfaceTtoW1;half3 internalSurfaceTtoW2;
    #define WorldReflectionVector(data,normal)reflect(data.worldRefl,half3(dot(data.internalSurfaceTtoW0,normal),dot(data.internalSurfaceTtoW1,normal),dot(data.internalSurfaceTtoW2,normal)))
    #define WorldNormalVector(data,normal)fixed3(dot(data.internalSurfaceTtoW0,normal),dot(data.internalSurfaceTtoW1,normal),dot(data.internalSurfaceTtoW2,normal))
    UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_INSTANCING_BUFFER_END  (Props)
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
        UNITY_POSITION(pos);
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
        UNITY_SHADOW_COORDS(11)
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
       };
       FragmentData VertexToFragment(AppVertexData v){
        UNITY_SETUP_INSTANCE_ID(v);
        FragmentData o;
        UNITY_INITIALIZE_OUTPUT(FragmentData,o);
        UNITY_TRANSFER_INSTANCE_ID(v,o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
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
        float3 worldNormal=UnityObjectToWorldNormal(v.normal);
        fixed3 worldTangent=UnityObjectToWorldDir(v.tangent.xyz);
        fixed tangentSign=v.tangent.w*unity_WorldTransformParams.w;
        fixed3 worldBinormal=cross(worldNormal,worldTangent)*tangentSign;
        o.tSpace0=float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
        o.tSpace1=float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
        o.tSpace2=float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
        o.color=v.color;
        return o;
       }
       fixed4 FragmentToColor(FragmentData i):SV_Target{
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        UNITY_EXTRACT_TBN(i);
        float3 worldPos=float3(i.tSpace0.w,i.tSpace1.w,i.tSpace2.w);
        fixed3 lightDir=normalize(UnityWorldSpaceLightDir(worldPos));
        float3 worldViewDir=normalize(UnityWorldSpaceViewDir(worldPos));
        float3 viewDir=_unity_tbn_0*worldViewDir.x+_unity_tbn_1*worldViewDir.y+_unity_tbn_2*worldViewDir.z;
        SurfaceOutputStandard o;
        o.Albedo=0.0;
        o.Emission=0.0;
        o.Alpha=0.0;
        o.Occlusion=1.0;
        fixed3 normalWorldVertex=fixed3(0,0,1);
        o.Normal=fixed3(0,0,1);
        o.Emission=0.0;
        o.Occlusion=1.0;
        o.Albedo=fixed3(.5,.5,.5);
        o.Normal=fixed3(0,0,1);
        o.Alpha=1.0;
        UNITY_LIGHT_ATTENUATION(atten,i,worldPos)
        fixed4 c=0;
        float3 worldN;
        worldN.x=dot(_unity_tbn_0,o.Normal);
        worldN.y=dot(_unity_tbn_1,o.Normal);
        worldN.z=dot(_unity_tbn_2,o.Normal);
        worldN=normalize(worldN);
        o.Normal=worldN;
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
    giInput.lightmapUV = 0.0;
    giInput.ambient.rgb = 0.0;
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  LightingStandard_GI(o, giInput, gi);
  //c += LightingStandard (o, worldViewDir, gi);
        return c;
       }
   ENDCG
  }
 }
}