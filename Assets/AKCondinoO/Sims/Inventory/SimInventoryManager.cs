#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class SimInventoryManager:MonoBehaviour,ISingletonInitialization{
     internal static SimInventoryManager singleton{get;set;}
     internal PersistentSimInventoryDataSavingBackgroundContainer persistentSimInventoryDataSavingBG;
     internal PersistentSimInventoryDataSavingMultithreaded       persistentSimInventoryDataSavingBGThread;
     internal PersistentSimInventoryDataLoadingBackgroundContainer persistentSimInventoryDataLoadingBG;
     internal PersistentSimInventoryDataLoadingMultithreaded       persistentSimInventoryDataLoadingBGThread;
     internal static string simInventorySavePath;
     internal static string simInventoryDataSavePath;
     internal static string idsFile;
     internal static string releasedIdsFile;
     internal readonly HashSet<Type>registeredSimInventoryTypes=new HashSet<Type>();
     internal readonly List<Type>simInventoryTypesPendingRegistrationForDataSaving=new List<Type>();
     internal readonly Dictionary<Type,ulong>ids=new Dictionary<Type,ulong>();
     internal readonly Dictionary<Type,HashSet<ulong>>releasedIds=new Dictionary<Type,HashSet<ulong>>();
     internal readonly Dictionary<Type,LinkedList<SimInventory>>pool=new Dictionary<Type,LinkedList<SimInventory>>();
     internal readonly Dictionary<(Type simInventoryType,ulong idNumber),SimInventory>assigned                 =new Dictionary<(Type,ulong),SimInventory>();
     internal readonly Dictionary<(Type simInventoryType,ulong idNumber),SimInventory>active                   =new Dictionary<(Type,ulong),SimInventory>();
     internal readonly Dictionary<(Type simInventoryType,ulong idNumber),SimInventory>unassigningAndReleasingId=new Dictionary<(Type,ulong),SimInventory>();
        void RegisterAsValidSimInventoryType(Type simInventoryType){
         if(registeredSimInventoryTypes.Contains(simInventoryType)){
          Log.DebugMessage("SimInventoryType already registered:"+simInventoryType);
          return;
         }
         Log.DebugMessage("registering SimInventoryType:"+simInventoryType);
         if(!ids.TryGetValue(simInventoryType,out _)){
          ids.Add(simInventoryType,0uL);
         }
         if(!releasedIds.TryGetValue(simInventoryType,out _)){
          releasedIds.Add(simInventoryType,new HashSet<ulong>());
         }
         pool.Add(simInventoryType,new LinkedList<SimInventory>());
         simInventoryTypesPendingRegistrationForDataSaving.Add(simInventoryType);
         registeredSimInventoryTypes.Add(simInventoryType);
        }
        internal void OnSaveSimInventoryTypeRegistration(Type simInventoryType){
         Log.DebugMessage("On save SimInventoryType registration:"+simInventoryType);
         persistentSimInventoryDataSavingBG.idsToRelease.Add(simInventoryType,new HashSet<ulong>());
         persistentSimInventoryDataSavingBG.persistentIds.Add(simInventoryType,0);
         persistentSimInventoryDataSavingBG.persistentReleasedIds.Add(simInventoryType,new HashSet<ulong>());
         persistentSimInventoryDataSavingBG.onSavedReleasedIds.Add(simInventoryType,new HashSet<ulong>());
        }
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
          FileStream releasedIdsFileStream=persistentSimInventoryDataSavingBGThread.releasedIdsFileStream=new FileStream(releasedIdsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter releasedIdsFileStreamWriter=persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamWriter=new StreamWriter(releasedIdsFileStream);
          StreamReader releasedIdsFileStreamReader=persistentSimInventoryDataSavingBGThread.releasedIdsFileStreamReader=new StreamReader(releasedIdsFileStream);
          releasedIdsFileStream.Position=0L;
          releasedIdsFileStreamReader.DiscardBufferedData();
          string line;
          while((line=releasedIdsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int simInventoryTypeStringStart=line.IndexOf("type=")+5;
           int simInventoryTypeStringEnd  =line.IndexOf(" , ",simInventoryTypeStringStart);
           string simInventoryTypeString=line.Substring(simInventoryTypeStringStart,simInventoryTypeStringEnd-simInventoryTypeStringStart);
           Type simInventoryType=Type.GetType(simInventoryTypeString);
           if(simInventoryType==null){continue;}
           releasedIds[simInventoryType]=new HashSet<ulong>();
           int releasedIdsListStringStart=line.IndexOf("{ ",simInventoryTypeStringEnd)+2;
           int releasedIdsListStringEnd  =line.IndexOf(", } , } , endOfLine",releasedIdsListStringStart);
           if(releasedIdsListStringEnd>=0){
            string releasedIdsListString=line.Substring(releasedIdsListStringStart,releasedIdsListStringEnd-releasedIdsListStringStart);
            string[]simInventoryIdNumberStrings=releasedIdsListString.Split(',');
            foreach(string simInventoryIdNumberString in simInventoryIdNumberStrings){
             //Log.DebugMessage("adding releasedId of "+t+": "+idString+"...");
             ulong simInventoryIdNumber=ulong.Parse(simInventoryIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
             releasedIds[simInventoryType].Add(simInventoryIdNumber);
            }
           }
           RegisterAsValidSimInventoryType(simInventoryType);
          }
          FileStream idsFileStream=persistentSimInventoryDataSavingBGThread.idsFileStream=new FileStream(idsFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          StreamWriter idsFileStreamWriter=persistentSimInventoryDataSavingBGThread.idsFileStreamWriter=new StreamWriter(idsFileStream);
          StreamReader idsFileStreamReader=persistentSimInventoryDataSavingBGThread.idsFileStreamReader=new StreamReader(idsFileStream);
          idsFileStream.Position=0L;
          idsFileStreamReader.DiscardBufferedData();
          while((line=idsFileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int simInventoryTypeStringStart=line.IndexOf("type=")+5;
           int simInventoryTypeStringEnd  =line.IndexOf(" , ",simInventoryTypeStringStart);
           string simInventoryTypeString=line.Substring(simInventoryTypeStringStart,simInventoryTypeStringEnd-simInventoryTypeStringStart);
           Type simInventoryType=Type.GetType(simInventoryTypeString);
           if(simInventoryType==null){continue;}
           Log.DebugMessage("t:"+simInventoryType);
           int nextSimInventoryIdNumberStringStart=line.IndexOf("nextId=",simInventoryTypeStringEnd)+7;
           int nextSimInventoryIdNumberStringEnd  =line.IndexOf(" } , endOfLine",nextSimInventoryIdNumberStringStart);
           string nextSimInventoryIdNumberString=line.Substring(nextSimInventoryIdNumberStringStart,nextSimInventoryIdNumberStringEnd-nextSimInventoryIdNumberStringStart);
           ulong nextSimInventoryIdNumber=ulong.Parse(nextSimInventoryIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
           ids[simInventoryType]=nextSimInventoryIdNumber;
           RegisterAsValidSimInventoryType(simInventoryType);
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
        readonly Dictionary<Type,(ConstructorInfo ctorInfo,object[]ctorParams)>ctorCache=new Dictionary<Type,(ConstructorInfo,object[])>();
        internal void AddInventoryTo(SimObject simObject,Type simInventoryType){
         ulong number;
         this.releasedIds.TryGetValue(simInventoryType,out HashSet<ulong>releasedIds);
         if(releasedIds!=null&&releasedIds.Count>0){
          number=releasedIds.Last();
          releasedIds.Remove(number);
         }else{
          if(!ids.TryGetValue(simInventoryType,out number)){
           ids.Add(simInventoryType,1uL);
           number=0uL;
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
          if(ctorCache.TryGetValue(simInventoryType,out(ConstructorInfo ctorInfo,object[]ctorParams)ctorCached)){
           //Log.DebugMessage(simInventoryType+":using ctorCached:"+ctorCached);
           result=(SimInventory)ctorCached.ctorInfo.Invoke(ctorCached.ctorParams);
           return result;
          }
          ConstructorInfo ctor=simInventoryType.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,simInventoryCtorParamsTypes,null);
          if(ctor!=null){
           ctorCache[simInventoryType]=(ctor,simInventoryCtorParams);
           result=(SimInventory)ctor.Invoke(simInventoryCtorParams);
           return result;
          }
          ctor=simInventoryType.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,simInventoryBaseCtorParamsTypes,null);
          if(ctor!=null){
           simInventoryBaseCtorParams[0]=0;
           ctorCache[simInventoryType]=(ctor,simInventoryBaseCtorParams);
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
          RegisterAsValidSimInventoryType(simInventoryType);
          (Type simInventoryType,ulong idNumber)simInventoryId=(simInventoryType,number);
          simInventory.OnAssign(simInventoryId,simObject);
         }
        }
    }
}