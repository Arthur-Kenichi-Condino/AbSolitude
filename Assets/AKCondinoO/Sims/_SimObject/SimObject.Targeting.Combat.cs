#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal virtual bool OnTargetedBySkill(Skill skill,SimObject caster){
         return false;
        }
        internal virtual bool OnHitByTargetedSkill(Skill skill,SimObject caster){
         return false;
        }
        internal virtual bool IsMonster(){
         return false;
        }
        internal virtual bool IsFriendlyTo(SimObject sim){
         return false;
        }
        internal virtual bool OnShotByWeapon(SimWeapon simWeapon,Hurtboxes hurtbox=null){
         return false;
        }
        protected virtual void OnDeath(bool instant=false){
        }
        internal virtual bool IsDead(){
         return false;
        }
    }
}