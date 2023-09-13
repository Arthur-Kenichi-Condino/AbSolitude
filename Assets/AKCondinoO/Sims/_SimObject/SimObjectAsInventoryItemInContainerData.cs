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
    internal class SimObjectAsInventoryItemInContainerData:MonoBehaviour{
     [SerializeField]internal HandsUsage handsUsage;
     [SerializeField]internal WeaponTypes weaponType;
     [SerializeField]internal InContainerData[]dataArrays;
         [Serializable]internal class InContainerData{
          [SerializeField]internal string simInventoryType;
          [SerializeField]internal int simInventorySpacesUsage;
          [SerializeField]internal InContainerTransformData[]transformData;
         }
         //  TO DO: create a "fallback" transform for any motion, which is the transform for a container that doesn't need to change by sim actor's motion
    }
}