#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class PersistentDataLoadingBackgroundContainer:BackgroundContainer{
     internal readonly HashSet<int>terraincnkIdxToLoad=new HashSet<int>();
     internal readonly Dictionary<(Type simType,ulong number),(Vector3 position,Vector3 eulerAngles,Vector3 localScale)>specificIdsToLoad=new Dictionary<(Type,ulong),(Vector3,Vector3,Vector3)>();
     internal readonly SpawnData spawnDataFromFiles=new SpawnData();
    }
    internal class PersistentDataLoadingMultithreaded:BaseMultithreaded<PersistentDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>fileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>fileStreamReader=new Dictionary<Type,StreamReader>();
       internal readonly Dictionary<Type,FileStream>simActorFileStream=new Dictionary<Type,FileStream>();
        internal readonly Dictionary<Type,StreamReader>simActorFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<Type,Dictionary<ulong,int>>simActorSpawnAtIndexByType=new Dictionary<Type,Dictionary<ulong,int>>();
        protected override void Cleanup(){
         foreach(var typeSimActorSpawnAtIndexPair in simActorSpawnAtIndexByType){
          typeSimActorSpawnAtIndexPair.Value.Clear();
         }
        }
        protected override void Execute(){
         container.spawnDataFromFiles.dequeued=false;
         foreach(var typeFileStreamPair in this.fileStream){
          Type t=typeFileStreamPair.Key;
          Dictionary<ulong,int>simActorSpawnAtIndex=null;
          if(SimObjectUtil.IsSimActor(t)){
           if(!simActorSpawnAtIndexByType.TryGetValue(t,out simActorSpawnAtIndex)){
            simActorSpawnAtIndexByType.Add(t,simActorSpawnAtIndex=new Dictionary<ulong,int>());
           }
          }
          FileStream fileStream=typeFileStreamPair.Value;
          StreamReader fileStreamReader=this.fileStreamReader[t];
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
             int idNumberStringStart=simObjectString.IndexOf("id=")+3;
             int idNumberStringEnd  =simObjectString.IndexOf(" , ",idNumberStringStart);
             string idNumberString=simObjectString.Substring(idNumberStringStart,idNumberStringEnd-idNumberStringStart);
             ulong idNumber=ulong.Parse(idNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
             (Type simType,ulong number)id=(t,idNumber);
             int persistentDataStringStart=simObjectString.IndexOf("persistentData=",idNumberStringEnd+3);
             int persistentDataStringEnd  =simObjectString.IndexOf(" , }",persistentDataStringStart)+4;
             string persistentDataString=simObjectString.Substring(persistentDataStringStart,persistentDataStringEnd-persistentDataStringStart);
             SimObject.PersistentData persistentData=SimObject.PersistentData.Parse(persistentDataString);
             if(container.specificIdsToLoad.TryGetValue(id,out var specificIdData)){
              persistentData.position=specificIdData.position;
              persistentData.rotation=Quaternion.Euler(specificIdData.eulerAngles);
              persistentData.localScale=specificIdData.localScale;
              container.specificIdsToLoad.Remove(id);
             }
             container.spawnDataFromFiles.at.Add((persistentData.position,persistentData.rotation.eulerAngles,persistentData.localScale,id.simType,id.number,persistentData));
             if(simActorSpawnAtIndex!=null){
              simActorSpawnAtIndex.Add(id.number,container.spawnDataFromFiles.at.Count-1);
             }
             simObjectStringStart=simObjectStringEnd;
            }
           }
          }
          if(SimObjectUtil.IsSimActor(t)){
           fileStream      =this.simActorFileStream      [t];
           fileStreamReader=this.simActorFileStreamReader[t];
           fileStream.Position=0L;
           fileStreamReader.DiscardBufferedData();
           line=null;
           while((line=fileStreamReader.ReadLine())!=null){
            if(string.IsNullOrEmpty(line)){continue;}
            int idStringStart=line.IndexOf("id=")+3;
            int idStringEnd  =line.IndexOf(" , ",idStringStart);
            ulong id=ulong.Parse(line.Substring(idStringStart,idStringEnd-idStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            if(simActorSpawnAtIndex.TryGetValue(id,out int index)){
             //Log.DebugMessage("sim actor data has to be loaded for id:"+id);
             SimActor.PersistentSimActorData persistentSimActorData;
            }
           }
          }
         }
         foreach(var specificIdToLoad in container.specificIdsToLoad){
          (Type simType,ulong number)id=specificIdToLoad.Key;
          var specificIdData=specificIdToLoad.Value;
          container.spawnDataFromFiles.at.Add((specificIdData.position,specificIdData.eulerAngles,specificIdData.localScale,id.simType,id.number,new SimObject.PersistentData()));
         }
         container.specificIdsToLoad.Clear();
        }
    }
}