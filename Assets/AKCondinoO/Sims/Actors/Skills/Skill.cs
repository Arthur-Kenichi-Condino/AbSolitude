using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class Skill:MonoBehaviour{
        public enum SkillUseContext{
         OnCallSlaves,
         OnWillTakeDamage,
         OnTookDamage,
        }
        internal static Skill GetBest(BaseAI actor,SkillUseContext context){
         Skill result=null;
         switch(context){
          case SkillUseContext.OnCallSlaves:{
           if(actor.skills.TryGetValue(typeof(GenerateHomunculus),out Skill skill)){
            result=skill;
           }
           break;
          }
         }
         return result;
        }
     internal int level=1;
        internal virtual bool DoSkill(BaseAI actor,BaseAI target,int level){
         return false;
        }
    }
}