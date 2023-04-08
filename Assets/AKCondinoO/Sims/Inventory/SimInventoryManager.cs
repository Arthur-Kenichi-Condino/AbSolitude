#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryManager:MonoBehaviour,ISingletonInitialization{
     internal static SimInventoryManager singleton{get;set;}
     internal static string simInventorySavePath;
     internal static string simInventoryDataSavePath;
     internal static string idsFile;
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          simInventorySavePath=string.Format("{0}{1}",Core.savePath,"SimInventory/");
          Directory.CreateDirectory(simInventorySavePath);
          simInventoryDataSavePath=string.Format("{0}{1}",simInventorySavePath,"Data/");
          Directory.CreateDirectory(simInventoryDataSavePath);
          idsFile=string.Format("{0}{1}",simInventorySavePath,"ids.txt");
          FileStream idsFileStream=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStream=new FileStream(idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter idsFileStreamWriter=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamWriter=new StreamWriter(idsFileStream);
          StreamReader idsFileStreamReader=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamReader=new StreamReader(idsFileStream);
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
          string line;
          while((line=idsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int typeStringStart=line.IndexOf("type=")+5;
           int typeStringEnd  =line.IndexOf(" , ",typeStringStart);
           string typeString=line.Substring(typeStringStart,typeStringEnd-typeStringStart);
           Type t=Type.GetType(typeString);
           if(t==null){continue;}
           Log.DebugMessage("t:"+t);
           int nextIdStringStart=line.IndexOf("nextId=",typeStringEnd)+7;
           int nextIdStringEnd  =line.IndexOf(" } , endOfLine",nextIdStringStart);
           string nextIdString=line.Substring(nextIdStringStart,nextIdStringEnd-nextIdStringStart);
           ulong nextId=ulong.Parse(nextIdString,NumberStyles.Any,CultureInfoUtil.en_US);
           ids[t]=nextId;
          }
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimInventoryManager:OnDestroyingCoreEvent");
         if(Core.singleton.isServer){
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