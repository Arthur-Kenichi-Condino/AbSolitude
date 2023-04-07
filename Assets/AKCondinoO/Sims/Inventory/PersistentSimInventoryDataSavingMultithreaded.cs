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
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims.Inventory{
    internal class PersistentSimInventoryDataSavingBackgroundContainer:BackgroundContainer{
     internal readonly Dictionary<Type,Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>>simInventoryDataToSerializeToFile=new Dictionary<Type,Dictionary<ulong,Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>>();
      internal static readonly ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>simInventoryDataTypeDictionaryPool=new ConcurrentQueue<Dictionary<Type,Dictionary<ulong,SimInventory.PersistentSimInventoryData>>>();
       internal static readonly ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>simInventoryDataIdDictionaryPool=new ConcurrentQueue<Dictionary<ulong,SimInventory.PersistentSimInventoryData>>();
     internal readonly Dictionary<Type,ulong>persistentIds=new Dictionary<Type,ulong>();
    }
    internal class PersistentSimInventoryDataSavingMultithreaded:BaseMultithreaded<PersistentSimInventoryDataSavingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>simInventoryFileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamWriter>simInventoryFileStreamWriter=new Dictionary<Type,StreamWriter>();
      internal readonly Dictionary<Type,StreamReader>simInventoryFileStreamReader=new Dictionary<Type,StreamReader>();
       readonly StringBuilder stringBuilder=new StringBuilder();
        readonly StringBuilder lineStringBuilder=new StringBuilder();
     internal FileStream idsFileStream;
      internal StreamWriter idsFileStreamWriter;
      internal StreamReader idsFileStreamReader;
       readonly StringBuilder idsStringBuilder=new StringBuilder();
        protected override void Cleanup(){
        }
        protected override void Execute(){
         Log.DebugMessage("Execute()");
         foreach(var typePersistentSimInventoryDataToSavePair in container.simInventoryDataToSerializeToFile){
          Type t=typePersistentSimInventoryDataToSavePair.Key;
          var persistentSimInventoryDataToSave=typePersistentSimInventoryDataToSavePair.Value;
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
           //Log.DebugMessage("id:"+id);
           if(!persistentSimInventoryDataToSave.ContainsKey(id)){
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0}{1}",line,Environment.NewLine);
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
            Dictionary<ulong,SimInventory.PersistentSimInventoryData>idDictionary=simInventoryTypeDictionaryPair.Value;
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[ simInventoryType={0} , {{ ",simInventoryType);
            foreach(var simInventoryIdDataPair in idDictionary){
             ulong simInventoryId=simInventoryIdDataPair.Key;
             SimInventory.PersistentSimInventoryData persistentSimInventoryData=simInventoryIdDataPair.Value;
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[ simInventoryIdNumber={0} , {{ {1} }} ] , ",simInventoryId,persistentSimInventoryData.ToString());
            }
            stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} ] , ");
           }
           stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
          }
          fileStream.SetLength(0L);
          fileStreamWriter.Write(stringBuilder.ToString());
          fileStreamWriter.Flush();
          _Skip:{}
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
         if(idsFileStream!=null){
          idsStringBuilder.Clear();
          foreach(var typeIdsPair in container.persistentIds){
           Type t=typeIdsPair.Key;
           ulong nextId=typeIdsPair.Value;
           if(nextId>0){
            Log.DebugMessage("t:"+t+";nextId:"+nextId);
           }
          }
         }
        }
    }
}