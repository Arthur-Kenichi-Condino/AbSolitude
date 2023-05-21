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
         alpha=new CloudParticleAlpha(value:color.a     ,CloudParticleSystem.singleton.alphaSettings.minIncrementSpeed,CloudParticleSystem.singleton.alphaSettings.reverseInterval,CloudParticleSystem.singleton.alphaSettings.changeIncrementSpeedValueInterval);
         angle=new CloudParticleAngle(value:Vector3.zero,CloudParticleSystem.singleton.angleSettings.minIncrementSpeed,CloudParticleSystem.singleton.angleSettings.reverseInterval,CloudParticleSystem.singleton.angleSettings.changeIncrementSpeedValueInterval);
         angle.RandomlyReverseSpeed();
         angle.RandomlyChangeIncrementSpeedValue();
         orbit=new CloudParticleOrbit(value:Vector3.zero,CloudParticleSystem.singleton.orbitSettings.minIncrementSpeed,CloudParticleSystem.singleton.orbitSettings.reverseInterval,CloudParticleSystem.singleton.orbitSettings.changeIncrementSpeedValueInterval);
         orbit.value=new Vector3(
          Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())*360f,
          Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())*360f,
          0f
         );
         orbit.RandomlyReverseSpeed();
         orbit.RandomlyChangeIncrementSpeedValue();
         distance=new CloudParticleDistance(value:CloudParticleSystem.singleton.distanceSettings.min,CloudParticleSystem.singleton.distanceSettings.minIncrementSpeed,CloudParticleSystem.singleton.distanceSettings.reverseInterval,CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueInterval);
         distance.RandomlyReverseSpeed();
         distance.RandomlyChangeIncrementSpeedValue();
         scale=new CloudParticleScale(value:Vector3.one ,CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed,CloudParticleSystem.singleton.scaleSettings.reverseInterval,CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueInterval);
         scale.RandomlyReverseSpeed();
         scale.RandomlyChangeIncrementSpeedValue();
        }
     internal bool fadeIn;
      Color color;
        void Update(){
         if(fadeIn){
          if(!meshRenderer.enabled){
           meshRenderer.enabled=true;
           Log.DebugMessage("cloud particle spawning:fadeIn==true");
           alpha.incrementSpeed=CloudParticleSystem.singleton.alphaSettings.minIncrementSpeed;
          }
          if(alpha.value<CloudParticleSystem.singleton.alphaSettings.min){
           alpha.value+=alpha.incrementSpeed*Core.magicDeltaTimeNumber;
          }else{
           fadeIn=false;
           Log.DebugMessage("cloud particle spawned:fadeIn=false;");
           alpha.RandomlyReverseSpeed();
           alpha.RandomlyChangeIncrementSpeedValue();
          }
         }else{
          //
             alpha.reverseTimer-=Core.magicDeltaTimeNumber;
          if(alpha.reverseTimer<=0f){
             alpha.reverseTimer=CloudParticleSystem.singleton.alphaSettings.reverseInterval;
           bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.alphaSettings.reverseChance;
           if(reverse){
            alpha.RandomlyReverseSpeed();
           }
          }
             alpha.changeIncrementSpeedValueTimer-=Core.magicDeltaTimeNumber;
          if(alpha.changeIncrementSpeedValueTimer<=0f){
             alpha.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.alphaSettings.changeIncrementSpeedValueInterval;
           bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.alphaSettings.changeIncrementSpeedValueChance;
           if(change){
            alpha.RandomlyChangeIncrementSpeedValue();
           }
          }
         }
            angle.reverseTimer-=Core.magicDeltaTimeNumber;
         if(angle.reverseTimer<=0f){
            angle.reverseTimer=CloudParticleSystem.singleton.angleSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.angleSettings.reverseChance;
          if(reverse){
           angle.RandomlyReverseSpeed();
           //Log.DebugMessage("reversed cloud particle angle.incrementSpeed to:"+angle.incrementSpeed);
          }
         }
            angle.changeIncrementSpeedValueTimer-=Core.magicDeltaTimeNumber;
         if(angle.changeIncrementSpeedValueTimer<=0f){
            angle.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.angleSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.angleSettings.changeIncrementSpeedValueChance;
          if(change){
           angle.RandomlyChangeIncrementSpeedValue();
           //Log.DebugMessage("changed cloud particle angle.incrementSpeed value to:"+angle.incrementSpeed);
          }
         }
            orbit.reverseTimer-=Core.magicDeltaTimeNumber;
         if(orbit.reverseTimer<=0f){
            orbit.reverseTimer=CloudParticleSystem.singleton.orbitSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.orbitSettings.reverseChance;
          if(reverse){
           orbit.RandomlyReverseSpeed();
           //Log.DebugMessage("reversed cloud particle orbit.incrementSpeed to:"+orbit.incrementSpeed);
          }
         }
            orbit.changeIncrementSpeedValueTimer-=Core.magicDeltaTimeNumber;
         if(orbit.changeIncrementSpeedValueTimer<=0f){
            orbit.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.orbitSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.orbitSettings.changeIncrementSpeedValueChance;
          if(change){
           orbit.RandomlyChangeIncrementSpeedValue();
           //Log.DebugMessage("changed cloud particle orbit.incrementSpeed value to:"+orbit.incrementSpeed);
          }
         }
            distance.reverseTimer-=Core.magicDeltaTimeNumber;
         if(distance.reverseTimer<=0f){
            distance.reverseTimer=CloudParticleSystem.singleton.distanceSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.distanceSettings.reverseChance;
          if(reverse){
           //Log.DebugMessage("reverse cloud particle distance.incrementSpeed");
           distance.RandomlyReverseSpeed();
           //Log.DebugMessage("reversed cloud particle distance.incrementSpeed to:"+distance.incrementSpeed);
          }
         }
            distance.changeIncrementSpeedValueTimer-=Core.magicDeltaTimeNumber;
         if(distance.changeIncrementSpeedValueTimer<=0f){
            distance.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.distanceSettings.changeIncrementSpeedValueChance;
          if(change){
           //Log.DebugMessage("change cloud particle distance.incrementSpeed value");
           distance.RandomlyChangeIncrementSpeedValue();
           //Log.DebugMessage("changed cloud particle distance.incrementSpeed value to:"+distance.incrementSpeed);
          }
         }
            scale.reverseTimer-=Core.magicDeltaTimeNumber;
         if(scale.reverseTimer<=0f){
            scale.reverseTimer=CloudParticleSystem.singleton.scaleSettings.reverseInterval;
          bool reverse=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.scaleSettings.reverseChance;
          if(reverse){
           scale.RandomlyReverseSpeed();
           //Log.DebugMessage("reversed cloud particle scale.incrementSpeed to:"+scale.incrementSpeed);
          }
         }
            scale.changeIncrementSpeedValueTimer-=Core.magicDeltaTimeNumber;
         if(scale.changeIncrementSpeedValueTimer<=0f){
            scale.changeIncrementSpeedValueTimer=CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueInterval;
          bool change=Mathf.Clamp01((float)CloudParticleSystem.singleton.random.NextDouble())<CloudParticleSystem.singleton.scaleSettings.changeIncrementSpeedValueChance;
          if(change){
           scale.RandomlyChangeIncrementSpeedValue();
           //Log.DebugMessage("changed cloud particle scale.incrementSpeed value to:"+scale.incrementSpeed);
          }
         }
         scale.value+=scale.incrementSpeed*Core.magicDeltaTimeNumber;
         if(scale.value.x>=CloudParticleSystem.singleton.scaleSettings.max.x){scale.value.x=CloudParticleSystem.singleton.scaleSettings.max.x;}
         if(scale.value.x<=CloudParticleSystem.singleton.scaleSettings.min.x){scale.value.x=CloudParticleSystem.singleton.scaleSettings.min.x;}
         if(scale.value.y>=CloudParticleSystem.singleton.scaleSettings.max.y){scale.value.y=CloudParticleSystem.singleton.scaleSettings.max.y;}
         if(scale.value.y<=CloudParticleSystem.singleton.scaleSettings.min.y){scale.value.y=CloudParticleSystem.singleton.scaleSettings.min.y;}
         if(scale.value.z>=CloudParticleSystem.singleton.scaleSettings.max.z){scale.value.z=CloudParticleSystem.singleton.scaleSettings.max.z;}
         if(scale.value.z<=CloudParticleSystem.singleton.scaleSettings.min.z){scale.value.z=CloudParticleSystem.singleton.scaleSettings.min.z;}
         meshRenderer.transform.localScale=scale.value;
         distance.value+=distance.incrementSpeed*Core.magicDeltaTimeNumber;
         if(distance.value>=CloudParticleSystem.singleton.distanceSettings.max){distance.value=CloudParticleSystem.singleton.distanceSettings.max;}
         if(distance.value<=CloudParticleSystem.singleton.distanceSettings.min){distance.value=CloudParticleSystem.singleton.distanceSettings.min;}
         orbit.value+=orbit.incrementSpeed*Core.magicDeltaTimeNumber;
         orbit.value.x=orbit.value.x%360f;
         orbit.value.y=orbit.value.y%360f;
         orbit.value.z=orbit.value.z%360f;
         //Log.DebugMessage("orbit.value:"+orbit.value);
         transform.position=CloudParticleSystem.singleton.cloudsCamera.transform.position+Quaternion.Euler(orbit.value)*Vector3.forward*distance.value;
         angle.value+=angle.incrementSpeed*Core.magicDeltaTimeNumber;
         angle.value.x=angle.value.x%360f;
         angle.value.y=angle.value.y%360f;
         angle.value.z=angle.value.z%360f;
         //Log.DebugMessage("angle.value:"+angle.value);
         transform.rotation=Quaternion.LookRotation((CloudParticleSystem.singleton.cloudsCamera.transform.position-transform.position).normalized,Vector3.zero);
         transform.eulerAngles=new Vector3(
          transform.eulerAngles.x,
          transform.eulerAngles.y,
          0f
         );
         meshRenderer.transform.localRotation=Quaternion.Euler(0f,0f,angle.value.z);
         color.a=alpha.value;
         meshRenderer.material.SetColor("_TintColor",color);
        }
     internal CloudParticleAlpha alpha;
        internal struct CloudParticleAlpha{
         internal float value;
         internal float incrementSpeed;
         internal float reverseTimer;
         internal float changeIncrementSpeedValueTimer;
            internal CloudParticleAlpha(float value,float incrementSpeed,float reverseTimer,float changeIncrementSpeedValueTimer){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
             this.reverseTimer=reverseTimer;
             this.changeIncrementSpeedValueTimer=changeIncrementSpeedValueTimer;
            }
            internal void RandomlyReverseSpeed(){
            }
            internal void RandomlyChangeIncrementSpeedValue(){
            }
        }
     internal CloudParticleAngle angle;
        internal struct CloudParticleAngle{
         internal Vector3 value;
         internal Vector3 incrementSpeed;
         internal float reverseTimer;
         internal float changeIncrementSpeedValueTimer;
            internal CloudParticleAngle(Vector3 value,Vector3 incrementSpeed,float reverseTimer,float changeIncrementSpeedValueTimer){
             this.value=value;
             this.incrementSpeed=incrementSpeed;
             this.reverseTimer=reverseTimer;
             this.changeIncrementSpeedValueTimer=changeIncrementSpeedValueTimer;
            }
            internal void RandomlyReverseSpeed(){
             incrementSpeed.x=0f;
             incrementSpeed.y=0f;
             incrementSpeed.z=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.z:-incrementSpeed.z;
            }
            internal void RandomlyChangeIncrementSpeedValue(){
             Vector3Int sign=new Vector3Int(
              0,
              0,
              Math.Sign(incrementSpeed.z)
             );
             incrementSpeed.x=0f;
             incrementSpeed.y=0f;
             incrementSpeed.z=Mathf.Lerp(CloudParticleSystem.singleton.angleSettings.minIncrementSpeed.z,CloudParticleSystem.singleton.angleSettings.maxIncrementSpeed.z,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             if(sign.z!=0){incrementSpeed.z*=sign.z;}
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
            internal void RandomlyReverseSpeed(){
             incrementSpeed.x=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.x:-incrementSpeed.x;
             incrementSpeed.y=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.y:-incrementSpeed.y;
             incrementSpeed.z=0f;
            }
            internal void RandomlyChangeIncrementSpeedValue(){
             Vector3Int sign=new Vector3Int(
              Math.Sign(incrementSpeed.x),
              Math.Sign(incrementSpeed.y),
              0
             );
             incrementSpeed.x=Mathf.Lerp(CloudParticleSystem.singleton.orbitSettings.minIncrementSpeed.x,CloudParticleSystem.singleton.orbitSettings.maxIncrementSpeed.x,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             incrementSpeed.y=Mathf.Lerp(CloudParticleSystem.singleton.orbitSettings.minIncrementSpeed.y,CloudParticleSystem.singleton.orbitSettings.maxIncrementSpeed.y,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             incrementSpeed.z=0f;
             if(sign.x!=0){incrementSpeed.x*=sign.x;}
             if(sign.y!=0){incrementSpeed.y*=sign.y;}
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
            internal void RandomlyReverseSpeed(){
             incrementSpeed=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed:-incrementSpeed;
            }
            internal void RandomlyChangeIncrementSpeedValue(){
             int sign=Math.Sign(incrementSpeed);
             incrementSpeed=Mathf.Lerp(
              CloudParticleSystem.singleton.distanceSettings.minIncrementSpeed,
              CloudParticleSystem.singleton.distanceSettings.maxIncrementSpeed,
              CloudParticleSystem.singleton.random.Next(0,101)/100f
             );
             if(sign!=0){
              incrementSpeed*=sign;
             }
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
            internal void RandomlyReverseSpeed(bool maintainProportion=true){
             incrementSpeed.x=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.x:-incrementSpeed.x;
             incrementSpeed.y=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.y:-incrementSpeed.y;
             incrementSpeed.z=CloudParticleSystem.singleton.random.Next(0,2)==0?incrementSpeed.z:-incrementSpeed.z;
             if(maintainProportion){
              incrementSpeed.x=incrementSpeed.y=incrementSpeed.z;
             }
            }
            internal void RandomlyChangeIncrementSpeedValue(bool maintainProportion=true){
             Vector3Int sign=new Vector3Int(
              Math.Sign(incrementSpeed.x),
              Math.Sign(incrementSpeed.y),
              Math.Sign(incrementSpeed.z)
             );
             incrementSpeed.x=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.x,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.x,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             incrementSpeed.y=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.y,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.y,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             incrementSpeed.z=Mathf.Lerp(CloudParticleSystem.singleton.scaleSettings.minIncrementSpeed.z,CloudParticleSystem.singleton.scaleSettings.maxIncrementSpeed.z,CloudParticleSystem.singleton.random.Next(0,101)/100f);
             if(sign.x!=0){incrementSpeed.x*=sign.x;}
             if(sign.y!=0){incrementSpeed.y*=sign.y;}
             if(sign.z!=0){incrementSpeed.z*=sign.z;}
             if(maintainProportion){
              incrementSpeed.x=incrementSpeed.y=incrementSpeed.z=(incrementSpeed.x+incrementSpeed.y+incrementSpeed.z)/3f;
             }
            }
        }
    }
}