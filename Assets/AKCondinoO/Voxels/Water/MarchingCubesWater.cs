using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    //  marching cubes functions
    internal class MarchingCubesWater{
     internal const double isoLevel=-50.0d;
        internal struct VoxelWater{
         public double density;
         public double previousDensity;
         public bool sleeping;
         public float evaporateAfter;
            internal VoxelWater(double density,double previousDensity,bool sleeping):this(density,previousDensity,sleeping,-1f){
            }
            internal VoxelWater(double density,double previousDensity,bool sleeping,float evaporateAfter){
             this.density=density;this.previousDensity=previousDensity;this.sleeping=sleeping;this.evaporateAfter=evaporateAfter;
            }
        }
    }
}