#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Pathfinding{
    internal class AStarPathfinding:MonoBehaviour,ISingletonInitialization{
     internal static AStarPathfinding singleton{get;set;}
     internal readonly AStarPathfindingMultithreaded[]aStarPathfindingBGThreads=new AStarPathfindingMultithreaded[Environment.ProcessorCount];
     internal readonly List<(BaseAI ai,AStarPathfindingBackgroundContainer aStar)>aStarPathfindingContainers=new();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         AStarPathfindingMultithreaded.Stopped=false;
         for(int i=0;i<aStarPathfindingBGThreads.Length;++i){
                       aStarPathfindingBGThreads[i]=new AStarPathfindingMultithreaded();
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("AStarPathfinding:OnDestroyingCoreEvent");
         foreach(var container in aStarPathfindingContainers){
          container.aStar.IsCompleted(aStarPathfindingBGThreads[0].IsRunning,-1);
          container.aStar.getGroundRaycastCommandJobHandle.Complete();
          if(container.ai!=null&&container.ai.gameObject!=null){
           if(container.ai.nativeToManagedCoroutine!=null){container.ai.StopCoroutine(container.ai.nativeToManagedCoroutine);}
          }
         }
         if(AStarPathfindingMultithreaded.Clear()!=0){
          Log.Error("AStarPathfindingMultithreaded will stop with pending work");
         }
         AStarPathfindingMultithreaded.Stopped=true;
         for(int i=0;i<aStarPathfindingBGThreads.Length;++i){
                       aStarPathfindingBGThreads[i].Wait();
         }
         foreach(var container in aStarPathfindingContainers){
          container.aStar.Dispose();
          if(container.aStar.GetGroundRays.IsCreated)container.aStar.GetGroundRays.Dispose();
          if(container.aStar.GetGroundHits.IsCreated)container.aStar.GetGroundHits.Dispose();
          if(container.aStar.GetObstaclesCommands.IsCreated)container.aStar.GetObstaclesCommands.Dispose();
          if(container.aStar.GetObstaclesOverlaps.IsCreated)container.aStar.GetObstaclesOverlaps.Dispose();
         }
         aStarPathfindingContainers.Clear();
        }
        internal class Node:IHeapItem<Node>{
         public int heapIndex{get;set;}
         public float heuristics{get;private set;}//  heuristics
         public float disToStart {get{return g;}set{g=value;heuristics=g+h;}}float g;//  node dis to start
         public float disToTarget{get{return h;}set{h=value;heuristics=g+h;}}float h;//  node dis to target
         public Vector3 center{get;set;}
            public int CompareTo(Node toCompare){
             int comparison=heuristics.CompareTo(toCompare.heuristics);
             if(comparison==0){
              comparison=disToTarget.CompareTo(toCompare.disToTarget);
             }
            return -comparison;}
        }
    }
}