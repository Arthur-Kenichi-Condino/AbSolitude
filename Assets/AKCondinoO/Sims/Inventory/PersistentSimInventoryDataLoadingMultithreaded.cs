#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataLoadingBackgroundContainer:BackgroundContainer{
    }
    internal class PersistentSimInventoryDataLoadingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataLoadingBackgroundContainer>{
        protected override void Cleanup(){
        }
        protected override void Execute(){
        }
    }
}