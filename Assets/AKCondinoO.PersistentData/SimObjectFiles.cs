using System;
using UnityEngine;
namespace AKCondinoO.PersistentData{
    internal class SimObjectFiles:PersistentDataFileManager{
        internal SimObjectFiles(string saveFolderPath):base(saveFolderPath){
        }
        internal void OpenSpawnMapSaveFile(Type simObjectType){
         var filePath=GetSimObjectSaveFilePath(simObjectType);
         using(var fileHandle=GetOrOpenSaveFile(filePath)){
         }
        }
        internal string GetSimObjectSaveFilePath(Type simObjectType){
         return string.Format("{0}{1}{2}",saveFolderPath,simObjectType,".bin");
        }
    }
}