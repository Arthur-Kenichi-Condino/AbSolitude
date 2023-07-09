#include"CatlikeCodingVoxelTerrainInput.cginc"
    struct SurfaceData{
     float3 albedo,emission,normal;
     float alpha,metallic,occlusion,smoothness;
    };
    struct SurfaceParameters{
     float3 normal,position;
    };