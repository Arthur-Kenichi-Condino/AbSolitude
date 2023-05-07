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
        internal void ManualUpdate(float deltaTime){
         foreach(var typeSkillBuffListPair in activeBuffs){
          List<SkillBuff>skillBuffList=typeSkillBuffListPair.Value;
          foreach(SkillBuff skillBuff in skillBuffList){
           if(!skillBuff.hasBeenUpdatedThisFrame){
            skillBuff.ManualUpdate(deltaTime);
           }
           skillBuff.hasBeenUpdatedThisFrame=false;
          }
         }
        }
    }
}