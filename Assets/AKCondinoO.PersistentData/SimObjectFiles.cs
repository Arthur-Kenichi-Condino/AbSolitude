using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.SimObjectFiles;
using static AKCondinoO.PersistentData.SimObjectPersistentData;
namespace AKCondinoO.PersistentData{
    internal class SimObjectFiles:PersistentDataFileManager{
     internal static int version=0;
        internal SimObjectFiles(string saveFolderPath):base(saveFolderPath){
        }
        protected override PersistentDataFileStreaming Rent(){
         return PersistentDataFileStreaming.Rent(typeof(SimObjectFile));
        }
        protected override void Return(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.Return(typeof(SimObjectFile),file);
        }
        protected override PersistentDataFileHandle CreateHandle(string filePath){
         var handle=PersistentDataFileHandle.Rent(typeof(SimObjectFileHandle));
         handle.Create(this,filePath);
         return handle;
        }
        internal void OpenSimObjectSaveFile(Vector2Int cnkRgn,Type simObjectType,string variant){
         //Logs.Debug(()=>"'simObjectType':"+simObjectType.FullName);
         var filePath=GetSimObjectSaveFilePath(cnkRgn,simObjectType,variant);
         //Logs.Debug(()=>"filePath:"+filePath);
         using(var fileHandle=GetOrOpenSaveFile(filePath)){
         }
        }
        internal string GetSimObjectSaveFilePath(Vector2Int cnkRgn,Type simObjectType,string variant){
         //Logs.Debug(()=>"'simObjectType':"+simObjectType.FullName);
         return string.Format("{0}{1}/{2}.{3}{4}",saveFolderPath,cnkRgn,simObjectType,variant,".bin");
        }
        internal void WriteToSimObjectFile(Vector2Int cnkRgn,Type type,string variant,ulong id,SimObjectPersistentData persistentData){
         var filePath=GetSimObjectSaveFilePath(cnkRgn,type,variant);
         using(var fileHandle=GetOrOpenSaveFile(filePath)){
          using(var readerLease=fileHandle.AcquireReader(true)){
           var reader=readerLease.reader;
           bool indexed=FindIndex(reader,(SimObjectFileHandle)fileHandle,
            id,
            out SimObjectIndex index
           );
           if(indexed||index.offset<0){return;}
           using(var writerLease=fileHandle.AcquireWriter(readerLease)){
            var writer=writerLease.writer;
            WriteSimObject(writer,(SimObjectFileHandle)fileHandle,id,persistentData);
           }
          }
         }
        }
        internal void WriteSimObject(BinaryWriter writer,SimObjectFileHandle fileHandle,ulong id,SimObjectPersistentData persistentData){
         if(writer==null){
          return;
         }
         var stream=writer.BaseStream;
         long offset=stream.Length;
         stream.Position=offset;
         var index=new SimObjectIndex{
          offset=offset,
          version=version,
          id=id,
         };
         CalculateSerializationSize(ref index,persistentData);
         WriteTo(writer,index,persistentData);
         fileHandle.SetIndex(id,index);
        }
        internal int CalculateSerializationSize(ref SimObjectIndex index,SimObjectPersistentData persistentData){
         var serializationSize=
          simObjectIndexSerializer.CalculateSerializedSize(index,version)+
          simObjectSerializer.CalculateSerializedSize(persistentData,version);
         index.serializationSize=serializationSize;
         return serializationSize;
        }
        void WriteTo(BinaryWriter writer,SimObjectIndex index,SimObjectPersistentData persistentData){
         simObjectIndexSerializer.WriteTo(writer,index,version);
         simObjectSerializer.WriteTo(writer,persistentData,version);
        }
        internal bool FindIndex(BinaryReader reader,SimObjectFileHandle fileHandle,
         ulong id,
         out SimObjectIndex index
        ){
         if(reader==null){
          index=new(){offset=-1,};
          return false;
         }
         return fileHandle.TryGetIndex(
          id,
          out index
         );
        }
        internal struct SimObjectIndex{
         public long offset;
         public int version;
         public int serializationSize;
         public ulong id;
        }
     internal static readonly SimObjectIndexSerializer simObjectIndexSerializer=new();
        internal sealed class SimObjectIndexSerializer:PersistentDataSerializer<SimObjectIndex>{
            protected override int OnCalculateSerializedSize(SimObjectIndex value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                sizeof(int)+//  version
                sizeof(int)+//  serializationSize
                sizeof(ulong);//  id
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SimObjectIndex value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               writer.Write(value.version);
               writer.Write(value.serializationSize);
               writer.Write(value.id);
               break;
              }
             }
            }
            protected override SimObjectIndex OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               return new(){
                version=reader.ReadInt32(),
                serializationSize=reader.ReadInt32(),
                id=reader.ReadUInt64()
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
     internal static readonly SimObjectSerializer simObjectSerializer=new();
        internal sealed class SimObjectSerializer:PersistentDataSerializer<SimObjectPersistentData>{
            protected override int OnCalculateSerializedSize(SimObjectPersistentData value,int version,int supportedVersion){
             switch(value.GetType()){
              default:{
               switch(supportedVersion){
                default:{
                 var serializer=(SimObjectPersistentDataSerializer)SimObjectPersistentData.serializers[typeof(SimObjectPersistentData)];
                 return serializer.CalculateSerializedSize(value,version);
                }
               }
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SimObjectPersistentData value,int requestedVersion,int supportedVersion){
             switch(value.GetType()){
              default:{
               switch(supportedVersion){
                default:{
                 var serializer=(SimObjectPersistentDataSerializer)SimObjectPersistentData.serializers[typeof(SimObjectPersistentData)];
                 serializer.WriteTo(writer,value,requestedVersion);
                 break;
                }
               }
               break;
              }
             }
            }
        }
    }
    internal class SimObjectFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         PersistentDataFileHeader.Return(typeof(SimObjectFileHeader),fileHeader);
         fileHeader=null;
         indexes.Clear();
         base.OnReturnToPoolRecycle();
        }
        protected override void EnsureHeader(){
         using(var readerLease=AcquireReader(true)){
          var reader=readerLease.reader;
          if(reader==null){
           return;
          }
          var readerStream=reader.BaseStream;
          if(readerStream.Length==0){
           fileHeader=SimObjectFileHeader.serializer.Generate(SimObjectFileHeader.version);
           using(var writerLease=AcquireWriter()){
            var writer=writerLease.writer;
            SimObjectFileHeader.serializer.WriteTo(writer,(SimObjectFileHeader)fileHeader,SimObjectFileHeader.version);
           }
          }else{
           fileHeader=SimObjectFileHeader.serializer.ReadFrom(reader,SimObjectFileHeader.version);
           int oldHeaderVersion=fileHeader.headerVersion;
           int newHeaderVersion=SimObjectFileHeader.version;
           if(oldHeaderVersion!=newHeaderVersion){
            using(var writerLease=AcquireWriter()){
             var writer=writerLease.writer;
             if(writer==null){
              return;
             }
             var writerStream=writer.BaseStream;
             int newHeaderSize=SimObjectFileHeader.serializer.CalculateSerializedSize((SimObjectFileHeader)fileHeader,SimObjectFileHeader.version);
             EnsureHeaderSize((FileStream)writerStream,fileHeader,newHeaderSize,out bool sizeChanged);
             if(sizeChanged){
              Logs.Debug(()=>"'EnsureHeaderSize' size changed for file:"+saveFilePath);
             }
             SimObjectFileHeader.serializer.WriteTo(writer,(SimObjectFileHeader)fileHeader,SimObjectFileHeader.version);
            }
           }
          }
         }
        }
     internal readonly Dictionary<ulong,SimObjectIndex>indexes=new();
        internal override void OnOpen(){
         base.OnOpen();
         RebuildIndexes();
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
           int version=simObjectIndexSerializer.PeekVersion(reader);
           var index=simObjectIndexSerializer.ReadFrom(reader,version);
           index.offset=offset;
           var key=index.id;
           indexes.Add(key,index);
           var serializationSize=index.serializationSize;
           stream.Position=offset+serializationSize;
           //Logs.Debug(()=>"'RebuildIndexes()' key:"+key+";index:"+index+";serializationSize:"+serializationSize);
          }
         }
        }
    }
    internal sealed class SimObjectFileHeader:PersistentDataFileHeader{
     internal override int magicBytes=>0x534F4648;//  SOFH
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
     internal static readonly SimObjectFileHeaderSerializer serializer=new();
        internal class SimObjectFileHeaderSerializer:PersistentDataFileHeaderSerializer<SimObjectFileHeader>{
            protected override int OnCalculateSerializedSize(SimObjectFileHeader value,int version,int supportedVersion){
             return value.CalculateSerializedSize(version,supportedVersion);
            }
            protected override void OnWriteTo(BinaryWriter writer,SimObjectFileHeader value,int requestedVersion,int supportedVersion){
             var stream=writer.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             value.WriteTo(writer,requestedVersion,supportedVersion);
            }
            protected override SimObjectFileHeader OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             var stream=reader.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             var fileHeader=(SimObjectFileHeader)PersistentDataFileHeader.Rent(typeof(SimObjectFileHeader));
             fileHeader.ReadFrom(reader,savedVersion,supportedVersion);
             return fileHeader;
            }
            protected override SimObjectFileHeader OnGenerate(int requestedVersion,int supportedVersion){
             var fileHeader=(SimObjectFileHeader)PersistentDataFileHeader.Rent(typeof(SimObjectFileHeader));
             fileHeader.Generate(requestedVersion,supportedVersion);
             return fileHeader;
            }
        }
    }
    internal class SimObjectFileHandle:PersistentDataFileHandle{
        protected SimObjectFile GetSimObjectFile(){
         return CurrentFile()as SimObjectFile;
        }
        internal void SetIndex(
         ulong id,
         SimObjectIndex index
        ){
         var file=GetSimObjectFile();
         if(file!=null){
          file.indexes.Add(id,index);
         }
        }
        internal bool TryGetIndex(ulong id,out SimObjectIndex index){
         var file=GetSimObjectFile();
         if(file!=null){
          return file.indexes.TryGetValue(id,out index);
         }
         index=new(){offset=-1,};
         return false;
        }
    }
}