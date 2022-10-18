using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water{
    //  marching cubes functions
    internal class MarchingCubesWater{
        internal struct VoxelWater{
         public double density;
         public double spreading;
         public double absorbing;
         public bool sleeping{
          get{
           return spreading==0d&&absorbing==0d;
          }
         }
        }
    }
}