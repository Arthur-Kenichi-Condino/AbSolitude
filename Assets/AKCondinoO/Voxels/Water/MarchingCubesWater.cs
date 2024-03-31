using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    //  marching cubes functions
    internal class MarchingCubesWater{
     internal const double isoLevel=-50.0d;
        internal struct VoxelWater{
         internal double density;
         internal double previousDensity;
         internal bool sleeping;
         internal float evaporateAfter;
         internal Vector3 normal;
         internal bool isCreated;
            internal VoxelWater(double density,double previousDensity,bool sleeping):this(density,previousDensity,sleeping,-1f){
            }
            internal VoxelWater(double density,double previousDensity,bool sleeping,float evaporateAfter){
             this.density=density;this.previousDensity=previousDensity;this.sleeping=sleeping;this.evaporateAfter=evaporateAfter;normal=Vector3.zero;isCreated=true;
            }
         internal static VoxelWater air    {get;}=new VoxelWater(0.0,0.0,true);
         internal static VoxelWater bedrock{get;}=new VoxelWater(0.0,0.0,true);
        }
    }
}