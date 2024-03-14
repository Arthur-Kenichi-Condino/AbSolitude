using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO{
    internal static class PhysUtil{
     internal static int considerGroundLayer;
      internal static readonly string[]considerGroundLayerNames=new string[]{
       "VoxelTerrain",
       "Construction",
      };
     internal static int physObstaclesLayer;
      internal static readonly string[]physObstaclesLayerNames=new string[]{
       "Default",
       "VoxelTerrain",
       "Construction",
      };
     internal static int shootingHitsLayer;
      internal static readonly string[]shootingHitsLayerNames=new string[]{
       "Default",
       "VoxelTerrain",
       "Construction",
       "Hurtbox",
      };
        internal static void SetUtil(){
         SetLayer(ref considerGroundLayer,considerGroundLayerNames);
         SetLayer(ref physObstaclesLayer,physObstaclesLayerNames);
         SetLayer(ref shootingHitsLayer,shootingHitsLayerNames);
        }
        static void SetLayer(ref int layer,string[]layerNames){
         for(int i=0;i<layerNames.Length;++i){
          if(i==0){
           layer= LayerMask.GetMask(layerNames[i]);
          }else{
           layer|=LayerMask.GetMask(layerNames[i]);
          }
         }
        }
        /// <summary>
        ///  [https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html]
        /// </summary>
        internal static bool LayerMaskContains(int layerMask,int layer){
         return layerMask==(layerMask|(1<<layer));
        }
    }
}