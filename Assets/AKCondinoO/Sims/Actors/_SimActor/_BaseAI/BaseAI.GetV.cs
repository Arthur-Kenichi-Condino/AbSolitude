#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal bool IsAttacking(bool shooting=true){
      bool result=motionFlagForAttackAnimation||(shooting&&motionFlagForShootingAnimation);
      return result;
     }
     internal bool IsFasterThan(SimObject simObject){
      if(simObject is BaseAI baseAI&&(
       moveMaxVelocity.z>baseAI.moveMaxVelocity.z||
       moveMaxVelocity.y>baseAI.moveMaxVelocity.y||
       moveMaxVelocity.x>baseAI.moveMaxVelocity.x)
      ){
       return true;
      }
      return false;
     }
    }
}