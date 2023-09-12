#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal class ArquimedesAnimatorControllerMotionUpdater:VanilmirthAnimatorControllerMotionUpdater{
        internal override void UpdateAnimatorWeaponLayer(){
         base.UpdateAnimatorWeaponLayer();
        }
        internal override void UpdateAnimatorMotionValue(){
         base.UpdateAnimatorMotionValue();
        }
    }
}