#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataSavingBackgroundContainer:BackgroundContainer{
     #region input
         internal readonly Dictionary<Type,Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>>simInventoryDataToSerializeToFile=new Dictionary<Type,Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>>();
          internal static readonly ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>simInventoryDataTypeDictionaryPool=new ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>();
           internal static readonly ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>simInventoryDataIdDictionaryPool=new ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>();
         internal readonly Dictionary<Type,List<ulong>>persistentReleasedIds=new Dictionary<Type,List<ulong>>();
          internal readonly Dictionary<Type,List<ulong>>idsToRelease=new Dictionary<Type,List<ulong>>();
         internal readonly Dictionary<Type,ulong>persistentIds=new Dictionary<Type,ulong>();
     #endregion
     #region output
         internal AutoResetEvent waitingForSimInventoryReleasedSimObjectsIdsToRelease;
          internal readonly Dictionary<Type,List<ulong>>simInventoryReleasedSimObjectsIdsToRelease=new Dictionary<Type,List<ulong>>();
         internal readonly Dictionary<Type,List<ulong>>onSavedReleasedIds=new Dictionary<Type,List<ulong>>();
     #endregion
    }
    internal class PersistentSimInventoryDataSavingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataSavingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simInventoryFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamWriter>simInventoryFileStreamWriter=new Dictionary<Type,StreamWriter>();
      internal readonly Dictionary<Type,StreamReader>simInventoryFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly StringBuilder stringBuilder=new StringBuilder();
        readonly StringBuilder lineStringBuilder=new StringBuilder();
         readonly List<KeyValuePair<Type,string>>releasedInventoryStrings=new List<KeyValuePair<Type,string>>();
          readonly Dictionary<Type,string>inventoryStringsToReleaseSimObjects=new Dictionary<Type,string>();
           readonly Dictionary<Type,List<ulong>>releasedSimObjects=new Dictionary<Type,List<ulong>>();
     internal FileStream releasedIdsFileStream;
      internal StreamWriter releasedIdsFileStreamWriter;
      internal StreamReader releasedIdsFileStreamReader;
       readonly StringBuilder releasedIdsStringBuilder=new StringBuilder();
     internal FileStream idsFileStream;
      internal StreamWriter idsFileStreamWriter;
      internal StreamReader idsFileStreamReader;
       readonly StringBuilder idsStringBuilder=new StringBuilder();
        protected override void Cleanup(){
        }
        protected override void Execute(){
         Log.DebugMessage("Execute()");
         _Loop:{}
         foreach(var typePersistentSimInventoryDataToSavePair in container.simInventoryDataToSerializeToFile){
          Type t=typePersistentSimInventoryDataToSavePair.Key;
          var persistentSimInventoryDataToSave=typePersistentSimInventoryDataToSavePair.Value;
          if(!this.releasedSimObjects.TryGetValue(t,out List<ulong>releasedSimObjectIds)){
           this.releasedSimObjects.Add(t,releasedSimObjectIds=new List<ulong>());
          }
          Log.DebugMessage("persistentSimInventoryDataToSave.Count:"+persistentSimInventoryDataToSave.Count);
          if(!simInventoryFileStream.ContainsKey(t)){
           goto _Skip;
          }
          FileStream fileStream=this.simInventoryFileStream[t];
          StreamWriter fileStreamWriter=this.simInventoryFileStreamWriter[t];
          StreamReader fileStreamReader=this.simInventoryFileStreamReader[t];
          stringBuilder.Clear();
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int idStringStart=line.IndexOf("id=")+3;
           int idStringEnd  =line.IndexOf(" , ",idStringStart);
           ulong id=ulong.Parse(line.Substring(idStringStart,idStringEnd-idStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
           bool releasingSimObject=releasedSimObjectIds.Contains(id);
           //Log.DebugMessage("id:"+id);
           if(!persistentSimInventoryDataToSave.ContainsKey(id)){
            bool noInventory=true;
            int totalCharactersRemoved=0;
            lineStringBuilder.Clear();
            lineStringBuilder.Append(line);
            int simInventoryListStringStart=idStringEnd+3;
            while((simInventoryListStringStart=line.IndexOf("[ [ simInventoryType=",simInventoryListStringStart))>=0){
             int simInventoryListStringEnd=line.IndexOf("} ] ] , ",simInventoryListStringStart)+8;
             string simInventoryListString=line.Substring(simInventoryListStringStart,simInventoryListStringEnd-simInventoryListStringStart);
             int simInventoryTypeStringStart=simInventoryListString.IndexOf("simInventoryType=")+17;
             int simInventoryTypeStringEnd=simInventoryListString.IndexOf(" , ",simInventoryTypeStringStart);
             string simInventoryTypeString=simInventoryListString.Substring(simInventoryTypeStringStart,simInventoryTypeStringEnd-simInventoryTypeStringStart);
             Type simInventoryType=Type.GetType(simInventoryTypeString);
             if(simInventoryType!=null){
              bool toRelease;
              if(!(toRelease=container.idsToRelease.TryGetValue(simInventoryType,out List<ulong>idsToReleaseIdNumberList))){
               container.idsToRelease.Add(simInventoryType,idsToReleaseIdNumberList=new List<ulong>());
              }
              if(toRelease||releasingSimObject){
               ReleaseInventory();
              }
              void ReleaseInventory(){
               int simInventoryStringStart=simInventoryTypeStringEnd+3;
               while((simInventoryStringStart=simInventoryListString.IndexOf("[ simInventoryIdNumber=",simInventoryStringStart))>=0){
                int simInventoryStringEnd=simInventoryListString.IndexOf("} ] , ",simInventoryStringStart)+6;
                string simInventoryString=simInventoryListString.Substring(simInventoryStringStart,simInventoryStringEnd-simInventoryStringStart);
                int simInventoryIdStart=simInventoryString.IndexOf("simInventoryIdNumber=")+21;
                int simInventoryIdEnd=simInventoryString.IndexOf(" , ",simInventoryIdStart);
                string simInventoryIdString=simInventoryString.Substring(simInventoryIdStart,simInventoryIdEnd-simInventoryIdStart);
                ulong simInventoryId=ulong.Parse(simInventoryIdString,NumberStyles.Any,CultureInfoUtil.en_US);
                bool releaseId;
                if((releaseId=releasingSimObject)|!(releaseId=!idsToReleaseIdNumberList.Contains(simInventoryId))){
                 releasedInventoryStrings.Add(new KeyValuePair<Type,string>(simInventoryType,simInventoryString));
                 int toRemoveLength=(simInventoryListStringStart+simInventoryStringEnd)-totalCharactersRemoved-((simInventoryListStringStart+simInventoryStringStart)-totalCharactersRemoved);
                 lineStringBuilder.Remove((simInventoryListStringStart+simInventoryStringStart)-totalCharactersRemoved,toRemoveLength);
                 totalCharactersRemoved+=toRemoveLength;
                 if(releaseId){
                  idsToReleaseIdNumberList.Add(simInventoryId);
                 }
                }else{
                 noInventory=false;
                }
                simInventoryStringStart=simInventoryStringEnd;
               }
              }
             }
             simInventoryListStringStart=simInventoryListStringEnd;
            }
            if(!releasingSimObject&&!noInventory){
             line=lineStringBuilder.ToString();
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0}{1}",line,Environment.NewLine);
            }
           }
          }
          foreach(var idPersistentSimInventoryDataPair in persistentSimInventoryDataToSave){
           ulong id=idPersistentSimInventoryDataPair.Key;
           Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>typeDictionary=idPersistentSimInventoryDataPair.Value;
           if(typeDictionary.Count<=0){
            continue;
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ id={0} , {{ ",id);
           foreach(var simInventoryTypeDictionaryPair in typeDictionary){
            Type simInventoryType=simInventoryTypeDictionaryPair.Key;
            container.idsToRelease.TryGetValue(simInventoryType,out List<ulong>idsToReleaseIdNumberList);
            Dictionary<ulong,SimInventory.PersistentSimInventoryData>idDictionary=simInventoryTypeDictionaryPair.Value;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[ [ simInventoryType={0} , {{ ",simInventoryType);
            foreach(var simInventoryIdDataPair in idDictionary){
             ulong simInventoryId=simInventoryIdDataPair.Key;
             SimInventory.PersistentSimInventoryData persistentSimInventoryData=simInventoryIdDataPair.Value;
             string simInventoryString=string.Format(CultureInfoUtil.en_US,"[ simInventoryIdNumber={0} , {{ {1} }} ] , ",simInventoryId,persistentSimInventoryData.ToString());
             if(idsToReleaseIdNumberList!=null&&idsToReleaseIdNumberList.Contains(simInventoryId)){
              releasedInventoryStrings.Add(new KeyValuePair<Type,string>(simInventoryType,simInventoryString));
              continue;
             }
             stringBuilder.Append(simInventoryString);
            }
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} ] ] , ");
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
          }
          fileStream.SetLength(0L);
          fileStreamWriter.Write(stringBuilder.ToString());
          fileStreamWriter.Flush();
          _Skip:{}
          releasedSimObjectIds.Clear();
          foreach(var idInventoryPair in persistentSimInventoryDataToSave){
           Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>typeDictionary=idInventoryPair.Value;
           foreach(var simInventoryTypeDictionaryPair in typeDictionary){
            Dictionary<ulong,SimInventory.PersistentSimInventoryData>idDictionary=simInventoryTypeDictionaryPair.Value;
            idDictionary.Clear();
            PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataIdDictionaryPool.Enqueue(idDictionary);
           }
           typeDictionary.Clear();
           PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataTypeDictionaryPool.Enqueue(typeDictionary);
          }
          persistentSimInventoryDataToSave.Clear();
         }
         bool loop=false;
         releasedInventoryStrings.ForEach(kvp=>inventoryStringsToReleaseSimObjects.Add(kvp.Key,kvp.Value));
         releasedInventoryStrings.Clear();
         foreach(var releasedInventoryString in inventoryStringsToReleaseSimObjects){
          SimInventory.PersistentSimInventoryData persistentSimInventoryData=SimInventory.PersistentSimInventoryData.Parse(releasedInventoryString.Value);
          persistentSimInventoryData.inventoryItems.Reset();
          while(persistentSimInventoryData.inventoryItems.MoveNext()){
           SimInventory.PersistentSimInventoryData.SimInventoryItemData inventoryItem=persistentSimInventoryData.inventoryItems.Current;
           container.simInventoryReleasedSimObjectsIdsToRelease[inventoryItem.simType].Add(inventoryItem.number);
           Type t=inventoryItem.simType;
           ulong idNumber=inventoryItem.number;
           if(!this.releasedSimObjects.TryGetValue(t,out List<ulong>releasedSimObjectIds)){
            this.releasedSimObjects.Add(t,releasedSimObjectIds=new List<ulong>());
           }
           releasedSimObjectIds.Add(idNumber);
           loop=true;
          }
         }
         inventoryStringsToReleaseSimObjects.Clear();
         if(loop){
          goto _Loop;
         }
         container.waitingForSimInventoryReleasedSimObjectsIdsToRelease.Set();
         #region releasedIds
         if(releasedIdsFileStream!=null){
          releasedIdsStringBuilder.Clear();
          foreach(var typeIdsToReleasePair in container.idsToRelease){
           Type t=typeIdsToReleasePair.Key;
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , {{ ",t);
           var releasedIds=container.persistentReleasedIds[t];
           foreach(ulong releasedId in releasedIds){
            //Log.DebugMessage("type:"+t+", id is already released:"+releasedId);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",releasedId);
           }
           releasedIds.Clear();
           var idsToRelease=typeIdsToReleasePair.Value;
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
          foreach(var typeIdsPair in container.persistentIds){
           Type t=typeIdsPair.Key;
           ulong nextId=typeIdsPair.Value;
           if(nextId>0){
            Log.DebugMessage("t:"+t+";nextId:"+nextId);
            idsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , nextId={1} }} , endOfLine{2}",t,nextId,Environment.NewLine);
           }
          }
          idsFileStream.SetLength(0L);
          idsFileStreamWriter.Write(idsStringBuilder.ToString());
          idsFileStreamWriter.Flush();
         }
        }
    }
}