#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Terrain;
using AKCondinoO.Voxels.Water.MarchingCubes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        void OnDrawGizmos(){
         #if UNITY_EDITOR
          DrawVoxelsDensity();
         #endif
        }
        VoxelWater?[]DEBUG_DRAW_WATER_DENSITY_VOXELS=null;
        #if UNITY_EDITOR
        void DrawVoxelsDensity(){
         if(tCnk!=null&&tCnk.DEBUG_DRAW_WATER_DENSITY&&tCnk.id!=null){
          if(VoxelSystem.Concurrent.waterCache_rwl.TryEnterReadLock(0)){
           try{
            if(!VoxelSystem.Concurrent.waterCache.TryGetValue(tCnk.id.Value.cnkIdx,out var cache)){
             return;
            }
            if(DEBUG_DRAW_WATER_DENSITY_VOXELS==null){
             DEBUG_DRAW_WATER_DENSITY_VOXELS=new VoxelWater?[VoxelsPerChunk];
            }
            cache.stream.Position=0L;
            while(cache.reader.BaseStream.Position!=cache.reader.BaseStream.Length){
             var v=WaterSpreadingMultithreaded.BinaryReadVoxelWater(cache.reader);
             DEBUG_DRAW_WATER_DENSITY_VOXELS[v.vxlIdx]=v.voxel;
            }
           }catch{
            throw;
           }finally{
            VoxelSystem.Concurrent.waterCache_rwl.ExitReadLock();
           }
          }
          Vector3Int vCoord1;
          for(vCoord1=new Vector3Int();vCoord1.y<Height;vCoord1.y++){
          for(vCoord1.x=0             ;vCoord1.x<Width ;vCoord1.x++){
          for(vCoord1.z=0             ;vCoord1.z<Depth ;vCoord1.z++){
           int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
           VoxelWater voxel;
           lock(DEBUG_DRAW_WATER_DENSITY_VOXELS){
            if(DEBUG_DRAW_WATER_DENSITY_VOXELS[vxlIdx1]!=null){
             voxel=DEBUG_DRAW_WATER_DENSITY_VOXELS[vxlIdx1].Value;
            }else{
             continue;
            }
           }
           double density=voxel.density;
           if(density==0d){
            continue;
           }
           //Log.DebugMessage("density:"+density);
           if(-density<MarchingCubesWater.isoLevel){
            Gizmos.color=Color.white;
           }else{
            Gizmos.color=Color.black;
           }
           Vector3 center=new Vector3(
             tCnk.id.Value.cnkRgn.x-Mathf.FloorToInt(Width/2.0f),
             -Mathf.FloorToInt(Height/2.0f),
             tCnk.id.Value.cnkRgn.y-Mathf.FloorToInt(Depth/2.0f)
            )+vCoord1;
           Gizmos.DrawCube(center,Vector3.one*(float)(density*.01d));
          }}}
         }
        }
        #endif
    }
}