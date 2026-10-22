using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataSerialization;
using static AKCondinoO.PersistentData.SpatialMapFiles;
namespace AKCondinoO.PersistentData{
    internal sealed class SpatialMapFileHeader:PersistentDataFileHeader{
     internal override int magicBytes=>0x534D4150;//  SMAP
     internal static int version=0;
        protected override void OnReturnToPoolRecycle(){
         //  limpar aqui
         base.OnReturnToPoolRecycle();
        }
        protected override int OnCalculateSerializedSize(int version,int supportedVersion){
         switch(supportedVersion){
          default:{
           return 0;
          }
         }
        }
        protected override void OnWriteTo(BinaryWriter writer,int requestedVersion,int supportedVersion){
         switch(supportedVersion){
          default:{
           break;
          }
         }
        }
        protected override void OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
         switch(headerVersion){
          default:{
           //  preencher com dados faltantes
           break;
          }
         }
        }
        protected override void OnGenerate(int requestedVersion,int supportedVersion){
         //  adicionar dados aqui
        }
     internal static readonly SpatialMapFileHeaderSerializer serializer=new();
        internal class SpatialMapFileHeaderSerializer:PersistentDataFileHeaderSerializer<SpatialMapFileHeader>{
            protected override int OnCalculateSerializedSize(SpatialMapFileHeader value,int version,int supportedVersion){
             return value.CalculateSerializedSize(version,supportedVersion);
            }
            protected override void OnWriteTo(BinaryWriter writer,SpatialMapFileHeader value,int requestedVersion,int supportedVersion){
             var stream=writer.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             value.WriteTo(writer,requestedVersion,supportedVersion);
            }
            protected override SpatialMapFileHeader OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             var stream=reader.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             var fileHeader=(SpatialMapFileHeader)PersistentDataFileHeader.Rent(typeof(SpatialMapFileHeader));
             fileHeader.ReadFrom(reader,savedVersion,supportedVersion);
             return fileHeader;
            }
            protected override SpatialMapFileHeader OnGenerate(int requestedVersion,int supportedVersion){
             var fileHeader=(SpatialMapFileHeader)PersistentDataFileHeader.Rent(typeof(SpatialMapFileHeader));
             fileHeader.Generate(requestedVersion,supportedVersion);
             return fileHeader;
            }
        }
    }
    internal class SpatialMapFiles:PersistentDataFileManager{
     internal static int version=0;
        internal SpatialMapFiles(string saveFolderPath):base(saveFolderPath){
        }
        internal readonly struct SpatialMapKey:IEquatable<SpatialMapKey>{
         public readonly Type type;
         public readonly string variant;
         public readonly ulong id;
            internal SpatialMapKey(Type type,string variant,ulong id){
             this.type=type;
             this.variant=variant;
             this.id=id;
            }
            public bool Equals(SpatialMapKey other){
             return type==other.type&&variant==other.variant&&id==other.id;
            }
            public override bool Equals(object obj){
             return obj is SpatialMapKey other&&Equals(other);
            }
            public override int GetHashCode(){
             return HashCode.Combine(type,variant,id);
            }
            public override string ToString(){
             return$"type:{type},variant:{variant},id:{id}";
            }
        }
     internal static readonly SpatialMapKeySerializer spatialMapKeySerializer=new();
        internal sealed class SpatialMapKeySerializer:IPersistentDataSerializer<SpatialMapKey>{
            protected override int OnCalculateSerializedSize(SpatialMapKey value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                GetStringSerializationSize(value.type.AssemblyQualifiedName)+//  type
                GetStringSerializationSize(value.variant)+//  variant
                sizeof(ulong);//  id
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpatialMapKey value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.type.AssemblyQualifiedName);
               writer.Write(value.variant);
               writer.Write(value.id);
               break;
              }
             }
            }
            protected override SpatialMapKey OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               var simObjectTypeName=reader.ReadString();
               var simObjectType=Type.GetType(simObjectTypeName);
               var simObjectVariant=reader.ReadString();
               ulong id=reader.ReadUInt64();
               return new(simObjectType,simObjectVariant,id);
              }
             }
            }
        }
        internal struct SpatialMapIndex{
         public long offset;
         public int version;
         public int serializationSize;
            public override string ToString(){
             return$"version:{version},serializationSize:{serializationSize}";
            }
        }
     internal static readonly SpatialMapIndexSerializer spatialMapIndexSerializer=new();
        internal sealed class SpatialMapIndexSerializer:IPersistentDataSerializer<SpatialMapIndex>{
            protected override int OnCalculateSerializedSize(SpatialMapIndex value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                sizeof(int)+//  version
                sizeof(int);//  serializationSize
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SpatialMapIndex value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.version);
               writer.Write(value.serializationSize);
               break;
              }
             }
            }
            protected override SpatialMapIndex OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
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
        internal void OpenSpatialMapSaveFile(Vector2Int cnkRgn){
         var filePath=GetSpatialMapSaveFilePath(cnkRgn);
         //Logs.Debug(()=>"filePath:"+filePath);
         using(var fileHandle=GetOrOpenSaveFile(filePath)){
         }
        }
        internal string GetSpatialMapSaveFilePath(Vector2Int cnkRgn){
         return string.Format("{0}{1}{2}",saveFolderPath,cnkRgn,".bin");
        }
        internal void WriteToSpatialMapFile(Vector2Int cnkRgn,SimObject simObject){
         var filePath=GetSpatialMapSaveFilePath(cnkRgn);
         using(var fileHandle=(SpatialMapFileHandle)GetOrOpenSaveFile(filePath)){
          Type type=simObject.simObjectType;
          string variant=simObject.variant;
          ulong id=simObject.id;
          using(var readerLease=fileHandle.AcquireReader(true)){
           var reader=readerLease.reader;
           bool indexed=FindIndex(reader,fileHandle,type,variant,id,
            out SpatialMapKey key,out SpatialMapIndex index
           );
           //Logs.Debug(()=>"'WriteToSpatialMapFile':indexed:"+indexed+";index.offset:"+index.offset);
           if(indexed||index.offset<0){return;}
           using(var writerLease=fileHandle.AcquireWriter(readerLease)){
            var writer=writerLease.writer;
            WriteSpatialMapSimObject(writer,fileHandle,key,simObject);
           }
          }
         }
        }
        private void WriteSpatialMapSimObject(BinaryWriter writer,SpatialMapFileHandle fileHandle,SpatialMapKey key,SimObject simObject){
         if(writer==null){
          return;
         }
         long offset=writer.BaseStream.Length;
         writer.BaseStream.Seek(
          offset,
          SeekOrigin.Begin
         );
         var index=new SpatialMapIndex(){
          offset=offset,
          version=version,
         };
         var serializationSize=CalculateSerializationSize(ref index,key,simObject);
         WriteTo(writer,index,key,simObject);
         fileHandle.SetIndex(key,index);
         Logs.Debug(()=>"'WriteSpatialMapSimObject':key:"+key+";index:"+index);
        }
        internal int CalculateSerializationSize(ref SpatialMapIndex index,SpatialMapKey key,SimObject simObject){
         var serializationSize=
          spatialMapIndexSerializer.CalculateSerializedSize(index,version)+
          spatialMapKeySerializer.CalculateSerializedSize(key,version);
         index.serializationSize=serializationSize;
         return serializationSize;
        }
        void WriteTo(BinaryWriter writer,SpatialMapIndex index,SpatialMapKey key,SimObject simObject){
         spatialMapIndexSerializer.WriteTo(writer,index,version);
         spatialMapKeySerializer.WriteTo(writer,key,version);
        }
        internal bool FindIndex(BinaryReader reader,SpatialMapFileHandle fileHandle,
         Type type,
         string variant,
         ulong id,
         out SpatialMapKey key,
         out SpatialMapIndex index
        ){
         if(reader==null){
          key=default;
          index=new(){offset=-1,};
          return false;
         }
         return fileHandle.TryGetIndex(
          key=new SpatialMapKey(type,variant,id),
          out index
         );
        }
        protected override PersistentDataFileStreaming Rent(){
         return PersistentDataFileStreaming.Rent(typeof(SpatialMapFile));
        }
        protected override void Return(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.Return(typeof(SpatialMapFile),file);
        }
        protected override PersistentDataFileHandle CreateHandle(string filePath){
         var handle=PersistentDataFileHandle.Rent(typeof(SpatialMapFileHandle));
         handle.Create(this,filePath);
         return handle;
        }
    }
    internal class SpatialMapFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         PersistentDataFileHeader.Return(typeof(SpatialMapFileHeader),fileHeader);
         fileHeader=null;
         indexes.Clear();
         base.OnReturnToPoolRecycle();
        }
     internal readonly Dictionary<SpatialMapKey,SpatialMapIndex>indexes=new();
        internal override void OnOpen(){
         base.OnOpen();
        }
        protected override void EnsureHeader(){
         using(var readerLease=AcquireReader(true)){
          var reader=readerLease.reader;
          if(reader==null){
           return;
          }
          var readerStream=reader.BaseStream;
          if(readerStream.Length==0){
           fileHeader=SpatialMapFileHeader.serializer.Generate(SpatialMapFileHeader.version);
           using(var writerLease=AcquireWriter()){
            var writer=writerLease.writer;
            SpatialMapFileHeader.serializer.WriteTo(writer,(SpatialMapFileHeader)fileHeader,SpatialMapFileHeader.version);
           }
          }else{
           fileHeader=SpatialMapFileHeader.serializer.ReadFrom(reader,SpatialMapFileHeader.version);
           int oldHeaderVersion=fileHeader.headerVersion;
           int newHeaderVersion=SpatialMapFileHeader.version;
           if(oldHeaderVersion!=newHeaderVersion){
            using(var writerLease=AcquireWriter()){
             var writer=writerLease.writer;
             if(writer==null){
              return;
             }
             var writerStream=writer.BaseStream;
             int newHeaderSize=SpatialMapFileHeader.serializer.CalculateSerializedSize((SpatialMapFileHeader)fileHeader,SpatialMapFileHeader.version);
             EnsureHeaderSize((FileStream)writerStream,fileHeader,newHeaderSize,out bool sizeChanged);
             if(sizeChanged){
              Logs.Debug(()=>"'EnsureHeaderSize' size changed for file:"+saveFilePath);
             }
             SpatialMapFileHeader.serializer.WriteTo(writer,(SpatialMapFileHeader)fileHeader,SpatialMapFileHeader.version);
            }
           }
          }
         }
        }
    }
    internal class SpatialMapFileHandle:PersistentDataFileHandle{
        protected SpatialMapFile GetSpatialMapFile(){
         return CurrentFile()as SpatialMapFile;
        }
        internal bool TryGetIndex(SpatialMapKey key,out SpatialMapIndex index){
         var file=GetSpatialMapFile();
         if(file!=null){
          return file.indexes.TryGetValue(key,out index);
         }
         index=new(){offset=-1,};
         return false;
        }
        internal void SetIndex(
         SpatialMapKey key,
         SpatialMapIndex index
        ){
         var file=GetSpatialMapFile();
         if(file!=null){
          file.indexes.Add(key,index);
         }
        }
    }
}