#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
using static AKCondinoO.Sims.Actors.SimActor;
using static AKCondinoO.Sims.Inventory.SimInventoryItemsInContainerSettings;
namespace AKCondinoO.Sims.Inventory{
    internal class SimObjectAsInventoryItemSettings:MonoBehaviour{
     [SerializeField]internal HandsUsage handsUsage;
     [SerializeField]internal WeaponTypes weaponType;
     //  TO DO: create a "fallback" transform for any motion, which is the transform for a container that doesn't need to change by sim actor's motion
     [SerializeField]internal SimInventoryItemInContainerData[]inventorySettings;
        [Serializable]internal class SimInventoryItemInContainerData{
         [SerializeField]internal string simInventoryType;
         [SerializeField]internal int simInventorySpaceUse;
         [SerializeField]internal SimObjectAsInventoryItemTransformForMotion[]transformSettingsForMotion;
            [Serializable]internal class SimObjectAsInventoryItemTransformForMotion{
             [SerializeField]internal ActorMotion motion;
             [SerializeField]internal string parentBodyPartName;
             [SerializeField]internal bool transformIsDeterminant;
             [SerializeField]internal Transform transform;
            }
        }
    }
}