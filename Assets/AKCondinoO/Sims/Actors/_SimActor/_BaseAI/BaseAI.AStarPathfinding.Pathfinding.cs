#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected Vector3?MyAStarDest=null;
        internal void GetAStarPath(Vector3 dest){
         MyAStarDest=dest;
        }
     bool settingGetGroundRays;
     Vector3?curGetAStarDest=null;
        internal void ManualUpdateAStarPathfinding(){
         Log.DebugMessage("ManualUpdateAStarPathfinding");
         if(settingGetGroundRays){
             if(aStarPathfindingBG.IsCompleted(AStarPathfinding.singleton.aStarPathfindingBGThreads[0].IsRunning)){
              Log.DebugMessage("aStarPathfindingBG.IsCompleted");
              settingGetGroundRays=false;
             }
         }else{
             if(curGetAStarDest!=MyAStarDest){
              if(MyAStarDest!=null){
               aStarPathfindingBG.dest=MyAStarDest.Value;
               curGetAStarDest=MyAStarDest;
               settingGetGroundRays=true;
               AStarPathfindingMultithreaded.Schedule(aStarPathfindingBG);
              }
             }
         }
        }
    }
}