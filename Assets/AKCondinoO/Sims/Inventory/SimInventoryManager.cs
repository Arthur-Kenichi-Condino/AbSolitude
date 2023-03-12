#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryManager:MonoBehaviour,ISingletonInitialization{
     internal static SimInventoryManager singleton{get;set;}
     internal static string simInventorySavePath;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          simInventorySavePath=string.Format("{0}{1}",Core.savePath,"SimInventory/");
          Directory.CreateDirectory(simInventorySavePath);
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimInventoryManager:OnDestroyingCoreEvent");
        }
    }
}