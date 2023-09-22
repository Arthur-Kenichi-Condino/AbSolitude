#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimAnimatorController{
     internal BaseAnimatorControllerMotionUpdater motionUpdater=null;
      internal WeaponTypes lastWeaponType=WeaponTypes.None;
       internal int?currentWeaponLayerIndex=null;
        internal int?currentWeaponAimLayerIndex=null;
     BaseAI.ActorMotion lastMotion=BaseAI.ActorMotion.MOTION_STAND;
        protected virtual void UpdateMotion(BaseAI baseAI){
          if(lastMotion!=baseAI.motion){
           //Log.DebugMessage("actor motion will be set from:"+lastMotion+" to:"+baseAI.motion);
          }
          if(motionUpdater==null){
           if(actor.simUMA!=null){
            motionUpdater=actor.simUMA.transform.root.GetComponentInChildren<BaseAnimatorControllerMotionUpdater>();
           }
           if(motionUpdater!=null){
            motionUpdater.controller=this;
           }
          }
          if(motionUpdater!=null){
             motionUpdater.UpdateAnimatorWeaponLayer();
             motionUpdater.UpdateAnimatorMotionValue();
          }
          if(lastMotion!=baseAI.motion){
           //Log.DebugMessage("actor changed motion from:"+lastMotion+" to:"+baseAI.motion);
          }
          lastMotion=baseAI.motion;
        }
    }
}