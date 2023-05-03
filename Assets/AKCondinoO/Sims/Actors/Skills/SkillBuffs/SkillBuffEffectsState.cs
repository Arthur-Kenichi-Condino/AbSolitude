using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SkillBuffEffectsState:MonoBehaviour{
     internal SimObject targetSimObject;
     internal List<SkillBuff>activeBuffs=new List<SkillBuff>();
        internal void ManualUpdate(float deltaTime){
         foreach(SkillBuff skillBuff in activeBuffs){
         }
        }
    }
}