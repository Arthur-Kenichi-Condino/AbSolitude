#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class TeleportSkillBuff:SkillBuff{
        internal TeleportSkillBuff():base(){
         Log.DebugMessage("TeleportEffect ctor");
         teleportEffect=new TeleportEffect();
        }
     internal TeleportEffect teleportEffect{
      get{
       return(TeleportEffect)effect;
      }
      private set{
       effect=value;
      }
     }
     internal Vector3 targetDest;
        internal class TeleportEffect:Effect{
         internal SimObject target;
         internal Vector3 targetDest;
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("TeleportEffect:Apply");
             //  use a function and check safety for position change instead of only changing position
             target.OnTeleportTo(targetDest,target.transform.rotation);
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
            }
        }
        internal override void OnApply(bool gameExiting=false){
         if(!applied){
          if(!gameExiting){
           //  apply buff effects here
           teleportEffect.target=applyingEffectsOn.targetSimObject;
           teleportEffect.targetDest=targetDest;
          }
         }
         base.OnApply(gameExiting);
        }
    }
}