using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.MarchingCubes{
    //  marching cubes functions
    internal class MarchingCubesWater{
        internal struct VoxelWater{
         public double density;
         public double absorbing;public bool absorbingB,absorbingT,absorbingW,absorbingE,absorbingS,absorbingN;
         public double spreading;public bool spreadingB,spreadingT,spreadingW,spreadingE,spreadingS,spreadingN;
         public bool sleeping{
          get{
           return!absorbingB&&!absorbingT&&!absorbingW&&!absorbingE&&!absorbingS&&!absorbingN&&
                 !spreadingB&&!spreadingT&&!spreadingW&&!spreadingE&&!spreadingS&&!spreadingN;
          }
         }
        }
    }
}