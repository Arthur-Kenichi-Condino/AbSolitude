#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Pathfinding;
using AKCondinoO.Voxels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     [SerializeField]internal int aStarPathfindingWidth=VoxelSystem.Width;
     [SerializeField]internal int aStarPathfindingDepth=VoxelSystem.Depth;
     [SerializeField]internal int aStarPathfindingHeight=16;
     protected Vector3?MyAStarDest=null;
        internal void GetAStarPath(Vector3 dest){
         MyAStarDest=dest;
        }
     bool settingGetGroundRays;
     bool buildingHeap;
     Vector3?curGetAStarDest=null;
        internal void ManualUpdateAStarPathfinding(){
         Log.DebugMessage("ManualUpdateAStarPathfinding");
         if(buildingHeap){
             if(aStarPathfindingBG.IsCompleted(AStarPathfinding.singleton.aStarPathfindingBGThreads[0].IsRunning)){
              Log.DebugMessage("aStarPathfindingBG.IsCompleted");
              buildingHeap=false;
             }
         }else{
             if(curGetAStarDest!=MyAStarDest){
              if(MyAStarDest!=null){
               aStarPathfindingBG.nodeWidth=GetRadius()*2f;
               aStarPathfindingBG.nodeHeight=GetHeight();
               aStarPathfindingBG.dest=MyAStarDest.Value;
               aStarPathfindingBG.execution=AStarPathfindingBackgroundContainer.Execution.BuildHeap;
               curGetAStarDest=MyAStarDest;
               buildingHeap=true;
               AStarPathfindingMultithreaded.Schedule(aStarPathfindingBG);
              }
             }
         }
        }
    }
}