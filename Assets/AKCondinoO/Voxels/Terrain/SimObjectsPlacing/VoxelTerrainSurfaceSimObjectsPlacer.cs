using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal class VoxelTerrainSurfaceSimObjectsPlacer{
     readonly VoxelTerrainSimObjectsPlacing simObjectsPlacing;
        internal VoxelTerrainSurfaceSimObjectsPlacer(VoxelTerrainSimObjectsPlacing simObjectsPlacing){
         this.simObjectsPlacing=simObjectsPlacing;
        }
     internal bool isBusy;
     bool settingGetGroundRays;
      bool setGetGroundRays;
        internal void OnAddingSurfaceSimObjects(){
            if(isBusy){
               isBusy=false;
            }
        }
    }
}
