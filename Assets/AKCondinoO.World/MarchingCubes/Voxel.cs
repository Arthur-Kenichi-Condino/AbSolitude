using UnityEngine;
namespace AKCondinoO.World.MarchingCubes{
    internal struct Voxel{
     internal bool isCreated;
     internal float density;
     internal Vector3 normal;
        internal Voxel(float d,Vector3 n){
         density=d;normal=n;isCreated=true;
        }
     internal static Voxel air    {get;}=new Voxel(  0.0f,Vector3.zero);
     internal static Voxel bedrock{get;}=new Voxel(100.0f,Vector3.zero);
    }
}