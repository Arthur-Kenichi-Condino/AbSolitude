#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Inventory.SimInventoryItemsInContainerSettings;
namespace AKCondinoO.Sims.Inventory{
    internal class SimObjectAsInventoryItemSettings:MonoBehaviour{
     [SerializeField]internal HandsUsage handsUsage;
        [Serializable]public struct SimInventoryItemInContainerData{
         [SerializeField]internal string SimInventoryType;
         [SerializeField]internal int SimInventorySpaceUse;
        }
     [SerializeField]internal SimInventoryItemInContainerData[]inventorySettings;
    }
}