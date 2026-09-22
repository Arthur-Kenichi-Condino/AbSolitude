using AKCondinoO.Bootstrap;
using AKCondinoO.PersistentData;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using AKCondinoO.World.Spawning;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.PersistentData.SpawnMapFiles;
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
         bool scheduled=ThreadDispatcher.TrySchedule(doBiomeSpawnerJob,3);
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
            foreach(var kvp in item.debugSpawnCoords){
             kvp.Value.Clear();
            }
            item.visited.Clear();
            item.spawner=null;
           }
          );
         internal ChunkSimObjectSpawner spawner;
         internal Vector2Int cCoord;
         private readonly Dictionary<Vector3Int,SpawnCandidate>visited=new();
         internal readonly Dictionary<int,HashSet<SpawnReserve>>debugSpawnCoords=new();
         protected SpawnList spawnList;
            public void CancelGraciously(){
            }
            public void OnDoScheduleSetContainerData(){
            }
         private Vector2Int cnkRgn;
         readonly System.Diagnostics.Stopwatch layerSw=new();
            public void ExecuteAtBackgroundThread(){
             spawnList=SpawnList.pool.Rent();
             cnkRgn=cCoordTocnkRgn(cCoord);
             using(ReadScope.Enter()){
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
                layerSw.Restart();
                int layer=kvp.Key;
                var spawnLayerData=kvp.Value;
                var iterationSetup=new GridIterationSetup(){
                 layer=layer,
                 spawnLayerData=spawnLayerData,
                };
                RecursivelyReserveBounds(iterationSetup,cnkRgn);
                visited.Clear();
                layerSw.Stop();
                Logs.Debug(()=>"'layer "+layer+" spawn recursion execution time':"+layerSw.ElapsedMilliseconds+" ms");
               }
              }catch(Exception e){
               Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
              }finally{
              }
             }
             Logs.Debug(()=>"'biome sim object spawning done':"+cCoord);
            }
            /// <summary>
            ///  "Eu só desisto se a função disser que eu perdi e quem me derrota realmente conseguir existir.", chatGPT
            /// </summary>
            /// <param name="setup"></param>
            /// <param name="center"></param>
            void RecursivelyReserveBounds(GridIterationSetup setup,Vector2Int center){
             SpawnMapFiles spawnMapFiles=(PersistentDataManager.singleton.GetFileManager(typeof(SpawnMapFiles))as SpawnMapFiles);
             int layer=setup.layer;
             var gridIteration=SetupGridIteration(setup,center);
             var gridSize=gridIteration.gridSize;
             var start=gridIteration.start;
             var end=gridIteration.end;
             Logs.Debug(()=>"gridSize:"+gridSize);
             for(int x=start.x;x<=end.x;x+=gridSize){
             for(int z=start.z;z<=end.z;z+=gridSize){
              Vector3Int worldCoord=new(x,0,z);
              DoRecursion(setup,worldCoord,null,spawnMapFiles);
             }}
             if(debugSpawnCoords.TryGetValue(layer,out var debugSpawns)){
              Logs.Debug(()=>"layer.."+layer+"..spawn count:"+debugSpawns.Count);
             }
            }
         static readonly Utilities.ObjectPool<List<SpawnConflict>>conflictsListPool=
          Pool.GetPool<List<SpawnConflict>>("",()=>new(),(List<SpawnConflict>item)=>{item.Clear();});
            internal struct SpawnCandidate{
             internal CandidateState state;
             internal Vector3Int worldCoord;
             internal SpawnEntry spawnEntry;
             internal SpawnVariation variation;
             internal SpawnSurface surface;
             internal OrientedBounds obb;
             internal Quaternion rot;
            }
            internal struct SpawnEntry{
             public SimObject prefab;
             public Bounds bounds;
            }
            internal enum CandidateState{
             Unknown=0,
             Resolving,
             Rejected,
             Accepted,
            }
            bool DoRecursion(GridIterationSetup setup,Vector3Int worldCoord,Vector3Int?caller,SpawnMapFiles spawnMapFiles){
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
             Vector3Int coord=worldCoord-new Vector3Int(this.cnkRgn.x,0,this.cnkRgn.y)+new Vector3Int(Width/2,0,Depth/2);
             var cCoord=this.cCoord;
             var vCoord=coord;
             ValidatevCoord(ref cCoord,ref vCoord);
             var cnkRgn=cCoordTocnkRgn(cCoord);
             if(spawnMapFiles.ReadFromSpawnMapFile(cnkRgn,layer,vCoord,out var spawnObject,out SpawnMapFileReadResult result)){
              //Logs.Debug(()=>"'ReadFromSpawnMapFile':"+spawnObject.spawnEntry.prefab.GetType());
              candidate.spawnEntry=spawnObject.spawnEntry;
              candidate.variation=spawnObject.variation;
              candidate.surface=spawnObject.surface;
              candidate.obb=CalculateOrientedBounds(
               candidate.spawnEntry,candidate.variation,candidate.surface,
               out candidate.rot
              );
              candidate.state=CandidateState.Accepted;
              visited[worldCoord]=candidate;
              var reserve=Reserve(layer,vCoord,cCoord,candidate);
              if(cnkRgn==this.cnkRgn){
               var prefab=candidate.spawnEntry.prefab;
               SimObjectSpawn spawn=new(prefab.GetType(),prefab.variant,
                reserve.pos,reserve.rot,reserve.scale
               ){
               };
               spawnList.Add(spawn);
              }
              return true;
             }
             if(result==SpawnMapFileReadResult.Empty){
              //Logs.Debug(()=>"'ReadFromSpawnMapFile':empty:"+empty);
              candidate.state=CandidateState.Rejected;
              visited[worldCoord]=candidate;
              return false;
             }
             //Logs.Debug(()=>"'get entry should not be called anymore if generation was completed and saved':result:"+result);
             if(!GetEntry(layer,vCoord,cCoord,out var spawnEntry,out SpawnVariation variation,out SpawnSurface surface)){
              candidate.state=CandidateState.Rejected;
              visited[worldCoord]=candidate;
              spawnMapFiles?.WriteEmptyToSpawnMapFile(cnkRgn,layer,vCoord);
              return false;
             }
             candidate.spawnEntry=new SpawnEntry(){
              prefab=spawnEntry.prefab,
              bounds=spawnEntry.bounds
             };
             candidate.variation=variation;
             candidate.surface=surface;
             candidate.obb=CalculateOrientedBounds(
              candidate.spawnEntry,candidate.variation,candidate.surface,
              out candidate.rot
             );
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
              if(DoRecursion(setup,conflict.worldCoord,worldCoord,spawnMapFiles)){
               blocked=true;
               break;
              }
             }
             conflictsListPool.Return(conflictsList);
             if(!blocked){
              if(WorldSimObjectSpatialMap.HasSpawnConflicts(setup.layer,cCoord,candidate)){
               blocked=true;
               //Logs.Debug(()=>"'HasSpawnConflicts'");
              }
             }
             if(blocked){
              candidate.state=CandidateState.Rejected;
              visited[worldCoord]=candidate;
              spawnMapFiles?.WriteEmptyToSpawnMapFile(cnkRgn,layer,vCoord);
              return false;
             }else{
              candidate.state=CandidateState.Accepted;
              visited[worldCoord]=candidate;
              var reserve=Reserve(layer,vCoord,cCoord,candidate);
              spawnMapFiles?.WriteToSpawnMapFile(cnkRgn,layer,vCoord,candidate,reserve);
              if(cnkRgn==this.cnkRgn){
               var prefab=candidate.spawnEntry.prefab;
               SimObjectSpawn spawn=new(prefab.GetType(),prefab.variant,
                reserve.pos,reserve.rot,reserve.scale
               ){
               };
               spawnList.Add(spawn);
              }
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
             internal SpawnEntry spawnEntry;
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
              if(!GetEntry(layer,vCoord,cCoord,out var spawnEntry,out SpawnVariation conflictVariation,out SpawnSurface conflictSurface)){
               continue;
              }
              SpawnEntry conflictSpawnEntry=new(){
               prefab=spawnEntry.prefab,
               bounds=spawnEntry.bounds,
              };
              SpawnConflict conflict=new(){
               worldCoord=worldCoord,
               spawnEntry=conflictSpawnEntry,
               variation=conflictVariation,
               surface=conflictSurface,
               obb=CalculateOrientedBounds(
                conflictSpawnEntry,conflictVariation,conflictSurface,
                out Quaternion rot
               ),
              };
              conflictsList.Add(
               conflict
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
            internal OrientedBounds CalculateOrientedBounds(SpawnEntry spawnEntry,SpawnVariation variation,SpawnSurface surface,out Quaternion rot){
             if(variation.alignToTerrain){
              Quaternion align=Quaternion.FromToRotation(Vector3.up,surface.normal);
              rot=align*Quaternion.Euler(variation.rot);
             }else{
              Quaternion yaw=Quaternion.AngleAxis(variation.rot.y,Vector3.up);
              rot=yaw;
             }
             Vector3 ext=Vector3.Scale(spawnEntry.bounds.extents,variation.scale);
             Vector3 axisX=(rot*Vector3.right  ).normalized;
             Vector3 axisY=(rot*Vector3.up     ).normalized;
             Vector3 axisZ=(rot*Vector3.forward).normalized;
             Vector3 pivotOffsetLocal=spawnEntry.bounds.center;
             Vector3 pivotOffsetScaled=Vector3.Scale(pivotOffsetLocal,variation.scale);
             Vector3 pivotOffsetWorld=rot*pivotOffsetScaled;
             Vector3 spawnPos=surface.hitPoint-pivotOffsetWorld+(axisY*ext.y);
             Vector3 center=spawnPos+pivotOffsetWorld;
             OrientedBounds obb=new(){
              spawnPos=spawnPos,
              center=center,
              axisX=axisX,
              axisY=axisY,
              axisZ=axisZ,
              extents=ext
             };
             if(!FitOrientedBoundsToTerrain(ref obb,1.0f)){
              //Logs.Error("obb could not be put in a suitable position on the terrain");
             }
             return obb;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool FitOrientedBoundsToTerrain(ref OrientedBounds obb,float sampleMultiplier=1f,float baseDrop=0f){
             sampleMultiplier=Mathf.Clamp01(sampleMultiplier);
             int sampleStep=sampleMultiplier<=0f?int.MaxValue:sampleMultiplier>=1f?1:Mathf.CeilToInt(1f/sampleMultiplier);
             Vector3 spawnPos=obb.spawnPos;
             Vector3 obbCenter=obb.center;Vector3 obbAxisX=obb.axisX;Vector3 obbAxisY=obb.axisY;Vector3 obbAxisZ=obb.axisZ;Vector3 obbExtents=obb.extents;
             Vector3 bottomCenter=obbCenter-obbAxisY*obbExtents.y;
             Vector2Int bottomCenterStep=new(Mathf.FloorToInt(bottomCenter.x),Mathf.FloorToInt(bottomCenter.z));
             Vector3 p0=bottomCenter-obbAxisX*obbExtents.x-obbAxisZ*obbExtents.z;Vector2Int p0Step=new(Mathf.FloorToInt(p0.x),Mathf.FloorToInt(p0.z));
             Vector3 p1=bottomCenter+obbAxisX*obbExtents.x-obbAxisZ*obbExtents.z;Vector2Int p1Step=new(Mathf.FloorToInt(p1.x),Mathf.FloorToInt(p1.z));
             Vector3 p2=bottomCenter+obbAxisX*obbExtents.x+obbAxisZ*obbExtents.z;Vector2Int p2Step=new(Mathf.FloorToInt(p2.x),Mathf.FloorToInt(p2.z));
             Vector3 p3=bottomCenter-obbAxisX*obbExtents.x+obbAxisZ*obbExtents.z;Vector2Int p3Step=new(Mathf.FloorToInt(p3.x),Mathf.FloorToInt(p3.z));
             bool IsMandatoryStep(int x,int z){
              return
               (x==bottomCenterStep.x&&z==bottomCenterStep.y)||
               (x==p0Step.x          &&z==p0Step.y)          ||
               (x==p1Step.x          &&z==p1Step.y)          ||
               (x==p2Step.x          &&z==p2Step.y)          ||
               (x==p3Step.x          &&z==p3Step.y);
             }
             Vector3 bottomPlaneNormal=Vector3.Cross(obbAxisX,obbAxisZ);
             //  Face praticamente vertical: não conseguimos determinar Y
             // por uma linha vertical.
             if(Mathf.Abs(bottomPlaneNormal.y)<0.0001f){
              //Logs.Error("cannot determine Y for vertical face");
              return false;
             }
             float maxDrop=0f;
             SamplePoint(bottomCenter,out float bottomCenterY,out float bottomCenterSurface,out float bottomCenterDrop,true);
             SamplePoint(p0,out float p0Y,out float p0Surface,out float p0Drop,true);
             SamplePoint(p1,out float p1Y,out float p1Surface,out float p1Drop,true);
             SamplePoint(p2,out float p2Y,out float p2Surface,out float p2Drop,true);
             SamplePoint(p3,out float p3Y,out float p3Surface,out float p3Drop,true);
             //Logs.Debug(()=>"'spawnPos':"+spawnPos+";'obbCenter':"+obbCenter+";'obbAxisX':"+obbAxisX+";'obbAxisY':"+obbAxisY+";'obbAxisZ':"+obbAxisZ+";'obbExtents':"+obbExtents+"'bottomCenter':"+bottomCenter+";'bottomCenterY':"+bottomCenterY+";'bottomCenterSurface':"+bottomCenterSurface+";'bottomCenterDrop':"+bottomCenterDrop+";'bottomCenterStep':"+bottomCenterStep+";'p0':"+p0+";'p0Y':"+p0Y+";'p0Surface':"+p0Surface+";'p0Drop':"+p0Drop+";'p0Step':"+p0Step+";'p1':"+p1+";'p1Y':"+p1Y+";'p1Surface':"+p1Surface+";'p1Drop':"+p1Drop+";'p1Step':"+p1Step+";'p2':"+p2+";'p2Y':"+p2Y+";'p2Surface':"+p2Surface+";'p2Drop':"+p2Drop+";'p2Step':"+p2Step+";'p3':"+p3+";'p3Y':"+p3Y+";'p3Surface':"+p3Surface+";'p3Drop':"+p3Drop+";'p3Step':"+p3Step+";'maxDrop':"+maxDrop);
             void SamplePoint(Vector3 point,out float y,out float surface,out float drop,bool mandatory=false){
              if(!mandatory&&!IsPointInBottomPlane(point,p0,p1,p2,p3)){
               y=-1f;
               surface=-1f;
               drop=-1f;
               return;
              }
              y=GetPlanePointHeight(
               bottomCenter,
               bottomPlaneNormal,
               point.x,
               point.z
              );
              Vector3 samplePoint=new(
               point.x,
               Height-1,
               point.z
              );
              Vector2Int cCoord=vecPosTocCoord(samplePoint);
              Vector3Int vCoord=vecPosTovCoord(samplePoint);
              surface=MarchingCubesHelper.GetMarchingSurfaceHeight(cCoord,vCoord,point.x,point.z);
              drop=(y-surface);
              if(drop>maxDrop){
               maxDrop=drop;
              }
             }
             //  Projeção da face inferior no plano XZ.
             float minX=Mathf.Min(p0.x,p1.x,p2.x,p3.x);
             float maxX=Mathf.Max(p0.x,p1.x,p2.x,p3.x);
             float minZ=Mathf.Min(p0.z,p1.z,p2.z,p3.z);
             float maxZ=Mathf.Max(p0.z,p1.z,p2.z,p3.z);
             int startX=Mathf.FloorToInt(minX);
             int   endX=Mathf. CeilToInt(maxX);
             int startZ=Mathf.FloorToInt(minZ);
             int   endZ=Mathf. CeilToInt(maxZ);
             for(int x=startX;x<=endX;x+=sampleStep){
             for(int z=startZ;z<=endZ;z+=sampleStep){
              if(IsMandatoryStep(x,z)){
               continue;
              }
              Vector3 p=new(x+0.5f,0f,z+0.5f);
              SamplePoint(p,out float pY,out float pSurface,out float pDrop);
             }}
             float slopeDrop=
              Mathf.Abs(obbAxisX.y)*obbExtents.x+
              Mathf.Abs(obbAxisZ.y)*obbExtents.z;
             if(slopeDrop>maxDrop){
              maxDrop=slopeDrop;
             }
             maxDrop+=baseDrop;
             if(maxDrop<=0f){
              return false;
             }
             obb.center-=Vector3.up*maxDrop;
             obb.spawnPos-=Vector3.up*maxDrop;
             return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool IsPointInBottomPlane(Vector3 p,Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3){
             float c0=CrossXZ(p0,p1,p);
             float c1=CrossXZ(p1,p2,p);
             float c2=CrossXZ(p2,p3,p);
             float c3=CrossXZ(p3,p0,p);
             bool hasPositive=c0>0f||c1>0f||c2>0f||c3>0f;
             bool hasNegative=c0<0f||c1<0f||c2<0f||c3<0f;
             return!(hasPositive&&hasNegative);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float CrossXZ(Vector3 a,Vector3 b,Vector3 p){
             return
              (b.x-a.x)*(p.z-a.z)-
              (b.z-a.z)*(p.x-a.x);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static float GetPlanePointHeight(Vector3 facePoint,Vector3 normal,float x,float z){
             return 
              facePoint.y-(
               normal.x*(x-facePoint.x)+
               normal.z*(z-facePoint.z)
              )/normal.y;
            }
            internal struct SpawnReserve{
             internal Vector3 pos;
             internal Bounds bounds;
             internal Quaternion rot;
             internal Vector3 scale;
             internal OrientedBounds obb;
            }
            SpawnReserve Reserve(int layer,Vector3Int vCoord,Vector2Int cCoord,SpawnCandidate candidate){
             Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
             Vector3 pos=candidate.obb.spawnPos;
             var bounds=candidate.spawnEntry.bounds;
             var spawnReserve=new SpawnReserve(){
              pos=pos,
              bounds=bounds,
              rot=candidate.rot,
              scale=candidate.variation.scale,
              obb=candidate.obb,
             };
             WorldSimObjectSpatialMap.RegisterSimObjectSpawn(layer,cCoord,spawnReserve);
             if(!debugSpawnCoords.TryGetValue(layer,out var debugSpawns)){
              debugSpawnCoords.Add(layer,debugSpawns=new());
             }
             debugSpawns.Add(spawnReserve);
             return spawnReserve;
            }
            public void OnCompletedDoAtMainThread(){
             EnqueueSpawnList();
             foreach(var kvp in spawner.debugSpawnCoords){
              kvp.Value.Clear();
             }
             foreach(var kvp in debugSpawnCoords){
              if(!spawner.debugSpawnCoords.TryGetValue(kvp.Key,out var debugSpawns)){
               spawner.debugSpawnCoords.Add(kvp.Key,debugSpawns=new());
              }
              spawner.debugSpawnCoords[kvp.Key].UnionWith(kvp.Value);
             }
             BiomesSimObjectSpawnerJob.pool.Return(this);
            }
            protected void EnqueueSpawnList(){
             var singleton=SimObjectManager.singleton;
             if(!object.ReferenceEquals(singleton,null)){
              singleton.spawnQueue.Enqueue(spawnList);
             }else{
              SpawnList.pool.Return(spawnList);
             }
             spawnList=null;
            }
        }
     private readonly Dictionary<int,HashSet<SpawnReserve>>debugSpawnCoords=new();
        internal void GizmosSelected(bool selected){
         #if UNITY_EDITOR
         var singleton=SimObjectManager.singleton;
         foreach(var kvp in debugSpawnCoords){
          foreach(var reserve in kvp.Value){
           Vector3 pos   =reserve.pos   ;
           Bounds bounds =reserve.bounds;
           Quaternion rot=reserve.rot   ;
           Vector3 scale =reserve.scale ;
           DrawGizmos.RotatedBounds(bounds,pos,rot,scale,Color.green);
          }
         }
         #endif
        }
    }
}