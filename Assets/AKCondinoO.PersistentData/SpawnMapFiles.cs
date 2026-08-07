using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataFileStreaming;
using static AKCondinoO.PersistentData.SpawnMapFiles;
using static AKCondinoO.World.SimObjects.ChunkSimObjectSpawner.BiomesSimObjectSpawnerJob;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.PersistentData{
    internal class SpawnMapFiles:PersistentDataFileManager{
     protected int version=0;
        internal SpawnMapFiles(string saveFolderPath):base(saveFolderPath){
        }
        internal void OpenSpawnMapSaveFile(Vector2Int cnkRgn){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath)){
         }
        }
        internal string GetSpawnMapSaveFilePath(Vector2Int cnkRgn){
         return string.Format("{0}{1}{2}",saveFolderPath,cnkRgn,".bin");
        }
        protected override PersistentDataFileStreaming Rent(){
         return PersistentDataFileStreaming.Rent(typeof(SpawnMapFile));
        }
        protected override void Return(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.Return(typeof(SpawnMapFile),file);
        }
        protected override PersistentDataFileHandle CreateHandle(string filePath){
         var handle=PersistentDataFileHandle.Rent(typeof(SpawnMapFileHandle));
         handle.Create(this,filePath);
         return handle;
        }
        internal void WriteToSpawnMapFile(Vector2Int cnkRgn,int layer,Vector3Int vCoord,SpawnCandidate candidate,SpawnReserve reserve){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath)){
          int coordinateIndex=GetCoordinateIndex(vCoord);
          bool indexed=FindIndex(fileHandle,coordinateIndex,layer,
           out SpawnMapKey key,out SpawnMapIndex index
          );
          if(indexed||index.offset<0){return;}
          SpawnMapObject spawnObject=new(){
           type=candidate.spawnEntry.prefab.GetType().ToString(),
           position=reserve.pos,
           rotation=reserve.rot,
           scale=reserve.scale
          };
          WriteSpawnMapSimObject(fileHandle,layer,vCoord,key,spawnObject);
         }
        }
        private void WriteSpawnMapSimObject(SpawnMapFileHandle fileHandle,int layer,Vector3Int vCoord,SpawnMapKey key,SpawnMapObject spawnObject){
         using(var writerLease=fileHandle.AcquireWriter()){
          var writer=writerLease.writer;
          var indexes=fileHandle.indexes;
          if(writer==null||indexes==null){
           return;
          }
          long offset=writer.BaseStream.Length;
          writer.BaseStream.Seek(
           offset,
           SeekOrigin.Begin
          );
          writer.Write(layer);
          spawnObject.WriteTo(writer);
          indexes[key]=new SpawnMapIndex{
           offset=offset,
           version=this.version,
           size=spawnObject.GetSerializedSize(),
          };
         }
        }
        internal bool FindIndex(SpawnMapFileHandle fileHandle,
         int coordinate,
         int layer,
         out SpawnMapKey key,
         out SpawnMapIndex index
        ){
         using(var readerLease=fileHandle.AcquireReader()){
          var reader=readerLease.reader;
          var indexes=fileHandle.indexes;
          if(reader==null||indexes==null){
           key=default;
           index=new(){offset=-1,};
           return false;
          }
          return indexes.TryGetValue(
           key=new SpawnMapKey(coordinate,layer),
           out index
          );
         }
        }
        internal int GetCoordinateIndex(Vector3Int vCoord){
         int index=vCoord.x*Depth+vCoord.z;
         return index;
        }
        internal readonly struct SpawnMapKey:IEquatable<SpawnMapKey>{
         public readonly int coordinateIndex;
         public readonly int layer;
            internal SpawnMapKey(int coordinate,int layer){
             this.coordinateIndex=coordinate;
             this.layer=layer;
            }
            public bool Equals(SpawnMapKey other){
             return coordinateIndex==other.coordinateIndex&&layer==other.layer;
            }
            public override bool Equals(object obj){
             return obj is SpawnMapKey other&&Equals(other);
            }
            public override int GetHashCode(){
             return HashCode.Combine(coordinateIndex,layer);
            }
        }
        internal struct SpawnMapIndex{
         public long offset;
         public int version;
         public int size;
        }
        internal struct SpawnMapObject{
         public string type;
         public Vector3 position;
         public Quaternion rotation;
         public Vector3 scale;
            internal void WriteTo(BinaryWriter writer){
             writer.Write(type);
             WriteVector3(writer,position);
             WriteQuaternion(writer,rotation);
             WriteVector3(writer,scale);
            }
            internal int GetSerializedSize(){
             return GetStringSerializedSize(type)+
              sizeof(float)*3+//  position
              sizeof(float)*4+//  rotation
              sizeof(float)*3;//  scale
            }
        }
    }
    internal class SpawnMapFile:PersistentDataFileStreaming{
     internal readonly Dictionary<SpawnMapKey,SpawnMapIndex>indexes=new();
        protected override void OnReturnToPoolRecycle(){
         base.OnReturnToPoolRecycle();
         indexes.Clear();
        }
    }
    internal class SpawnMapFileHandle:PersistentDataFileHandle{
     internal Dictionary<SpawnMapKey,SpawnMapIndex>indexes{
      get{
       return(GetFile()as SpawnMapFile)?.indexes;
      }
     }
    }
}