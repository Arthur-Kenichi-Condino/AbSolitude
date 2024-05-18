#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Pathfinding;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.VisualScripting;
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
     bool buildingHeap;
     bool runningGetGroundNativeToManagedCoroutine;
     bool doingGetGroundRaycasts;
     bool settingGetGroundRays;
     Vector3?curGetAStarDest=null;
        internal void ManualUpdateAStarPathfinding(){
         //Log.DebugMessage("ManualUpdateAStarPathfinding");
         if(buildingHeap){
             if(OnBuiltHeap()){
              Log.DebugMessage("aStarPathfindingBG.IsCompleted");
              buildingHeap=false;
             }
         }else{
             if(runningGetGroundNativeToManagedCoroutine){
                 if(OnRanGetGroundNativeToManagedCoroutine()){
                  runningGetGroundNativeToManagedCoroutine=false;
                  aStarPathfindingBG.execution=AStarPathfindingBackgroundContainer.Execution.BuildHeap;
                  buildingHeap=true;
                  AStarPathfindingMultithreaded.Schedule(aStarPathfindingBG);
                 }
             }else{
                 if(doingGetGroundRaycasts){
                     if(OnGetGroundRaycastsDone()){
                      doingGetGroundRaycasts=false;
                      runningGetGroundNativeToManagedCoroutine=true;
                      runNativeToManagedCoroutineMode=0;
                     }
                 }else{
                     if(settingGetGroundRays){
                         if(OnSetGetGroundRays()){
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
         }
        }
     internal Coroutine nativeToManagedCoroutine;
      WaitUntil runNativeToManagedCoroutine;
       int runNativeToManagedCoroutineMode;
        IEnumerator NativeToManagedCoroutine(){
         runNativeToManagedCoroutineMode=-1;
         runNativeToManagedCoroutine=new WaitUntil(()=>{return runNativeToManagedCoroutineMode>=0;});
         System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
         _Loop:{
          yield return runNativeToManagedCoroutine;
          sw.Restart();
          switch(runNativeToManagedCoroutineMode){
           case(0):{
            if(aStarPathfindingBG.GetGroundHits.Length>0){
             Log.DebugMessage("aStarPathfindingBG.GetGroundHits.Length>0");
            }
            Vector3Int nCoord1=new Vector3Int(0,0,0);
            int c=0;
            for(nCoord1.x=0             ;nCoord1.x<aStarPathfindingBG.width;nCoord1.x++){
            for(nCoord1.z=0             ;nCoord1.z<aStarPathfindingBG.depth;nCoord1.z++){
             for(nCoord1.y=0;nCoord1.y<aStarPathfindingBG.height;nCoord1.y++){
              ++c;
             }
            }}
            break;
           }
          }
          runNativeToManagedCoroutineMode=-1;
         }
         sw.Stop();
         Log.DebugMessage("NativeToManagedCoroutine sw elapsed time:"+sw.ElapsedMilliseconds+" ms");
         goto _Loop;
        }
        bool OnSetGetGroundRays(){
         if(aStarPathfindingBG.IsCompleted(AStarPathfinding.singleton.aStarPathfindingBGThreads[0].IsRunning)){
          return true;
         }
         return false;
        }
        void ScheduleGetGroundRaycastCommandJob(){
         aStarPathfindingBG.getGroundRaycastCommandJobHandle.Complete();
         aStarPathfindingBG.getGroundRaycastCommandJobHandle=RaycastCommand.ScheduleBatch(
          commands:aStarPathfindingBG.GetGroundRays.AsArray(),
           results:aStarPathfindingBG.GetGroundHits.AsArray(),
          minCommandsPerJob:1,maxHits:1
         );
         ScheduleGetObstaclesCommandJob();
        }
        void ScheduleGetObstaclesCommandJob(){
         aStarPathfindingBG.getObstaclesCommandJobHandle.Complete();
         aStarPathfindingBG.getObstaclesCommandJobHandle=OverlapBoxCommand.ScheduleBatch(
          commands:aStarPathfindingBG.GetObstaclesCommands.AsArray(),
           results:aStarPathfindingBG.GetObstaclesOverlaps.AsArray(),
          minCommandsPerJob:1,maxHits:aStarPathfindingBG.getObstaclesMaxHits
         );
        }
        bool OnGetGroundRaycastsDone(){
         if(
          aStarPathfindingBG.getGroundRaycastCommandJobHandle.IsCompleted&&
          aStarPathfindingBG.    getObstaclesCommandJobHandle.IsCompleted
         ){
          aStarPathfindingBG.getGroundRaycastCommandJobHandle.Complete();
          aStarPathfindingBG.    getObstaclesCommandJobHandle.Complete();
          Log.DebugMessage("aStarPathfindingBG.getGroundRaycastCommandJobHandle.IsCompleted");
          return true;
         }
         return false;
        }
        bool OnRanGetGroundNativeToManagedCoroutine(){
         if(runNativeToManagedCoroutineMode<0){
          return true;
         }
         return false;
        }
        bool OnBuiltHeap(){
         if(aStarPathfindingBG.IsCompleted(AStarPathfinding.singleton.aStarPathfindingBGThreads[0].IsRunning)){
          return true;
         }
         return false;
        }
    }
}