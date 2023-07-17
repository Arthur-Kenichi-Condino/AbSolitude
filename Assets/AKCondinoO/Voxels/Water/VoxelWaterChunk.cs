#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Water{
    internal class VoxelWaterChunk:MonoBehaviour{
     internal VoxelTerrainChunk tCnk;
     internal WaterSpreadingContainer waterSpreadingBG=new WaterSpreadingContainer();
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         waterSpreadingBG.IsCompleted(VoxelSystem.singleton.waterSpreadingBGThreads[0].IsRunning,-1);
        }
     [SerializeField]float spreadTimeInterval=1.0f;
     float spreadTimer=1.0f;
     bool waitingMarchingCubes;
     bool pendingMarchingCubes;
     bool waitingWaterSpread;
        internal void ManualUpdate(){
         //Log.DebugMessage("ManualUpdate");
         if(waitingWaterSpread){
             if(OnWaterSpread()){
                 waitingWaterSpread=false;
                 pendingMarchingCubes=true;
             }
         }else{
             if(CanSpreadWater()){
                 waitingWaterSpread=true;
             }
         }
        }
        bool CanSpreadWater(){
         spreadTimer-=Time.deltaTime;
         if(spreadTimer<=0f){
          //Log.DebugMessage("CanSpreadWater");
          spreadTimer=spreadTimeInterval;
          waterSpreadingBG.cCoord=tCnk.id.Value.cCoord;
          waterSpreadingBG.cnkRgn=tCnk.id.Value.cnkRgn;
          waterSpreadingBG.cnkIdx=tCnk.id.Value.cnkIdx;
          WaterSpreadingMultithreaded.Schedule(waterSpreadingBG);
          return true;
         }
         return false;
        }
        bool OnWaterSpread(){
         if(waterSpreadingBG.IsCompleted(VoxelSystem.singleton.waterSpreadingBGThreads[0].IsRunning)){
          //Log.DebugMessage("OnWaterSpread");
          return true;
         }
         return false;
        }
    }
}