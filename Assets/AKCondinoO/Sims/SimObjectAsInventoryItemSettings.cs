#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimObjectAsInventoryItemSettings:MonoBehaviour{
        [Serializable]public struct SimInventorySpaceUse{
         [SerializeField]internal string SimInventoryType;
         [SerializeField]internal int Count;
        }
     [SerializeField]internal SimInventorySpaceUse[]inventorySpace;
    }
}