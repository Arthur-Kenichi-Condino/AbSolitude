using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.Bootstrap.SharedCoroutines;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Terrain{
    internal class TerrainChunk{
     private readonly WorldChunk chunk;
     private readonly MeshData meshData=new();
        internal class MeshData{
         internal NativeList<Vertex>tempVer;
         internal NativeList<UInt32>tempTri;
        }
     internal Mesh mesh;
     private MeshUpdateFlags meshFlags=
      MeshUpdateFlags.DontValidateIndices|
      MeshUpdateFlags.DontNotifyMeshUsers|
      MeshUpdateFlags.DontRecalculateBounds|
      MeshUpdateFlags.DontResetBoneBounds;
        internal TerrainChunk(WorldChunk chunk){
         this.chunk=chunk;
         meshData.tempVer=new NativeList<Vertex>(Allocator.Persistent);
         meshData.tempTri=new NativeList<UInt32>(Allocator.Persistent);
         mesh=new Mesh(){
          bounds=chunk.bounds,
         };
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
         Logs.Debug("'doing update job for':"+chunk.cnkRgn);
         debugDrawMeshWireframeVer.Clear();
         debugDrawMeshWireframeTri.Clear();
         if(this.updateJob==null||chunk.cCoord!=updateJob.cCoord||!ValidJob(this.updateJob)){
          var updateJob=UpdateJob.pool.Rent();
          updateJob.dependency=this.updateJob;
          updateJob.chunk=chunk;
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
            item.chunk=null;
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
         internal WorldChunk chunk;
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
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             pendingMarchingCubes=true;
             waitingMarchingCubes=false;
             pendingBakeJob      =true;
             waitingBakeJob      =false;
            }
            public int OnLoopExecuteStep(bool flush=false){
             bool valid=chunk.terrain.ValidJob(this);if(!valid){cancelled=true;}
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
              if(flush){
               return -1;
              }
              waitingBakeJob=false;
             }
             if(cancelled){return -1;}
             if(!flush){
              if(pendingMarchingCubes){
               doMarchingCubesJob=DoMarchingCubesJob.pool.Rent();
               doMarchingCubesJob.updateJob=this;
               bool scheduled=ThreadDispatcher.TrySchedule(doMarchingCubesJob,3);
               if(!scheduled){
                DoMarchingCubesJob.pool.Return(doMarchingCubesJob);
                return -1;
               }
               waitingMarchingCubes=true;//  ...job is scheduled
               pendingMarchingCubes=false;
               return 1;
              }
              if(pendingBakeJob){
               ref var tempVer=ref chunk.terrain.meshData.tempVer;
               ref var tempTri=ref chunk.terrain.meshData.tempTri;
               var mesh=chunk.terrain.mesh;
               var meshFlags=chunk.terrain.meshFlags;
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
               return 1;
              }
             }
             return -1;//  ...end
            }
            public void OnLoopCompleted(){
             bool valid=chunk.terrain.ValidJob(this);if(!valid){cancelled=true;}
             chunk.terrain.OnUpdateJobDone(this,cancelled);
             sw.Stop();
             Logs.Debug("'terrain update job execution time':"+sw.ElapsedMilliseconds+" ms");
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
            item.chunk=null;
           }
          );
         internal UpdateJob updateJob;
         internal WorldChunk chunk;
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
             chunk=updateJob.chunk;
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             context=MarchingCubesContext.pool.Rent();
             context.meshData=chunk.terrain.meshData;
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
             Logs.Debug("'build terrain mesh execution time':"+sw.ElapsedMilliseconds+" ms");
            }
            public void OnCompletedDoAtMainThread(){
             bool valid=chunk.terrain.ValidJob(updateJob);if(!valid){cancelled=true;}
             if(valid){
              if(!cancelled){
               chunk.transform.position=chunk.bounds.center=new Vector3(
                cnkRgn.x+(Width/2f),
                Height/2f,
                cnkRgn.y+(Depth/2f)
               );
               if(chunk.debugDrawMeshWireframe){
                ref var tempVer=ref context.meshData.tempVer;
                ref var tempTri=ref context.meshData.tempTri;
                chunk.terrain.debugDrawMeshWireframeVer.Clear();for(int i=0;i<tempVer.Length;i++){chunk.terrain.debugDrawMeshWireframeVer.Add(tempVer[i]);}
                chunk.terrain.debugDrawMeshWireframeTri.Clear();for(int i=0;i<tempTri.Length;i++){chunk.terrain.debugDrawMeshWireframeTri.Add(tempTri[i]);}
               }
              }
             }
             BiomesConfigurationContext.pool.Return(context.biomeContext);
             context.biomeContext=null;
             MarchingCubesContext.pool.Return(context);
             context=null;
             updateJob.waitingMarchingCubes=false;
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