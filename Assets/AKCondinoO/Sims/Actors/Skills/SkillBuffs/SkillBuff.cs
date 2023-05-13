#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal partial class SkillBuff{
     internal Skill activatedBySkill;
     internal SkillBuffEffectsState applyingEffectsOn;
        internal SkillBuff(){
        }
        internal virtual void OnReset(){
         activatedBySkill=null;
         applyingEffectsOn=null;
         duration=0f;
         delay=0f;
         elapsedTime=0f;
         expired=false;
         applied=false;
         hasBeenUpdatedThisFrame=false;
        }
     internal float duration;
      internal float delay;
       internal float elapsedTime;
     internal bool expired=false;
     internal bool applied=false;
     internal bool hasBeenUpdatedThisFrame=false;
        internal virtual void ManualUpdate(float deltaTime){
         if(elapsedTime>=delay){
          if(!expired){
           OnApply();
          }
          if(applied){
           if(elapsedTime>=duration){
            expired=true;
           }
          }
          if(expired){
           OnUnapply();
          }
         }
         elapsedTime+=deltaTime;
         hasBeenUpdatedThisFrame=true;
        }
        internal virtual void OnApply(bool gameExiting=false){
         if(!applied){
          if(!gameExiting){
           applyingEffectsOn.targetSimObject.stats.Value.OnAppliedSkillBuff(this);
          }
          applied=true;
         }
        }
        internal virtual void OnUnapply(bool pooling=false,bool gameExiting=false){
         if(gameExiting){
         }
         if(pooling){
          if(!expired){
           //  should the skill be applied immediately
           OnApply(gameExiting);
          }
         }
         if(applied){
          if(!gameExiting){
           applyingEffectsOn.targetSimObject.stats.Value.OnUnappliedSkillBuff(this);
          }
          applied=false;
         }
        }
    }
}