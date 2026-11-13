using AKCondinoO.Bootstrap;
using AKCondinoO.PersistentData;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimIds{
     private readonly Dictionary<(Type type,string variant),ulong>counters=new();
        internal ulong Next((Type type,string variant)key){
         bool initialized=counters.TryGetValue(key,out ulong id);
         if(!initialized){
          (PersistentDataManager.singleton.GetFileManager(typeof(SimIdsFiles))as SimIdsFiles)?.ReadFromSimIdsFile(key.type,key.variant,out id);
          Logs.Debug(()=>"'Next':'init':key:"+key+":id:"+id);
         }
         id++;
         counters[key]=id;
         (PersistentDataManager.singleton.GetFileManager(typeof(SimIdsFiles))as SimIdsFiles)?.WriteToSimIdsFile(key.type,key.variant,id);
         return id;
        }
    }
}