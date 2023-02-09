#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
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
    }
}