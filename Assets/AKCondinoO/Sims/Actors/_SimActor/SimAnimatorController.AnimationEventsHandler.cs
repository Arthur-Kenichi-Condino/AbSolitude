#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimAnimatorController{
     internal AnimationEventsHandler animationEventsHandler;
        internal void AddAnimationEventsHandler(){
         animationEventsHandler=animator.AddComponent<AnimationEventsHandler>();
         animationEventsHandler.actor=this.actor;
        }
    }
}