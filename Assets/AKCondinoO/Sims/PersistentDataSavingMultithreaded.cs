#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal class PersistentDataSavingBackgroundContainer:BackgroundContainer{
     #region input
         internal readonly AutoResetEvent waitingForSimInventoryReleasedSimObjectsIdsToRelease=new AutoResetEvent(false);
          internal Dictionary<Type,List<ulong>>simInventoryReleasedSimObjectsIdsToRelease;
         internal readonly Dictionary<Type,Dictionary<ulong,SimObject.PersistentData>>simObjectDataToSerializeToFile=new Dictionary<Type,Dictionary<ulong,SimObject.PersistentData>>();
          internal readonly Dictionary<Type,Dictionary<ulong,SimActor.PersistentSimActorData>>simActorDataToSerializeToFile=new Dictionary<Type,Dictionary<ulong,SimActor.PersistentSimActorData>>();
         internal readonly Dictionary<Type,List<ulong>>persistentReleasedIds=new Dictionary<Type,List<ulong>>();
          internal readonly Dictionary<Type,List<ulong>>idsToRelease=new Dictionary<Type,List<ulong>>();
         internal readonly Dictionary<Type,ulong>persistentIds=new Dictionary<Type,ulong>();
     #endregion
     #region output
         internal readonly Dictionary<Type,List<ulong>>onSavedReleasedIds=new Dictionary<Type,List<ulong>>();
     #endregion
    }
    internal class PersistentDataSavingMultithreaded:BaseMultithreaded<PersistentDataSavingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simObjectFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamWriter>simObjectFileStreamWriter=new Dictionary<Type,StreamWriter>();
      internal readonly Dictionary<Type,StreamReader>simObjectFileStreamReader=new Dictionary<Type,StreamReader>();
       internal readonly Dictionary<Type,FileStream>simActorFileStream=new Dictionary<Type,FileStream>();
        internal readonly Dictionary<Type,StreamWriter>simActorFileStreamWriter=new Dictionary<Type,StreamWriter>();
        internal readonly Dictionary<Type,StreamReader>simActorFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<Type,Dictionary<int,List<(ulong simObjectIdNumber,SimObject.PersistentData persistentData)>>>simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType=new Dictionary<Type,Dictionary<int,List<(ulong,SimObject.PersistentData)>>>();
        readonly Queue<List<(ulong simObjectIdNumber,SimObject.PersistentData persistentData)>>simObjectIdNumberPersistentDataListPool=new Queue<List<(ulong,SimObject.PersistentData)>>();
       readonly Dictionary<Type,List<ulong>>simObjectIdNumberListByType=new Dictionary<Type,List<ulong>>();
       readonly List<int>processedcnkIdx=new List<int>();
       readonly StringBuilder stringBuilder=new StringBuilder();
        readonly StringBuilder lineStringBuilder=new StringBuilder();
     internal FileStream releasedIdsFileStream;
      internal StreamWriter releasedIdsFileStreamWriter;
      internal StreamReader releasedIdsFileStreamReader;
       readonly StringBuilder releasedIdsStringBuilder=new StringBuilder();
     internal FileStream idsFileStream;
      internal StreamWriter idsFileStreamWriter;
      internal StreamReader idsFileStreamReader;
       readonly StringBuilder idsStringBuilder=new StringBuilder();
        protected override void Cleanup(){
         foreach(var kvp1 in simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType){
          var simObjectIdNumberPersistentDataListBycnkIdx=kvp1.Value;
          foreach(var kvp2 in simObjectIdNumberPersistentDataListBycnkIdx){
           var simObjectIdNumberPersistentDataList=kvp2.Value;
           simObjectIdNumberPersistentDataList.Clear();
           simObjectIdNumberPersistentDataListPool.Enqueue(simObjectIdNumberPersistentDataList);
          }
          simObjectIdNumberPersistentDataListBycnkIdx.Clear();
         }
         foreach(var kvp in simObjectIdNumberListByType){
          var idList=kvp.Value;
          idList.Clear();
         }
        }
        protected override void Execute(){
         //  TO DO: onSavedReleasedIds lists Clear;
         foreach(var simObjectTypeIdNumberListPair in container.onSavedReleasedIds){
          Type simObjectType=simObjectTypeIdNumberListPair.Key;
          List<ulong>onSavedReleasedIds=simObjectTypeIdNumberListPair.Value;
          onSavedReleasedIds.Clear();
         }
         foreach(var simObjectTypePersistentDataToSavePair in container.simObjectDataToSerializeToFile){
          Type simObjectType=simObjectTypePersistentDataToSavePair.Key;
          if(!simObjectIdNumberListByType.ContainsKey(simObjectType)){
           simObjectIdNumberListByType.Add(simObjectType,new List<ulong>());
          }
          if(!simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType.ContainsKey(simObjectType)){
           simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType.Add(simObjectType,new Dictionary<int,List<(ulong,SimObject.PersistentData)>>());
          }
          var persistentDataToSave=simObjectTypePersistentDataToSavePair.Value;
          foreach(var simObjectIdNumberPersistentDataPair in persistentDataToSave){
           ulong simObjectIdNumber=simObjectIdNumberPersistentDataPair.Key;
           simObjectIdNumberListByType[simObjectType].Add(simObjectIdNumber);
           SimObject.PersistentData persistentData=simObjectIdNumberPersistentDataPair.Value;
           Vector2Int cCoord=vecPosTocCoord(persistentData.position);
           int cnkIdx=GetcnkIdx(cCoord.x,cCoord.y);
           if(!simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType[simObjectType].ContainsKey(cnkIdx)){
            if(simObjectIdNumberPersistentDataListPool.Count>0){
             simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType[simObjectType].Add(cnkIdx,simObjectIdNumberPersistentDataListPool.Dequeue());
            }else{
             simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType[simObjectType].Add(cnkIdx,new List<(ulong simObjectIdNumber,SimObject.PersistentData persistentData)>());
            }
           }
           simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType[simObjectType][cnkIdx].Add((simObjectIdNumber,persistentData));
           //Log.DebugMessage("simObject of type:"+t+" and id:"+id+" needs to be saved");
          }
          persistentDataToSave.Clear();
         }
         container.waitingForSimInventoryReleasedSimObjectsIdsToRelease.WaitOne();
         foreach(var simObjectTypeIdNumberListPair in container.simInventoryReleasedSimObjectsIdsToRelease){
          Type simObjectType=simObjectTypeIdNumberListPair.Key;
          List<ulong>simObjectIdNumberList=simObjectTypeIdNumberListPair.Value;
          //  TO DO: add ids to be released and to be removed from the game on saved
          var releasedIds=container.persistentReleasedIds[simObjectType];
          var idsToRelease=container.idsToRelease[simObjectType];
          foreach(ulong simObjectIdNumber in simObjectIdNumberList){
           if(!releasedIds.Contains(simObjectIdNumber)&&!idsToRelease.Contains(simObjectIdNumber)){
            Log.DebugMessage("releasing sim object during save:"+simObjectType+";id:"+simObjectIdNumber);
            idsToRelease.Add(simObjectIdNumber);//  remove all save data and add to released ids file
            container.onSavedReleasedIds[simObjectType].Add(simObjectIdNumber);
           }
          }
          simObjectIdNumberList.Clear();
         }
         foreach(var kvp1 in simObjectIdNumberPersistentDataListBycnkIdxBySimObjectType){
          Type simObjectType=kvp1.Key;
          var simObjectIdNumberPersistentDataListBycnkIdx=kvp1.Value;
          processedcnkIdx.Clear();
          if(!simObjectFileStream.ContainsKey(simObjectType)){
           continue;
          }
          FileStream fileStream=this.simObjectFileStream[simObjectType];
          StreamWriter fileStreamWriter=this.simObjectFileStreamWriter[simObjectType];
          StreamReader fileStreamReader=this.simObjectFileStreamReader[simObjectType];
          stringBuilder.Clear();
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int totalCharactersRemoved=0;
           lineStringBuilder.Clear();
           lineStringBuilder.Append(line);
           int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
           int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
           int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
           int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
           processedcnkIdx.Add(cnkIdx);
           //Log.DebugMessage("process save file of "+t+" at cnkIdx:"+cnkIdx);
           int simObjectStringStart=cnkIdxStringEnd+3;
           int endOfLineStart=simObjectStringStart;
           while((simObjectStringStart=line.IndexOf("simObject=",simObjectStringStart))>=0){
            int simObjectStringEnd=line.IndexOf("} , ",simObjectStringStart)+4;
            string simObjectString=line.Substring(simObjectStringStart,simObjectStringEnd-simObjectStringStart);
            //Log.DebugMessage("simObjectString:"+simObjectString);
            int simObjectIdNumberStringStart=simObjectString.IndexOf("id=")+3;
            int simObjectIdNumberStringEnd  =simObjectString.IndexOf(" , ",simObjectIdNumberStringStart);
            ulong simObjectIdNumber=ulong.Parse(simObjectString.Substring(simObjectIdNumberStringStart,simObjectIdNumberStringEnd-simObjectIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            //Log.DebugMessage("id:"+id);
            if(simObjectIdNumberListByType[simObjectType].Contains(simObjectIdNumber)||container.idsToRelease[simObjectType].Contains(simObjectIdNumber)){
             int toRemoveLength=simObjectStringEnd-totalCharactersRemoved-(simObjectStringStart-totalCharactersRemoved);
             lineStringBuilder.Remove(simObjectStringStart-totalCharactersRemoved,toRemoveLength);
             totalCharactersRemoved+=toRemoveLength;
            }
            simObjectStringStart=simObjectStringEnd;
            endOfLineStart=simObjectStringStart;
           }
           endOfLineStart  =line.IndexOf("} } , endOfLine",endOfLineStart);
           int endOfLineEnd=line.IndexOf(" , endOfLine",endOfLineStart)+12;
           lineStringBuilder.Remove(endOfLineStart-totalCharactersRemoved,endOfLineEnd-totalCharactersRemoved-(endOfLineStart-totalCharactersRemoved));
           line=lineStringBuilder.ToString();
           stringBuilder.Append(line);
           if(simObjectIdNumberPersistentDataListBycnkIdx.ContainsKey(cnkIdx)){
            foreach(var simObjectIdNumberPersistentData in simObjectIdNumberPersistentDataListBycnkIdx[cnkIdx]){
             ulong simObjectIdNumber=simObjectIdNumberPersistentData.simObjectIdNumber;
             SimObject.PersistentData persistentData=simObjectIdNumberPersistentData.persistentData;
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"simObject={{ id={0} , {1} }} , ",simObjectIdNumber,persistentData.ToString());
            }
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
          }
          foreach(var kvp2 in simObjectIdNumberPersistentDataListBycnkIdx){
           int cnkIdx=kvp2.Key;
           var simObjectIdNumberPersistentDataList=kvp2.Value;
           if(processedcnkIdx.Contains(cnkIdx)){continue;}
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ cnkIdx={0} , {{ ",cnkIdx);
           foreach(var simObjectIdNumberPersistentData in simObjectIdNumberPersistentDataList){
            ulong simObjectIdNumber=simObjectIdNumberPersistentData.simObjectIdNumber;
            SimObject.PersistentData persistentData=simObjectIdNumberPersistentData.persistentData;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"simObject={{ id={0} , {1} }} , ",simObjectIdNumber,persistentData.ToString());
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
          }
          fileStream.SetLength(0L);
          fileStreamWriter.Write(stringBuilder.ToString());
          fileStreamWriter.Flush();
         }
         foreach(var simActorTypePersistentSimActorDataToSavePair in container.simActorDataToSerializeToFile){
          Type simActorType=simActorTypePersistentSimActorDataToSavePair.Key;
          var persistentSimActorDataToSave=simActorTypePersistentSimActorDataToSavePair.Value;
          Log.DebugMessage("persistentSimActorDataToSave.Count:"+persistentSimActorDataToSave.Count);
          if(!simActorFileStream.ContainsKey(simActorType)){
           goto _Skip;
          }
          FileStream fileStream=this.simActorFileStream[simActorType];
          StreamWriter fileStreamWriter=this.simActorFileStreamWriter[simActorType];
          StreamReader fileStreamReader=this.simActorFileStreamReader[simActorType];
          stringBuilder.Clear();
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int simActorIdNumberStringStart=line.IndexOf("id=")+3;
           int simActorIdNumberStringEnd  =line.IndexOf(" , ",simActorIdNumberStringStart);
           ulong simActorIdNumber=ulong.Parse(line.Substring(simActorIdNumberStringStart,simActorIdNumberStringEnd-simActorIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
           //Log.DebugMessage("id:"+id);
           if(!persistentSimActorDataToSave.ContainsKey(simActorIdNumber)){
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0}{1}",line,Environment.NewLine);
           }
          }
          foreach(var simActorIdNumberPersistentSimActorDataPair in persistentSimActorDataToSave){
           ulong simActorIdNumber=simActorIdNumberPersistentSimActorDataPair.Key;
           SimActor.PersistentSimActorData persistentSimActorData=simActorIdNumberPersistentSimActorDataPair.Value;
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ id={0} , {{ {1} }} }} , endOfLine{2}",simActorIdNumber,persistentSimActorData.ToString(),Environment.NewLine);
          }
          fileStream.SetLength(0L);
          fileStreamWriter.Write(stringBuilder.ToString());
          fileStreamWriter.Flush();
          _Skip:{}
          persistentSimActorDataToSave.Clear();
         }
         #region releasedIds
         if(releasedIdsFileStream!=null){
          releasedIdsStringBuilder.Clear();
          foreach(var simObjectTypeIdNumbersToReleasePair in container.idsToRelease){
           Type simObjectType=simObjectTypeIdNumbersToReleasePair.Key;
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , {{ ",simObjectType);
           var releasedIds=container.persistentReleasedIds[simObjectType];
           foreach(ulong releasedId in releasedIds){
            //Log.DebugMessage("type:"+t+", id is already released:"+releasedId);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",releasedId);
           }
           releasedIds.Clear();
           var idsToRelease=simObjectTypeIdNumbersToReleasePair.Value;
           foreach(ulong idToRelease in idsToRelease){
            //Log.DebugMessage("type:"+t+", release id:"+idToRelease);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",idToRelease);
           }
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US," }} , }} , endOfLine{0}",Environment.NewLine);
           idsToRelease.Clear();
          }
          releasedIdsFileStream.SetLength(0L);
          releasedIdsFileStreamWriter.Write(releasedIdsStringBuilder.ToString());
          releasedIdsFileStreamWriter.Flush();
         }
         #endregion
         if(idsFileStream!=null){
          idsStringBuilder.Clear();
          foreach(var simObjectTypeNextIdNumberPair in container.persistentIds){
           Type simObjectType=simObjectTypeNextIdNumberPair.Key;
           ulong nextIdNumber=simObjectTypeNextIdNumberPair.Value;
           if(nextIdNumber>0){
            idsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , nextId={1} }} , endOfLine{2}",simObjectType,nextIdNumber,Environment.NewLine);
           }
          }
          idsFileStream.SetLength(0L);
          idsFileStreamWriter.Write(idsStringBuilder.ToString());
          idsFileStreamWriter.Flush();
         }
        }
    }
}