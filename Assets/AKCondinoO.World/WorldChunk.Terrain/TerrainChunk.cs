using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Bootstrap.SharedCoroutines;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Terrain{
    internal class TerrainChunk{
     private readonly WorldChunk chunk;
     private NativeList<Vertex>tempVer;
     private NativeList<UInt32>tempTri;
     internal Mesh mesh;
        internal TerrainChunk(WorldChunk chunk){
         this.chunk=chunk;
         tempVer=new NativeList<Vertex>(Allocator.Persistent);
         tempTri=new NativeList<UInt32>(Allocator.Persistent);
        }
        internal void Destroy(){
                updateJob=null;
         runningUpdateJob=null;
         if(tempVer.IsCreated)tempVer.Dispose();
         if(tempTri.IsCreated)tempTri.Dispose();
        }
     internal UpdateJob        updateJob;
     internal UpdateJob runningUpdateJob;
        internal void DoUpdateJob(){
         //Logs.Message(Logs.LogType.Debug,"'schedule updateJob'");
         updateJob=updateJobPool.Rent();
         updateJob.chunk=this.chunk;
         SharedCoroutines.Schedule(updateJob);
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
            public void SetContainerDataOnBegin(){
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             pendingMarchingCubes=true;
            }
            public bool LoopExecuteStep(bool flush=false){
             if(!chunk.terrain.ValidJob(this)){return false;}
             if(chunk.terrain.runningUpdateJob==null){chunk.terrain.runningUpdateJob=this;}
             if(chunk.terrain.runningUpdateJob!=this){return true;}
             if(waitingMarchingCubes){return true;}
             if(pendingMarchingCubes){
              DoMarchingCubesJob doMarchingCubesJob=doMarchingCubesJobPool.Rent();
              doMarchingCubesJob.updateJob=this;
              bool scheduled=ThreadDispatcher.TrySchedule(doMarchingCubesJob);
              if(!scheduled){
               doMarchingCubesJobPool.Return(doMarchingCubesJob);
               return(false);
              }
              waitingMarchingCubes=true;
              pendingMarchingCubes=false;
              return true;
             }
             return false;
            }
            public void OnCompletedDoAtEnd(){
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
            public void SetContainerDataAtMainThread(){
             chunk=updateJob.chunk;
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
             context=MarchingCubesCore.marchingCubesContextPool.Rent();
             context.tempVer=chunk.terrain.tempVer;
             context.tempTri=chunk.terrain.tempTri;
            }
            public void BackgroundExecute(){
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
              Logs.Message(Logs.LogType.Debug,"context.tempVer.Length:"+context.tempVer.Length);
              chunk.terrain.debugDrawMeshWireframeVer.Clear();for(int i=0;i<context.tempVer.Length;i++){chunk.terrain.debugDrawMeshWireframeVer.Add(context.tempVer[i]);}
              chunk.terrain.debugDrawMeshWireframeTri.Clear();for(int i=0;i<context.tempTri.Length;i++){chunk.terrain.debugDrawMeshWireframeTri.Add(context.tempTri[i]);}
             }
             MarchingCubesCore.marchingCubesContextPool.Return(context);
             context=null;
             updateJob.waitingMarchingCubes=false;
             doMarchingCubesJobPool.Return(this);
            }
        }
     private readonly List<Vertex>debugDrawMeshWireframeVer=new();
     private readonly List<UInt32>debugDrawMeshWireframeTri=new();
        internal void Gizmos(){
         #if UNITY_EDITOR
          if(chunk.debugDrawMeshWireframe){
           DrawGizmos.DrawMeshWireframe(debugDrawMeshWireframeVer,debugDrawMeshWireframeTri,Color.green);
          }
         #endif
        }
    }
}