#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor;
using static AKCondinoO.Sims.Inventory.SimInventoryItemsInContainerSettings;
namespace AKCondinoO.Sims.Inventory{
    internal class SimObjectAsInventoryItemSettings:MonoBehaviour{
     [SerializeField]internal HandsUsage handsUsage;
     [SerializeField]internal WeaponTypes weaponType;
     [SerializeField]internal Transform  leftHandGrabTransform;
     [SerializeField]internal Transform rightHandGrabTransform;
        [Serializable]public struct SimInventoryItemInContainerData{
         [SerializeField]internal string simInventoryType;
         [SerializeField]internal int simInventorySpaceUse;
        }
     [SerializeField]internal SimInventoryItemInContainerData[]inventorySettings;
    }
}