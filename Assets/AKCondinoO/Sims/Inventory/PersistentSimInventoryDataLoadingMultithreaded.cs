#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataLoadingBackgroundContainer:BackgroundContainer{
     internal SpawnData spawnDataFromFiles;
    }
    internal class PersistentSimInventoryDataLoadingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simInventoryFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>simInventoryFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<(Type simType,ulong number),int>simInventorySpawnAtIndexById=new Dictionary<(Type simType,ulong number),int>();
        protected override void Cleanup(){
         simInventorySpawnAtIndexById.Clear();
        }
        protected override void Execute(){
         if(container.spawnDataFromFiles==null){
          Log.Error("container.spawnDataFromFiles==null");
          return;
         }else{
          Log.DebugMessage("Execute()");
          for(int i=0;i<container.spawnDataFromFiles.at.Count;++i){
           var at=container.spawnDataFromFiles.at[i];
           if(at.id!=null){
            (Type simType,ulong number)id=(at.type,at.id.Value);
            simInventorySpawnAtIndexById.Add(id,i);
           }
          }
          foreach(var typeFileStreamPair in this.simInventoryFileStream){
           Type t=typeFileStreamPair.Key;
           FileStream fileStream=typeFileStreamPair.Value;
           StreamReader fileStreamReader=this.simInventoryFileStreamReader[t];
           Log.DebugMessage("loading sim inventory data for type:"+t);
           fileStream.Position=0L;
           fileStreamReader.DiscardBufferedData();
           string line;
           while((line=fileStreamReader.ReadLine())!=null){
            if(string.IsNullOrEmpty(line)){continue;}
            int idStringStart=line.IndexOf("id=")+3;
            int idStringEnd  =line.IndexOf(" , ",idStringStart);
            ulong id=ulong.Parse(line.Substring(idStringStart,idStringEnd-idStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            (Type simType,ulong number)simObjectId=(t,id);
            if(simInventorySpawnAtIndexById.TryGetValue(simObjectId,out int simObjectSpawnAtIndex)){
             Log.DebugMessage("found sim inventory data for simObjectId:"+simObjectId);
            }
           }
          }
         }
        }
    }
}