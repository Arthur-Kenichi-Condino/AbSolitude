using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.Bootstrap.SharedCoroutines;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Terrain{
    internal class TerrainChunkBuilder{
     private readonly WorldChunk chunk;
     private readonly WorldChunkTerrain terrain;
     private readonly MeshData meshData;
        internal class MeshData{
         internal NativeList<Vertex>tempVer;
         internal NativeList<UInt32>tempTri;
         internal BakeJob bakeJob;
         internal JobHandle bakeJobHandle;
            internal struct BakeJob:IJob{
             public int meshId;
                public void Execute(){
                 Physics.BakeMesh(meshId,false);
                }
            }
            internal MeshData(int meshId){
             tempVer=new NativeList<Vertex>(Allocator.Persistent);
             tempTri=new NativeList<UInt32>(Allocator.Persistent);
             bakeJob=new BakeJob(){
              meshId=meshId,
             };
            }
        }
     internal Mesh mesh;
     private MeshUpdateFlags meshFlags=
      MeshUpdateFlags.DontValidateIndices|
      MeshUpdateFlags.DontNotifyMeshUsers|
      MeshUpdateFlags.DontRecalculateBounds|
      MeshUpdateFlags.DontResetBoneBounds;
        internal TerrainChunkBuilder(WorldChunk chunk,WorldChunkTerrain terrain){
         this.chunk=chunk;this.terrain=terrain;
         mesh=new Mesh(){
          bounds=chunk.bounds,
         };
         meshData=new(mesh.GetInstanceID());
        }
        internal void Destroy(){
         updateJob=null;
         GameObject.Destroy(mesh);
         mesh=null;
         if(meshData.tempVer.IsCreated)meshData.tempVer.Dispose();
         if(meshData.tempTri.IsCreated)meshData.tempTri.Dispose();
        }
        internal void OnChunkPool(){
         if(updateJob!=null){updateJob.CancelGraciously();}
        }
     internal UpdateJob updateJob;
        internal void DoUpdateJob(){
         Logs.Debug(()=>"'doing update job for':"+chunk.cnkRgn);
         debugDrawMeshWireframeVer.Clear();
         debugDrawMeshWireframeTri.Clear();
         if(this.updateJob==null||chunk.cCoord!=updateJob.cCoord||!ValidJob(this.updateJob)){
          var updateJob=UpdateJob.pool.Rent();
          updateJob.dependency=this.updateJob;
          updateJob.builder=this;
          if(!SharedCoroutines.TrySchedule(updateJob)){
           UpdateJob.pool.Return(updateJob);
          }
          if(updateJob.dependency!=null){updateJob.dependency.CancelGraciously();}
          this.updateJob=updateJob;
         }
        }
     internal bool cancelled;
        internal void OnUpdateJobDone(UpdateJob updateJob,bool cancelled){
         if(this.updateJob==updateJob){
          this.updateJob=null;
          this.cancelled=cancelled;
          terrain.OnGenerateUpdate(cancelled);
         }
        }
        internal bool ValidJob(UpdateJob updateJob){
         if(this.chunk==null){return false;}
         if(this.updateJob!=updateJob){return false;}
         return true;
        }
        internal class UpdateJob:SharedCoroutineContainerJob{
         internal static readonly Utilities.ObjectPool<UpdateJob>pool=
          Pool.GetPool<UpdateJob>(
           "",
           ()=>new(),
           (UpdateJob item)=>{
            item.dependency=null;
            item.isCancelledCanStop=false;
            item.builder=null;
            item.pendingMarchingCubes=false;
            item.waitingMarchingCubes=false;
            item.pendingBakeJob      =false;
            item.waitingBakeJob      =false;
            item.doMarchingCubesJob=null;
           }
          );
         public SharedCoroutineContainerJob dependency{
          get{return doFirst;}set{doFirst=value;}
         }
         private SharedCoroutineContainerJob doFirst;
         internal TerrainChunkBuilder builder;
         internal Vector2Int cCoord;
         internal Vector2Int cnkRgn;
         internal bool waitingMarchingCubes;
         internal bool pendingMarchingCubes;
         internal bool waitingBakeJob;
         internal bool pendingBakeJob;
         private DoMarchingCubesJob doMarchingCubesJob;
         public bool isCancelledCanStop{
          get{return cancelled&&!waitingMarchingCubes;}
          set{cancelled=value;}
         }
         private bool cancelled;
         readonly System.Diagnostics.Stopwatch sw=new();
            public void CancelGraciously(){
             cancelled=true;
            }
            public void OnScheduleSetContainerData(){
             sw.Restart();
             cancelled=false;
             cCoord=builder.chunk.cCoord;
             cnkRgn=builder.chunk.cnkRgn;
             pendingMarchingCubes=true;
             waitingMarchingCubes=false;
             pendingBakeJob      =true;
             waitingBakeJob      =false;
            }
            public int OnLoopExecuteStep(bool flush=false){
             bool valid=builder.ValidJob(this);if(!valid){cancelled=true;}
             if(flush){
              if(doMarchingCubesJob!=null){
               doMarchingCubesJob.updateJob=null;
              }
             }
             if(waitingMarchingCubes){
              if(flush){
               return -1;
              }
              if(cancelled){
               //Logs.Debug("'wasting time doing marching cubes on chunk that changed...':"+sw.ElapsedMilliseconds+" ms");
               doMarchingCubesJob.CancelGraciously();
              }
              return 0;
             }
             if(waitingBakeJob){
              ref var bakeJobHandle=ref builder.meshData.bakeJobHandle;
              if(flush){
               bakeJobHandle.Complete();
               return -1;
              }
              if(bakeJobHandle.IsCompleted){
               bakeJobHandle.Complete();
               waitingBakeJob=false;
              }
             }
             if(cancelled){return -1;}
             if(!flush){
              if(pendingMarchingCubes){
               doMarchingCubesJob=DoMarchingCubesJob.pool.Rent();
               doMarchingCubesJob.updateJob=this;
               bool scheduled=ThreadDispatcher.TrySchedule(doMarchingCubesJob,7);
               if(!scheduled){
                DoMarchingCubesJob.pool.Return(doMarchingCubesJob);
                doMarchingCubesJob=null;
                return -1;
               }
               waitingMarchingCubes=true;//  ...job is scheduled
               pendingMarchingCubes=false;
               return 1;
              }
              if(pendingBakeJob){
               ref var tempVer=ref builder.meshData.tempVer;
               ref var tempTri=ref builder.meshData.tempTri;
               var mesh=builder.mesh;
               var meshFlags=builder.meshFlags;
               bool resize;
               if(resize=tempVer.Length>mesh.vertexCount){
                mesh.SetVertexBufferParams(tempVer.Length,Vertex.layout);
               }
               mesh.SetVertexBufferData(tempVer.AsArray(),0,0,tempVer.Length,0,meshFlags);
               if(resize){
                mesh.SetIndexBufferParams(tempTri.Length,IndexFormat.UInt32);
               }
               mesh.SetIndexBufferData(tempTri.AsArray(),0,0,tempTri.Length,meshFlags);
               mesh.subMeshCount=1;
               mesh.SetSubMesh(0,new SubMeshDescriptor(0,tempTri.Length){firstVertex=0,vertexCount=tempVer.Length},meshFlags);
               waitingBakeJob=true;
               pendingBakeJob=false;
               ref var bakeJob=ref builder.meshData.bakeJob;
               ref var bakeJobHandle=ref builder.meshData.bakeJobHandle;
               bakeJobHandle.Complete();
               bakeJobHandle=bakeJob.Schedule();
               return 1;
              }
             }
             return -1;//  ...end
            }
            public void OnLoopCompleted(){
             bool valid=builder.ValidJob(this);if(!valid){cancelled=true;}
             builder.OnUpdateJobDone(this,cancelled);
             sw.Stop();
             Logs.Debug(()=>"'terrain update job execution time':"+sw.ElapsedMilliseconds+" ms");
             UpdateJob.pool.Return(this);
            }
        }
        internal class DoMarchingCubesJob:MultithreadedContainerJob{
         internal static readonly Utilities.ObjectPool<DoMarchingCubesJob>pool=
          Pool.GetPool<DoMarchingCubesJob>(
           "",
           ()=>new(),
           (DoMarchingCubesJob item)=>{
            item.updateJob=null;
           }
          );
         internal UpdateJob updateJob;
         internal Vector2Int cCoord;
         internal Vector2Int cnkRgn;
         private MarchingCubesContext context;
         private bool cancelled;
            public void CancelGraciously(){
             if(!cancelled){
              Volatile.Write(ref context.cancel,1);
             }
             cancelled=true;
            }
            public void OnDoScheduleSetContainerData(){
             cancelled=false;
             cCoord=updateJob.builder.chunk.cCoord;
             cnkRgn=updateJob.builder.chunk.cnkRgn;
             context=MarchingCubesContext.pool.Rent();
             context.meshData=updateJob.builder.meshData;
             context.biomeContext=BiomesConfigurationContext.pool.Rent();
            }
         readonly System.Diagnostics.Stopwatch sw=new();
            public void ExecuteAtBackgroundThread(){
             sw.Restart();
             BiomesConfigurationSnapshot.IsReading();
             try{
              context.meshData.tempVer.Clear();
              context.meshData.tempTri.Clear();
              MarchingCubesCore.BuildMeshData(new(-1,0,-1),new(Width,Height-1,Depth),cCoord,context);
             }catch(Exception e){
              Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }finally{
              BiomesConfigurationSnapshot.StoppedReading();
             }
             sw.Stop();
             Logs.Debug(()=>"'build terrain mesh execution time':"+sw.ElapsedMilliseconds+" ms");
            }
            public void OnCompletedDoAtMainThread(){
             if(updateJob==null){
              cancelled=true;
             }else{
              bool valid=updateJob.builder.ValidJob(updateJob);if(!valid){cancelled=true;}
             }
             if(!cancelled){
              if(updateJob.builder.chunk.debugDrawMeshWireframe){
               ref var tempVer=ref context.meshData.tempVer;
               ref var tempTri=ref context.meshData.tempTri;
               updateJob.builder.debugDrawMeshWireframeVer.Clear();for(int i=0;i<tempVer.Length;i++){updateJob.builder.debugDrawMeshWireframeVer.Add(tempVer[i]);}
               updateJob.builder.debugDrawMeshWireframeTri.Clear();for(int i=0;i<tempTri.Length;i++){updateJob.builder.debugDrawMeshWireframeTri.Add(tempTri[i]);}
              }
             }
             BiomesConfigurationContext.pool.Return(context.biomeContext);
             context.biomeContext=null;
             MarchingCubesContext.pool.Return(context);
             context=null;
             if(updateJob!=null){updateJob.waitingMarchingCubes=false;}
             DoMarchingCubesJob.pool.Return(this);
            }
        }
     private readonly List<Vertex>debugDrawMeshWireframeVer=new();
     private readonly List<UInt32>debugDrawMeshWireframeTri=new();
        internal void GizmosSelected(bool selected){
         #if UNITY_EDITOR
         if(chunk.debugDrawMeshWireframe){
          if(!chunk.debugDrawMeshWireframeWhenSelectedOnly||(chunk.debugDrawMeshWireframeWhenSelectedOnly&&selected)){
           DrawGizmos.DrawMeshWireframe(debugDrawMeshWireframeVer,debugDrawMeshWireframeTri,Color.gray,chunk.debugDrawMeshWireframeDrawTriangles,chunk.debugDrawMeshWireframeDrawNormals,new Vector3(chunk.cnkRgn.x,0,chunk.cnkRgn.y)+new Vector3(Width/2f,Height/2f,Depth/2f));
          }
         }
         #endif
        }
    }
}