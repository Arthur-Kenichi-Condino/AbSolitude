using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal class VoxelTerrainSurfaceSimObjectsPlacer{
     readonly VoxelTerrainSimObjectsPlacing simObjectsPlacing;
     internal VoxelTerrainSurfaceSimObjectsPlacerContainer surfaceSimObjectsPlacerBG=new VoxelTerrainSurfaceSimObjectsPlacerContainer();
        internal VoxelTerrainSurfaceSimObjectsPlacer(VoxelTerrainSimObjectsPlacing simObjectsPlacing){
         this.simObjectsPlacing=simObjectsPlacing;
        }
     internal bool isBusy;
     bool settingGetGroundRays;
        internal void OnAddingSurfaceSimObjects(){
            if(settingGetGroundRays){
                if(isBusy){
                   isBusy=false;
                }
            }else{
                surfaceSimObjectsPlacerBG.GetGroundRays.Clear();
                surfaceSimObjectsPlacerBG.GetGroundHits.Clear();
                surfaceSimObjectsPlacerBG.gotGroundHits.Clear();
                //addSimObjectsBG.cCoord=cCoord;
                //addSimObjectsBG.cnkRgn=cnkRgn;
                //addSimObjectsBG.cnkIdx=cnkIdx.Value;
                settingGetGroundRays=true;
            }
        }
    }
}
