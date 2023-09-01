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
     [SerializeField]internal bool useMultipleMaterials=true;
      internal readonly Dictionary<Renderer,Dictionary<int,Dictionary<int,Material>>>derivedMaterials=new Dictionary<Renderer,Dictionary<int,Dictionary<int,Material>>>();
       protected readonly Dictionary<Renderer,List<Material>>derivedMaterialsBlendModeOpaque=new Dictionary<Renderer,List<Material>>();
       protected readonly Dictionary<Renderer,List<Material>>derivedMaterialsBlendModeFade  =new Dictionary<Renderer,List<Material>>();
        protected int usingDerivedMaterialsBlendMode=0;
     [SerializeField]internal bool autoSetMaterialsToFade=true;
        protected virtual void UpdateRenderers(){
         if(updateRenderersFlag){
          if((Core.singleton.currentRenderingTargetCamera!=null)||Core.singleton.currentRenderingTargetCamera==Camera.main){
           //Log.DebugMessage("UpdateRenderers:update");
           float viewDistance=Vector3.Distance(Core.singleton.currentRenderingTargetCamera.transform.position,transform.root.position);
           float opacity=(VoxelSystem.fadeEndDis-viewDistance)/(VoxelSystem.fadeEndDis-VoxelSystem.fadeStartDis);
           opacity=Mathf.Clamp01(opacity);
           //Log.DebugMessage("opacity:"+opacity);
           foreach(Renderer renderer in renderers){
            if(useMultipleMaterials){
             if(opacity!=1f){
              if(usingDerivedMaterialsBlendMode==0){
               renderer.SetMaterials(derivedMaterialsBlendModeFade  [renderer]);
               usingDerivedMaterialsBlendMode=2;
              }
              foreach(Material material in renderer.materials){
               Color c=material.GetColor("_Color");
               c.a=opacity;
               material.SetColor("_Color",c);
              }
             }else{
              if(usingDerivedMaterialsBlendMode==2){
               renderer.SetMaterials(derivedMaterialsBlendModeOpaque[renderer]);
               usingDerivedMaterialsBlendMode=0;
              }
             }
            }else{
             if(autoSetMaterialsToFade){
              foreach(Material material in renderer.materials){
               if(opacity!=1f){
                if(material.shader!=RenderingUtil.StandardShader){
                 material.shader=RenderingUtil.StandardShader;
                 RenderingUtil.SetupStandardShaderMaterialBlendMode(material,RenderingUtil.BlendMode.Fade,true);
                }
                Color c=material.GetColor("_Color");
                c.a=opacity;
                material.SetColor("_Color",c);
               }else{
                if(material.shader==RenderingUtil.StandardShader){
                 material.shader=materialShader[material];
                 Color c=material.GetColor("_Color");
                 c.a=1f;
                 material.SetColor("_Color",c);
                }
               }
              }
             }
            }
            if(Core.singleton.isServer){
             renderer.enabled=interactionsEnabled;
            }
            if(Core.singleton.isClient){
             if(!IsOwner){
              renderer.enabled=true;
             }
            }
           }
           updateRenderersFlag=false;
          }
         }
        }
    }
}