using AKCondinoO.Sims.Inventory;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human{
    internal partial class HumanAI:HumanoidAI{
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         SetBodyPart("head","head",out head);
         SetBodyPart( "leftEye","lEye",out  leftEye);
         SetBodyPart("rightEye","rEye",out rightEye);
         SetBodyPart( "leftHand","lHand",out  leftHand);
         SetBodyPart("rightHand","rHand",out rightHand);
         SetBodyPart("abdomen","abdomenLower",out _);
         SetBodyPart("chest","chestUpper",out _);
         SetBodyPart("back","chestLower",out _);
         SetBodyPart("pelvis","pelvis",out _);
         SetBodyPart( "leftThigh","lThighTwist",out _);
         SetBodyPart("rightThigh","rThighTwist",out _);
         SetBodyPart( "leftKnee","lShin",out _);
         SetBodyPart("rightKnee","rShin",out _);
         SetBodyPart( "leftShin","lShin",out _);
         SetBodyPart("rightShin","rShin",out _);
         SetBodyPart( "leftAnkle","lShin",out _);
         SetBodyPart("rightAnkle","rShin",out _);
         SetBodyPart( "leftFoot","lFoot",out  leftFoot);
         SetBodyPart("rightFoot","rFoot",out rightFoot);
         SetBodyPart( "leftArm","lShldrBend",out _);
         SetBodyPart("rightArm","rShldrBend",out _);
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