#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataLoadingBackgroundContainer:BackgroundContainer{
    }
    internal class PersistentSimInventoryDataLoadingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simInventoryFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>simInventoryFileStreamReader=new Dictionary<Type,StreamReader>();
        protected override void Cleanup(){
        }
        protected override void Execute(){
         Log.DebugMessage("Execute()");
        }
    }
}