#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class PersistentDataLoadingBackgroundContainer:BackgroundContainer{
     internal readonly HashSet<int>terraincnkIdxToLoad=new HashSet<int>();
     internal readonly SpawnData spawnDataFromFiles=new SpawnData();
    }
    internal class PersistentDataLoadingMultithreaded:BaseMultithreaded<PersistentDataLoadingBackgroundContainer>{
     internal readonly Dictionary<Type,FileStream>fileStream=new Dictionary<Type,FileStream>();
      internal readonly Dictionary<Type,StreamReader>fileStreamReader=new Dictionary<Type,StreamReader>();
        protected override void Execute(){
         container.spawnDataFromFiles.dequeued=false;
         foreach(var typeFileStreamPair in this.fileStream){
          Type t=typeFileStreamPair.Key;
          FileStream fileStream=typeFileStreamPair.Value;
          StreamReader fileStreamReader=this.fileStreamReader[t];
          Log.DebugMessage("loading data for type:"+t);
          fileStream.Position=0L;
          fileStreamReader.DiscardBufferedData();
          string line;
          while((line=fileStreamReader.ReadLine())!=null){
           if(string.IsNullOrEmpty(line)){continue;}
           int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
           int cnkIdxStringEnd  =line.IndexOf(" ,",cnkIdxStringStart);
           string cnkIdxString=line.Substring(cnkIdxStringStart,cnkIdxStringEnd-cnkIdxStringStart);
           int cnkIdx=int.Parse(cnkIdxString);
           Log.DebugMessage("reading line for cnkIdx:"+cnkIdx);
           if(container.terraincnkIdxToLoad.Contains(cnkIdx)){
            Log.DebugMessage("must load sim objects at line for cnkIdx:"+cnkIdx);
            int simObjectStringStart=cnkIdxStringEnd+2;
            while((simObjectStringStart=line.IndexOf("simObject=",simObjectStringStart))>=0){
             int simObjectStringEnd=line.IndexOf("}, ",simObjectStringStart)+3;
             simObjectStringStart=simObjectStringEnd;
            }
           }
          }
         }
        }
    }
}