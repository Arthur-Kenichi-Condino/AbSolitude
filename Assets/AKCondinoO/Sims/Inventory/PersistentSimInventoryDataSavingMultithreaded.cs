using AKCondinoO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataSavingBackgroundContainer:BackgroundContainer{
    }
    internal class PersistentSimInventoryDataSavingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataSavingBackgroundContainer>{
        protected override void Cleanup(){
        }
        protected override void Execute(){
        }
    }
}