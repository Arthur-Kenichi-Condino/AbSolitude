#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class AudioRange:MonoBehaviour{
     internal AISignal aiSignal;
        void Awake(){
        }
        internal void OnDeactivate(){
         //  OnTriggerExit will not be called
         aiSignal.actor.inAudioRange.Clear();
         simObjectCollidersInRange.Clear();
         foreach(var kvp in gotInRangeOf){
          SimObject simObjectGotInRangeOf=kvp.Key;
          if(simObjectGotInRangeOf is BaseAI simActor&&simActor.aiSignal!=null&&simActor.aiSignal.audioRange!=null){
           simActor.aiSignal.audioRange.simObjectCollidersInRange.RemoveWhere(collider=>{return collider.transform.root==this.transform.root;});
          }
         }
         gotInRangeOf.Clear();
        }
     internal readonly Dictionary<SimObject,int>gotInRangeOf=new Dictionary<SimObject,int>();
     internal readonly HashSet<Collider>simObjectCollidersInRange=new HashSet<Collider>();
        void OnTriggerEnter(Collider other){
         //Log.DebugMessage("OnTriggerEnter:other:"+other,other);
         if(!Core.singleton.isServer){
          return;
         }
         if(other.transform.root==this.transform.root){
          return;
         }
         if(IsValidForHearing(other,out SimObject otherSimObject,out BaseAI otherSimActor)){
          simObjectCollidersInRange.Add(other);
          if(!otherSimActor.aiSignal.audioRange.gotInRangeOf.ContainsKey(aiSignal.actor)){
           otherSimActor.aiSignal.audioRange.gotInRangeOf.Add(aiSignal.actor,0);
          }else{
           otherSimActor.aiSignal.audioRange.gotInRangeOf[aiSignal.actor]++;
          }
          aiSignal.actor.OnSimObjectIsInAudioRange(otherSimObject,otherSimActor);
         }
        }
        void OnTriggerExit(Collider other){
         if(!Core.singleton.isServer){
          return;
         }
         simObjectCollidersInRange.Remove(other);
         if(IsValidForHearing(other,out SimObject otherSimObject,out BaseAI otherSimActor)){
          aiSignal.actor.OnSimObjectIsOutOfAudioRange(otherSimObject,otherSimActor);
          if(otherSimActor.aiSignal.audioRange.gotInRangeOf.ContainsKey(aiSignal.actor)){
           otherSimActor.aiSignal.audioRange.gotInRangeOf[aiSignal.actor]--;
           if(otherSimActor.aiSignal.audioRange.gotInRangeOf[aiSignal.actor]<0){
            otherSimActor.aiSignal.audioRange.gotInRangeOf.Remove(aiSignal.actor);
           }
          }
         }
        }
        internal bool IsValidForHearing(Collider other,out SimObject otherSimObject,out BaseAI otherSimActor){
         otherSimActor=null;
         if(other.CompareTag("SimObjectVolume")&&!other.isTrigger&&(otherSimObject=other.GetComponentInParent<SimObject>())!=null&&otherSimObject is BaseAI otherIsSimActor&&otherIsSimActor.aiSignal!=null&&otherIsSimActor.aiSignal.audioRange!=null){
          otherSimActor=otherIsSimActor;
          return true;
         }
         otherSimObject=null;
         return false;
        }
    }
}