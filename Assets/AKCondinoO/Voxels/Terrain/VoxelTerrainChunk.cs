#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesBackgroundContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain{
    internal class VoxelTerrainChunk:MonoBehaviour{
     internal MarchingCubesBackgroundContainer marchingCubesBG=new MarchingCubesBackgroundContainer();
     internal LinkedListNode<VoxelTerrainChunk>expropriated;
     internal (Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
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
         Log.DebugMessage("Allocate NativeLists");
         marchingCubesBG.TempVer=new NativeList<Vertex>(Allocator.Persistent);
         marchingCubesBG.TempTri=new NativeList<UInt32>(Allocator.Persistent);
        }
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         bakeJobHandle.Complete();
         marchingCubesBG.IsCompleted(VoxelSystem.Singleton.marchingCubesBGThreads[0].IsRunning,-1);
         Log.DebugMessage("Deallocate NativeLists");
         if(marchingCubesBG.TempVer.IsCreated)marchingCubesBG.TempVer.Dispose();
         if(marchingCubesBG.TempTri.IsCreated)marchingCubesBG.TempTri.Dispose();
        }
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         hasPhysMeshBaked=false;
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          pendingMarchingCubes=true;
         }
        }
     bool waitingBakeJob;
     bool waitingMarchingCubes;
     bool pendingMarchingCubes;
        internal void ManualUpdate(){
            if(waitingBakeJob){
                if(OnPhysMeshBaked()){
                    waitingBakeJob=false;
                }
            }else{
                if(waitingMarchingCubes){
                    if(OnMarchingCubesDone()){
                        waitingMarchingCubes=false;
                        SchedulePhysBakeMeshJob();
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
     internal static int sMarchingCubesExecutionCount;
        bool CanBeginMarchingCubes(){
         if(sMarchingCubesExecutionCount>=VoxelSystem.Singleton._MarchingCubesExecutionCountLimit){
          return false;
         }
         if(marchingCubesBG.IsCompleted(VoxelSystem.Singleton.marchingCubesBGThreads[0].IsRunning)){
          worldBounds.center=transform.position=new Vector3(id.Value.cnkRgn.x,0,id.Value.cnkRgn.y);
          marchingCubesBG.cCoord=id.Value.cCoord;
          marchingCubesBG.cnkRgn=id.Value.cnkRgn;
          marchingCubesBG.cnkIdx=id.Value.cnkIdx;
          sMarchingCubesExecutionCount++;
          MarchingCubesMultithreaded.Schedule(marchingCubesBG);
          return true;
         }
         return false;
        }
     #region Rendering
         static readonly VertexAttributeDescriptor[]layout=new[]{
          new VertexAttributeDescriptor(VertexAttribute.Position ,VertexAttributeFormat.Float32,3),
          new VertexAttributeDescriptor(VertexAttribute.Normal   ,VertexAttributeFormat.Float32,3),
          new VertexAttributeDescriptor(VertexAttribute.Color    ,VertexAttributeFormat.Float32,4),
          new VertexAttributeDescriptor(VertexAttribute.TexCoord0,VertexAttributeFormat.Float32,2),
          new VertexAttributeDescriptor(VertexAttribute.TexCoord1,VertexAttributeFormat.Float32,2),
          new VertexAttributeDescriptor(VertexAttribute.TexCoord2,VertexAttributeFormat.Float32,2),
          new VertexAttributeDescriptor(VertexAttribute.TexCoord3,VertexAttributeFormat.Float32,2),
         };
         MeshUpdateFlags meshFlags=MeshUpdateFlags.DontValidateIndices|
                                   MeshUpdateFlags.DontNotifyMeshUsers|
                                   MeshUpdateFlags.DontRecalculateBounds|
                                   MeshUpdateFlags.DontResetBoneBounds;
         internal Mesh mesh;
     #endregion
        bool OnMarchingCubesDone(){
         if(marchingCubesBG.IsCompleted(VoxelSystem.Singleton.marchingCubesBGThreads[0].IsRunning)){
          sMarchingCubesExecutionCount--;
          bool resize;
          if(resize=marchingCubesBG.TempVer.Length>mesh.vertexCount){
           mesh.SetVertexBufferParams(marchingCubesBG.TempVer.Length,layout);
          }
          mesh.SetVertexBufferData(marchingCubesBG.TempVer.AsArray(),0,0,marchingCubesBG.TempVer.Length,0,meshFlags);
          if(resize){
           mesh.SetIndexBufferParams(marchingCubesBG.TempTri.Length,IndexFormat.UInt32);
          }
          mesh.SetIndexBufferData(marchingCubesBG.TempTri.AsArray(),0,0,marchingCubesBG.TempTri.Length,meshFlags);
          mesh.subMeshCount=1;
          mesh.SetSubMesh(0,new SubMeshDescriptor(0,marchingCubesBG.TempTri.Length){firstVertex=0,vertexCount=marchingCubesBG.TempVer.Length},meshFlags);
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
          SimObjectSpawner.Singleton.OnVoxelTerrainChunkPhysMeshBaked(this);
          return true;
         }
         return false;
        }
        #if UNITY_EDITOR
            void OnDrawGizmos(){
                //DebugGizmos.DrawBounds(worldBounds,Color.gray);
            }
        #endif
    }
}