#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI:HomunculusAI{   
        internal override void OnActivated(){
         Log.DebugMessage("VanilmirthAI:OnActivated():masterId:"+masterId);
         requiredSkills.Clear();
         requiredSkills.Add(typeof(ChaoticBlessing),new SkillData(){skill=typeof(ChaoticBlessing),level=10,});
         base.OnActivated();
        }
        protected override void OnIDLE_ST(){
         //Log.DebugMessage("VanilmirthAI:OnIDLE_ST():masterId:"+masterId);
         Log.DebugMessage("VanilmirthAI:OnIDLE_ST():masterSimObject:"+masterSimObject);
         SetMySkill();
         base.OnIDLE_ST();
        }
        protected override void DoSkill(){
         if(MySkill is ChaoticBlessing chaoticBlessingSkill){
          chaoticBlessingSkill.DoSkill(this,chaoticBlessingSkill.level);
         }
         base.DoSkill();
        }
    }
}