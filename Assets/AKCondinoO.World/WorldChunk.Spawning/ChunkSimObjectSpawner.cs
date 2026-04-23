using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Spawning;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.SimObjects{
    internal class ChunkSimObjectSpawner{
     private readonly WorldChunk chunk;
     private readonly WorldChunkSpawning spawning;
        internal void DoBiomeSpawnJob(){
         Vector2Int cCoord=chunk.cCoord;
         var doBiomeSpawnerJob=BiomesSimObjectSpawnerJob.pool.Rent();
         doBiomeSpawnerJob.spawner=this;
         doBiomeSpawnerJob.cCoord=cCoord;
         bool scheduled=ThreadDispatcher.TrySchedule(doBiomeSpawnerJob,7);
         if(!scheduled){
          BiomesSimObjectSpawnerJob.pool.Return(doBiomeSpawnerJob);
          doBiomeSpawnerJob=null;
         }
        }
        internal class BiomesSimObjectSpawnerJob:MultithreadedContainerJob{
         internal static readonly Utilities.ObjectPool<BiomesSimObjectSpawnerJob>pool=
          Pool.GetPool<BiomesSimObjectSpawnerJob>(
           "",
           ()=>new(),
           (BiomesSimObjectSpawnerJob item)=>{
            item.debugSpawnCoords.Clear();
            item.visited.Clear();
            item.spawner=null;
           }
          );
         internal ChunkSimObjectSpawner spawner;
         internal Vector2Int cCoord;
         private readonly Dictionary<Vector3Int,SpawnCandidate>visited=new();
         internal readonly HashSet<Vector3>debugSpawnCoords=new();
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
            }
         private Vector2Int cnkRgn;
            public void ExecuteAtBackgroundThread(){
             cnkRgn=cCoordTocnkRgn(cCoord);
             BiomesConfigurationSnapshot.IsReading();
             try{
              var settings=BiomesConfigurationSnapshot.GetSpawnSettings();
              int minLayer=settings.minLayer;
              int maxLayer=settings.maxLayer;
              Logs.Debug(()=>"minLayer:"+minLayer+";maxLayer:"+maxLayer);
              foreach(var kvp in settings.layerData){
               int layer=kvp.Key;
               var spawnLayerData=kvp.Value;
               var iterationSetup=new GridIterationSetup(){
                layer=layer,
                spawnLayerData=spawnLayerData,
               };
               RecursivelyTryReserveBoundsAt(iterationSetup,cnkRgn);
               visited.Clear();
              }
             }catch(Exception e){
              Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }finally{
              BiomesConfigurationSnapshot.StoppedReading();
             }
             Logs.Debug(()=>"'biome sim object spawning done':"+cCoord);
            }
            internal struct SpawnCandidate{
             internal bool state;
            }
            /// <summary>
            ///  "Eu só desisto se quem me derrota realmente existir.", chatGPT
            /// </summary>
            /// <param name="setup"></param>
            /// <param name="center"></param>
            void RecursivelyTryReserveBoundsAt(GridIterationSetup setup,Vector2Int center){
             int layer=setup.layer;
             var gridIteration=SetupGridIteration(setup,center);
             var gridSize=gridIteration.gridSize;
             var start=gridIteration.start;
             var end=gridIteration.end;
             Logs.Debug(()=>"gridSize:"+gridSize);
             for(int x=start.x;x<=end.x;x+=gridSize){
             for(int z=start.z;z<=end.z;z+=gridSize){
              Vector3Int worldCoord=new(x,0,z);
              Vector3Int coord=worldCoord-new Vector3Int(cnkRgn.x,0,cnkRgn.y)+new Vector3Int(Width/2,0,Depth/2);
              var cCoord=this.cCoord;
              var vCoord=coord;
              ValidatevCoord(ref cCoord,ref vCoord);
              if(!GetEntry(layer,vCoord,cCoord,out var spawnEntry)){
               continue;
              }
              //  test if conflict blocks
              //  if it does, do recursion for the conflict
              //  if conflict will be blocked in its recursion, then this is valid
              //  reserve if nothing blocks
              Reserve(vCoord,cCoord,spawnEntry);
             }}
            }
            bool GetEntry(int layer,Vector3Int vCoord,Vector2Int cCoord,out ByChanceObjectSpawnEntry<SimObject>spawnEntry){
             spawnEntry=BiomesConfigurationSnapshot.GetSpawnEntry(vCoord,cCoord,layer);
             if(spawnEntry!=null){
              return true;
             }
             return false;
            }
            internal struct SpawnReserve{
            }
            void Reserve(Vector3Int vCoord,Vector2Int cCoord,ByChanceObjectSpawnEntry<SimObject>spawnEntry){
             Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
             Vector3 pos=vCoord+new Vector3(0.5f,0.5f,0.5f)-new Vector3(Width/2f,0,Depth/2f)+new Vector3(cnkRgn.x,0,cnkRgn.y);
             var bounds=spawnEntry.bounds;
             debugSpawnCoords.Add(pos);
            }
            struct GridIterationSetup{
             internal int layer;
             internal SnapshotSpawnLayerData spawnLayerData;
            }
            struct GridIteration{
             internal int gridSize;
             internal Vector3Int start;
             internal Vector3Int end;
            }
            GridIteration SetupGridIteration(GridIterationSetup input,Vector2Int center){
             var spawnLayerData=input.spawnLayerData;
             int gridSize=spawnLayerData.gridSize;
             Vector3 maxBoundsSize=spawnLayerData.maxBoundsSize;
             Vector3 halfBounds=maxBoundsSize/2f;
             Vector3Int worldMin=new(
              center.x-Width/2,0,
              center.y-Depth/2
             );
             Vector3Int worldMax=new(
              worldMin.x+Width,Height-1,
              worldMin.z+Depth
             );
             Vector3Int start=new(
              AlignDown(worldMin.x-Mathf.CeilToInt(halfBounds.x),gridSize),worldMin.y,
              AlignDown(worldMin.z-Mathf.CeilToInt(halfBounds.z),gridSize)
             );
             Vector3Int end=new(
              AlignUp(worldMax.x+Mathf.CeilToInt(halfBounds.x),gridSize),worldMax.y,
              AlignUp(worldMax.z+Mathf.CeilToInt(halfBounds.z),gridSize)
             );
             return new(){
              gridSize=gridSize,
              start=start,
              end=end,
             };
            }
            int AlignDown(int value,int gridSize){
             return Mathf.FloorToInt((float)value/gridSize)*gridSize;
            }
            int AlignUp(int value,int gridSize){
             return Mathf.CeilToInt((float)value/gridSize)*gridSize;
            }
            public void OnCompletedDoAtMainThread(){
             spawner.debugSpawnCoords.Clear();
             spawner.debugSpawnCoords.UnionWith(debugSpawnCoords);
             BiomesSimObjectSpawnerJob.pool.Return(this);
            }
        }
        internal ChunkSimObjectSpawner(WorldChunk chunk,WorldChunkSpawning spawning){
         this.chunk=chunk;this.spawning=spawning;
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