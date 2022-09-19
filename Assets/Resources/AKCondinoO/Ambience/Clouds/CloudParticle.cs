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
         orbit=new CloudParticleOrbit(value:Vector3.zero);
         distance=new CloudParticleDistance(value:CloudParticleSystem.singleton.distanceSettings.min,CloudParticleSystem.singleton.distanceSettings.minIncrementSpeed);
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
          //
         }
         //transform.rotation=;
         transform.position=CloudParticleSystem.singleton.cloudsCamera.transform.position+Quaternion.Euler(orbit.value)*Vector3.forward*distance.value;
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
     internal CloudParticleOrbit orbit;
        internal struct CloudParticleOrbit{
         internal Vector3 value;
            internal CloudParticleOrbit(Vector3 value){
             this.value=value;
            }
        }
     internal CloudParticleDistance distance;
        internal struct CloudParticleDistance{
         internal float value;
         internal float incrementSpeed;
            internal CloudParticleDistance(float value,float incrementSpeed){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
            }
        }
    }
}