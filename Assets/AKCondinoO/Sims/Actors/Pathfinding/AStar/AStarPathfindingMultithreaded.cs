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
     internal Vector3 dest;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
      internal JobHandle doRaycastsHandle{get;set;}
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class AStarPathfindingMultithreaded:BaseMultithreaded<AStarPathfindingBackgroundContainer>{
        protected override void Execute(){
        }
    }
}