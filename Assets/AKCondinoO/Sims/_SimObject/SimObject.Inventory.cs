#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Animations;
using static AKCondinoO.Sims.Actors.BaseAI;
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
             if(simHandsInventory.Add(simObject,out SimInventoryItemsInContainerSettings.InContainerSettings settings,!addingList)){
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
         void OnAddedToInventory(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings){
          simObject.ChangeInteractionsToActAsInventoryItem(simInventory,settings);
          OnInventoryItemAdded(simInventory,settings,simObject);
         }
        }
        protected virtual void OnInventoryItemAdded(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings,SimObject simObjectAdded){
         Log.DebugMessage(this+":OnInventoryItemAdded,simInventory:"+simInventory);
         if(simInventory is SimHands simHands){
          Log.DebugMessage(this+":OnInventoryItemAdded,simObjectAdded:"+simObjectAdded);
          if(simObjectAdded is SimWeapon simWeapon){
           if(settings.handsUsage==SimActor.HandsUsage.TwoHanded){
            SetCurrentToolsOrWeapons(simWeapon,simWeapon);
           }else{
            SetCurrentToolsOrWeapons(simWeapon,null);
           }
          }
         }
        }
        internal void RemoveFromInventory(SimObject simObject,SimInventory simInventory,bool delete=true,bool removingList=false){
         simInventory.Remove(simObject.asInventoryItem,delete,!removingList);
         if(!delete){
          simObject.ChangeInteractionsToActAsNonInventorySimObject(simInventory);
         }
         OnInventoryItemRemoved(simInventory,simObject);
        }
        protected virtual void OnInventoryItemRemoved(SimInventory simInventory,SimObject simObjectRemoved){
         RemoveFromCurrentToolsOrWeapons(simObjectRemoved);
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
     internal readonly HashSet<(SimObject forAction1,SimObject forAction2)>itemsInToolbar=new HashSet<(SimObject,SimObject)>();
      internal readonly Dictionary<int,(SimObject forAction1,SimObject forAction2)>itemsByToolbarSlot=new Dictionary<int,(SimObject,SimObject)>();
       internal(SimObject forAction1,SimObject forAction2)?itemsEquipped=null;
        protected virtual void SetCurrentToolsOrWeapons(SimObject forAction1,SimObject forAction2){
         //  TO DO: put in toolbar slots only if available and only equip one item pair selected by its toolbar slot
         itemsInToolbar.Add((forAction1,forAction2));
         itemsEquipped=(forAction1,forAction2);
         Log.DebugMessage(this+":itemsEquipped:"+itemsEquipped);
        }
        protected virtual void RemoveFromCurrentToolsOrWeapons(SimObject simObject){
         itemsInToolbar.RemoveWhere(
          (itemPair)=>{
           return(
            (itemPair.forAction1==null&&itemPair.forAction2==null)||
            (itemPair.forAction1!=null&&itemPair.forAction1==simObject)||
            (itemPair.forAction2!=null&&itemPair.forAction2==simObject)
           );
          }
         );
         if(itemsEquipped!=null){
          if(
           (itemsEquipped.Value.forAction1==null&&itemsEquipped.Value.forAction2==null)||
           (itemsEquipped.Value.forAction1!=null&&itemsEquipped.Value.forAction1==simObject)||
           (itemsEquipped.Value.forAction2!=null&&itemsEquipped.Value.forAction2==simObject)
          ){
           itemsEquipped=null;
          }
         }
        }
     internal bool actingAsInventoryItem=false;
        internal void ChangeInteractionsToActAsInventoryItem(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings){
         Log.DebugMessage("ChangeInteractionsToActAsInventoryItem:"+id);
         //  TO DO: disable some types of collisions and enable triggers or special collisions
         actingAsInventoryItem=true;
         OnActAsInventoryItem();
         persistentData.UpdateData(this);
        }
        protected void OnActAsInventoryItem(){
         foreach(Collider collider in colliders){
          if(collider==null){
           continue;
          }
          collider.enabled=false;
         }
        }
        internal void ChangeInteractionsToActAsNonInventorySimObject(SimInventory simInventory){
         Log.DebugMessage("ChangeInteractionsToActAsNonInventorySimObject:"+id);
         actingAsInventoryItem=false;
         OnActAsNonInventorySimObject();
         persistentData.UpdateData(this);
        }
        protected void OnActAsNonInventorySimObject(){
         if(interactionsEnabled){
          foreach(Collider collider in colliders){
           if(collider==null){
            continue;
           }
           collider.enabled=true;
          }
         }
         if(parentConstraint!=null){
          parentConstraint.locked=false;
          for(int i=0;i<parentConstraint.sourceCount;++i){
           parentConstraint.RemoveSource(i);
          }
          parentConstraint.constraintActive=false;
         }
         inContainerTransformData=null;
        }
     protected InContainerTransformData inContainerTransformData=null;
        internal void SetAsInventoryItemTransform(){
         //Log.DebugMessage("SetAsInventoryItemTransform:"+id);
         if(asInventoryItem.container is SimHands simHands){
          if(simHands.leftHand!=null&&simHands.rightHand!=null){
           Debug.DrawLine(simHands.leftHand.transform.position,simHands.rightHand.transform.position,Color.blue);
           Vector3 lineBetweenHandsDir=(simHands.leftHand.transform.position-simHands.rightHand.transform.position).normalized;
           Quaternion lineBetweenHandsRot=Quaternion.LookRotation(lineBetweenHandsDir,asInventoryItem.container.asSimObject.transform.up);
           SimInventoryItemsInContainerSettings.InContainerSettings settings=asInventoryItem.settings;
           if(asInventoryItem.container.asSimObject is BaseAI containerAsBaseAI){
            if(containerAsBaseAI.animatorController!=null){
             if(containerAsBaseAI.animatorController.currentWeaponAimLayerIndex!=null){
              string layerName=null;
              int layerIndex=containerAsBaseAI.animatorController.currentWeaponAimLayerIndex.Value;
              GetLayerAtIndex(layerIndex,out layerName);
              if(layerName==null){//  Aim layer has higher priority
               layerIndex=containerAsBaseAI.animatorController.currentWeaponLayerIndex.Value;
               GetLayerAtIndex(layerIndex,out layerName);
              }
              void GetLayerAtIndex(int layerIndex,out string layerName){
               if(containerAsBaseAI.animatorController.layerIndexToName.TryGetValue(layerIndex,out layerName)){
                if(containerAsBaseAI.animatorController.layerTargetWeight[layerIndex]!=1f){
                 layerName=null;
                }
               }
              }
              if(layerName!=null){
               //Log.DebugMessage("currentWeaponAimLayerIndex.Value:layerName:"+layerName);
               if(settings.transformSettings.TryGetValue(typeof(SimHands),out var transformSettingsForSimHands)){
                (Type containerSimType,ActorMotion?containerSimMotion,string layer)key=(containerAsBaseAI.GetType(),containerAsBaseAI.motion,layerName);
                if(transformSettingsForSimHands.TryGetValue(key,out var transformSettingsForParentBodyPartName)){
                 int priority=int.MaxValue;
                 Transform bodyPart=null;
                 InContainerTransformData inContainerTransformData=null;
                 foreach(var kvp in transformSettingsForParentBodyPartName){
                  string parentBodyPartName=kvp.Key;
                  if(containerAsBaseAI.nameToBodyPart.TryGetValue(parentBodyPartName,out Transform parentBodyPart)){
                   int layerPriority=kvp.Value.layerPriority;
                   if(layerPriority<=priority){
                    priority=layerPriority;
                    bodyPart=parentBodyPart;
                    inContainerTransformData=kvp.Value;
                    //Log.DebugMessage("SetAsInventoryItemTransform:layerName:"+layerName);
                   }
                  }
                 }
                 if(bodyPart!=null){
                  if(inContainerTransformData!=this.inContainerTransformData){
                   Log.DebugMessage("set ParentConstraint:"+layerName);
                   this.inContainerTransformData=inContainerTransformData;
                   if(parentConstraint!=null){
                    parentConstraint.locked=false;
                    for(int i=0;i<parentConstraint.sourceCount;++i){
                     parentConstraint.RemoveSource(i);
                    }
                    ConstraintSource source=new ConstraintSource{
                     sourceTransform=bodyPart,
                     weight=1f,
                    };
                    parentConstraint.AddSource(source);
                    Log.DebugMessage("set ParentConstraint:localRotation:"+inContainerTransformData.transform.localRotation.eulerAngles);
                    parentConstraint.SetRotationOffset   (parentConstraint.sourceCount-1,inContainerTransformData.transform.localRotation.eulerAngles);
                    Log.DebugMessage("set ParentConstraint:localPosition:"+inContainerTransformData.transform.localPosition);
                    parentConstraint.SetTranslationOffset(parentConstraint.sourceCount-1,inContainerTransformData.transform.localPosition);
                    parentConstraint.locked=true;
                    parentConstraint.constraintActive=true;
                   }
                  }
                 }
                }
               }
              }
             }
            }
           }
          }
         }
        }
    }
}