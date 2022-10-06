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
         meshRenderer=GetComponentInChildren<MeshRenderer>();
         color=CloudParticleSystem.singleton.sharedColor;
         alpha=new CloudParticleAlpha(value:color.a     ,CloudParticleSystem.singleton.alphaSettings.minIncrementSpeed);
         angle=new CloudParticleAngle(value:Vector3.zero,CloudParticleSystem.singleton.angleSettings.minIncrementSpeed);
         orbit=new CloudParticleOrbit(value:Vector3.zero,CloudParticleSystem.singleton.orbitSettings.minIncrementSpeed);
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
         orbit.value+=orbit.incrementSpeed*Time.deltaTime;
         orbit.value.x=orbit.value.x%360f;
         orbit.value.y=orbit.value.y%360f;
         orbit.value.z=orbit.value.z%360f;
         //Log.DebugMessage("orbit.value:"+orbit.value);
         transform.position=CloudParticleSystem.singleton.cloudsCamera.transform.position+Quaternion.Euler(orbit.value)*Vector3.forward*distance.value;
         angle.value+=angle.incrementSpeed*Time.deltaTime;
         angle.value.x=angle.value.x%360f;
         angle.value.y=angle.value.y%360f;
         angle.value.z=angle.value.z%360f;
         //Log.DebugMessage("angle.value:"+angle.value);
         transform.rotation=Quaternion.LookRotation((CloudParticleSystem.singleton.cloudsCamera.transform.position-transform.position).normalized,Vector3.zero);
         meshRenderer.transform.localRotation=Quaternion.Euler(0f,0f,angle.value.z);
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
     internal CloudParticleAngle angle;
        internal struct CloudParticleAngle{
         internal Vector3 value;
         internal Vector3 incrementSpeed;
            internal CloudParticleAngle(Vector3 value,Vector3 incrementSpeed){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
            }
        }
     internal CloudParticleOrbit orbit;
        internal struct CloudParticleOrbit{
         internal Vector3 value;
         internal Vector3 incrementSpeed;
            internal CloudParticleOrbit(Vector3 value,Vector3 incrementSpeed){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
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