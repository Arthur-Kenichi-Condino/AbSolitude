#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal Renderer[]renderers;
        protected virtual void EnableRenderers(){
         foreach(Renderer renderer in renderers){
          renderer.enabled=true;
         }
        }
        protected virtual void DisableRenderers(){
         foreach(Renderer renderer in renderers){
          renderer.enabled=false;
         }
        }
        protected virtual void UpdateRenderers(bool updateFlag){
         if(updateFlag){
          if(Core.singleton.currentRenderingTargetCamera!=null){
           //Log.DebugMessage("UpdateRenderers:update");
           float viewDistance=Vector3.Distance(Core.singleton.currentRenderingTargetCamera.transform.position,transform.root.position);
           float opacity=(VoxelSystem.fadeEndDis-viewDistance)/(VoxelSystem.fadeEndDis-VoxelSystem.fadeStartDis);
           opacity=Mathf.Clamp01(opacity);
           //Log.DebugMessage("transparencyStrength:"+transparencyStrength);
           foreach(Renderer renderer in renderers){
            foreach(Material material in renderer.materials){
             if(opacity!=1f){
             }else{
             }
             Color c=material.GetColor("_Color");
             c.a=opacity;
             material.SetColor("_Color",c);
            }
           }
          }
         }
        }
    }
}