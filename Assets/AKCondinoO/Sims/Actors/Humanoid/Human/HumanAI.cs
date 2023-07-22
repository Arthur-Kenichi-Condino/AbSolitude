using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal partial class HumanAI:HumanoidAI{
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         if(head==null){
          head=Util.FindChildRecursively(simUMA.transform,"head");
          if(head!=null){
           nameToBodyPart["head"]=head;
          }else{
           nameToBodyPart.Remove("head");
          }
          Log.DebugMessage("head:"+head);
         }
         if( leftEye==null){
           leftEye=Util.FindChildRecursively(simUMA.transform,"lEye");
          if( leftEye!=null){
           nameToBodyPart[ "leftEye"]= leftEye;
          }else{
           nameToBodyPart.Remove( "leftEye");
          }
          Log.DebugMessage( "leftEye:"+ leftEye);
         }
         if(rightEye==null){
          rightEye=Util.FindChildRecursively(simUMA.transform,"rEye");
          if(rightEye!=null){
           nameToBodyPart["rightEye"]=rightEye;
          }else{
           nameToBodyPart.Remove("rightEye");
          }
          Log.DebugMessage("rightEye:"+rightEye);
         }
         if( leftHand==null){
           leftHand=Util.FindChildRecursively(simUMA.transform,"lHand");
          if( leftHand!=null){
           nameToBodyPart[ "leftHand"]= leftHand;
          }else{
           nameToBodyPart.Remove( "leftHand");
          }
          Log.DebugMessage( "leftHand:"+ leftHand);
         }
         if(rightHand==null){
          rightHand=Util.FindChildRecursively(simUMA.transform,"rHand");
          if(rightHand!=null){
           nameToBodyPart["rightHand"]=rightHand;
          }else{
           nameToBodyPart.Remove("rightHand");
          }
          Log.DebugMessage("rightHand:"+rightHand);
         }
         if(!nameToBodyPart.TryGetValue("abdomen",out Transform abdomen)||abdomen==null){
          abdomen=Util.FindChildRecursively(simUMA.transform,"abdomenLower");
          if(abdomen!=null){
           nameToBodyPart["abdomen"]=abdomen;
          }else{
           nameToBodyPart.Remove("abdomen");
          }
         }
         if(!nameToBodyPart.TryGetValue("chest",out Transform chest)||chest==null){
          chest=Util.FindChildRecursively(simUMA.transform,"chestUpper");
          if(chest!=null){
           nameToBodyPart["chest"]=chest;
          }else{
           nameToBodyPart.Remove("chest");
          }
         }
         if(!nameToBodyPart.TryGetValue("back",out Transform back)||back==null){
          back=Util.FindChildRecursively(simUMA.transform,"chestLower");
          if(back!=null){
           nameToBodyPart["back"]=back;
          }else{
           nameToBodyPart.Remove("back");
          }
         }
         base.OnUMACharacterUpdated(simUMAData);
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