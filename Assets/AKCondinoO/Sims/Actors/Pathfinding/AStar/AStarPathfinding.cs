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
     internal readonly List<AStarPathfindingBackgroundContainer>aStarPathfindingContainers=new List<AStarPathfindingBackgroundContainer>();
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
         foreach(AStarPathfindingBackgroundContainer container in aStarPathfindingContainers){
          container.IsCompleted(aStarPathfindingBGThreads[0].IsRunning,-1);
         }
         if(AStarPathfindingMultithreaded.Clear()!=0){
          Log.Error("AStarPathfindingMultithreaded will stop with pending work");
         }
         AStarPathfindingMultithreaded.Stopped=true;
         for(int i=0;i<aStarPathfindingBGThreads.Length;++i){
                       aStarPathfindingBGThreads[i].Wait();
         }
         foreach(AStarPathfindingBackgroundContainer container in aStarPathfindingContainers){
          container.Dispose();
          container.doRaycastsHandle.Complete();
          if(container.GetGroundRays.IsCreated)container.GetGroundRays.Dispose();
          if(container.GetGroundHits.IsCreated)container.GetGroundHits.Dispose();
         }
         aStarPathfindingContainers.Clear();
        }
        internal class Node:IHeapItem<Node>{
         public int heapIndex{get;set;}
         public float F{get;private set;}//  heuristics
         public float G{get{return g;}set{g=value;F=g+h;}}float g;//  node dis to start
         public float H{get{return h;}set{h=value;F=g+h;}}float h;//  node dis to target
            public int CompareTo(Node toCompare){
             int comparison=F.CompareTo(toCompare.F);
             if(comparison==0){
              comparison=H.CompareTo(toCompare.H);
             }
            return -comparison;}
        }
    }
}