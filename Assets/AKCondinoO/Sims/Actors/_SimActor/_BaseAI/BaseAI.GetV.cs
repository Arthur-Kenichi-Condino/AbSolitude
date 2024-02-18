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
    }
}