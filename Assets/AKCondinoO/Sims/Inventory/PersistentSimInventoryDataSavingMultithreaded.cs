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
          internal static readonly ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>simInventoryDataBySimInventoryTypeDictionaryPool=new ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>();
           internal static readonly ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>simInventoryDataByIdNumberDictionaryPool=new ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>();
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
           internal readonly Dictionary<Type,List<ulong>>releasedSimObjects=new Dictionary<Type,List<ulong>>();
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
         //  TO DO: onSavedReleasedIds lists Clear;
         foreach(var simInventoryTypeIdNumberListPair in container.onSavedReleasedIds){
          Type simInventoryType=simInventoryTypeIdNumberListPair.Key;
          List<ulong>onSavedReleasedIds=simInventoryTypeIdNumberListPair.Value;
          onSavedReleasedIds.Clear();
         }
         _Loop:{}
         foreach(var simInventoryDataBySimObjectTypeDictionary in container.simInventoryDataToSerializeToFile){
          Type simObjectType=simInventoryDataBySimObjectTypeDictionary.Key;
          var persistentSimInventoryDataToSave=simInventoryDataBySimObjectTypeDictionary.Value;
          Log.DebugMessage("persistentSimInventoryDataToSave.Count:"+persistentSimInventoryDataToSave.Count);
          if(!simInventoryFileStream.ContainsKey(simObjectType)){
           goto _Skip;
          }
          FileStream fileStream=this.simInventoryFileStream[simObjectType];
          StreamWriter fileStreamWriter=this.simInventoryFileStreamWriter[simObjectType];
          StreamReader fileStreamReader=this.simInventoryFileStreamReader[simObjectType];
          stringBuilder.Clear();
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int simObjectIdNumberStringStart=line.IndexOf("id=")+3;
           int simObjectIdNumberStringEnd  =line.IndexOf(" , ",simObjectIdNumberStringStart);
           ulong simObjectIdNumber=ulong.Parse(line.Substring(simObjectIdNumberStringStart,simObjectIdNumberStringEnd-simObjectIdNumberStringStart),NumberStyles.Any,CultureInfoUtil.en_US);
           bool releasingSimObject=releasedSimObjects[simObjectType].Contains(simObjectIdNumber);
           //Log.DebugMessage("id:"+id);
           if(!persistentSimInventoryDataToSave.ContainsKey(simObjectIdNumber)){
            bool noInventory=true;
            int totalCharactersRemoved=0;
            lineStringBuilder.Clear();
            lineStringBuilder.Append(line);
            int simInventoryListStringStart=simObjectIdNumberStringEnd+3;
            while((simInventoryListStringStart=line.IndexOf("[ [ simInventoryType=",simInventoryListStringStart))>=0){
             int simInventoryListStringEnd=line.IndexOf("} ] ] , ",simInventoryListStringStart)+8;
             string simInventoryListString=line.Substring(simInventoryListStringStart,simInventoryListStringEnd-simInventoryListStringStart);
             int simInventoryTypeStringStart=simInventoryListString.IndexOf("simInventoryType=")+17;
             int simInventoryTypeStringEnd=simInventoryListString.IndexOf(" , ",simInventoryTypeStringStart);
             string simInventoryTypeString=simInventoryListString.Substring(simInventoryTypeStringStart,simInventoryTypeStringEnd-simInventoryTypeStringStart);
             Type simInventoryType=Type.GetType(simInventoryTypeString);
             if(simInventoryType!=null){
              var idsToRelease=container.idsToRelease[simInventoryType];
              bool toRelease=idsToRelease.Count>0;
              if(toRelease||releasingSimObject){
               ReleaseInventory();
              }
              void ReleaseInventory(){
               int simInventoryStringStart=simInventoryTypeStringEnd+3;
               while((simInventoryStringStart=simInventoryListString.IndexOf("[ simInventoryIdNumber=",simInventoryStringStart))>=0){
                int simInventoryStringEnd=simInventoryListString.IndexOf("} ] , ",simInventoryStringStart)+6;
                string simInventoryString=simInventoryListString.Substring(simInventoryStringStart,simInventoryStringEnd-simInventoryStringStart);
                int simInventoryIdNumberStart=simInventoryString.IndexOf("simInventoryIdNumber=")+21;
                int simInventoryIdNumberEnd  =simInventoryString.IndexOf(" , ",simInventoryIdNumberStart);
                string simInventoryIdNumberString=simInventoryString.Substring(simInventoryIdNumberStart,simInventoryIdNumberEnd-simInventoryIdNumberStart);
                ulong simInventoryIdNumber=ulong.Parse(simInventoryIdNumberString,NumberStyles.Any,CultureInfoUtil.en_US);
                bool releaseId;
                if((releaseId=releasingSimObject)|!(releaseId=!idsToRelease.Contains(simInventoryIdNumber))){
                 releasedInventoryStrings.Add(new KeyValuePair<Type,string>(simInventoryType,simInventoryString));
                 int toRemoveLength=(simInventoryListStringStart+simInventoryStringEnd)-totalCharactersRemoved-((simInventoryListStringStart+simInventoryStringStart)-totalCharactersRemoved);
                 lineStringBuilder.Remove((simInventoryListStringStart+simInventoryStringStart)-totalCharactersRemoved,toRemoveLength);
                 totalCharactersRemoved+=toRemoveLength;
                 if(releaseId){
                  Log.DebugMessage("releasing sim inventory during save:"+simInventoryType+";idNumber:"+simInventoryIdNumber);
                  idsToRelease.Add(simInventoryIdNumber);
                  container.onSavedReleasedIds[simInventoryType].Add(simInventoryIdNumber);
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
          foreach(var simObjectIdNumberSimInventoryDataBySimInventoryTypeDictionaryPair in persistentSimInventoryDataToSave){
           ulong simObjectIdNumber=simObjectIdNumberSimInventoryDataBySimInventoryTypeDictionaryPair.Key;
           Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>simInventoryDataBySimInventoryTypeDictionary=simObjectIdNumberSimInventoryDataBySimInventoryTypeDictionaryPair.Value;
           if(simInventoryDataBySimInventoryTypeDictionary.Count<=0){
            continue;
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ id={0} , {{ ",simObjectIdNumber);
           foreach(var simInventoryTypeSimInventoryDataByIdNumberDictionaryPair in simInventoryDataBySimInventoryTypeDictionary){
            Type simInventoryType=simInventoryTypeSimInventoryDataByIdNumberDictionaryPair.Key;
            var idsToRelease=container.idsToRelease[simInventoryType];
            Dictionary<ulong,SimInventory.PersistentSimInventoryData>simInventoryDataByIdNumberDictionary=simInventoryTypeSimInventoryDataByIdNumberDictionaryPair.Value;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[ [ simInventoryType={0} , {{ ",simInventoryType);
            foreach(var simInventoryIdNumberSimInventoryDataPair in simInventoryDataByIdNumberDictionary){
             ulong simInventoryIdNumber=simInventoryIdNumberSimInventoryDataPair.Key;
             SimInventory.PersistentSimInventoryData persistentSimInventoryData=simInventoryIdNumberSimInventoryDataPair.Value;
             string simInventoryString=string.Format(CultureInfoUtil.en_US,"[ simInventoryIdNumber={0} , {{ {1} }} ] , ",simInventoryIdNumber,persistentSimInventoryData.ToString());
             if(idsToRelease.Contains(simInventoryIdNumber)){
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
          releasedSimObjects[simObjectType].Clear();
          foreach(var simObjectIdNumberSimInventoryDataBySimInventoryTypeDictionaryPair in persistentSimInventoryDataToSave){
           Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>simInventoryDataBySimInventoryTypeDictionary=simObjectIdNumberSimInventoryDataBySimInventoryTypeDictionaryPair.Value;
           foreach(var simInventoryTypeSimInventoryDataByIdNumberDictionaryPair in simInventoryDataBySimInventoryTypeDictionary){
            Dictionary<ulong,SimInventory.PersistentSimInventoryData>simInventoryDataByIdNumberDictionary=simInventoryTypeSimInventoryDataByIdNumberDictionaryPair.Value;
            simInventoryDataByIdNumberDictionary.Clear();
            PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataByIdNumberDictionaryPool.Enqueue(simInventoryDataByIdNumberDictionary);
           }
           simInventoryDataBySimInventoryTypeDictionary.Clear();
           PersistentSimInventoryDataSavingBackgroundContainer.simInventoryDataBySimInventoryTypeDictionaryPool.Enqueue(simInventoryDataBySimInventoryTypeDictionary);
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
           SimInventory.PersistentSimInventoryData.SimInventoryItemData simInventoryItem=persistentSimInventoryData.inventoryItems.Current;
           container.simInventoryReleasedSimObjectsIdsToRelease[simInventoryItem.simObjectType].Add(simInventoryItem.idNumber);
           Type simObjectType=simInventoryItem.simObjectType;
           ulong simObjectIdNumber=simInventoryItem.idNumber;
           releasedSimObjects[simObjectType].Add(simObjectIdNumber);
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
          foreach(var simInventoryTypeIdNumbersToReleasePair in container.idsToRelease){
           Type simInventoryType=simInventoryTypeIdNumbersToReleasePair.Key;
           releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , {{ ",simInventoryType);
           var releasedIds=container.persistentReleasedIds[simInventoryType];
           foreach(ulong releasedId in releasedIds){
            //Log.DebugMessage("type:"+t+", id is already released:"+releasedId);
            releasedIdsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0},",releasedId);
           }
           releasedIds.Clear();
           var idsToRelease=simInventoryTypeIdNumbersToReleasePair.Value;
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
          foreach(var simInventoryTypeNextIdNumberPair in container.persistentIds){
           Type simInventoryType=simInventoryTypeNextIdNumberPair.Key;
           ulong nextIdNumber=simInventoryTypeNextIdNumberPair.Value;
           if(nextIdNumber>0){
            Log.DebugMessage("t:"+simInventoryType+";nextId:"+nextIdNumber);
            idsStringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ type={0} , nextId={1} }} , endOfLine{2}",simInventoryType,nextIdNumber,Environment.NewLine);
           }
          }
          idsFileStream.SetLength(0L);
          idsFileStreamWriter.Write(idsStringBuilder.ToString());
          idsFileStreamWriter.Flush();
         }
        }
    }
}