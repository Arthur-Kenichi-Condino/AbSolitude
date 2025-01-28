#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class PersistentDataLoadingBackgroundContainer:BackgroundContainer{
     internal AutoResetEvent waitingForSimObjectSpawnData;
     internal readonly HashSet<int>terraincnkIdxToLoad=new HashSet<int>();
     internal readonly Dictionary<(Type simType,ulong number),(Vector3 position,Vector3 eulerAngles,Vector3 localScale,(Type simType,ulong number)?asInventoryItemOwnerId)>specificIdsToLoad=new Dictionary<(Type,ulong),(Vector3,Vector3,Vector3,(Type,ulong)?)>();
     internal readonly SpawnData spawnDataFromFiles=new SpawnData();
    }
    internal class PersistentDataLoadingMultithreaded:BaseMultithreaded<PersistentDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simObjectFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>simObjectFileStreamReader=new Dictionary<Type,StreamReader>();
       internal readonly Dictionary<Type,FileStream>simActorFileStream=new Dictionary<Type,FileStream>();
        internal readonly Dictionary<Type,StreamReader>simActorFileStreamReader=new Dictionary<Type,StreamReader>();
         internal readonly Dictionary<Type,FileStream>simStatsFileStream=new Dictionary<Type,FileStream>();
          internal readonly Dictionary<Type,StreamReader>simStatsFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<Type,Dictionary<ulong,int>>simObjectSpawnAtIndexByType=new Dictionary<Type,Dictionary<ulong,int>>();
        readonly Dictionary<Type,Dictionary<ulong,int>>simActorSpawnAtIndexByType=new Dictionary<Type,Dictionary<ulong,int>>();
        protected override void Cleanup(){
         foreach(var typeSimActorSpawnAtIndexPair in simActorSpawnAtIndexByType){
          typeSimActorSpawnAtIndexPair.Value.Clear();
         }
         foreach(var typeSimObjectSpawnAtIndexPair in simObjectSpawnAtIndexByType){
          typeSimObjectSpawnAtIndexPair.Value.Clear();
         }
        }
        protected override void Execute(){
         container.spawnDataFromFiles.dequeued=false;
         foreach(var simObjectTypeFileStreamPair in this.simObjectFileStream){
          Type simObjectType=simObjectTypeFileStreamPair.Key;
          Dictionary<ulong,int>simObjectSpawnAtIndex=null;
          if(!simObjectSpawnAtIndexByType.TryGetValue(simObjectType,out simObjectSpawnAtIndex)){
           simObjectSpawnAtIndexByType.Add(simObjectType,simObjectSpawnAtIndex=new Dictionary<ulong,int>());
          }
          Dictionary<ulong,int>simActorSpawnAtIndex=null;
          if(SimObjectUtil.IsSimActor(simObjectType)){
           if(!simActorSpawnAtIndexByType.TryGetValue(simObjectType,out simActorSpawnAtIndex)){
            simActorSpawnAtIndexByType.Add(simObjectType,simActorSpawnAtIndex=new Dictionary<ulong,int>());
           }
          }
          FileStream fileStream=simObjectTypeFileStreamPair.Value;
          StreamReader fileStreamReader=this.simObjectFileStreamReader[simObjectType];
          //Log.DebugMessage("loading data for type:"+t);
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
           int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
           string cnkIdxString=line.Substring(cnkIdxStringStart,cnkIdxStringEnd-cnkIdxStringStart);
           int cnkIdx=int.Parse(cnkIdxString,NumberStyles.Any,CultureInfoUtil.en_US);
           //Log.DebugMessage("reading line for cnkIdx:"+cnkIdx);
           if(container.specificIdsToLoad.Count>0||container.terraincnkIdxToLoad.Contains(cnkIdx)){
            //Log.DebugMessage("must load sim objects at line for cnkIdx:"+cnkIdx);
            int simObjectStringStart=cnkIdxStringEnd+3;
            while((simObjectStringStart=line.IndexOf("simObject=",simObjectStringStart))>=0){
             //Log.DebugMessage("sim object found at cnkIdx:"+cnkIdx);
             int simObjectStringEnd=line.IndexOf("} , ",simObjectStringStart)+4;
             string simObjectString=line.Substring(simObjectStringStart,simObjectStringEnd-simObjectStringStart);
             int simObjectIdNumberStringStart=simObjectString.IndexOf("id=")+3;
             int simObjectIdNumberStringEnd  =simObjectString.IndexOf(" , ",simObjectIdNumberStringStart);
             string simObjectIdNumberString=simObjectString.Substring(simObjectIdNumberStringStart,simObjectIdNumberStringEnd-simObjectIdNumberStringStart);
             ulong simObjectIdNumber=ulong.Parse(simObjectIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
             (Type simObjectType,ulong idNumber)simObjectId=(simObjectType,simObjectIdNumber);
             int persistentDataStringStart=simObjectString.IndexOf("persistentData=",simObjectIdNumberStringEnd+3);
             int persistentDataStringEnd  =simObjectString.IndexOf(" , }",persistentDataStringStart)+4;
             string persistentDataString=simObjectString.Substring(persistentDataStringStart,persistentDataStringEnd-persistentDataStringStart);
             SimObject.PersistentData persistentData=SimObject.PersistentData.Parse(persistentDataString);
             if(container.specificIdsToLoad.TryGetValue(simObjectId,out var specificIdData)){
              persistentData.position=specificIdData.position;
              persistentData.rotation=Quaternion.Euler(specificIdData.eulerAngles);
              persistentData.localScale=specificIdData.localScale;
              container.specificIdsToLoad.Remove(simObjectId);
             }else{
              if(persistentData.isInventoryItem){
               //Log.DebugMessage("don't load sim object that is persistentData.isInventoryItem");
               goto _Skip;
              }
             }
             container.spawnDataFromFiles.at.Add((persistentData.position,persistentData.rotation.eulerAngles,persistentData.localScale,simObjectId.simObjectType,simObjectId.idNumber,persistentData));
             //  now add id to be searched for persistent stats
             simObjectSpawnAtIndex.Add(simObjectId.idNumber,container.spawnDataFromFiles.at.Count-1);
             if(simActorSpawnAtIndex!=null){
              simActorSpawnAtIndex.Add(simObjectId.idNumber,container.spawnDataFromFiles.at.Count-1);
             }
             _Skip:{}
             simObjectStringStart=simObjectStringEnd;
            }
           }
          }
          //  TO DO: sim stats
          fileStream      =this.simStatsFileStream      [simObjectType];
          fileStreamReader=this.simStatsFileStreamReader[simObjectType];
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          line=null;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int simObjectIdNumberStringStart=line.IndexOf("id=")+3;
           int simObjectIdNumberStringEnd  =line.IndexOf(" , ",simObjectIdNumberStringStart);
           ulong simObjectIdNumber=ulong.Parse(line.Substring(simObjectIdNumberStringStart,simObjectIdNumberStringEnd-simObjectIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
           if(simObjectSpawnAtIndex.TryGetValue(simObjectIdNumber,out int index)){
            Log.DebugMessage("sim object stats data has to be loaded for id:"+simObjectIdNumber);
            int persistentStatsStringStart=line.IndexOf("persistentStats=",simObjectIdNumberStringEnd+3);
            int persistentStatsStringEnd  =line.IndexOf(" , }",persistentStatsStringStart)+4;
            string persistentStatsString=line.Substring(persistentStatsStringStart,persistentStatsStringEnd-persistentStatsStringStart);
            SimObject.PersistentStats persistentStats=SimObject.PersistentStats.Parse(persistentStatsString);
            container.spawnDataFromFiles.statsData.Add(index,persistentStats);
           }
          }
          if(SimObjectUtil.IsSimActor(simObjectType)){
           Type simActorType=simObjectType;
           fileStream      =this.simActorFileStream      [simActorType];
           fileStreamReader=this.simActorFileStreamReader[simActorType];
           fileStream.Position=0L;
           fileStreamReader.DiscardBufferedData();
           line=null;
           while((line=fileStreamReader.ReadLine())!=null){
            if(string.IsNullOrEmpty(line)){continue;}
            int simActorIdNumberStringStart=line.IndexOf("id=")+3;
            int simActorIdNumberStringEnd  =line.IndexOf(" , ",simActorIdNumberStringStart);
            ulong simActorIdNumber=ulong.Parse(line.Substring(simActorIdNumberStringStart,simActorIdNumberStringEnd-simActorIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            if(simActorSpawnAtIndex.TryGetValue(simActorIdNumber,out int index)){
             //Log.DebugMessage("sim actor data has to be loaded for id:"+id);
             int persistentSimActorDataStringStart=line.IndexOf("persistentSimActorData=",simActorIdNumberStringEnd+3);
             int persistentSimActorDataStringEnd  =line.IndexOf(" , }",persistentSimActorDataStringStart)+4;
             string persistentSimActorDataString=line.Substring(persistentSimActorDataStringStart,persistentSimActorDataStringEnd-persistentSimActorDataStringStart);
             SimActor.PersistentSimActorData persistentSimActorData=SimActor.PersistentSimActorData.Parse(persistentSimActorDataString);
             container.spawnDataFromFiles.actorData.Add(index,persistentSimActorData);
            }
           }
          }
         }
         foreach(var specificIdToLoad in container.specificIdsToLoad){
          (Type simObjectType,ulong idNumber)id=specificIdToLoad.Key;
          var specificIdData=specificIdToLoad.Value;
          container.spawnDataFromFiles.at.Add((specificIdData.position,specificIdData.eulerAngles,specificIdData.localScale,id.simObjectType,id.idNumber,new SimObject.PersistentData()));
         }
         container.specificIdsToLoad.Clear();
         container.waitingForSimObjectSpawnData.Set();
        }
    }
}