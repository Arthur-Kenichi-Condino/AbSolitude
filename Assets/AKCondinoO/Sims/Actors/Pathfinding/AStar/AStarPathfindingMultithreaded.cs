#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Pathfinding{
    internal class AStarPathfindingBackgroundContainer:BackgroundContainer{
     internal readonly int width;
     internal readonly int depth;
     internal readonly int height;
        internal AStarPathfindingBackgroundContainer(int width,int depth,int height){
         this.width=width;
         this.depth=depth;
         this.height=height;
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
      internal JobHandle doRaycastsHandle{get;set;}
        internal enum Execution{
         BuildHeap,
         GetGround,
        }
     internal Execution execution;
    }
    internal class AStarPathfindingMultithreaded:BaseMultithreaded<AStarPathfindingBackgroundContainer>{
        protected override void Execute(){
         switch(container.execution){
          case(AStarPathfindingBackgroundContainer.Execution.BuildHeap):{
           Log.DebugMessage("Execution.BuildHeap");
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
            }
           }}
           break;
          }
          case(AStarPathfindingBackgroundContainer.Execution.GetGround):{
           Log.DebugMessage("Execution.GetGround");
           QueryParameters queryParameters=new QueryParameters(AStarPathfindingHelper.aStarLayer);
           break;
          }
         }
        }
    }
}