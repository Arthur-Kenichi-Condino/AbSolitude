#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid{
    internal class DisfiguringHomunculusAI:HumanoidAI{
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         if(head==null){
          head=Util.FindChildRecursively(simUMA.transform,"face");
          if(head!=null){
           nameToBodyPart["head"]=head;
          }else{
           nameToBodyPart.Remove("head");
          }
          Log.DebugMessage("head:"+head);
         }
         base.OnUMACharacterUpdated(simUMAData);
        }
        internal override bool IsMonster(){
         return true;
        }
        internal override void OnActivated(){
         if(Core.singleton.isServer){
          if(!inventory.ContainsKey(typeof(SimHands))||inventory[typeof(SimHands)].Count<=0){
           SimInventoryManager.singleton.AddInventoryTo(this,typeof(SimHands));
          }
         }
         base.OnActivated();
        }
    }
}