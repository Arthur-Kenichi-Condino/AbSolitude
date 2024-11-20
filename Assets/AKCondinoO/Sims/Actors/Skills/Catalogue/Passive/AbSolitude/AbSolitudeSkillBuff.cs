#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class AbSolitudeSkillBuff:SkillBuff{
     internal float applyNewSkillCooldown=8f;
        internal AbSolitudeSkillBuff():base(){
         //Log.DebugMessage("AbSolitudeSkillBuff ctor");
         abSolitudeEffect=new AbSolitudeEffect(this);
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
         internal readonly System.Random dice=new System.Random();
         internal float deltaTime;
            internal AbSolitudeEffect(SkillBuff buff):base(buff){
            }
         internal readonly List<Type>possibleSkillTypes=new List<Type>(){
          typeof(Quagmire),
         };
          internal readonly List<Skill>possibleSkills=new List<Skill>();
            internal override void Apply(SimObject.Stats stats){
             //Log.DebugMessage("AbSolitudeEffect:Apply");
             if(this.buff.applyingEffectsOn!=null&&this.buff.applyingEffectsOn.targetSimObject!=null){
              this.target=this.buff.applyingEffectsOn.targetSimObject;
             }
             if(this.buff.skill!=null&&this.buff.skill.actor!=null){
             }
             if(this.target is BaseAI target){
              foreach(Type skillType in possibleSkillTypes){
               if(!ReflectionUtil.IsTypeDerivedFrom(skillType,typeof(Skill))){
                Log.Warning("invalid skill type:"+skillType);
                continue;
               }
               (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(skillType,this.buff.level,target);
               possibleSkills.Add(spawnedSkill.skill);
               skillsForRandomUse.Add(spawnedSkill.skill);
              }
             }
            }
         internal float applyNewSkillTimer;
         internal readonly List<Skill>skillsForRandomUse=new List<Skill>();
          internal readonly List<Skill>skillsInEffect=new List<Skill>();
            internal override bool ApplyRepeating(SimObject.Stats stats){
             //Log.DebugMessage("AbSolitudeEffect:ApplyRepeating");
             if(applyNewSkillTimer>0f){
              applyNewSkillTimer-=deltaTime;
             }
             if(applyNewSkillTimer<=0f){
              if(skillsForRandomUse.Count>0){
               //Log.DebugMessage("AbSolitudeEffect:apply new skill");
               Skill skillToBeUsed=skillsForRandomUse[dice.Next(0,skillsForRandomUse.Count)];
               skillsInEffect.Add(skillToBeUsed);
               skillsForRandomUse.Remove(skillToBeUsed);
              }
              applyNewSkillTimer=((AbSolitudeSkillBuff)this.buff).applyNewSkillCooldown;
              for(int i=skillsInEffect.Count-1;i>=0;--i){
               Skill skill=skillsInEffect[i];
               if(skill==null){
                skillsInEffect.RemoveAt(i);
                continue;
               }
               //Log.DebugMessage("AbSolitudeEffect:do skill");
               skill.DoSkill(this.target,this.buff.level);
              }
             }
             return false;
            }
            internal override void Unapply(SimObject.Stats stats){
             skillsForRandomUse.Clear();
             skillsInEffect.Clear();
             foreach(Skill skill in possibleSkills){
              SkillsManager.singleton.Pool(skill.GetType(),skill);
             }
             possibleSkills.Clear();
             this.target=null;
            }
        }
        internal override void OnApply(bool gameExiting=false){
         abSolitudeEffect.deltaTime=this.deltaTime;
         base.OnApply(gameExiting);
        }
    }
}