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
     internal readonly int maxHits;
        internal AStarPathfindingBackgroundContainer(int width,int depth,int maxHits){
         this.width=width;
         this.depth=depth;
         this.maxHits=maxHits;
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
     internal Vector3 dest;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
      internal JobHandle doRaycastsHandle{get;set;}
        internal enum Execution{
         GetGround,
        }
     internal Execution execution;
    }
    internal class AStarPathfindingMultithreaded:BaseMultithreaded<AStarPathfindingBackgroundContainer>{
        protected override void Execute(){
         switch(container.execution){
          case(AStarPathfindingBackgroundContainer.Execution.GetGround):{
           Log.DebugMessage("Execution.GetGround");
           QueryParameters queryParameters=new QueryParameters(AStarPathfindingHelper.aStarLayer);
           Vector3Int vCoord1=new Vector3Int(0,0,0);
           int i=0;
           for(vCoord1.x=0             ;vCoord1.x<container.width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<container.depth;vCoord1.z++){
           }}
           break;
          }
         }
        }
    }
}