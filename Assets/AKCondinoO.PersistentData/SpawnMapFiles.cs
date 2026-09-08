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
    internal sealed class SpawnMapFileHeader:PersistentDataFileHeader{
     internal override int magicBytes=>0x534D4648;//  SMFH
     internal static int version=1;
     internal bool done;
        protected override void OnReturnToPoolRecycle(){
         //  limpar aqui
         done=false;
         base.OnReturnToPoolRecycle();
        }
        protected override int OnCalculateSerializedSize(int version,int supportedVersion){
         switch(supportedVersion){
          default:{
           return sizeof(bool);
          }
         }
        }
        protected override void OnWriteTo(BinaryWriter writer,int requestedVersion,int supportedVersion){
         switch(supportedVersion){
          default:{
           writer.Write(done);
           break;
          }
         }
        }
        protected override void OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
         switch(headerVersion){
          default:{
           //  preencher com dados faltantes
           if(headerVersion>=1){
            done=reader.ReadBoolean();
           }else{
            done=false;
           }
           break;
          }
         }
        }
        protected override void OnGenerate(int requestedVersion,int supportedVersion){
         //  adicionar dados aqui
        }
     internal static readonly SpawnMapFileHeaderSerializer serializer=new();
        internal class SpawnMapFileHeaderSerializer:PersistentDataFileHeaderSerializer<SpawnMapFileHeader>{
            protected override int OnCalculateSerializedSize(SpawnMapFileHeader value,int version,int supportedVersion){
             return value.CalculateSerializedSize(version,supportedVersion);
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnMapFileHeader value,int requestedVersion,int supportedVersion){
             var stream=writer.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             value.WriteTo(writer,requestedVersion,supportedVersion);
            }
            protected override SpawnMapFileHeader OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             var stream=reader.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             var fileHeader=(SpawnMapFileHeader)PersistentDataFileHeader.Rent(typeof(SpawnMapFileHeader));
             fileHeader.ReadFrom(reader,savedVersion,supportedVersion);
             return fileHeader;
            }
            protected override SpawnMapFileHeader OnGenerate(int requestedVersion,int supportedVersion){
             var fileHeader=(SpawnMapFileHeader)PersistentDataFileHeader.Rent(typeof(SpawnMapFileHeader));
             fileHeader.Generate(requestedVersion,supportedVersion);
             return fileHeader;
            }
        }
    }
    internal class SpawnMapFiles:PersistentDataFileManager{
     internal static int version=1;
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
            protected override int OnCalculateSerializedSize(SpawnMapKey value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                sizeof(int)+//  coordinateIndex
                sizeof(int);//  layer
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnMapKey value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.coordinateIndex);
               writer.Write(value.layer);
               break;
              }
             }
            }
            protected override SpawnMapKey OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
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
         public bool empty;
            public override string ToString(){
             return$"version:{version},serializationSize:{serializationSize},empty:{empty}";
            }
        }
     internal static readonly SpawnMapIndexSerializer spawnMapIndexSerializer=new();
        internal sealed class SpawnMapIndexSerializer:IPersistentDataSerializer<SpawnMapIndex>{
            protected override int OnCalculateSerializedSize(SpawnMapIndex value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               if(version>=1){
                return
                 sizeof(int)+//  version
                 sizeof(int)+//  serializationSize
                 sizeof(bool);//  empty
               }
               return
                sizeof(int)+//  version
                sizeof(int);//  serializationSize
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnMapIndex value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               if(requestedVersion>=1){
                writer.Write(value.version);
                writer.Write(value.serializationSize);
                writer.Write(value.empty);
                break;
               }
               writer.Write(value.version);
               writer.Write(value.serializationSize);
               break;
              }
             }
            }
            protected override SpawnMapIndex OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               if(savedVersion>=1){
                return new(){
                 version=reader.ReadInt32(),
                 serializationSize=reader.ReadInt32(),
                 empty=reader.ReadBoolean(),
                };
               }
               return new(){
                version=reader.ReadInt32(),
                serializationSize=reader.ReadInt32(),
                empty=false,
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
            protected override int OnCalculateSerializedSize(SpawnMapObject value,int version,int supportedVersion){
             switch(supportedVersion){
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
            protected override void OnWriteTo(BinaryWriter writer,SpawnMapObject value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               spawnEntrySerializer.WriteTo(writer,value.spawnEntry,requestedVersion);
               spawnVariationSerializer.WriteTo(writer,value.variation,requestedVersion);
               spawnSurfaceSerializer.WriteTo(writer,value.surface,requestedVersion);
               WriteQuaternion(writer,value.rot);
               WriteVector3(writer,value.pos);
               WriteVector3(writer,value.scale);
               break;
              }
             }
            }
            protected override SpawnMapObject OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               return new(){
                spawnEntry=spawnEntrySerializer.ReadFrom(reader,savedVersion),
                variation=spawnVariationSerializer.ReadFrom(reader,savedVersion),
                surface=spawnSurfaceSerializer.ReadFrom(reader,savedVersion),
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
            protected override int OnCalculateSerializedSize(SpawnVariation value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                sizeof(bool)+//  alignToTerrain
                sizeof(float)*3+//  rot
                sizeof(float)*3;//  scale
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnVariation value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.alignToTerrain);
               WriteVector3(writer,value.rot);
               WriteVector3(writer,value.scale);
               break;
              }
             }
            }
            protected override SpawnVariation OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
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
            protected override int OnCalculateSerializedSize(SpawnSurface value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                sizeof(float)*3+//  hitPoint
                sizeof(float)*3;//  normal
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnSurface value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               WriteVector3(writer,value.hitPoint);
               WriteVector3(writer,value.normal);
               break;
              }
             }
            }
            protected override SpawnSurface OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
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
            protected override int OnCalculateSerializedSize(SpawnEntry value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                GetStringSerializationSize(value.prefab.GetType().AssemblyQualifiedName)+
                GetStringSerializationSize(value.prefab.variant)+
                sizeof(float)*3+sizeof(float)*3;//  bounds: center and size
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpawnEntry value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.prefab.GetType().AssemblyQualifiedName);
               writer.Write(value.prefab.variant);
               WriteBounds(writer,value.bounds);
               break;
              }
             }
            }
            protected override SpawnEntry OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
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
          using(var readerLease=fileHandle.AcquireReader(true)){
           var reader=readerLease.reader;
           bool indexed=FindIndex(reader,fileHandle,coordinateIndex,layer,
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
           using(var writerLease=fileHandle.AcquireWriter(readerLease)){
            var writer=writerLease.writer;
            WriteSpawnMapSimObject(writer,fileHandle,key,spawnObject);
           }
          }
         }
        }
        private void WriteSpawnMapSimObject(BinaryWriter writer,SpawnMapFileHandle fileHandle,SpawnMapKey key,SpawnMapObject spawnObject){
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
          empty=false,
         };
         var serializationSize=CalculateSerializationSize(ref index,key,spawnObject);
         WriteTo(writer,index,key,spawnObject);
         fileHandle.SetIndex(key,index);
         fileHandle.SetSpawnMapObject(key,spawnObject);
         Logs.Debug(()=>"'WriteSpawnMapSimObject':key:"+key+";index:"+index);
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
        internal void WriteEmptyToSpawnMapFile(Vector2Int cnkRgn,int layer,Vector3Int vCoord){
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath)){
          int coordinateIndex=GetCoordinateIndex(vCoord);
          using(var readerLease=fileHandle.AcquireReader(true)){
           var reader=readerLease.reader;
           bool indexed=FindIndex(reader,fileHandle,coordinateIndex,layer,
            out SpawnMapKey key,out SpawnMapIndex index
           );
           if(indexed||index.offset<0){return;}
           using(var writerLease=fileHandle.AcquireWriter(readerLease)){
            var writer=writerLease.writer;
            WriteEmpty(writer,fileHandle,key);
           }
          }
         }
        }
        void WriteEmpty(BinaryWriter writer,SpawnMapFileHandle fileHandle,SpawnMapKey key){
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
          empty=true,
         };
         CalculateSerializationSize(ref index,key);
         WriteEmptyTo(writer,index,key);
         fileHandle.SetIndex(key,index);
         fileHandle.SetSpawnMapObject(key,default);
        }
        internal int CalculateSerializationSize(ref SpawnMapIndex index,SpawnMapKey key){
         var serializationSize=
          spawnMapIndexSerializer.CalculateSerializedSize(index,version)+
          spawnMapKeySerializer.CalculateSerializedSize(key,version);
         index.serializationSize=serializationSize;
         return serializationSize;
        }
        void WriteEmptyTo(BinaryWriter writer,SpawnMapIndex index,SpawnMapKey key){
         spawnMapIndexSerializer.WriteTo(writer,index,version);
         spawnMapKeySerializer.WriteTo(writer,key,version);
        }
        internal enum SpawnMapFileReadResult:byte{
         NotFound,
         Empty,
         SpawnObject,
        }
        internal bool ReadFromSpawnMapFile(Vector2Int cnkRgn,int layer,Vector3Int vCoord,out SpawnMapObject spawnObject,out SpawnMapFileReadResult result){
         result=SpawnMapFileReadResult.NotFound;
         var filePath=GetSpawnMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpawnMapFileHandle)GetOrOpenSaveFile(filePath,true)){
          int coordinateIndex=GetCoordinateIndex(vCoord);
          using(var readerLease=fileHandle.AcquireReader(false)){
           var reader=readerLease.reader;
           bool indexed=FindIndex(reader,fileHandle,coordinateIndex,layer,
            out SpawnMapKey key,out SpawnMapIndex index
           );
           if(!indexed){
            spawnObject=default;
            return false;
           }
           if(index.empty){
            result=SpawnMapFileReadResult.Empty;
           }
           bool returnResult=ReadSpawnMapSimObject(reader,fileHandle,key,index,out spawnObject);
           if(returnResult){
            result=SpawnMapFileReadResult.SpawnObject;
           }
           return returnResult;
          }
         }
        }
        internal bool ReadSpawnMapSimObject(BinaryReader reader,SpawnMapFileHandle fileHandle,SpawnMapKey key,SpawnMapIndex index,out SpawnMapObject spawnObject){
         if(reader==null){
          spawnObject=default;
          return false;
         }
         if(fileHandle.TryGetSpawnMapObject(key,out spawnObject)){
          return false;
         }
         var stream=reader.BaseStream;
         if(index.offset+index.serializationSize>stream.Length){
          spawnObject=default;
          return false;
         }
         int version=index.version;
         if(index.empty){
          spawnObject=default;
          return false;
         }
         SeekToSpawnMapSimObject(stream,index,key,version);
         spawnObject=spawnMapObjectSerializer.ReadFrom(reader,version);
         return true;
        }
        void SeekToSpawnMapSimObject(Stream stream,SpawnMapIndex index,SpawnMapKey key,int version){
         stream.Seek(
          index.offset+
          spawnMapIndexSerializer.CalculateSerializedSize(index,version)+
          spawnMapKeySerializer.CalculateSerializedSize(key,version),
          SeekOrigin.Begin
         );
        }
        internal bool FindIndex(BinaryReader reader,SpawnMapFileHandle fileHandle,
         int coordinate,
         int layer,
         out SpawnMapKey key,
         out SpawnMapIndex index
        ){
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
        internal int GetCoordinateIndex(Vector3Int vCoord){
         int index=vCoord.x*Depth+vCoord.z;
         return index;
        }
    }
    internal class SpawnMapFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         PersistentDataFileHeader.Return(typeof(SpawnMapFileHeader),fileHeader);
         fileHeader=null;
         indexes.Clear();
         values.Clear();
         base.OnReturnToPoolRecycle();
        }
     internal readonly Dictionary<SpawnMapKey,SpawnMapIndex>indexes=new();
     internal readonly Dictionary<SpawnMapKey,SpawnMapObject>values=new();
        internal override void OnOpen(){
         base.OnOpen();
         RebuildIndexes();
        }
        protected override void EnsureHeader(){
         using(var readerLease=AcquireReader(true)){
          var reader=readerLease.reader;
          if(reader==null){
           return;
          }
          var readerStream=reader.BaseStream;
          if(readerStream.Length==0){
           fileHeader=SpawnMapFileHeader.serializer.Generate(SpawnMapFileHeader.version);
           using(var writerLease=AcquireWriter()){
            var writer=writerLease.writer;
            SpawnMapFileHeader.serializer.WriteTo(writer,(SpawnMapFileHeader)fileHeader,SpawnMapFileHeader.version);
           }
          }else{
           fileHeader=SpawnMapFileHeader.serializer.ReadFrom(reader,SpawnMapFileHeader.version);
           int oldHeaderVersion=fileHeader.headerVersion;
           int newHeaderVersion=SpawnMapFileHeader.version;
           if(oldHeaderVersion!=newHeaderVersion){
            using(var writerLease=AcquireWriter()){
             var writer=writerLease.writer;
             if(writer==null){
              return;
             }
             var writerStream=writer.BaseStream;
             int newHeaderSize=SpawnMapFileHeader.serializer.CalculateSerializedSize((SpawnMapFileHeader)fileHeader,SpawnMapFileHeader.version);
             EnsureHeaderSize((FileStream)writerStream,fileHeader,newHeaderSize,out bool sizeChanged);
             if(sizeChanged){
              Logs.Debug(()=>"'EnsureHeaderSize' size changed for file:"+saveFilePath);
             }
             SpawnMapFileHeader.serializer.WriteTo(writer,(SpawnMapFileHeader)fileHeader,SpawnMapFileHeader.version);
            }
           }
          }
         }
        }
        protected void RebuildIndexes(){
         using(var readerLease=AcquireReader(false)){
          var reader=readerLease.reader;
          if(reader==null){
           return;
          }
          var stream=reader.BaseStream;
          long fileLength=stream.Length;
          stream.Seek(fileHeader.headerSize,SeekOrigin.Begin);
          //Logs.Debug(()=>"'RebuildIndexes':stream.Position:"+stream.Position+";fileLength:"+fileLength);
          while(stream.Position<fileLength){
           long offset=stream.Position;
           int version=spawnMapIndexSerializer.PeekVersion(reader);
           var index=spawnMapIndexSerializer.ReadFrom(reader,version);
           index.offset=offset;
           var key=spawnMapKeySerializer.ReadFrom(reader,version);
           indexes.Add(key,index);
           var serializationSize=index.serializationSize;
           stream.Position=offset+serializationSize;
           Logs.Debug(()=>"'RebuildIndexes()' key:"+key+";index:"+index+";serializationSize:"+serializationSize);
          }
         }
        }
    }
    internal class SpawnMapFileHandle:PersistentDataFileHandle{
        protected SpawnMapFile GetSpawnMapFile(){
         return CurrentFile()as SpawnMapFile;
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
          file.indexes.Add(key,index);
         }
        }
        internal bool TryGetSpawnMapObject(SpawnMapKey key,out SpawnMapObject spawnMapObject){
         var file=GetSpawnMapFile();
         if(file!=null){
          return file.values.TryGetValue(key,out spawnMapObject);
         }
         spawnMapObject=default;
         return false;
        }
        internal void SetSpawnMapObject(
         SpawnMapKey key,
         SpawnMapObject spawnMapObject
        ){
         var file=GetSpawnMapFile();
         if(file!=null){
          file.values.Add(key,spawnMapObject);
         }
        }
    }
}