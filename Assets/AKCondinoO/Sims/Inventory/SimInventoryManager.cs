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
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
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
        internal void AddInventoryTo(SimObject simObject,Type simInventoryType){
         if(!ids.TryGetValue(simInventoryType,out ulong idNumber)){
          ids.Add(simInventoryType,1uL);
          idNumber=0uL;
         }else{
          ids[simInventoryType]++;
         }
         if(!simObject.inventory.ContainsKey(simInventoryType)){
          simObject.inventory.Add(simInventoryType,new Dictionary<ulong,SimInventory>());
         }
         if(
            simInventoryType==typeof(SimInventory)
         ){
          simObject.inventory[typeof(SimInventory)].Add(idNumber,new SimInventory(idNumber,simObject,0));
         }else if(
            simInventoryType==typeof(SimHands)
         ){
          simObject.inventory[typeof(SimHands)].Add(idNumber,new SimHands(idNumber,simObject));
         }else{
          Log.Warning("add inventory of simInventoryType could not be handled:"+simInventoryType);
         }
        }
    }
}