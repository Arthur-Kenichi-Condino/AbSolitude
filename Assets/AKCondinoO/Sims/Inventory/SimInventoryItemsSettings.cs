#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryItemsSettings{
        internal struct SimObjectSettings{
         public int inventorySpaces;
            internal SimObjectSettings(int inventorySpaces){
             this.inventorySpaces=inventorySpaces;
            }
        }
     internal readonly Dictionary<Type,SimObjectSettings>allSettings=new Dictionary<Type,SimObjectSettings>();
        internal SimInventoryItemsSettings(){
        }
        internal void Set(){
         Log.DebugMessage("Set SimInventoryItemsSettings");
         allSettings.Add(typeof(RemingtonModel700BDL),new SimObjectSettings(2));
        }
    }
}