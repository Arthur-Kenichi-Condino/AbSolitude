#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Pathfinding{
    internal class AStarPathfindingHelper{
     internal static int aStarLayer;
      internal static readonly string[]aStarLayerNames=new string[]{
       "VoxelTerrain",
       "Construction",
      };
     internal static int aStarGetObstaclesLayer;
      internal static readonly string[]aStarGetObstaclesLayerNames=new string[]{
       "Default",
       "VoxelTerrain",
       "Construction",
      };
        internal static void SetAStarPathfindingSettings(){
         for(int i=0;i<aStarLayerNames.Length;++i){
          if(i==0){
           aStarLayer= LayerMask.GetMask(aStarLayerNames[i]);
          }else{
           aStarLayer|=LayerMask.GetMask(aStarLayerNames[i]);
          }
         }
         for(int i=0;i<aStarGetObstaclesLayerNames.Length;++i){
          if(i==0){
           aStarGetObstaclesLayer= LayerMask.GetMask(aStarGetObstaclesLayerNames[i]);
          }else{
           aStarGetObstaclesLayer|=LayerMask.GetMask(aStarGetObstaclesLayerNames[i]);
          }
         }
        }
    }
}