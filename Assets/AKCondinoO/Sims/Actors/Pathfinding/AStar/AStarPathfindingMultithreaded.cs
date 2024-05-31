#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static AKCondinoO.Sims.Actors.Pathfinding.AStarPathfinding;
namespace AKCondinoO.Sims.Actors.Pathfinding{
    internal class AStarPathfindingBackgroundContainer:BackgroundContainer{
     internal readonly int width;
     internal readonly int depth;
     internal readonly int height;
        internal AStarPathfindingBackgroundContainer(int width,int depth,int height,int getObstaclesMaxHits=8){
         this.width=width;
         this.depth=depth;
         this.height=height;
         this.getObstaclesMaxHits=getObstaclesMaxHits;
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
     internal float nodeWidth;
     internal float nodeHeight;
     internal Vector3 dest;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
      internal JobHandle getGroundRaycastCommandJobHandle{get;set;}
     internal NativeList<OverlapBoxCommand>GetObstaclesCommands;
     internal NativeList<ColliderHit      >GetObstaclesOverlaps;
      internal readonly int getObstaclesMaxHits;
      internal JobHandle getObstaclesCommandJobHandle{get;set;}
     internal NativeList<BoxcastCommand>GetFloorRays;
     internal NativeList<RaycastHit    >GetFloorHits;
     internal NativeList<BoxcastCommand>GetCeilingRays;
     internal NativeList<RaycastHit    >GetCeilingHits;
     internal readonly Dictionary<Vector3Int,Node>nodes=new();
        internal enum Execution{
         GetGround,
         BuildHeap,
        }
     internal Execution execution;
    }
    internal class AStarPathfindingMultithreaded:BaseMultithreaded<AStarPathfindingBackgroundContainer>{
        protected override void Execute(){
         switch(container.execution){
          case(AStarPathfindingBackgroundContainer.Execution.GetGround):{
           Log.DebugMessage("Execution.GetGround");
           container.GetGroundRays.Clear();
           container.GetGroundHits.Clear();
           container.GetObstaclesCommands.Clear();
           container.GetObstaclesOverlaps.Clear();
           QueryParameters getGroundQueryParameters=new QueryParameters(AStarPathfindingHelper.aStarLayer,true);
           QueryParameters getObstaclesQueryParameters=new QueryParameters(AStarPathfindingHelper.aStarGetObstaclesLayer,true);
           Vector3Int nCoord1=new Vector3Int(0,0,0);
           int c=0;
           for(nCoord1.x=0             ;nCoord1.x<container.width;nCoord1.x++){
           for(nCoord1.z=0             ;nCoord1.z<container.depth;nCoord1.z++){
            Vector3 center=nCoord1;
                    center.x*=container.nodeWidth;
                    center.z*=container.nodeWidth;
                    center.x+=container.dest.x-(container.width*container.nodeWidth)/2f+container.nodeWidth/2f;
                    center.z+=container.dest.z-(container.depth*container.nodeWidth)/2f+container.nodeWidth/2f;
            for(nCoord1.y=0;nCoord1.y<container.height;nCoord1.y++){
             center.y=nCoord1.y*container.nodeHeight;
             center.y+=container.dest.y-(container.height*container.nodeHeight)/2f+container.nodeHeight/2f;
             if(!container.nodes.TryGetValue(nCoord1,out Node node)){
              container.nodes.Add(nCoord1,node=new Node());
             }
             node.center=center;
             Vector3 from=center+new Vector3(0f,container.nodeHeight/2f,0f);
             RaycastCommand raycast=new RaycastCommand(from,Vector3.down,getGroundQueryParameters,container.nodeHeight);
             container.GetGroundRays.AddNoResize(raycast         );
             container.GetGroundHits.AddNoResize(new RaycastHit());
             OverlapBoxCommand overlapBox=new OverlapBoxCommand(
              center,
              new Vector3(
               container.nodeWidth /2f,
               container.nodeHeight/2f,
               container.nodeWidth /2f
              ),
              Quaternion.identity,
              getObstaclesQueryParameters
             );
              container.GetObstaclesCommands.AddNoResize(overlapBox       );
             for(int i=0;i<container.getObstaclesMaxHits;++i){
              container.GetObstaclesOverlaps.AddNoResize(new ColliderHit());
             }
             ++c;
            }
           }}
           break;
          }
          case(AStarPathfindingBackgroundContainer.Execution.BuildHeap):{
           Log.DebugMessage("Execution.BuildHeap");
           Vector3Int nCoord1=new Vector3Int(0,0,0);
           int c=0;
           for(nCoord1.x=0             ;nCoord1.x<container.width;nCoord1.x++){
           for(nCoord1.z=0             ;nCoord1.z<container.depth;nCoord1.z++){
            for(nCoord1.y=0;nCoord1.y<container.height;nCoord1.y++){
             var ground=container.GetGroundHits.ElementAt(c);
             bool hasGround=ground.colliderInstanceID!=0;
             //Log.DebugMessage("hasGround:"+hasGround);
             bool invalidObstaclesOverlap=false;
             bool hasObstacle=false;
             for(int i=0;i<container.getObstaclesMaxHits;++i){
              int index=(c*container.getObstaclesMaxHits)+i;
              var obstacle=container.GetObstaclesOverlaps.ElementAt(index);
              invalidObstaclesOverlap|=obstacle.instanceID==0;
              hasObstacle|=!invalidObstaclesOverlap;
              if(!invalidObstaclesOverlap){
              }
             }
             //Log.DebugMessage("hasObstacle:"+hasObstacle);
             if(container.nodes.TryGetValue(nCoord1,out Node node)){
              node.hasGround=hasGround;
              node.hasObstacle=hasObstacle;
             }
             ++c;
            }
           }}
           break;
          }
         }
        }
    }
}