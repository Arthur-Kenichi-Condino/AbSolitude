using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using AKCondinoO.World.Spawning;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.PhysicsUtil;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.SimObjects.ChunkSimObjectSpawner.BiomesSimObjectSpawnerJob;
using static AKCondinoO.World.Spawning.ByChanceObjectSpawnEntry<AKCondinoO.SimObjects.SimObject>;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.SimObjects{
    internal class ChunkSimObjectSpawner{
     private readonly WorldChunk chunk;
     private readonly WorldChunkSpawning spawning;
        internal ChunkSimObjectSpawner(WorldChunk chunk,WorldChunkSpawning spawning){
         this.chunk=chunk;this.spawning=spawning;
        }
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
         internal readonly HashSet<SpawnReserve>debugSpawnCoords=new();
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
            }
         private Vector2Int cnkRgn;
            public void ExecuteAtBackgroundThread(){
             cnkRgn=cCoordTocnkRgn(cCoord);
             BiomesConfigurationSnapshot.IsReading();
             try{
              var settings=BiomesConfigurationSnapshot.GetSpawnSettings(NoiseChannel.TerrainSurfaceSpawn);
              if(settings==null){
               Logs.Debug(()=>"'failed to get spawn settings!'");
               return;
              }
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
               RecursivelyReserveBounds(iterationSetup,cnkRgn);
               visited.Clear();
              }
             }catch(Exception e){
              Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }finally{
              BiomesConfigurationSnapshot.StoppedReading();
             }
             Logs.Debug(()=>"'biome sim object spawning done':"+cCoord);
            }
            /// <summary>
            ///  "Eu só desisto se a função disser que eu perdi e quem me derrota realmente conseguir existir.", chatGPT
            /// </summary>
            /// <param name="setup"></param>
            /// <param name="center"></param>
            void RecursivelyReserveBounds(GridIterationSetup setup,Vector2Int center){
             int layer=setup.layer;
             var gridIteration=SetupGridIteration(setup,center);
             var gridSize=gridIteration.gridSize;
             var start=gridIteration.start;
             var end=gridIteration.end;
             Logs.Debug(()=>"gridSize:"+gridSize);
             for(int x=start.x;x<=end.x;x+=gridSize){
             for(int z=start.z;z<=end.z;z+=gridSize){
              Vector3Int worldCoord=new(x,0,z);
              DoRecursion(setup,worldCoord,null);
             }}
             Logs.Debug(()=>"spawn count:"+debugSpawnCoords.Count);
            }
         static readonly Utilities.ObjectPool<List<SpawnConflict>>conflictsListPool=
          Pool.GetPool<List<SpawnConflict>>("",()=>new(),(List<SpawnConflict>item)=>{item.Clear();});
            internal struct SpawnCandidate{
             internal CandidateState state;
             internal Vector3Int worldCoord;
             internal ByChanceObjectSpawnEntry<SimObject>spawnEntry;
             internal SpawnVariation variation;
             internal SpawnSurface surface;
             internal OrientedBounds obb;
             internal Quaternion rot;
            }
            internal enum CandidateState{
             Unknown=0,
             Resolving,
             Rejected,
             Accepted,
            }
            bool DoRecursion(GridIterationSetup setup,Vector3Int worldCoord,Vector3Int?caller){
             if(visited.TryGetValue(worldCoord,out var visitedCandidate)){
              switch(visitedCandidate.state){
               case(CandidateState.Resolving):{
                if(caller.HasValue)
                 return ResolveCycle(setup,worldCoord,caller.Value);
                return false;
               }
               case(CandidateState.Rejected ):{return false;}
               case(CandidateState.Accepted ):{return true ;}
              }
             }
             var candidate=new SpawnCandidate(){
              state=CandidateState.Resolving,
              worldCoord=worldCoord,
             };
             visited[worldCoord]=candidate;
             int layer=setup.layer;
             Vector3Int coord=worldCoord-new Vector3Int(cnkRgn.x,0,cnkRgn.y)+new Vector3Int(Width/2,0,Depth/2);
             var cCoord=this.cCoord;
             var vCoord=coord;
             ValidatevCoord(ref cCoord,ref vCoord);
             if(!GetEntry(layer,vCoord,cCoord,out var spawnEntry,out SpawnVariation variation,out SpawnSurface surface)){
              candidate.state=CandidateState.Rejected;
              visited[worldCoord]=candidate;
              return false;
             }
             candidate.spawnEntry=spawnEntry;
             candidate.variation=variation;
             candidate.surface=surface;
             candidate.obb=CalculateOrientedBounds(spawnEntry,variation,surface,out candidate.rot);
             visited[worldCoord]=candidate;
             var conflictsList=conflictsListPool.Rent();
             CollectConflicts(setup,worldCoord,conflictsList);
             bool blocked=false;
             //Logs.Debug(()=>"conflicts:"+conflictsList.Count);
             foreach(var conflict in conflictsList){
              if(conflict.worldCoord==candidate.worldCoord)
               continue;
              if(!ConflictBlocks(setup,conflict,candidate))
               continue;
              if(DoRecursion(setup,conflict.worldCoord,worldCoord)){
               blocked=true;
               break;
              }
             }
             conflictsListPool.Return(conflictsList);
             if(blocked){
              candidate.state=CandidateState.Rejected;
              visited[worldCoord]=candidate;
              return false;
             }else{
              candidate.state=CandidateState.Accepted;
              visited[worldCoord]=candidate;
              Reserve(vCoord,cCoord,candidate);
              return true;
             }
            }
            bool ResolveCycle(GridIterationSetup setup,Vector3Int A,Vector3Int B){
             var maxBoundsSize=setup.spawnLayerData.maxBoundsSize;
             int seqSize=Mathf.CeilToInt(
              Mathf.Max(maxBoundsSize.x,maxBoundsSize.z)*2f
             );
             return PositionalTieBreak(A,B,seqSize);
            }
            private bool ConflictBlocks(GridIterationSetup setup,SpawnConflict A,SpawnCandidate B){
             var maxBoundsSize=setup.spawnLayerData.maxBoundsSize;
             var boundsA=A.spawnEntry.bounds;
             var boundsB=B.spawnEntry.bounds;
             boundsA.center+=A.worldCoord;
             boundsB.center+=B.worldCoord;
             var obbA=A.obb;
             var obbB=B.obb;
             if(!obbA.Intersects(obbB)){
              return false;
             }
             float areaA=boundsA.size.x*boundsA.size.z;
             float areaB=boundsB.size.x*boundsB.size.z;
             if(areaA!=areaB)
              return areaA>areaB;
             int seqSize=Mathf.CeilToInt(
              Mathf.Max(maxBoundsSize.x,maxBoundsSize.z)*2f
             );
             return PositionalTieBreak(A.worldCoord,B.worldCoord,seqSize);
            }
            enum AxisPriority{
             Negative=0,//  West/South
             Positive=1,//  East/North
             Both=2
            }
            bool PositionalTieBreak(Vector3Int Apos,Vector3Int Bpos,int seqSize){
             int sxA=MathUtil.AlternatingSequenceWithSeparator(Apos.x,seqSize,0);
             int szA=MathUtil.AlternatingSequenceWithSeparator(Apos.z,seqSize,0);
             int sxB=MathUtil.AlternatingSequenceWithSeparator(Bpos.x,seqSize,0);
             int szB=MathUtil.AlternatingSequenceWithSeparator(Bpos.z,seqSize,0);
             int pxA=PriorityValue((AxisPriority)sxA);
             int pzA=PriorityValue((AxisPriority)szA);
             int pxB=PriorityValue((AxisPriority)sxB);
             int pzB=PriorityValue((AxisPriority)szB);
             return ResolveTieByPosition(
              pxA,pzA,sxA,szA,
              pxB,pzB,sxB,szB,
              Apos,Bpos
             );
            }
            int PriorityValue(AxisPriority p){
             switch(p){
              case AxisPriority.Both:    return 2;
              case AxisPriority.Positive:return 1;
              case AxisPriority.Negative:return 0;
             }
             return 0;
            }
            bool ResolveTieByPosition(
             int pxA,int pzA,int sxA,int szA,
             int pxB,int pzB,int sxB,int szB,
             Vector3Int Apos,Vector3Int Bpos
            ){
             if(pxA!=pxB)return pxA>pxB;
             if(pzA!=pzB)return pzA>pzB;
             if(sxA!=sxB)return sxA>sxB;
             if(szA!=szB)return szA>szB;
             if(Apos.x!=Bpos.x)return Apos.x<Bpos.x;
             if(Apos.z!=Bpos.z)return Apos.z<Bpos.z;
             return false;
            }
            internal struct SpawnConflict{
             internal Vector3Int worldCoord;
             internal ByChanceObjectSpawnEntry<SimObject>spawnEntry;
             internal SpawnVariation variation;
             internal SpawnSurface surface;
             internal OrientedBounds obb;
            }
            void CollectConflicts(GridIterationSetup setup,Vector3Int candidateCoord,List<SpawnConflict>conflictsList){
             int layer=setup.layer;
             var gridIteration=SetupGridIteration(setup,new(candidateCoord.x,candidateCoord.z));
             var gridSize=gridIteration.gridSize;
             var start=gridIteration.start;
             var end=gridIteration.end;
             for(int x=start.x;x<=end.x;x+=gridSize){
             for(int z=start.z;z<=end.z;z+=gridSize){
              Vector3Int worldCoord=new(x,0,z);
              Vector3Int coord=worldCoord-new Vector3Int(cnkRgn.x,0,cnkRgn.y)+new Vector3Int(Width/2,0,Depth/2);
              var cCoord=this.cCoord;
              var vCoord=coord;
              ValidatevCoord(ref cCoord,ref vCoord);
              if(!GetEntry(layer,vCoord,cCoord,out var spawnEntry,out SpawnVariation variation,out SpawnSurface surface)){
               continue;
              }
              conflictsList.Add(
               new(){
                worldCoord=worldCoord,
                spawnEntry=spawnEntry,
                variation=variation,
                surface=surface,
                obb=CalculateOrientedBounds(spawnEntry,variation,surface,out Quaternion rot),
               }
              );
             }}
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
            internal struct SpawnSurface{
             internal Vector3 hitPoint;
             internal Vector3 normal;
            }
            bool GetEntry(int layer,Vector3Int vCoord,Vector2Int cCoord,out ByChanceObjectSpawnEntry<SimObject>spawnEntry,out SpawnVariation variation,out SpawnSurface surface){
             surface=default;
             spawnEntry=BiomesConfigurationSnapshot.GetSpawnEntry(NoiseChannel.TerrainSurfaceSpawn,vCoord,cCoord,layer,out variation);
             if(spawnEntry!=null){
              if(MarchingCubesHelper.TryFindSurfaceTopDown(
               vCoord,
               cCoord,
               Height-1,
               0,
               out var hitPoint,
               out var normal
              )){
               surface=new SpawnSurface(){
                hitPoint=hitPoint,
                normal=normal,
               };
               return true;
              }
             }
             return false;
            }
            internal OrientedBounds CalculateOrientedBounds(ByChanceObjectSpawnEntry<SimObject>spawnEntry,SpawnVariation variation,SpawnSurface surface,out Quaternion rot){
             Quaternion align=Quaternion.FromToRotation(Vector3.up,surface.normal);
             if(variation.alignToTerrain){
              rot=align*Quaternion.Euler(variation.rot);
             }else{
              Quaternion yaw=Quaternion.AngleAxis(variation.rot.y,Vector3.up);
              rot=align*yaw;
             }
             Vector3 ext=Vector3.Scale(spawnEntry.bounds.extents,variation.scale);
             Vector3 up=rot*Vector3.up;
             Vector3 pivotOffsetLocal=spawnEntry.bounds.center;
             Vector3 pivotOffsetScaled=Vector3.Scale(pivotOffsetLocal,variation.scale);
             Vector3 pivotOffsetWorld=rot*pivotOffsetScaled;
             Vector3 center=surface.hitPoint-pivotOffsetWorld;
             Vector3 offset=(rot*Vector3.up)*ext.y;
             center+=offset;
             OrientedBounds obb=new(){
              center=center,
              axisX=(rot*Vector3.right  ).normalized,
              axisY=(rot*Vector3.up     ).normalized,
              axisZ=(rot*Vector3.forward).normalized,
              extents=ext
             };
             return obb;
            }
            internal struct SpawnReserve{
             internal Vector3 pos;
             internal Bounds bounds;
             internal Quaternion rot;
             internal Vector3 scale;
            }
            void Reserve(Vector3Int vCoord,Vector2Int cCoord,SpawnCandidate candidate){
             Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
             Vector3 pos=candidate.obb.center;
             var bounds=candidate.spawnEntry.bounds;
             var spawnReserve=new SpawnReserve(){
              pos=pos,
              bounds=bounds,
              rot=candidate.rot,
              scale=candidate.variation.scale,
             };
             debugSpawnCoords.Add(spawnReserve);
            }
            public void OnCompletedDoAtMainThread(){
             spawner.debugSpawnCoords.Clear();
             spawner.debugSpawnCoords.UnionWith(debugSpawnCoords);
             BiomesSimObjectSpawnerJob.pool.Return(this);
            }
        }
     private readonly HashSet<SpawnReserve>debugSpawnCoords=new();
        internal void GizmosSelected(bool selected){
         #if UNITY_EDITOR
         var singleton=SimObjectManager.singleton;
         foreach(var reserve in debugSpawnCoords){
          Vector3 pos   =reserve.pos   ;
          Bounds bounds =reserve.bounds;
          Quaternion rot=reserve.rot   ;
          Vector3 scale =reserve.scale ;
          DrawGizmos.RotatedBounds(bounds,pos,rot,scale,Color.green);
         }
         #endif
        }
    }
}