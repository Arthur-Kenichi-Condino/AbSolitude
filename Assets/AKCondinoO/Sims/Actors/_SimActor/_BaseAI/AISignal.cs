#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Combat{
    internal class AISignal:MonoBehaviour{
     internal BaseAI actor;
     internal AudioRange audioRange;
        void Awake(){
         Log.DebugMessage("AISignal:search audioRange");
         audioRange=GetComponentInChildren<AudioRange>();
         if(audioRange){
          Log.DebugMessage("AISignal:found audioRange");
          audioRange.aiSignal=this;
         }
        }
        void OnDestroy(){
        }
        internal void Activate(){
         this.gameObject.SetActive(true);
        }
        internal void Deactivate(){
         this.gameObject.SetActive(false);
         if(audioRange){
          audioRange.OnDeactivate();
         }
        }
    }
}