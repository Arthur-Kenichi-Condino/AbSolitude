#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Ambience.Clouds{
    public class CloudParticle:MonoBehaviour{
     internal MeshRenderer meshRenderer;
        void Awake(){
         meshRenderer=GetComponent<MeshRenderer>();
         color=CloudParticleSystem.singleton.sharedColor;
         alpha=new CloudParticleAlpha(value:color.a,CloudParticleSystem.singleton.alphaSettings.minIncrementSpeed);
        }
     internal bool fadeIn;
      Color color;
        void Update(){
         if(fadeIn){
          if(!meshRenderer.enabled){
           meshRenderer.enabled=true;
          }
          if(alpha.value<CloudParticleSystem.singleton.alphaSettings.min){
           alpha.value+=alpha.incrementSpeed*Time.deltaTime;
          }else{
           fadeIn=false;
           Log.DebugMessage("cloud particle spawned:fadeIn=false;");
          }
         }else{
          
         }
         color.a=alpha.value;
         meshRenderer.material.SetColor("_TintColor",color);
        }
     internal CloudParticleAlpha alpha;
        internal struct CloudParticleAlpha{
         internal float value;
         internal float incrementSpeed;
            internal CloudParticleAlpha(float value,float incrementSpeed){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
            }
        }
    }
}