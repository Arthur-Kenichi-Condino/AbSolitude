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
         orbit=new CloudParticleOrbit(value:Vector3.zero,CloudParticleSystem.singleton.orbitSettings.minIncrementSpeed,CloudParticleSystem.singleton.orbitSettings.reverseInterval,CloudParticleSystem.singleton.orbitSettings.changeIncrementSpeedValueInterval);
         distance=new CloudParticleDistance(value:CloudParticleSystem.singleton.distanceSettings.min,CloudParticleSystem.singleton.distanceSettings.minIncrementSpeed,CloudParticleSystem.singleton.distanceSettings.reverseInterval,CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueInterval);
         scale=new CloudParticleScale(value:Vector3.one ,CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed,CloudParticleSystem.singleton.scaleSettings.reverseInterval,CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueInterval);
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
            orbit.reverseTimer-=Time.deltaTime;
            distance.reverseTimer-=Time.deltaTime;
         if(distance.reverseTimer<=0f){
            distance.reverseTimer=CloudParticleSystem.singleton.distanceSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.distanceSettings.reverseChance;
          if(reverse){
           //Log.DebugMessage("reverse cloud particle distance.incrementSpeed");
           distance.incrementSpeed=-distance.incrementSpeed;
           Log.DebugMessage("reversed cloud particle distance.incrementSpeed to:"+distance.incrementSpeed);
          }
         }
            distance.changeIncrementSpeedValueTimer-=Time.deltaTime;
         if(distance.changeIncrementSpeedValueTimer<=0f){
            distance.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueChance;
          if(change){
           //Log.DebugMessage("change cloud particle distance.incrementSpeed value");
           int sign=Math.Sign(distance.incrementSpeed);
           distance.incrementSpeed=Mathf.Lerp(
            CloudParticleSystem.singleton.distanceSettings.minIncrementSpeed,
            CloudParticleSystem.singleton.distanceSettings.maxIncrementSpeed,
            CloudParticleSystem.singleton.random.Next(0,101)/100f
           );
           if(sign!=0){
            distance.incrementSpeed*=sign;
           }
           Log.DebugMessage("changed cloud particle distance.incrementSpeed value to:"+distance.incrementSpeed);
          }
         }
            scale.reverseTimer-=Time.deltaTime;
         if(scale.reverseTimer<=0f){
            scale.reverseTimer=CloudParticleSystem.singleton.scaleSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.scaleSettings.reverseChance;
          if(reverse){
           scale.incrementSpeed.x=CloudParticleSystem.singleton.random.Next(0,2)==0?scale.incrementSpeed.x:-scale.incrementSpeed.x;
           scale.incrementSpeed.y=CloudParticleSystem.singleton.random.Next(0,2)==0?scale.incrementSpeed.y:-scale.incrementSpeed.y;
           scale.incrementSpeed.z=CloudParticleSystem.singleton.random.Next(0,2)==0?scale.incrementSpeed.z:-scale.incrementSpeed.z;
           Log.DebugMessage("reversed cloud particle scale.incrementSpeed to:"+scale.incrementSpeed);
          }
         }
            scale.changeIncrementSpeedValueTimer-=Time.deltaTime;
         if(scale.changeIncrementSpeedValueTimer<=0f){
            scale.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueChance;
          if(change){
           Vector3Int sign=new Vector3Int(
            Math.Sign(scale.incrementSpeed.x),
            Math.Sign(scale.incrementSpeed.y),
            Math.Sign(scale.incrementSpeed.z)
           );
           scale.incrementSpeed.x=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.x,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.x,CloudParticleSystem.singleton.random.Next(0,101)/100f);
           scale.incrementSpeed.y=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.y,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.y,CloudParticleSystem.singleton.random.Next(0,101)/100f);
           scale.incrementSpeed.z=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.z,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.z,CloudParticleSystem.singleton.random.Next(0,101)/100f);
           if(sign.x!=0){scale.incrementSpeed.x*=sign.x;}
           if(sign.y!=0){scale.incrementSpeed.y*=sign.y;}
           if(sign.z!=0){scale.incrementSpeed.z*=sign.z;}
           Log.DebugMessage("changed cloud particle scale.incrementSpeed value to:"+scale.incrementSpeed);
          }
         }
         scale.value+=scale.incrementSpeed*Time.deltaTime;
         if(scale.value.x>=CloudParticleSystem.singleton.scaleSettings.max.x){scale.value.x=CloudParticleSystem.singleton.scaleSettings.max.x;}
         if(scale.value.x<=CloudParticleSystem.singleton.scaleSettings.min.x){scale.value.x=CloudParticleSystem.singleton.scaleSettings.min.x;}
         if(scale.value.y>=CloudParticleSystem.singleton.scaleSettings.max.y){scale.value.y=CloudParticleSystem.singleton.scaleSettings.max.y;}
         if(scale.value.y<=CloudParticleSystem.singleton.scaleSettings.min.y){scale.value.y=CloudParticleSystem.singleton.scaleSettings.min.y;}
         if(scale.value.z>=CloudParticleSystem.singleton.scaleSettings.max.z){scale.value.z=CloudParticleSystem.singleton.scaleSettings.max.z;}
         if(scale.value.z<=CloudParticleSystem.singleton.scaleSettings.min.z){scale.value.z=CloudParticleSystem.singleton.scaleSettings.min.z;}
         meshRenderer.transform.localScale=scale.value;
         distance.value+=distance.incrementSpeed*Time.deltaTime;
         if(distance.value>=CloudParticleSystem.singleton.distanceSettings.max){distance.value=CloudParticleSystem.singleton.distanceSettings.max;}
         if(distance.value<=CloudParticleSystem.singleton.distanceSettings.min){distance.value=CloudParticleSystem.singleton.distanceSettings.min;}
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
         internal float reverseTimer;
         internal float changeIncrementSpeedValueTimer;
            internal CloudParticleOrbit(Vector3 value,Vector3 incrementSpeed,float reverseTimer,float changeIncrementSpeedValueTimer){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
             this.reverseTimer=reverseTimer;
             this.changeIncrementSpeedValueTimer=changeIncrementSpeedValueTimer;
            }
        }
     internal CloudParticleDistance distance;
        internal struct CloudParticleDistance{
         internal float value;
         internal float incrementSpeed;
         internal float reverseTimer;
         internal float changeIncrementSpeedValueTimer;
            internal CloudParticleDistance(float value,float incrementSpeed,float reverseTimer,float changeIncrementSpeedValueTimer){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
             this.reverseTimer=reverseTimer;
             this.changeIncrementSpeedValueTimer=changeIncrementSpeedValueTimer;
            }
        }
     internal CloudParticleScale scale;
        internal struct CloudParticleScale{
         internal Vector3 value;
         internal Vector3 incrementSpeed;
         internal float reverseTimer;
         internal float changeIncrementSpeedValueTimer;
            internal CloudParticleScale(Vector3 value,Vector3 incrementSpeed,float reverseTimer,float changeIncrementSpeedValueTimer){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
             this.reverseTimer=reverseTimer;
             this.changeIncrementSpeedValueTimer=changeIncrementSpeedValueTimer;
            }
        }
    }
}