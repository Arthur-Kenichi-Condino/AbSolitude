#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class AnimationEventsHandler:MonoBehaviour{
     internal SimActor actor;
        internal void OnCanDamageAnimationEvent(string bodyPart){
         Log.DebugMessage("OnCanDamageAnimationEvent:"+bodyPart);
        }
        internal void OnCantDamageAnimationEvent(string bodyPart){
         Log.DebugMessage("OnCantDamageAnimationEvent:"+bodyPart);
        }
    }
}