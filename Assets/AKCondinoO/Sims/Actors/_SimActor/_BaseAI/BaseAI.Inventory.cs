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
        protected override void OnInventoryItemAdded(SimInventory simInventory,SimInventoryItemsInContainerSettings.InContainerSettings settings,SimObject simObjectAdded){
         MyWeaponType=settings.weaponType;
        }
        protected override void OnInventoryItemRemoved(SimInventory simInventory,SimObject simObjectAdded){
         MyWeaponType=WeaponTypes.None;
        }
    }
}