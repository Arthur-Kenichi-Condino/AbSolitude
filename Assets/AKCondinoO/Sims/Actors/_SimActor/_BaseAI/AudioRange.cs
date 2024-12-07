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
        }
        void OnTriggerExit(Collider other){
        }
    }
}