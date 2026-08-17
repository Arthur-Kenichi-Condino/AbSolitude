using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataFileStreaming;
using static AKCondinoO.PersistentData.PersistentDataSerialization;
using static AKCondinoO.PersistentData.SpawnMapFiles;
using static AKCondinoO.World.SimObjects.ChunkSimObjectSpawner.BiomesSimObjectSpawnerJob;
using static AKCondinoO.World.Spawning.ByChanceObjectSpawnEntry<AKCondinoO.SimObjects.SimObject>;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.PersistentData{
    internal class SpawnMapFiles:PersistentDataFileManager{
     internal static int version=0;
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
        internal readonly struct SpawnMapKey:IEquatable<SpawnMapKey>{
         public readonly int coordinateIndex;
         public readonly int layer;
            internal SpawnMapKey(int coordinateIndex,int layer){
             this.coordinateIndex=coordinateIndex;
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
            public override string ToString(){
             return$"coordinateIndex:{coordinateIndex},layer:{layer}";
            }
        }
     internal static readonly SpawnMapKeySerializer spawnMapKeySerializer=new();
        internal sealed class SpawnMapKeySerializer:IPersistentDataSerializer<SpawnMapKey>{
            public int CalculateSerializedSize(SpawnMapKey value,int version){
             switch(version){
              default:{
               return
                sizeof(int)+//  coordinateIndex
                sizeof(int);//  layer
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnMapKey value,int version){
             switch(version){
              default:{
               writer.Write(value.coordinateIndex);
               writer.Write(value.layer);
               break;
              }
             }
            }
            public SpawnMapKey ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               int coordinateIndex=reader.ReadInt32();
               int layer=reader.ReadInt32();
               return new(coordinateIndex,layer);
              }
             }
            }
        }
        internal struct SpawnMapIndex{
         public long offset;
         public int version;
         public int serializationSize;
            public override string ToString(){
             return$"version:{version},serializationSize:{serializationSize}";
            }
        }
     internal static readonly SpawnMapIndexSerializer spawnMapIndexSerializer=new();
        internal sealed class SpawnMapIndexSerializer:IPersistentDataSerializer<SpawnMapIndex>{
            public int CalculateSerializedSize(SpawnMapIndex value,int version){
             switch(version){
              default:{
               return
                sizeof(int)+//  version
                sizeof(int);//  serializationSize
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnMapIndex value,int version){
             switch(version){
              default:{
               writer.Write(value.version);
               writer.Write(value.serializationSize);
               break;
              }
             }
            }
            public SpawnMapIndex ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               return new(){
                version=reader.ReadInt32(),
                serializationSize=reader.ReadInt32(),
               };
              }
             }
            }
            internal int PeekVersion(BinaryReader reader){
             var stream=reader.BaseStream;
             int version=reader.ReadInt32();
             stream.Seek(-sizeof(int),SeekOrigin.Current);
             return version;
            }
        }
        internal struct SpawnMapObject{
         public SpawnEntry spawnEntry;
         public SpawnVariation variation;
         public SpawnSurface surface;
         public Quaternion rot;
         public Vector3 pos;
         public Vector3 scale;
        }
     internal static readonly SpawnMapObjectSerializer spawnMapObjectSerializer=new();
        internal sealed class SpawnMapObjectSerializer:IPersistentDataSerializer<SpawnMapObject>{
            public int CalculateSerializedSize(SpawnMapObject value,int version){
             switch(version){
              default:{
               return
                spawnEntrySerializer.CalculateSerializedSize(value.spawnEntry,version)+
                spawnVariationSerializer.CalculateSerializedSize(value.variation,version)+
                spawnSurfaceSerializer.CalculateSerializedSize(value.surface,version)+
                sizeof(float)*4+//  rot
                sizeof(float)*3+//  pos
                sizeof(float)*3;//  scale
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnMapObject value,int version){
             switch(version){
              default:{
               spawnEntrySerializer.WriteTo(writer,value.spawnEntry,version);
               spawnVariationSerializer.WriteTo(writer,value.variation,version);
               spawnSurfaceSerializer.WriteTo(writer,value.surface,version);
               WriteQuaternion(writer,value.rot);
               WriteVector3(writer,value.pos);
               WriteVector3(writer,value.scale);
               break;
              }
             }
            }
            public SpawnMapObject ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               return new(){
                spawnEntry=spawnEntrySerializer.ReadFrom(reader,version),
                variation=spawnVariationSerializer.ReadFrom(reader,version),
                surface=spawnSurfaceSerializer.ReadFrom(reader,version),
                rot=ReadQuaternion(reader),
                pos=ReadVector3(reader),
                scale=ReadVector3(reader),
               };
              }
             }
            }
        }
     internal static readonly SpawnVariationSerializer spawnVariationSerializer=new();
        internal sealed class SpawnVariationSerializer:IPersistentDataSerializer<SpawnVariation>{
            public int CalculateSerializedSize(SpawnVariation value,int version){
             switch(version){
              default:{
               return
                sizeof(bool)+//  alignToTerrain
                sizeof(float)*3+//  rot
                sizeof(float)*3;//  scale
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnVariation value,int version){
             switch(version){
              default:{
               writer.Write(value.alignToTerrain);
               WriteVector3(writer,value.rot);
               WriteVector3(writer,value.scale);
               break;
              }
             }
            }
            public SpawnVariation ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               return new(){
                alignToTerrain=reader.ReadBoolean(),
                rot=ReadVector3(reader),
                scale=ReadVector3(reader),
               };
              }
             }
            }
        }
     internal static readonly SpawnSurfaceSerializer spawnSurfaceSerializer=new();
        internal sealed class SpawnSurfaceSerializer:IPersistentDataSerializer<SpawnSurface>{
            public int CalculateSerializedSize(SpawnSurface value,int version){
             switch(version){
              default:{
               return
                sizeof(float)*3+//  hitPoint
                sizeof(float)*3;//  normal
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnSurface value,int version){
             switch(version){
              default:{
               WriteVector3(writer,value.hitPoint);
               WriteVector3(writer,value.normal);
               break;
              }
             }
            }
            public SpawnSurface ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               return new(){
                hitPoint=ReadVector3(reader),
                normal=ReadVector3(reader),
               };
              }
             }
            }
        }
     internal static readonly SpawnEntrySerializer spawnEntrySerializer=new();
        internal sealed class SpawnEntrySerializer:IPersistentDataSerializer<SpawnEntry>{
            public int CalculateSerializedSize(SpawnEntry value,int version){
             switch(version){
              default:{
               return
                GetStringSerializationSize(value.prefab.GetType().AssemblyQualifiedName)+
                GetStringSerializationSize(value.prefab.variant)+
                sizeof(float)*3+sizeof(float)*3;//  bounds: center and size
              }
             }
            }
            public void WriteTo(BinaryWriter writer,SpawnEntry value,int version){
             switch(version){
              default:{
               writer.Write(value.prefab.GetType().AssemblyQualifiedName);
               writer.Write(value.prefab.variant);
               WriteBounds(writer,value.bounds);
               break;
              }
             }
            }
            public SpawnEntry ReadFrom(BinaryReader reader,int version){
             switch(version){
              default:{
               var prefabTypeName=reader.ReadString();
               var prefabType=Type.GetType(prefabTypeName);
               var prefabVariant=reader.ReadString();
               var prefab=SimObjectManager.singleton.GetPrefab(prefabType,prefabVariant);
               return new(){
                prefab=prefab,
                bounds=ReadBounds(reader),
               };
              }
             }
            }
        }
        internal void WriteToSpawnMapFile(Vector2Int cnkRgn,int layer,Vector3Int vCoord,SpawnCandidate candidate,SpawnReserve reserve){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         //Logs.Debug(()=>"'WriteToSpawnMapFile':filePath:"+filePath);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath)){
          int coordinateIndex=GetCoordinateIndex(vCoord);
          bool indexed=FindIndex(fileHandle,coordinateIndex,layer,
           out SpawnMapKey key,out SpawnMapIndex index
          );
          //Logs.Debug(()=>"'WriteToSpawnMapFile':indexed:"+indexed+";index.offset:"+index.offset);
          if(indexed||index.offset<0){return;}
          SpawnMapObject spawnObject=new(){
           spawnEntry=candidate.spawnEntry,
           variation=candidate.variation,
           surface=candidate.surface,
           rot=reserve.rot,
           pos=reserve.pos,
           scale=reserve.scale
          };
          WriteSpawnMapSimObject(fileHandle,key,spawnObject);
         }
        }
        private void WriteSpawnMapSimObject(SpawnMapFileHandle fileHandle,SpawnMapKey key,SpawnMapObject spawnObject){
         using(var writerLease=fileHandle.AcquireWriter()){
          var writer=writerLease.writer;
          if(writer==null){
           return;
          }
          long offset=writer.BaseStream.Length;
          writer.BaseStream.Seek(
           offset,
           SeekOrigin.Begin
          );
          var index=new SpawnMapIndex(){
           offset=offset,
           version=version,
          };
          var serializationSize=CalculateSerializationSize(ref index,key,spawnObject);
          WriteTo(writer,index,key,spawnObject);
          fileHandle.SetIndex(key,index);
          Logs.Debug(()=>"'WriteSpawnMapSimObject':key:"+key+";index:"+index);
         }
        }
        internal int CalculateSerializationSize(ref SpawnMapIndex index,SpawnMapKey key,SpawnMapObject spawnObject){
         var serializationSize=
          spawnMapIndexSerializer.CalculateSerializedSize(index,version)+
          spawnMapKeySerializer.CalculateSerializedSize(key,version)+
          spawnMapObjectSerializer.CalculateSerializedSize(spawnObject,version);
         index.serializationSize=serializationSize;
         return serializationSize;
        }
        void WriteTo(BinaryWriter writer,SpawnMapIndex index,SpawnMapKey key,SpawnMapObject spawnObject){
         spawnMapIndexSerializer.WriteTo(writer,index,version);
         spawnMapKeySerializer.WriteTo(writer,key,version);
         spawnMapObjectSerializer.WriteTo(writer,spawnObject,version);
        }
        internal bool ReadFromSpawnMapFile(Vector2Int cnkRgn,int layer,Vector3Int vCoord,out SpawnMapObject spawnObject){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath,true)){
          int coordinateIndex=GetCoordinateIndex(vCoord);
          bool indexed=FindIndex(fileHandle,coordinateIndex,layer,
           out SpawnMapKey key,out SpawnMapIndex index
          );
          if(!indexed){
           spawnObject=default;
           return false;
          }
          return ReadSpawnMapSimObject(fileHandle,key,index,out spawnObject);
         }
        }
        internal bool ReadSpawnMapSimObject(SpawnMapFileHandle fileHandle,SpawnMapKey key,SpawnMapIndex index,out SpawnMapObject spawnObject){
         using(var readerLease=fileHandle.AcquireReader()){
          var reader=readerLease.reader;
          if(reader==null){
           spawnObject=default;
           return false;
          }
          var stream=reader.BaseStream;
          if(index.offset+index.serializationSize>stream.Length){
           spawnObject=default;
           return false;
          }
          int version=index.version;
          SeekToSpawnMapSimObject(stream,index,key,version);
          spawnObject=spawnMapObjectSerializer.ReadFrom(reader,version);
          return true;
         }
        }
        void SeekToSpawnMapSimObject(Stream stream,SpawnMapIndex index,SpawnMapKey key,int version){
         stream.Seek(
          index.offset+
          spawnMapIndexSerializer.CalculateSerializedSize(index,version)+
          spawnMapKeySerializer.CalculateSerializedSize(key,version),
          SeekOrigin.Begin
         );
        }
        internal bool FindIndex(SpawnMapFileHandle fileHandle,
         int coordinate,
         int layer,
         out SpawnMapKey key,
         out SpawnMapIndex index
        ){
         using(var readerLease=fileHandle.AcquireReader()){
          var reader=readerLease.reader;
          if(reader==null){
           key=default;
           index=new(){offset=-1,};
           return false;
          }
          return fileHandle.TryGetIndex(
           key=new SpawnMapKey(coordinate,layer),
           out index
          );
         }
        }
        internal int GetCoordinateIndex(Vector3Int vCoord){
         int index=vCoord.x*Depth+vCoord.z;
         return index;
        }
    }
    internal class SpawnMapFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         base.OnReturnToPoolRecycle();
         indexes.Clear();
        }
     internal readonly Dictionary<SpawnMapKey,SpawnMapIndex>indexes=new();
        internal override void OnOpen(){
         base.OnOpen();
         RebuildIndexes();
        }
        protected void RebuildIndexes(){
         using(var readerLease=AcquireReader()){
          var reader=readerLease.reader;
          if(reader==null){
           return;
          }
          var stream=reader.BaseStream;
          long fileLength=stream.Length;
          stream.Seek(0,SeekOrigin.Begin);
          //Logs.Debug(()=>"'RebuildIndexes':stream.Position:"+stream.Position+";fileLength:"+fileLength);
          while(stream.Position<fileLength){
           long offset=stream.Position;
           int version=spawnMapIndexSerializer.PeekVersion(reader);
           var index=spawnMapIndexSerializer.ReadFrom(reader,version);
           index.offset=offset;
           var key=spawnMapKeySerializer.ReadFrom(reader,version);
           indexes[key]=index;
           var serializationSize=index.serializationSize;
           stream.Position=offset+serializationSize;
           //Logs.Debug(()=>"'RebuildIndexes()' key:"+key+";index:"+index+";serializationSize:"+serializationSize);
          }
         }
        }
    }
    internal class SpawnMapFileHandle:PersistentDataFileHandle{
        protected SpawnMapFile GetSpawnMapFile(){
         return GetFile()as SpawnMapFile;
        }
        internal bool TryGetIndex(SpawnMapKey key,out SpawnMapIndex index){
         var file=GetSpawnMapFile();
         if(file!=null){
          return file.indexes.TryGetValue(key,out index);
         }
         index=new(){offset=-1,};
         return false;
        }
        internal void SetIndex(
         SpawnMapKey key,
         SpawnMapIndex index
        ){
         var file=GetSpawnMapFile();
         if(file!=null){
          file.indexes[key]=index;
         }
        }
    }
}