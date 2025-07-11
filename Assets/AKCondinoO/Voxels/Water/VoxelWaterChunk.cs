#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims;
using AKCondinoO.Voxels.Terrain;
using AKCondinoO.Voxels.Water.MarchingCubes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWaterBackgroundContainer;
namespace AKCondinoO.Voxels.Water{
    internal class VoxelWaterChunk:MonoBehaviour{
     internal VoxelTerrainChunk tCnk;
     internal WaterSpreadingBackgroundContainer waterSpreadingBG=new WaterSpreadingBackgroundContainer();
     internal MarchingCubesWaterBackgroundContainer marchingCubesWaterBG=new MarchingCubesWaterBackgroundContainer();
     internal Bounds worldBounds=new Bounds(
      Vector3.zero,
      new Vector3(Width,Height,Depth)
     );
     MeshFilter filter;
        void Awake(){
         mesh=new Mesh(){
          bounds=worldBounds,
         };
         filter=GetComponent<MeshFilter>();
         filter.mesh=mesh;
         bakeJob=new BakeJob(){
          meshId=mesh.GetInstanceID(),
         };
         meshCollider=GetComponent<MeshCollider>();
         marchingCubesWaterBG.TempVer=new NativeList<Vertex>(Allocator.Persistent);
         marchingCubesWaterBG.TempTri=new NativeList<UInt32>(Allocator.Persistent);
        }
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         waterSpreadingBG.IsCompleted(VoxelSystem.singleton.waterSpreadingBGThreads[0].IsRunning,-1);
         bakeJobHandle.Complete();
         marchingCubesWaterBG.IsCompleted(VoxelSystem.singleton.marchingCubesWaterBGThreads[0].IsRunning,-1);
         if(marchingCubesWaterBG.TempVer.IsCreated)marchingCubesWaterBG.TempVer.Dispose();
         if(marchingCubesWaterBG.TempTri.IsCreated)marchingCubesWaterBG.TempTri.Dispose();
        }
        internal void OncCoordChanged(bool rebuild){
         hasPhysMeshBaked=false;
         if(rebuild){
          pendingMarchingCubes=true;
          this.name=tCnk.id+".VoxelWaterChunk";
         }
        }
        internal void OnNeighbourhoodHadChanges(VoxelWaterChunk chunk){
         pendingMarchingCubes=true;
        }
     [SerializeField]float spreadTimeInterval=1.0f;
     float spreadTimer=1.0f;
     bool waitingBakeJob;
     bool waitingMarchingCubes;
     bool pendingMarchingCubes;
     bool waitingWaterSpread;
        internal void ManualUpdate(){
         //Log.DebugMessage("ManualUpdate");
         if(waitingWaterSpread){
             if(OnWaterSpread(out bool hadChanges)){
                 waitingWaterSpread=false;
                 if(hadChanges){
                  for(int x=-1;x<=1;x++){
                  for(int y=-1;y<=1;y++){
                   Vector2Int cCoord1=tCnk.id.Value.cCoord+new Vector2Int(x,y);
                   int        cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                   if(VoxelSystem.singleton.terrainActive.TryGetValue(cnkIdx1,out VoxelTerrainChunk chunk)){
                    chunk.wCnk.OnNeighbourhoodHadChanges(this);
                   }
                  }}
                 }
             }
         }else{
             if(CanSpreadWater()){
                 waitingWaterSpread=true;
             }
         }
         if(waitingBakeJob){
             if(OnPhysMeshBaked()){
                 waitingBakeJob=false;
                 //simObjectsPlacing.OnVoxelTerrainReady();
             }
         }else{
             if(waitingMarchingCubes){
                 if(OnMarchingCubesDone(out int vertexCount)){
                     waitingMarchingCubes=false;
                     if(vertexCount>0){
                      SchedulePhysBakeMeshJob();
                     }
                     waitingBakeJob=true;
                 }
             }else{
                 if(pendingMarchingCubes){
                     if(CanBeginMarchingCubes()){
                         pendingMarchingCubes=false;
                         waitingMarchingCubes=true;
                     }
                 }
             }
         }
        }
     internal static int sSpreadWaterExecutionCount;
        bool CanSpreadWater(){
         spreadTimer-=Time.deltaTime;
         if(spreadTimer<=0f){
          if(sSpreadWaterExecutionCount>=VoxelSystem.singleton._SpreadWaterExecutionCountLimit){
           return false;
          }
          //Log.DebugMessage("CanSpreadWater");
          spreadTimer=spreadTimeInterval;
          waterSpreadingBG.cCoord=tCnk.id.Value.cCoord;
          waterSpreadingBG.cnkRgn=tCnk.id.Value.cnkRgn;
          waterSpreadingBG.cnkIdx=tCnk.id.Value.cnkIdx;
          waterSpreadingBG.result=false;
          sSpreadWaterExecutionCount++;
          WaterSpreadingMultithreaded.Schedule(waterSpreadingBG);
          return true;
         }
         return false;
        }
        bool OnWaterSpread(out bool hadChanges){
         hadChanges=false;
         if(waterSpreadingBG.IsCompleted(VoxelSystem.singleton.waterSpreadingBGThreads[0].IsRunning)){
          sSpreadWaterExecutionCount--;
          //Log.DebugMessage("OnWaterSpread");
          hadChanges=waterSpreadingBG.result;
          return true;
         }
         return false;
        }
     internal static int sMarchingCubesWaterExecutionCount;
        bool CanBeginMarchingCubes(){
         if(sMarchingCubesWaterExecutionCount>=VoxelSystem.singleton._MarchingCubesWaterExecutionCountLimit){
          return false;
         }
         if(marchingCubesWaterBG.IsCompleted(VoxelSystem.singleton.marchingCubesWaterBGThreads[0].IsRunning)){
          worldBounds.center=transform.position=new Vector3(tCnk.id.Value.cnkRgn.x,0,tCnk.id.Value.cnkRgn.y);
          marchingCubesWaterBG.cCoord=tCnk.id.Value.cCoord;
          marchingCubesWaterBG.cnkRgn=tCnk.id.Value.cnkRgn;
          marchingCubesWaterBG.cnkIdx=tCnk.id.Value.cnkIdx;
          sMarchingCubesWaterExecutionCount++;
          MarchingCubesWaterMultithreaded.Schedule(marchingCubesWaterBG);
          return true;
         }
         return false;
        }
     #region Rendering
         static readonly VertexAttributeDescriptor[]layout=new[]{
          new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,4),
          new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
          //new VertexAttributeDescriptor(VertexAttribute.Color    ,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord1,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord2,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord3,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord4,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord5,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord6,VertexAttributeFormat.Float32,4),
          //new VertexAttributeDescriptor(VertexAttribute.TexCoord7,VertexAttributeFormat.Float32,4),
         };
         MeshUpdateFlags meshFlags=MeshUpdateFlags.DontValidateIndices|
                                   MeshUpdateFlags.DontNotifyMeshUsers|
                                   MeshUpdateFlags.DontRecalculateBounds|
                                   MeshUpdateFlags.DontResetBoneBounds;
         internal Mesh mesh;
     #endregion
        bool OnMarchingCubesDone(out int vertexCount){
         vertexCount=0;
         if(marchingCubesWaterBG.IsCompleted(VoxelSystem.singleton.marchingCubesWaterBGThreads[0].IsRunning)){
          sMarchingCubesWaterExecutionCount--;
          bool resize;
          if(resize=(vertexCount=marchingCubesWaterBG.TempVer.Length)>mesh.vertexCount){
           mesh.SetVertexBufferParams(marchingCubesWaterBG.TempVer.Length,layout);
          }
          mesh.SetVertexBufferData(marchingCubesWaterBG.TempVer.AsArray(),0,0,marchingCubesWaterBG.TempVer.Length,0,meshFlags);
          if(resize){
           mesh.SetIndexBufferParams(marchingCubesWaterBG.TempTri.Length,IndexFormat.UInt32);
          }
          mesh.SetIndexBufferData(marchingCubesWaterBG.TempTri.AsArray(),0,0,marchingCubesWaterBG.TempTri.Length,meshFlags);
          mesh.subMeshCount=1;
          mesh.SetSubMesh(0,new SubMeshDescriptor(0,marchingCubesWaterBG.TempTri.Length){firstVertex=0,vertexCount=marchingCubesWaterBG.TempVer.Length},meshFlags);
          return true;
         }
         return false;
        }
     BakeJob bakeJob;struct BakeJob:IJob{
          public int meshId;
             public void Execute(){
              Physics.BakeMesh(meshId,false);
             }
     }
      JobHandle bakeJobHandle;
     internal MeshCollider meshCollider;
        void SchedulePhysBakeMeshJob(){
         bakeJobHandle.Complete();
         bakeJobHandle=bakeJob.Schedule();
        }
     internal bool hasPhysMeshBaked{get;private set;}
        bool OnPhysMeshBaked(){
         if(bakeJobHandle.IsCompleted){
            bakeJobHandle.Complete();
          meshCollider.sharedMesh=null;
          meshCollider.sharedMesh=mesh;
          hasPhysMeshBaked=true;
          //navMeshSource.transform=transform.localToWorldMatrix;
          //VoxelSystem.singleton.navMeshSources[gameObject.GetInstanceID()]=navMeshSource;
          //VoxelSystem.singleton.navMeshMarkups[gameObject.GetInstanceID()]=navMeshMarkup;
          //VoxelSystem.singleton.navMeshSourcesCollectionChanged=true;
          //SimObjectSpawner.singleton.OnVoxelTerrainChunkPhysMeshBaked(this);
          //foreach(var gameplayer in GameplayerManagement.singleton.all){
          // gameplayer.Value.OnVoxelTerrainChunkBaked(this);
          //}
          return true;
         }
         return false;
        }
        void OnDrawGizmos(){
         #if UNITY_EDITOR
          DrawVoxelsDensity(UnityEditor.Selection.Contains(this.gameObject)||UnityEditor.Selection.Contains(tCnk.gameObject));
         #endif
        }
        VoxelWater?[]DEBUG_DRAW_WATER_DENSITY_VOXELS=null;
        #if UNITY_EDITOR
        void DrawVoxelsDensity(bool selected=false){
         if(tCnk!=null&&(tCnk.DEBUG_DRAW_WATER_DENSITY||selected)&&tCnk.id!=null){
          if(tCnk.DEBUG_DRAW_WATER_DENSITY_AUTO_ENABLE){
           tCnk.DEBUG_DRAW_WATER_DENSITY|=selected;
          }
          if(VoxelSystem.Concurrent.waterCache_rwl.TryEnterReadLock(0)){
           try{
            if(!VoxelSystem.Concurrent.waterCache.TryGetValue(tCnk.id.Value.cnkIdx,out var cache)){
             return;
            }
            if(DEBUG_DRAW_WATER_DENSITY_VOXELS==null){
             DEBUG_DRAW_WATER_DENSITY_VOXELS=new VoxelWater?[VoxelsPerChunk];
            }
            //Log.DebugMessage("DEBUG_DRAW_WATER_DENSITY_VOXELS:tCnk.id.Value.cCoord:"+tCnk.id.Value.cCoord+":cache.reader.BaseStream.Length:"+cache.reader.BaseStream.Length);
            lock(cache.stream){
             cache.stream.Position=0L;
             while(cache.reader.BaseStream.Position!=cache.reader.BaseStream.Length){
                int vxlIdx=WaterSpreadingMultithreaded.BinaryReadvxlIdx    (cache.reader);
              VoxelWater v=WaterSpreadingMultithreaded.BinaryReadVoxelWater(cache.reader);
              DEBUG_DRAW_WATER_DENSITY_VOXELS[vxlIdx]=v;
              //Log.DebugMessage("DEBUG_DRAW_WATER_DENSITY_VOXELS:v.density:"+v.density);
             }
            }
           }catch{
            throw;
           }finally{
            VoxelSystem.Concurrent.waterCache_rwl.ExitReadLock();
           }
          }
          if(DEBUG_DRAW_WATER_DENSITY_VOXELS==null){
           return;
          }
          //Log.DebugMessage("DrawVoxelsDensity:'DEBUG_DRAW_WATER_DENSITY_VOXELS!=null'");
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
           bool wakeUp           =voxel.wakeUp;
           double density        =voxel.density;
           double previousDensity=voxel.previousDensity;
           double absorbValue=0.0d;
           double spreadValue=0.0d;
           if(tCnk.DEBUG_DRAW_WATER_CHANGES){
            if      (voxel.density<voxel.previousDensity){
             absorbValue=Math.Max(voxel.density,voxel.previousDensity);
            }else if(voxel.density>voxel.previousDensity){
             spreadValue=voxel.density;
            }else if(voxel.wakeUp){
             if(voxel.density>0.0d){
              
             }
            }
            if      (absorbValue!=0.0d){
             Gizmos.color=Color.yellow;
             Vector3 center=new Vector3(
               tCnk.id.Value.cnkRgn.x-Mathf.FloorToInt(Width/2.0f),
               -Mathf.FloorToInt(Height/2.0f),
               tCnk.id.Value.cnkRgn.y-Mathf.FloorToInt(Depth/2.0f)
              )+vCoord1;
             Gizmos.DrawWireCube(center,Vector3.one*(float)(absorbValue*.01d));
            }else if(spreadValue!=0.0d){
             Gizmos.color=Color.blue;
             Vector3 center=new Vector3(
               tCnk.id.Value.cnkRgn.x-Mathf.FloorToInt(Width/2.0f),
               -Mathf.FloorToInt(Height/2.0f),
               tCnk.id.Value.cnkRgn.y-Mathf.FloorToInt(Depth/2.0f)
              )+vCoord1;
             Gizmos.DrawWireCube(center,Vector3.one*(float)(spreadValue*.01d));
            }
           }
           if(density!=0d){
            //Log.DebugMessage("DrawVoxelsDensity:density:"+density);
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
           }
          }}}
         }
        }
        #endif
    }
}