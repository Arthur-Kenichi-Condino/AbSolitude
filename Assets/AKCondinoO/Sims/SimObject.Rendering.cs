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
      internal readonly Dictionary<Material,Shader>materialShader=new();
        protected virtual void EnableRenderers(){
         updateRenderersFlag=true;
        }
        protected virtual void DisableRenderers(){
         foreach(Renderer renderer in renderers){
          renderer.enabled=false;
         }
        }
     [SerializeField]internal bool autoChangeMaterialsToFade=true;
     [SerializeField]internal bool autoToggleMaterialsToFade=false;
        protected virtual void UpdateRenderers(){
         if(updateRenderersFlag){
          if(Core.singleton.currentRenderingTargetCamera!=null&&Camera.current!=null){
           //Log.DebugMessage("UpdateRenderers:update");
           float viewDistance=Vector3.Distance(Core.singleton.currentRenderingTargetCamera.transform.position,transform.root.position);
           float opacity=(VoxelSystem.fadeEndDis-viewDistance)/(VoxelSystem.fadeEndDis-VoxelSystem.fadeStartDis);
           opacity=Mathf.Clamp01(opacity);
           //Log.DebugMessage("opacity:"+opacity);
           foreach(Renderer renderer in renderers){
            foreach(Material material in renderer.materials){
             if(autoToggleMaterialsToFade){
              if(opacity!=1f){
               if(material.shader!=RenderingUtil.StandardShader){
                material.shader=RenderingUtil.StandardShader;
                RenderingUtil.SetupStandardShaderMaterialBlendMode(material,RenderingUtil.BlendMode.Fade);
               }
              }else{
               if(material.shader==RenderingUtil.StandardShader){
                RenderingUtil.SetupStandardShaderMaterialBlendMode(material,RenderingUtil.BlendMode.Opaque);
                material.shader=materialShader[material];
               }
              }
             }
             Color c=material.GetColor("_Color");
             c.a=opacity;
             material.SetColor("_Color",c);
            }
            renderer.enabled=interactionsEnabled;
           }
           updateRenderersFlag=false;
          }
         }
        }
    }
}