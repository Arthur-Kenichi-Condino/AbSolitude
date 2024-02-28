#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal void CurrentWeapons(List<SimWeapon>simWeapons){
      simWeapons.Clear();
      if(inventory.TryGetValue(typeof(SimHands),out Dictionary<ulong,SimInventory>simHandsInventories)){
       foreach(var simInventory in simHandsInventories){
        if(simInventory.Value is SimHands simHandsInventory){
         foreach(SimInventoryItem item in simHandsInventory.items){
          if(item.simObject!=null){
           if(item.simObject is SimWeapon weapon){
            simWeapons.Add(weapon);
           }
          }
         }
        }
       }
      }
     }
     internal bool IsAttacking(bool shooting=true){
      bool result=motionFlagForAttackAnimation||(shooting&&motionFlagForShootingAnimation);
      return result;
     }
     internal bool IsFasterThan(SimObject simObject){
      if(simObject is BaseAI baseAI&&(
       moveMaxVelocity.z>baseAI.moveMaxVelocity.z||
       moveMaxVelocity.y>baseAI.moveMaxVelocity.y||
       moveMaxVelocity.x>baseAI.moveMaxVelocity.x)
      ){
       return true;
      }
      return false;
     }
    }
}