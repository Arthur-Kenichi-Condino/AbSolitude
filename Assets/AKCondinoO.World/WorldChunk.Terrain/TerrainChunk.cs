using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
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
         if(tempVer.IsCreated)tempVer.Dispose();
         if(tempTri.IsCreated)tempTri.Dispose();
        }
        internal void DoMarchingCubes(){
         DoMarchingCubesJob doMarchingCubesJob=doMarchingCubesJobPool.Rent();
         doMarchingCubesJob.chunk=this.chunk;
         bool scheduled=ThreadDispatcher.TrySchedule(doMarchingCubesJob);
        }
     static readonly Utilities.ObjectPool<DoMarchingCubesJob>doMarchingCubesJobPool=
      Pool.GetPool<DoMarchingCubesJob>(
       "",
       ()=>new(),
       (DoMarchingCubesJob item)=>{}
      );
        internal class DoMarchingCubesJob:MultithreadedContainerJob{
         internal WorldChunk chunk;
         private Vector2Int cCoord;
         private Vector2Int cnkRgn;
         private MarchingCubesContext context;
            public void SetContainerDataAtMainThread(){
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
             if(chunk!=null){
              if(cCoord==chunk.cCoord){
               chunk.bounds.center=chunk.transform.position=new Vector3(
                cnkRgn.x+(Width/2f),
                Height/2f,
                cnkRgn.y+(Depth/2f)
               );
               Logs.Message(Logs.LogType.Debug,"context.tempVer.Length:"+context.tempVer.Length);
               chunk.terrain.debugDrawMeshWireframeVer.Clear();for(int i=0;i<context.tempVer.Length;i++){chunk.terrain.debugDrawMeshWireframeVer.Add(context.tempVer[i]);}
               chunk.terrain.debugDrawMeshWireframeTri.Clear();for(int i=0;i<context.tempTri.Length;i++){chunk.terrain.debugDrawMeshWireframeTri.Add(context.tempTri[i]);}
              }
             }
             MarchingCubesCore.marchingCubesContextPool.Return(context);
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