using UnityEngine;
namespace AKCondinoO.World.MarchingCubes{
    internal struct Voxel{
     internal bool isCreated;
     internal float density;
     internal MaterialId material;
     internal Vector3 normal;
        internal Voxel(float d,MaterialId m,Vector3 n){
         density=d;material=m;normal=n;isCreated=true;
        }
     internal static Voxel air    {get;}=new Voxel(  0.0f,MaterialId.AirNone    ,Vector3.zero);
     internal static Voxel bedrock{get;}=new Voxel(100.0f,MaterialId.EdgeBedrock,Vector3.zero);
    }
}