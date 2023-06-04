#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal SimInventoryItem asInventoryItem=null;
     internal readonly Dictionary<Type,Dictionary<ulong,SimInventory>>inventory=new Dictionary<Type,Dictionary<ulong,SimInventory>>();
     internal readonly HashSet<SimInventoryItem>inventoryItemsToSpawn=new HashSet<SimInventoryItem>();
     internal SpawnData inventoryItemsSpawnData;
        internal bool AddToInventory(SimObject simObject,bool addingList=false){
         Log.DebugMessage("AddToInventory");
         if(InventoryContains(simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData)){
          Log.DebugMessage("AddToInventory InventoryContains True");
          //  TO DO: "materialize" if needed
         }else{
          Log.DebugMessage("AddToInventory InventoryContains False");
          if(inventory.TryGetValue(typeof(SimHands),out Dictionary<ulong,SimInventory>simHandsInventories)){
           foreach(var simInventory in simHandsInventories){
            if(simInventory.Value is SimHands simHandsInventory){
             Log.DebugMessage("simHandsInventory try Add");
             if(simHandsInventory.Add(simObject,out SimInventoryItemsInContainerSettings.SimObjectSettings settings,!addingList)){
              Log.DebugMessage("added to simHandsInventory:mark simObject to be saved as an inventory item");
              //  TO DO: create local function to save simObjects as an inventory item
              OnAddedToInventory(simHandsInventory,settings);
              return true;
             }
            }
           }
          }
         }
         return false;
         void OnAddedToInventory(SimInventory simInventory,SimInventoryItemsInContainerSettings.SimObjectSettings settings){
          simObject.ChangeInteractionsToActAsInventoryItem(simInventory,settings);
          OnInventoryItemAdded(simInventory,settings,simObject);
         }
        }
        protected virtual void OnInventoryItemAdded(SimInventory simInventory,SimInventoryItemsInContainerSettings.SimObjectSettings settings,SimObject simObjectAdded){
        }
        internal void RemoveFromInventory(SimObject simObject,SimInventory simInventory,bool delete=true,bool removingList=false){
         simInventory.Remove(simObject.asInventoryItem,delete,!removingList);
         if(!delete){
          simObject.ChangeInteractionsToActAsNonInventorySimObject(simInventory);
         }
         OnInventoryItemRemoved(simInventory,simObject);
        }
        protected virtual void OnInventoryItemRemoved(SimInventory simInventory,SimObject simObjectRemoved){
        }
        internal bool InventoryContains(SimObject simObject,out(SimInventory simInventory,SimInventoryItem asInventoryItem)?containerData){
         containerData=null;
         if(simObject.asInventoryItem==null){
          return false;
         }
         foreach(var inventoryTypeDictionary in inventory){
          foreach(var simInventory in inventoryTypeDictionary.Value){
           if(simInventory.Value.Contains(simObject)){
            containerData=(simInventory.Value,simObject.asInventoryItem);
            return true;
           }
          }
         }
         return false;
        }
        protected virtual void SetCurrentToolsOrWeapons(SimObject forAction1,SimObject forAction2){
        }
        internal void ChangeInteractionsToActAsInventoryItem(SimInventory simInventory,SimInventoryItemsInContainerSettings.SimObjectSettings settings){
         Log.DebugMessage("ChangeInteractionsToActAsInventoryItem:"+id);
         //  TO DO: disable some types of collisions and enable triggers or special collisions
         persistentData.UpdateData(this);
        }
        internal void ChangeInteractionsToActAsNonInventorySimObject(SimInventory simInventory){
         Log.DebugMessage("ChangeInteractionsToActAsNonInventorySimObject:"+id);
         persistentData.UpdateData(this);
        }
        internal void SetAsInventoryItemTransform(){
         //Log.DebugMessage("SetAsInventoryItemTransform:"+id);
         if(asInventoryItem.container is SimHands simHands){
          if(simHands.leftHand!=null&&simHands.rightHand!=null){
           Vector3 lineBetweenHandsDir=(simHands.leftHand.transform.position-simHands.rightHand.transform.position).normalized;
           Quaternion lineBetweenHandsRot=Quaternion.LookRotation(lineBetweenHandsDir,asInventoryItem.container.asSimObject.transform.up);
           SimInventoryItemsInContainerSettings.SimObjectSettings settings=asInventoryItem.settings;
           Vector3  leftHandGrabPos=settings. leftHandGrabPos;
           Vector3 rightHandGrabPos=settings.rightHandGrabPos;
           Quaternion rightHandGrabRot=settings.rightHandGrabRot;
           //Log.DebugMessage( "leftHandGrabPos:"+ leftHandGrabPos);
           //Log.DebugMessage("rightHandGrabPos:"+rightHandGrabPos);
           Debug.DrawLine(simHands.leftHand.transform.position,simHands.rightHand.transform.position,Color.blue);
           if(ZAxisIsUp){
            Vector3 lineBetweenHandsDirPerpendicularRight=Vector3.Cross(lineBetweenHandsDir,asInventoryItem.container.asSimObject.transform.up).normalized;
            transform.rotation=Quaternion.LookRotation(Quaternion.AngleAxis(-90f,lineBetweenHandsDir)*lineBetweenHandsDirPerpendicularRight,lineBetweenHandsDir);
           }else{
            transform.rotation=lineBetweenHandsRot;
           }
           transform.position=simHands.rightHand.transform.position+transform.rotation*rightHandGrabPos;
           if(simHands.asSimObject is BaseAI baseAI&&baseAI.simActorAnimatorController!=null&&baseAI.simActorAnimatorController.animator!=null){
            if(
             baseAI.motion==BaseAI.ActorMotion.MOTION_STAND||
             baseAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_STAND
            ){
             transform.rotation*=rightHandGrabRot;
            }else if(
             baseAI.motion==BaseAI.ActorMotion.MOTION_MOVE||
             baseAI.motion==BaseAI.ActorMotion.MOTION_RIFLE_MOVE
            ){
             Quaternion rot=Quaternion.LookRotation(baseAI.simActorAnimatorController.animator.transform.forward,baseAI.simActorAnimatorController.animator.transform.up);
             if(ZAxisIsUp){
              transform.rotation=Quaternion.AngleAxis(180f,baseAI.simActorAnimatorController.animator.transform.up)*Quaternion.AngleAxis(-90f,baseAI.simActorAnimatorController.animator.transform.right)*rot;
             }else{
              transform.rotation=rot;
             }
            }
           }
          }
         }
        }
    }
}