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
				     }
			      return result;
			     }
        internal virtual bool DoSkill(BaseAI actor,BaseAI target){
         return false;
			     }
    }
}