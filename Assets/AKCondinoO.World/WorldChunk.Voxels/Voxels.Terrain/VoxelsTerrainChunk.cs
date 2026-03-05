using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World.Voxels.Terrain{
    internal class VoxelTerrainChunk{
     private readonly WorldChunk chunk;
        internal VoxelTerrainChunk(WorldChunk chunk){
         this.chunk=chunk;
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
            public void SetContainerDataAtMainThread(){
             cCoord=chunk.cCoord;
             cnkRgn=chunk.cnkRgn;
            }
            public void BackgroundExecute(){
             Logs.Message(Logs.LogType.Debug,"DoMarchingCubesJob.BackgroundExecute");
            }
            public void OnCompletedDoAtMainThread(){
             if(chunk!=null){
              if(cCoord==chunk.cCoord){
               chunk.bounds.center=chunk.transform.position=new Vector3(
                cnkRgn.x+(Width/2f),
                Height/2f,
                cnkRgn.y+(Depth/2f)
               );
              }
             }
             doMarchingCubesJobPool.Return(this);
            }
        }
    }
}