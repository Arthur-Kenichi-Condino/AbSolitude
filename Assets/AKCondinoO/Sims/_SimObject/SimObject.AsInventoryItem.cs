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
    }
}