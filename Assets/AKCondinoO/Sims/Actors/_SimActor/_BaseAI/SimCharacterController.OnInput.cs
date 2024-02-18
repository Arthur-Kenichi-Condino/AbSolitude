#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimCharacterController{
     internal readonly HashSet<SimWeapon>weaponsReloading=new HashSet<SimWeapon>();
        internal void OnReloadInput(){
         if(Enabled.RELOAD.curState){
          if(actor is BaseAI baseAI){
           if(actor.inventory.TryGetValue(typeof(SimHands),out Dictionary<ulong,SimInventory>simHandsInventories)){
            foreach(var simInventory in simHandsInventories){
             if(simInventory.Value is SimHands simHandsInventory){
              foreach(SimInventoryItem item in simHandsInventory.items){
               if(item.simObject!=null){
                if(item.simObject is SimWeapon weapon){
                 if(weapon.TryStartReloadingAction(simAiming:actor)){
                  weaponsReloading.Add(weapon);
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
        internal void OnReloadEvent(){
         foreach(var weapon in weaponsReloading){
          weapon.Reload(simAiming:actor);
         }
         weaponsReloading.Clear();
        }
    }
}