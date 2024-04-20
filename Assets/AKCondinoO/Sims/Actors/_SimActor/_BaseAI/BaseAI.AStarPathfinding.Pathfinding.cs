#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Pathfinding;
using AKCondinoO.Voxels;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
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
     bool doingGetGroundRaycasts;
     bool buildingHeap;
     bool settingGetGroundRays;
     Vector3?curGetAStarDest=null;
        internal void ManualUpdateAStarPathfinding(){
         Log.DebugMessage("ManualUpdateAStarPathfinding");
         if(doingGetGroundRaycasts){
         }else{
             if(settingGetGroundRays){
                 if(aStarPathfindingBG.IsCompleted(AStarPathfinding.singleton.aStarPathfindingBGThreads[0].IsRunning)){
                  Log.DebugMessage("aStarPathfindingBG.IsCompleted");
                  settingGetGroundRays=false;
                  if(aStarPathfindingBG.GetGroundRays.Length>0){
                   Log.DebugMessage("aStarPathfindingBG.GetGroundRays.Length>0");
                   ScheduleGetGroundRaycastCommandJob();
                  }
                  doingGetGroundRaycasts=true;
                 }
             }else{
                 if(curGetAStarDest!=MyAStarDest){
                  if(MyAStarDest!=null){
                   aStarPathfindingBG.nodeWidth =GetRadius()*2f;
                   aStarPathfindingBG.nodeHeight=GetHeight();
                   aStarPathfindingBG.dest=MyAStarDest.Value;
                   aStarPathfindingBG.execution=AStarPathfindingBackgroundContainer.Execution.GetGround;
                   curGetAStarDest=MyAStarDest;
                   settingGetGroundRays=true;
                   AStarPathfindingMultithreaded.Schedule(aStarPathfindingBG);
                  }
                 }
             }
         }
        }
        void ScheduleGetGroundRaycastCommandJob(){
         aStarPathfindingBG.getGroundRaycastCommandJobHandle.Complete();
         aStarPathfindingBG.getGroundRaycastCommandJobHandle=RaycastCommand.ScheduleBatch(
          commands:aStarPathfindingBG.GetGroundRays.AsArray(),
           results:aStarPathfindingBG.GetGroundHits.AsArray(),
          minCommandsPerJob:1,maxHits:1
         );
        }
    }
}