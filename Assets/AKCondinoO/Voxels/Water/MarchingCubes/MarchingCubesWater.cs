using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    //  marching cubes functions
    internal class MarchingCubesWater{
        internal struct VoxelWater{
         public double density;
         public double previousDensity;
         public bool sleeping;
            internal VoxelWater(double density,double previousDensity,bool sleeping){
             this.density=density;this.previousDensity=previousDensity;this.sleeping=sleeping;
            }
        }
    }
}