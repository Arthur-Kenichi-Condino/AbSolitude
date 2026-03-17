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
     internal UpdateJob updateJob;
        internal void DoUpdateJob(){
         //Logs.Message(Logs.LogType.Debug,"'schedule updateJob'");
         debugDrawMeshWireframeVer.Clear();
         debugDrawMeshWireframeTri.Clear();
         var updateJob=updateJobPool.Rent();
         updateJob.dependency=this.updateJob;
         updateJob.chunk=this.chunk;
         if(!SharedCoroutines.TrySchedule(updateJob)){
          updateJobPool.Return(updateJob);
         }
         this.updateJob=updateJob;
        }
        internal bool ValidJob(UpdateJob updateJob){
         if(this.chunk==null){return false;}
         if(this.chunk!=updateJob.chunk){return false;}
         if(this.chunk.cCoord!=updateJob.cCoord){return false;}
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
         readonly System.Diagnostics.Stopwatch sw=new();
            public void OnScheduleSetContainerData(){
             sw.Restart();
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             pendingMarchingCubes=true;
             waitingMarchingCubes=false;
             pendingBakeJob      =true;
             waitingBakeJob      =false;
            }
            public bool OnLoopExecuteStep(bool flush=false){
             if(waitingMarchingCubes){if(flush){return false;}return true;}
             if(waitingBakeJob){
              if(flush){
               return false;
              }
              waitingBakeJob=false;
             }
             if(!chunk.terrain.ValidJob(this)){return false;}
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
               return true;
              }
             }
             return false;//  ...end
            }
            public void OnLoopCompleted(){
             sw.Stop();
             Logs.Message(Logs.LogType.Debug,"'terrain update job execution time':"+sw.ElapsedMilliseconds+" ms");
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
             context.meshData=chunk.terrain.meshData;
             context.biomeContext=BiomesConfigurationSnapshot.biomesConfigurationContextPool.Rent();
            }
         readonly System.Diagnostics.Stopwatch sw=new();
            public void ExecuteAtBackgroundThread(){
             sw.Restart();
             BiomesConfigurationSnapshot.IsReading();
             try{
              context.meshData.tempVer.Clear();
              context.meshData.tempTri.Clear();
              //Logs.Message(Logs.LogType.Debug,"DoMarchingCubesJob.BackgroundExecute");
              MarchingCubesCore.BuildMeshData(new(-1,0,-1),new(Width,Height-1,Depth),cCoord,context);
             }catch(Exception e){
              Logs.Message(Logs.LogType.Error,e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }finally{
              BiomesConfigurationSnapshot.StoppedReading();
             }
             sw.Stop();
             Logs.Message(Logs.LogType.Debug,"'build terrain mesh execution time':"+sw.ElapsedMilliseconds+" ms");
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
               ref var tempVer=ref context.meshData.tempVer;
               ref var tempTri=ref context.meshData.tempTri;
               chunk.terrain.debugDrawMeshWireframeVer.Clear();for(int i=0;i<tempVer.Length;i++){chunk.terrain.debugDrawMeshWireframeVer.Add(tempVer[i]);}
               chunk.terrain.debugDrawMeshWireframeTri.Clear();for(int i=0;i<tempTri.Length;i++){chunk.terrain.debugDrawMeshWireframeTri.Add(tempTri[i]);}
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