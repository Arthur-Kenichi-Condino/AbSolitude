using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static AKCondinoO.Bootstrap.SharedCoroutines;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Terrain{
    internal class TerrainChunk{
     private readonly WorldChunk chunk;
     private NativeList<Vertex>tempVer;
     private NativeList<UInt32>tempTri;
     internal Mesh mesh;
     private MeshUpdateFlags meshFlags=
      MeshUpdateFlags.DontValidateIndices|
      MeshUpdateFlags.DontNotifyMeshUsers|
      MeshUpdateFlags.DontRecalculateBounds|
      MeshUpdateFlags.DontResetBoneBounds;
        internal TerrainChunk(WorldChunk chunk){
         this.chunk=chunk;
         tempVer=new NativeList<Vertex>(Allocator.Persistent);
         tempTri=new NativeList<UInt32>(Allocator.Persistent);
         mesh=new Mesh(){
          bounds=chunk.bounds,
         };
        }
        internal void Destroy(){
                updateJob=null;
         runningUpdateJob=null;
         GameObject.Destroy(mesh);
         mesh=null;
         if(tempVer.IsCreated)tempVer.Dispose();
         if(tempTri.IsCreated)tempTri.Dispose();
        }
     internal UpdateJob        updateJob;
     internal UpdateJob runningUpdateJob;
        internal void DoUpdateJob(){
         //Logs.Message(Logs.LogType.Debug,"'schedule updateJob'");
         debugDrawMeshWireframeVer.Clear();
         debugDrawMeshWireframeTri.Clear();
         updateJob=updateJobPool.Rent();
         updateJob.chunk=this.chunk;
         if(!SharedCoroutines.TrySchedule(updateJob)){
          updateJobPool.Return(updateJob);
         }
        }
        internal bool ValidJob(UpdateJob updateJob){
         if(this.chunk==null){return false;}
         if(this.updateJob!=updateJob){return false;}
         if(this.chunk.cCoord!=updateJob.cCoord){return false;}
         if(this.chunk!=updateJob.chunk){return false;}
         if(this.chunk.terrain!=updateJob.chunk.terrain){return false;}
         return true;
        }
     static readonly Utilities.ObjectPool<UpdateJob>updateJobPool=
      Pool.GetPool<UpdateJob>(
       "",
       ()=>new(),
       (UpdateJob item)=>{
        item.chunk=null;
        item.waitingMarchingCubes=false;
       }
      );
        internal class UpdateJob:SharedCoroutineContainerJob{
         internal WorldChunk chunk;
         internal Vector2Int cCoord;
         internal Vector2Int cnkRgn;
         internal bool waitingMarchingCubes;
         internal bool pendingMarchingCubes;
         internal bool waitingBakeJob;
         internal bool pendingBakeJob;
            public void OnScheduleSetContainerData(){
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             pendingMarchingCubes=true;
             waitingMarchingCubes=false;
             pendingBakeJob      =true;
             waitingBakeJob      =false;
            }
            public bool OnLoopExecuteStep(bool flush=false){
             if(!chunk.terrain.ValidJob(this)){return false;}
             if(chunk.terrain.runningUpdateJob==null){chunk.terrain.runningUpdateJob=this;}
             if(chunk.terrain.runningUpdateJob!=this){if(flush){return false;}return true;}
             if(waitingMarchingCubes){if(flush){return false;}return true;}
             if(waitingBakeJob){
              if(flush){
               return false;
              }
              waitingBakeJob=false;
             }
             if(!flush){
              if(pendingMarchingCubes){
               DoMarchingCubesJob doMarchingCubesJob=doMarchingCubesJobPool.Rent();
               doMarchingCubesJob.updateJob=this;
               bool scheduled=ThreadDispatcher.TrySchedule(doMarchingCubesJob);
               if(!scheduled){
                doMarchingCubesJobPool.Return(doMarchingCubesJob);
                return false;
               }
               waitingMarchingCubes=true;//  ...job is scheduled
               pendingMarchingCubes=false;
               return true;
              }
              if(pendingBakeJob){
               var tempVer=chunk.terrain.tempVer;
               var tempTri=chunk.terrain.tempTri;
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
               return true;
              }
             }
             return false;//  ...end
            }
            public void OnLoopCompleted(){
             if(chunk.terrain.ValidJob(this)){
              if(chunk.terrain.runningUpdateJob==this){chunk.terrain.runningUpdateJob=null;}
             }
             updateJobPool.Return(this);
            }
        }
     static readonly Utilities.ObjectPool<DoMarchingCubesJob>doMarchingCubesJobPool=
      Pool.GetPool<DoMarchingCubesJob>(
       "",
       ()=>new(),
       (DoMarchingCubesJob item)=>{
        item.updateJob=null;
        item.chunk=null;
       }
      );
        internal class DoMarchingCubesJob:MultithreadedContainerJob{
         internal UpdateJob updateJob;
         internal WorldChunk chunk;
         internal Vector2Int cCoord;
         internal Vector2Int cnkRgn;
         private MarchingCubesContext context;
            public void OnScheduleSetContainerDataAtMainThread(){
             chunk=updateJob.chunk;
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             context=MarchingCubesCore.marchingCubesContextPool.Rent();
             context.tempVer=chunk.terrain.tempVer;
             context.tempTri=chunk.terrain.tempTri;
             context.biomeContext=BiomesConfigurationSnapshot.biomesConfigurationContextPool.Rent();
            }
            public void ExecuteAtBackgroundThread(){
             context.tempVer.Clear();
             context.tempTri.Clear();
             //Logs.Message(Logs.LogType.Debug,"DoMarchingCubesJob.BackgroundExecute");
             MarchingCubesCore.BuildMeshData(new(-1,0,-1),new(Width,Height-1,Depth),cCoord,context);
            }
            public void OnCompletedDoAtMainThread(){
             if(chunk.terrain.ValidJob(updateJob)){
              chunk.transform.position=chunk.bounds.center=new Vector3(
               cnkRgn.x+(Width/2f),
               Height/2f,
               cnkRgn.y+(Depth/2f)
              );
              //Logs.Message(Logs.LogType.Debug,"context.tempVer.Length:"+context.tempVer.Length);
              if(chunk.debugDrawMeshWireframe){
               chunk.terrain.debugDrawMeshWireframeVer.Clear();for(int i=0;i<context.tempVer.Length;i++){chunk.terrain.debugDrawMeshWireframeVer.Add(context.tempVer[i]);}
               chunk.terrain.debugDrawMeshWireframeTri.Clear();for(int i=0;i<context.tempTri.Length;i++){chunk.terrain.debugDrawMeshWireframeTri.Add(context.tempTri[i]);}
              }
             }
             BiomesConfigurationSnapshot.biomesConfigurationContextPool.Return(context.biomeContext);
             context.biomeContext=null;
             MarchingCubesCore.marchingCubesContextPool.Return(context);
             context=null;
             updateJob.waitingMarchingCubes=false;
             doMarchingCubesJobPool.Return(this);
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