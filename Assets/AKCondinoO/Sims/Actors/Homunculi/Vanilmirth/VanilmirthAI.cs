#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Homunculi.Vanilmirth{
    internal partial class VanilmirthAI:HomunculusAI{   
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         //Log.DebugMessage("OnUMACharacterUpdated");
         SetBodyPart( "leftEye","leye",out  leftEye);
         SetBodyPart("rightEye","reye",out rightEye);
         SetBodyPart("body","bodyBase",out _);
         base.OnUMACharacterUpdated(simUMAData);
        }
        internal override void OnActivated(){
         //Log.DebugMessage("VanilmirthAI:OnActivated():masterId:"+masterId);
         requiredSkills.Clear();
         requiredSkills.Add(typeof(ChaoticBlessing),new SkillData(){skill=typeof(ChaoticBlessing),level=10,});
         base.OnActivated();
         attackRange=new Vector3(0.5f,0.5f,.5f);
        }
        //protected override void OnIDLE_ST_Routine(){
        // //Log.DebugMessage("VanilmirthAI:OnIDLE_ST():masterId:"+masterId);
        // //Log.DebugMessage("VanilmirthAI:OnIDLE_ST():masterSimObject:"+masterSimObject);
        // base.OnIDLE_ST_Routine();
        //}
    }
}