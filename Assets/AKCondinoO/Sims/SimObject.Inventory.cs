#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static AKCondinoO.Sims.Inventory.SimObjectAsInventoryItemSettings.SimInventoryItemInContainerData.SimObjectAsInventoryItemTransformForMotion.SimObjectAsInventoryItemTransformForSimType;
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
         if(simInventory is SimHands simHands){
          SetCurrentToolsOrWeapons(null,null);
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
     internal bool actingAsInventoryItem=false;
        internal void ChangeInteractionsToActAsInventoryItem(SimInventory simInventory,SimInventoryItemsInContainerSettings.SimObjectSettings settings){
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
        }
        internal void SetAsInventoryItemTransform(){
         //Log.DebugMessage("SetAsInventoryItemTransform:"+id);
         if(asInventoryItem.container is SimHands simHands){
          if(simHands.leftHand!=null&&simHands.rightHand!=null){
           Debug.DrawLine(simHands.leftHand.transform.position,simHands.rightHand.transform.position,Color.blue);
           Vector3 lineBetweenHandsDir=(simHands.leftHand.transform.position-simHands.rightHand.transform.position).normalized;
           Quaternion lineBetweenHandsRot=Quaternion.LookRotation(lineBetweenHandsDir,asInventoryItem.container.asSimObject.transform.up);
           SimInventoryItemsInContainerSettings.SimObjectSettings settings=asInventoryItem.settings;
           if(asInventoryItem.container.asSimObject is BaseAI containerAsBaseAI){
            if(containerAsBaseAI.simActorAnimatorController!=null){
             if(containerAsBaseAI.simActorAnimatorController.currentWeaponAimLayerIndex!=null){
              if(containerAsBaseAI.simActorAnimatorController.layerIndexToName.TryGetValue(containerAsBaseAI.simActorAnimatorController.currentWeaponAimLayerIndex.Value,out string layerName)){
               if(settings.inventoryTransformSettings.TryGetValue(typeof(SimHands),out var transformSettingsForSimHands)){
                if(transformSettingsForSimHands.TryGetValue(containerAsBaseAI.motion,out var transformSettingsForMotion)){
                 if(transformSettingsForMotion.TryGetValue(containerAsBaseAI.GetType(),out var transformSettingsForSimType)){
                  int transformIndex=0;
                  int priority=int.MaxValue;
                  Transform bodyPart=null;
                  SimObjectAsInventoryItemTransformForParentBodyPartName transformSettings=null;
                  foreach(var transformSettingsForParentBodyPartName in transformSettingsForSimType){
                   string parentBodyPartName=transformSettingsForParentBodyPartName.Key;
                   if(containerAsBaseAI.nameToBodyPart.TryGetValue(parentBodyPartName,out Transform parentBodyPart)){
                    int index=Array.IndexOf(transformSettingsForParentBodyPartName.Value.layer,layerName);
                    if(index>=0){
                     int layerPriority=transformSettingsForParentBodyPartName.Value.layerPriority[index];
                     if(layerPriority<=priority){
                      priority=layerPriority;
                      transformIndex=index;
                      bodyPart=parentBodyPart;
                      transformSettings=transformSettingsForParentBodyPartName.Value;
                      Log.DebugMessage("SetAsInventoryItemTransform:layerName:"+layerName);
                     }
                    }
                   }
                  }
                  if(bodyPart!=null){
                   Vector3 grabPos=transformSettings.transform[transformIndex].localPosition;
                   Quaternion grabRot=transformSettings.transform[transformIndex].localRotation;
                   if(transformSettings.transformIsDeterminant[transformIndex]){
                    transform.rotation=bodyPart.rotation*grabRot;
                    transform.position=bodyPart.position+bodyPart.rotation*grabPos;
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