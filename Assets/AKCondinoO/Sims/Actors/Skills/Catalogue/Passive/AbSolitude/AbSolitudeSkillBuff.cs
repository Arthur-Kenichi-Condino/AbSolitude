#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class AbSolitudeSkillBuff:SkillBuff{
        internal AbSolitudeSkillBuff():base(){
         Log.DebugMessage("AbSolitudeSkillBuff ctor");
         abSolitudeEffect=new AbSolitudeEffect();
        }
     internal AbSolitudeEffect abSolitudeEffect{
      get{
       return(AbSolitudeEffect)effect;
      }
      private set{
       effect=value;
      }
     }
        internal class AbSolitudeEffect:Effect{
            internal override void Apply(SimObject.Stats stats){
             Log.DebugMessage("AbSolitudeEffect:Apply");
            }
            internal override bool ApplyRepeating(SimObject.Stats stats){
             //Log.DebugMessage("AbSolitudeEffect:ApplyRepeating");
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
            }
        }
    }
}