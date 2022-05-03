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
         }
        }
    }
}