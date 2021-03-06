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
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal class PersistentDataSavingBackgroundContainer:BackgroundContainer{
     internal readonly Dictionary<Type,Dictionary<ulong,SimObject.PersistentData>>gameDataToSerializeToFile=new Dictionary<Type,Dictionary<ulong,SimObject.PersistentData>>();
     internal readonly Dictionary<Type,List<ulong>>persistentReleasedIds=new Dictionary<Type,List<ulong>>();
     internal readonly Dictionary<Type,List<ulong>>idsToRelease=new Dictionary<Type,List<ulong>>();
     internal readonly Dictionary<Type,ulong>persistentIds=new Dictionary<Type,ulong>();
    }
    internal class PersistentDataSavingMultithreaded:BaseMultithreaded<PersistentDataSavingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>fileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamWriter>fileStreamWriter=new Dictionary<Type,StreamWriter>();
      internal readonly Dictionary<Type,StreamReader>fileStreamReader=new Dictionary<Type,StreamReader>();
       readonly Dictionary<Type,Dictionary<int,List<(ulong id,SimObject.PersistentData persistentData)>>>idPersistentDataListBycnkIdxByType=new Dictionary<Type,Dictionary<int,List<(ulong,SimObject.PersistentData)>>>();
        readonly Queue<List<(ulong id,SimObject.PersistentData persistentData)>>idPersistentDataListPool=new Queue<List<(ulong,SimObject.PersistentData)>>();
       readonly Dictionary<Type,List<ulong>>idListByType=new Dictionary<Type,List<ulong>>();
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
         foreach(var kvp1 in idPersistentDataListBycnkIdxByType){
          var idPersistentDataListBycnkIdx=kvp1.Value;
          foreach(var kvp2 in idPersistentDataListBycnkIdx){
           var idPersistentDataList=kvp2.Value;
           idPersistentDataList.Clear();
           idPersistentDataListPool.Enqueue(idPersistentDataList);
          }
          idPersistentDataListBycnkIdx.Clear();
         }
         foreach(var kvp in idListByType){
          var idList=kvp.Value;
          idList.Clear();
         }
        }
        protected override void Execute(){
         foreach(var typePersistentDataToSavePair in container.gameDataToSerializeToFile){
          Type t=typePersistentDataToSavePair.Key;
          if(!idListByType.ContainsKey(t)){
           idListByType.Add(t,new List<ulong>());
          }
          if(!idPersistentDataListBycnkIdxByType.ContainsKey(t)){
           idPersistentDataListBycnkIdxByType.Add(t,new Dictionary<int,List<(ulong,SimObject.PersistentData)>>());
          }
          var persistentDataToSave=typePersistentDataToSavePair.Value;
          foreach(var idPersistentDataPair in persistentDataToSave){
           ulong id=idPersistentDataPair.Key;
           idListByType[t].Add(id);
           SimObject.PersistentData persistentData=idPersistentDataPair.Value;
           Vector2Int cCoord=vecPosTocCoord(persistentData.position);
           int cnkIdx=GetcnkIdx(cCoord.x,cCoord.y);
           if(!idPersistentDataListBycnkIdxByType[t].ContainsKey(cnkIdx)){
            if(idPersistentDataListPool.Count>0){
             idPersistentDataListBycnkIdxByType[t].Add(cnkIdx,idPersistentDataListPool.Dequeue());
            }else{
             idPersistentDataListBycnkIdxByType[t].Add(cnkIdx,new List<(ulong id,SimObject.PersistentData persistentData)>());
            }
           }
           idPersistentDataListBycnkIdxByType[t][cnkIdx].Add((id,persistentData));
           Log.DebugMessage("simObject of type:"+t+" and id:"+id+" needs to be saved");
          }
          persistentDataToSave.Clear();
         }
         foreach(var kvp1 in idPersistentDataListBycnkIdxByType){
          Type t=kvp1.Key;
          var idPersistentDataListBycnkIdx=kvp1.Value;
          processedcnkIdx.Clear();
          FileStream fileStream=this.fileStream[t];
          StreamWriter fileStreamWriter=this.fileStreamWriter[t];
          StreamReader fileStreamReader=this.fileStreamReader[t];
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
           int cnkIdxStringEnd  =line.IndexOf(" ,",cnkIdxStringStart);
           int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
           int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
           processedcnkIdx.Add(cnkIdx);
           Log.DebugMessage("process save file of "+t+" at cnkIdx:"+cnkIdx);
           int simObjectStringStart=cnkIdxStringEnd+2;
           int endOfLineStart=simObjectStringStart;
           while((simObjectStringStart=line.IndexOf("simObject=",simObjectStringStart))>=0){
            int simObjectStringEnd=line.IndexOf("}, ",simObjectStringStart)+3;
            string simObjectString=line.Substring(simObjectStringStart,simObjectStringEnd-simObjectStringStart);
            Log.DebugMessage("simObjectString:"+simObjectString);
            int idStringStart=simObjectString.IndexOf("id=")+3;
            int idStringEnd  =simObjectString.IndexOf(", ",idStringStart);
            ulong id=ulong.Parse(simObjectString.Substring(idStringStart,idStringEnd-idStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
            Log.DebugMessage("id:"+id);
            if(idListByType[t].Contains(id)||container.idsToRelease[t].Contains(id)){
             int toRemoveLength=simObjectStringEnd-totalCharactersRemoved-(simObjectStringStart-totalCharactersRemoved);
             lineStringBuilder.Remove(simObjectStringStart-totalCharactersRemoved,toRemoveLength);
             totalCharactersRemoved+=toRemoveLength;
            }
            simObjectStringStart=simObjectStringEnd;
            endOfLineStart=simObjectStringStart;
           }
           endOfLineStart  =line.IndexOf("} }, ",endOfLineStart);
           int endOfLineEnd=line.IndexOf(", ",endOfLineStart)+2;
           lineStringBuilder.Remove(endOfLineStart-totalCharactersRemoved,endOfLineEnd-totalCharactersRemoved-(endOfLineStart-totalCharactersRemoved));
           line=lineStringBuilder.ToString();
           stringBuilder.Append(line);
           if(idPersistentDataListBycnkIdx.ContainsKey(cnkIdx)){
            foreach(var idPersistentData in idPersistentDataListBycnkIdx[cnkIdx]){
             ulong id=idPersistentData.id;
             SimObject.PersistentData persistentData=idPersistentData.persistentData;
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"simObject={{ id={0}, {1} }}, ",id,persistentData.ToString());
            }
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }}, {0}",Environment.NewLine);
          }
          foreach(var kvp2 in idPersistentDataListBycnkIdx){
           int cnkIdx=kvp2.Key;
           var idPersistentDataList=kvp2.Value;
           if(processedcnkIdx.Contains(cnkIdx)){continue;}
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ cnkIdx={0} , {{ ",cnkIdx);
           foreach(var idPersistentData in idPersistentDataList){
            ulong id=idPersistentData.id;
            SimObject.PersistentData persistentData=idPersistentData.persistentData;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"simObject={{ id={0}, {1} }}, ",id,persistentData.ToString());
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }}, {0}",Environment.NewLine);
          }
          fileStream.SetLength(0L);
          fileStreamWriter.Write(stringBuilder.ToString());
          fileStreamWriter.Flush();
         }
         #region releasedIds
          releasedIdsStringBuilder.Clear();
          foreach(var typeIdsToReleasePair in container.idsToRelease){
           Type t=typeIdsToReleasePair.Key;
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0}, {{ ",t);
           var releasedIds=container.persistentReleasedIds[t];
           foreach(ulong releasedId in releasedIds){
            Log.DebugMessage("type:"+t+", id is already released:"+releasedId);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",releasedId);
           }
           releasedIds.Clear();
           var idsToRelease=typeIdsToReleasePair.Value;
           foreach(ulong idToRelease in idsToRelease){
            Log.DebugMessage("type:"+t+", release id:"+idToRelease);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",idToRelease);
           }
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US," }}, }}, {0}",Environment.NewLine);
           idsToRelease.Clear();
          }
          releasedIdsFileStream.SetLength(0L);
          releasedIdsFileStreamWriter.Write(releasedIdsStringBuilder.ToString());
          releasedIdsFileStreamWriter.Flush();
         #endregion
         idsStringBuilder.Clear();
         foreach(var typeIdsPair in container.persistentIds){
          Type t=typeIdsPair.Key;
          ulong nextId=typeIdsPair.Value;
          if(nextId>0){
           idsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0}, nextId={1} }}, {2}",t,nextId,Environment.NewLine);
          }
         }
         idsFileStream.SetLength(0L);
         idsFileStreamWriter.Write(idsStringBuilder.ToString());
         idsFileStreamWriter.Flush();
        }
    }
}