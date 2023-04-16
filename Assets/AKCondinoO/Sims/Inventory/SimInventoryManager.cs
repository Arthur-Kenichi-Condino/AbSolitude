#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryManager:MonoBehaviour,ISingletonInitialization{
     internal static SimInventoryManager singleton{get;set;}
     internal static string simInventorySavePath;
     internal static string simInventoryDataSavePath;
     internal static string idsFile;
     internal static string releasedIdsFile;
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
     internal readonly Dictionary<Type,List<ulong>>releasedIds=new Dictionary<Type,List<ulong>>();
     internal readonly Dictionary<Type,LinkedList<SimInventory>>pool=new Dictionary<Type,LinkedList<SimInventory>>();
     internal readonly Dictionary<(Type simInventoryType,ulong number),SimInventory>spawned    =new Dictionary<(Type,ulong),SimInventory>();
     internal readonly Dictionary<(Type simInventoryType,ulong number),SimInventory>active     =new Dictionary<(Type,ulong),SimInventory>();
     internal readonly Dictionary<(Type simInventoryType,ulong number),SimInventory>releasingId=new Dictionary<(Type,ulong),SimInventory>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          simInventorySavePath=string.Format("{0}{1}",Core.savePath,"SimInventory/");
          Directory.CreateDirectory(simInventorySavePath);
          simInventoryDataSavePath=string.Format("{0}{1}",simInventorySavePath,"Data/");
          Directory.CreateDirectory(simInventoryDataSavePath);
                  idsFile=string.Format("{0}{1}",simInventorySavePath,        "ids.txt");
          releasedIdsFile=string.Format("{0}{1}",simInventorySavePath,"releasedIds.txt");
          FileStream releasedIdsFileStream=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.releasedIdsFileStream=new FileStream(releasedIdsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter releasedIdsFileStreamWriter=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamWriter=new StreamWriter(releasedIdsFileStream);
          StreamReader releasedIdsFileStreamReader=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamReader=new StreamReader(releasedIdsFileStream);
          releasedIdsFileStream.Position=0L;
          releasedIdsFileStreamReader.DiscardBufferedData();
          string line;
          while((line=releasedIdsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int typeStringStart=line.IndexOf("type=")+5;
           int typeStringEnd  =line.IndexOf(" , ",typeStringStart);
           string typeString=line.Substring(typeStringStart,typeStringEnd-typeStringStart);
           Type t=Type.GetType(typeString);
           if(t==null){continue;}
           releasedIds[t]=new List<ulong>();
           int releasedIdsListStringStart=line.IndexOf("{ ",typeStringEnd)+2;
           int releasedIdsListStringEnd  =line.IndexOf(", } , } , endOfLine",releasedIdsListStringStart);
           if(releasedIdsListStringEnd>=0){
            string releasedIdsListString=line.Substring(releasedIdsListStringStart,releasedIdsListStringEnd-releasedIdsListStringStart);
            string[]idStrings=releasedIdsListString.Split(',');
            foreach(string idString in idStrings){
             //Log.DebugMessage("adding releasedId of "+t+": "+idString+"...");
             ulong id=ulong.Parse(idString,NumberStyles.Any,CultureInfoUtil.en_US);
             releasedIds[t].Add(id);
            }
           }
          }
          FileStream idsFileStream=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStream=new FileStream(idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter idsFileStreamWriter=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamWriter=new StreamWriter(idsFileStream);
          StreamReader idsFileStreamReader=SimObjectManager.singleton.persistentSimInventoryDataSavingBGThread.idsFileStreamReader=new StreamReader(idsFileStream);
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
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
     readonly Type[]simInventoryBaseCtorParamsTypes=new Type[]{typeof(int)};
      readonly object[]simInventoryBaseCtorParams=new object[]{0};
      readonly Type[]simInventoryCtorParamsTypes=new Type[]{};
       readonly object[]simInventoryCtorParams=new object[]{};
        internal void AddInventoryTo(SimObject simObject,Type simInventoryType){
         ulong idNumber;
         this.releasedIds.TryGetValue(simInventoryType,out List<ulong>releasedIds);
         if(releasedIds!=null&&releasedIds.Count>0){
          idNumber=releasedIds[releasedIds.Count-1];
          releasedIds.RemoveAt(releasedIds.Count-1);
         }else{
          if(!ids.TryGetValue(simInventoryType,out idNumber)){
           ids.Add(simInventoryType,1uL);
           idNumber=0uL;
          }else{
           ids[simInventoryType]++;
          }
         }
         SimInventory SpawnInventory(){
          SimInventory result=null;
          if(this.pool.TryGetValue(simInventoryType,out LinkedList<SimInventory>pool)){
           if(pool.Count>0){
            result=pool.First.Value;
            pool.RemoveFirst();
            result.pooled=null;
            return result;
           }
          }
          ConstructorInfo ctor=simInventoryType.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,simInventoryCtorParamsTypes,null);
          if(ctor!=null){
           result=(SimInventory)ctor.Invoke(simInventoryCtorParams);
           return result;
          }
          ctor=simInventoryType.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,simInventoryBaseCtorParamsTypes,null);
          if(ctor!=null){
           simInventoryBaseCtorParams[0]=0;
           result=(SimInventory)ctor.Invoke(simInventoryBaseCtorParams);
           return result;
          }
          Log.Warning("adding inventory of simInventoryType "+simInventoryType+" error: type's Constructor could not be handled");
          return result;
         }
         if(!simObject.inventory.ContainsKey(simInventoryType)){
          simObject.inventory.Add(simInventoryType,new Dictionary<ulong,SimInventory>());
         }
         SimInventory simInventory=SpawnInventory();
         if(simInventory!=null){
          (Type simInventoryType,ulong number)simInventoryId=(simInventoryType,idNumber);
          simInventory.OnAssign(simInventoryId,simObject);
         }
        }
    }
}