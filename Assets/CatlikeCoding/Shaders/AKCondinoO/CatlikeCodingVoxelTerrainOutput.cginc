#include"CatlikeCodingVoxelTerrainInput.cginc"
    struct SurfaceParameters{
     float3 normal,position;
    };
    struct SurfaceData{
     float3 albedo,emission,normal;
     float alpha,metallic,occlusion,smoothness;
    };
    struct FragmentOutput{
     float4 color:SV_TARGET;
    };