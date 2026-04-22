using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Spawning{
    internal class BiomesSimObjectSpawningSystem{
        internal void DoBiomeSpawnJobFor(Vector2Int cCoord){
         var doBiomeSpawnJob=BiomesSimObjectSpawnJob.pool.Rent();
         doBiomeSpawnJob.cCoord=cCoord;
         bool scheduled=ThreadDispatcher.TrySchedule(doBiomeSpawnJob,7);
         if(!scheduled){
          BiomesSimObjectSpawnJob.pool.Return(doBiomeSpawnJob);
          doBiomeSpawnJob=null;
         }
        }
        internal class BiomesSimObjectSpawnJob:MultithreadedContainerJob{
         internal static readonly Utilities.ObjectPool<BiomesSimObjectSpawnJob>pool=
          Pool.GetPool<BiomesSimObjectSpawnJob>(
           "",
           ()=>new(),
           (BiomesSimObjectSpawnJob item)=>{
            item.debugSpawnCoords.Clear();
           }
          );
         internal Vector2Int cCoord;
         internal readonly HashSet<Vector3>debugSpawnCoords=new();
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
            }
         private Vector2Int cnkRgn;
            public void ExecuteAtBackgroundThread(){
             BiomesConfigurationSnapshot.IsReading();
             try{
              var settings=BiomesConfigurationSnapshot.GetSpawnSettings();
              int minLayer=settings.minLayer;
              int maxLayer=settings.maxLayer;
              Logs.Debug(()=>"minLayer:"+minLayer+";maxLayer:"+maxLayer);
              foreach(var kvp in settings.layerData){
               int layer=kvp.Key;
               var spawnLayerData=kvp.Value;
               int gridSize=spawnLayerData.gridSize;
               Vector3 maxBoundsSize=spawnLayerData.maxBoundsSize;
               Vector3 halfBounds=maxBoundsSize/2f;
               Logs.Debug(()=>"gridSize:"+gridSize);
               cnkRgn=cCoordTocnkRgn(cCoord);
               Vector3Int worldMin=new(
                cnkRgn.x-Width/2,0,
                cnkRgn.y-Depth/2
               );
               Vector3Int worldMax=new(
                worldMin.x+Width,Height-1,
                worldMin.z+Depth
               );
               Vector3Int start=new(
                AlignDown(worldMin.x-Mathf.CeilToInt(halfBounds.x)),worldMin.y,
                AlignDown(worldMin.z-Mathf.CeilToInt(halfBounds.z))
               );
                  int AlignDown(int value){
                   return Mathf.FloorToInt((float)value/gridSize)*gridSize;
                  }
               Vector3Int end=new(
                AlignUp(worldMax.x+Mathf.CeilToInt(halfBounds.x)),worldMax.y,
                AlignUp(worldMax.z+Mathf.CeilToInt(halfBounds.z))
               );
                  int AlignUp(int value){
                   return Mathf.CeilToInt((float)value/gridSize)*gridSize;
                  }
               for(int x=start.x;x<=end.x;x+=gridSize){
               for(int z=start.z;z<=end.z;z+=gridSize){
                Vector3Int worldCoord=new(x,0,z);
                Vector3Int coord=worldCoord-new Vector3Int(cnkRgn.x,0,cnkRgn.y);
                var cCoord=this.cCoord;
                var vCoord=coord;
                ValidatevCoord(ref cCoord,ref vCoord);
                var spawnEntry=BiomesConfigurationSnapshot.GetSpawnEntry(vCoord,cCoord,layer);
                if(spawnEntry!=null){
                 Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
                 Vector3 pos=vCoord+new Vector3(0.5f,0.5f,0.5f)-new Vector3(Width/2f,0,Depth/2f)+new Vector3(cnkRgn.x,0,cnkRgn.y);
                 var bounds=spawnEntry.bounds;
                 debugSpawnCoords.Add(pos);
                }
               }}
              }
             }catch(Exception e){
              Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }finally{
              BiomesConfigurationSnapshot.StoppedReading();
             }
             Logs.Debug(()=>"'biome sim object spawning done':"+cCoord);
            }
            public void OnCompletedDoAtMainThread(){
             SimObjectManager.singleton.biomesSpawningSystem.debugSpawnCoords.UnionWith(debugSpawnCoords);
             BiomesSimObjectSpawnJob.pool.Return(this);
            }
        }
     private readonly HashSet<Vector3>debugSpawnCoords=new();
        internal void GizmosSelected(bool selected){
         #if UNITY_EDITOR
         var singleton=SimObjectManager.singleton;
         foreach(var coord in debugSpawnCoords){
          Gizmos.color=new Color(.5f,.5f,.5f,.5f);
          Gizmos.DrawCube(coord+new Vector3(0,singleton.debugSpawnCoordsDrawHeight,0),Vector3.one);
         }
         #endif
        }
    }
}