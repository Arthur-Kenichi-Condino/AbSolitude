#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected WeaponTypes MyWeaponType=WeaponTypes.None;internal WeaponTypes weaponType{get{return MyWeaponType;}}
        protected override void OnInventoryItemAdded(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings,SimObject simObjectAdded){
         base.OnInventoryItemAdded(simInventory,settings,simObjectAdded);
         MyWeaponType=settings.weaponType;
        }
        protected override void OnInventoryItemRemoved(SimInventory simInventory,SimObject simObjectAdded){
         base.OnInventoryItemRemoved(simInventory,simObjectAdded);
         MyWeaponType=WeaponTypes.None;
        }
    }
}