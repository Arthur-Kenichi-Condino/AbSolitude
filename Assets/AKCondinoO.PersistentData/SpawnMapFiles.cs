using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataFileStreaming;
namespace AKCondinoO.PersistentData{
    internal class SpawnMapFiles:PersistentDataFileManager{
        internal SpawnMapFiles(string saveFolderPath):base(saveFolderPath){
        }
        internal void OpenSpawnMapSaveFile(Vector2Int cnkRgn){
         OpenSaveFile(GetSpawnMapSaveFilePath(cnkRgn));
        }
        internal string GetSpawnMapSaveFilePath(Vector2Int cnkRgn){
         return string.Format("{0}{1}{2}",saveFolderPath,cnkRgn,".bin");
        }
        internal void WriteToSpawnMapFile(Vector2Int cnkRgn){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         var file=GetSaveFile(filePath);
         if(file!=null){
          using(var writerLease=AcquireWriter(file)){
           var writer=writerLease.Writer;
          }
         }
        }
        private void WriteSpawnMapSimObject(BinaryWriter writer){
         //writer.Write(layer);
         //writer.Write(type);
         //writer.Write(position.x);
         //writer.Write(position.y);
         //writer.Write(position.z);
         //writer.Write(rotation.x);
         //writer.Write(rotation.y);
         //writer.Write(rotation.z);
         //writer.Write(rotation.w);
         //writer.Write(scale.x);
         //writer.Write(scale.y);
         //writer.Write(scale.z);
        }
        internal struct SpawnMapIndex{
         public long offset;
        }
        internal struct SpawnMapObject{
         public string type;
         public Vector3 position;
         public Quaternion rotation;
         public Vector3 scale;
        }
    }
}