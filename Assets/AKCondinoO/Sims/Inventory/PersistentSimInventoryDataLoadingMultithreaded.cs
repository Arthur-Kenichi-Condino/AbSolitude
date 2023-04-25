#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataLoadingBackgroundContainer:BackgroundContainer{
     internal readonly AutoResetEvent waitingForSimObjectSpawnData=new AutoResetEvent(false);
     internal SpawnData spawnDataFromFiles;
    }
    internal class PersistentSimInventoryDataLoadingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simInventoryFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>simInventoryFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<(Type simObjectType,ulong idNumber),int>simInventorySpawnAtIndexById=new Dictionary<(Type simObjectType,ulong idNumber),int>();
        protected override void Cleanup(){
         simInventorySpawnAtIndexById.Clear();
        }
        protected override void Execute(){
         if(container.spawnDataFromFiles==null){
          Log.Error("container.spawnDataFromFiles==null");
          return;
         }else{
          Log.DebugMessage("Execute()");
          container.waitingForSimObjectSpawnData.WaitOne();
          for(int i=0;i<container.spawnDataFromFiles.at.Count;++i){
           var at=container.spawnDataFromFiles.at[i];
           if(at.idNumber!=null){
            (Type simObjectType,ulong idNumber)id=(at.simObjectType,at.idNumber.Value);
            simInventorySpawnAtIndexById.Add(id,i);
           }
          }
          foreach(var simObjectTypeFileStreamPair in this.simInventoryFileStream){
           Type simObjectType=simObjectTypeFileStreamPair.Key;
           FileStream fileStream=simObjectTypeFileStreamPair.Value;
           StreamReader fileStreamReader=this.simInventoryFileStreamReader[simObjectType];
           Log.DebugMessage("loading sim inventory data for type:"+simObjectType);
           fileStream.Position=0L;
           fileStreamReader.DiscardBufferedData();
           string line;
           while((line=fileStreamReader.ReadLine())!=null){
            if(string.IsNullOrEmpty(line)){continue;}
            int simObjectIdNumberStringStart=line.IndexOf("id=")+3;
            int simObjectIdNumberStringEnd  =line.IndexOf(" , ",simObjectIdNumberStringStart);
            ulong simObjectIdNumber=ulong.Parse(line.Substring(simObjectIdNumberStringStart,simObjectIdNumberStringEnd-simObjectIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            (Type simObjectType,ulong idNumber)simObjectId=(simObjectType,simObjectIdNumber);
            if(simInventorySpawnAtIndexById.TryGetValue(simObjectId,out int simObjectSpawnAtIndex)){
             Log.DebugMessage("found sim inventory data for simObjectId:"+simObjectId);
            }
           }
          }
         }
        }
    }
}