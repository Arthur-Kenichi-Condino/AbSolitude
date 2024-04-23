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
      internal readonly List<(RaycastHit hit,bool hitTerrain)>getGroundHitsManaged=new();
     internal NativeList<OverlapBoxCommand>GetObstaclesCommands;
     internal NativeList<ColliderHit      >GetObstaclesOverlaps;
      internal readonly int getObstaclesMaxHits;
      internal JobHandle getGroundRaycastCommandJobHandle{get;set;}
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
           QueryParameters queryParameters=new QueryParameters(AStarPathfindingHelper.aStarLayer,true);
           Vector3Int vCoord1=new Vector3Int(0,0,0);
           int i=0;
           for(vCoord1.x=0             ;vCoord1.x<container.width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<container.depth;vCoord1.z++){
            Vector3 center=vCoord1;
                    center.x*=container.nodeWidth;
                    center.z*=container.nodeWidth;
                    center.x+=container.dest.x-(container.width*container.nodeWidth)/2f+container.nodeWidth/2f;
                    center.z+=container.dest.z-(container.depth*container.nodeWidth)/2f+container.nodeWidth/2f;
            for(vCoord1.y=0;vCoord1.y<container.height;vCoord1.y++){
             center.y=vCoord1.y*container.nodeHeight;
             center.y+=container.dest.y-(container.height*container.nodeHeight)/2f+container.nodeHeight/2f;
             if(!container.nodes.TryGetValue(vCoord1,out Node node)){
              container.nodes.Add(vCoord1,node=new Node());
             }
             node.center=center;
             Vector3 from=center+new Vector3(0f,container.nodeHeight/2f,0f);
             RaycastCommand raycast=new RaycastCommand(from,Vector3.down,queryParameters,container.nodeHeight);
             container.GetGroundRays.AddNoResize(raycast         );
             container.GetGroundHits.AddNoResize(new RaycastHit());
             for(int j=0;j<container.getObstaclesMaxHits;++j){
             }
             ++i;
            }
           }}
           break;
          }
          case(AStarPathfindingBackgroundContainer.Execution.BuildHeap):{
           Log.DebugMessage("Execution.BuildHeap");
           break;
          }
         }
        }
    }
}