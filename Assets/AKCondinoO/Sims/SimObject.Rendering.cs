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
          if(Core.singleton.currentCamera!=null){
           //Log.DebugMessage("UpdateRenderers:update");
           float viewDistance=Vector3.Distance(Core.singleton.currentCamera.transform.position,transform.root.position);
           float transparencyStrength=(VoxelSystem.fadeEndDis-viewDistance)/(VoxelSystem.fadeEndDis-VoxelSystem.fadeStartDis);
           transparencyStrength=Mathf.Clamp01(transparencyStrength);
           //Log.DebugMessage("transparencyStrength:"+transparencyStrength);
           foreach(Renderer renderer in renderers){
            foreach(Material material in renderer.materials){
             Color c=material.GetColor("_Color");
             c.a=transparencyStrength;
             material.SetColor("_Color",c);
            }
           }
          }
         }
        }
    }
}