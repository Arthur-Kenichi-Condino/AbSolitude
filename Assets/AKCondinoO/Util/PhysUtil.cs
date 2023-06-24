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
        internal static void SetUtil(){
         for(int i=0;i<considerGroundLayerNames.Length;++i){
          if(i==0){
           considerGroundLayer=1<<LayerMask.NameToLayer(considerGroundLayerNames[i]);
          }else{
           considerGroundLayer=considerGroundLayer|(1<<LayerMask.NameToLayer(considerGroundLayerNames[i]));
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