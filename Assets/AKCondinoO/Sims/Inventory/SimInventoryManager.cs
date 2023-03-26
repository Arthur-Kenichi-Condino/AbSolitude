#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryManager:MonoBehaviour,ISingletonInitialization{
     internal static SimInventoryManager singleton{get;set;}
     internal static string simInventorySavePath;
     internal static string idsFile;
     FileStream idsFileStream;
     StreamWriter idsFileStreamWriter;
     StreamReader idsFileStreamReader;
      readonly StringBuilder idsStringBuilder=new StringBuilder();
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          simInventorySavePath=string.Format("{0}{1}",Core.savePath,"SimInventory/");
          Directory.CreateDirectory(simInventorySavePath);
          idsFile=string.Format("{0}{1}",simInventorySavePath,"ids.txt");
          idsFileStream=new FileStream(idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          idsFileStreamWriter=new StreamWriter(idsFileStream);
          idsFileStreamReader=new StreamReader(idsFileStream);
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
          string line;
          while((line=idsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
          }
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimInventoryManager:OnDestroyingCoreEvent");
         if(Core.singleton.isServer){
          idsStringBuilder.Clear();
          foreach(var typeIdsPair in ids){
          }
          idsFileStreamWriter.Dispose();
          idsFileStreamReader.Dispose();
         }
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