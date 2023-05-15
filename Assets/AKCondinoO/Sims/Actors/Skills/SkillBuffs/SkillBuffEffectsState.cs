#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SkillBuffEffectsState:MonoBehaviour{
     internal SimObject targetSimObject;
     internal readonly Dictionary<Type,List<SkillBuff>>activeBuffs=new Dictionary<Type,List<SkillBuff>>();
      readonly List<(Type buffType,SkillBuff skillBuff)>buffsToPool=new List<(Type,SkillBuff)>();
        internal bool Add(SkillBuff skillBuff,Skill fromSkill){
         Type buffType=skillBuff.GetType();
         if(!activeBuffs.TryGetValue(buffType,out List<SkillBuff>skillBuffsList)){
          activeBuffs.Add(buffType,skillBuffsList=new List<SkillBuff>());
         }
         skillBuff.skill=fromSkill;
         skillBuff.applyingEffectsOn=this;
         skillBuffsList.Add(skillBuff);
         return true;
        }
        internal void ManualUpdate(float deltaTime){
         foreach(var typeSkillBuffsListPair in activeBuffs){
          Type buffType=typeSkillBuffsListPair.Key;
          List<SkillBuff>skillBuffsList=typeSkillBuffsListPair.Value;
          foreach(SkillBuff skillBuff in skillBuffsList){
           if(!skillBuff.hasBeenUpdatedThisFrame){
            skillBuff.ManualUpdate(deltaTime);
           }
           skillBuff.hasBeenUpdatedThisFrame=false;
           if(skillBuff.expired){
            buffsToPool.Add((buffType,skillBuff));
           }
          }
         }
         foreach((Type buffType,SkillBuff skillBuff)skillBuffToPool in buffsToPool){
          activeBuffs[skillBuffToPool.buffType].Remove(skillBuffToPool.skillBuff);
          SkillBuff.Pool(skillBuffToPool.skillBuff);
         }
         buffsToPool.Clear();
        }
        internal void Clear(bool gameExiting=false){
         foreach(var typeSkillBuffsListPair in activeBuffs){
          Type buffType=typeSkillBuffsListPair.Key;
          List<SkillBuff>skillBuffsList=typeSkillBuffsListPair.Value;
          foreach(SkillBuff skillBuff in skillBuffsList){
           buffsToPool.Add((buffType,skillBuff));
          }
         }
         foreach((Type buffType,SkillBuff skillBuff)skillBuffToPool in buffsToPool){
          activeBuffs[skillBuffToPool.buffType].Remove(skillBuffToPool.skillBuff);
          SkillBuff.Pool(skillBuffToPool.skillBuff,gameExiting);
         }
         buffsToPool.Clear();
        }
    }
}