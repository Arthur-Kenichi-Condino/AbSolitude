using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using System;
using System.IO;
using UnityEngine;
namespace AKCondinoO.PersistentData{
    internal class SimIdsFiles:PersistentDataFileManager{
     internal static int version=0;
        internal SimIdsFiles(string saveFolderPath):base(saveFolderPath){
        }
        protected override PersistentDataFileStreaming Rent(){
         return PersistentDataFileStreaming.Rent(typeof(SimIdsFile));
        }
        protected override void Return(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.Return(typeof(SimIdsFile),file);
        }
        protected override PersistentDataFileHandle CreateHandle(string filePath){
         var handle=PersistentDataFileHandle.Rent(typeof(SimIdsFileHandle));
         handle.Create(this,filePath);
         return handle;
        }
        internal void OpenSimIdsSaveFile(Type simObjectType,string variant){
         var filePath=GetSimIdsSaveFilePath(simObjectType,variant);
         using(var fileHandle=(SimIdsFileHandle)GetOrOpenSaveFile(filePath)){
         }
        }
        internal string GetSimIdsSaveFilePath(Type simObjectType,string variant){
         return string.Format("{0}{1}.{2}{3}",saveFolderPath,simObjectType,variant,".bin");
        }
        internal void WriteToSimIdsFile(Type simObjectType,string variant,ulong counter){
         var filePath=GetSimIdsSaveFilePath(simObjectType,variant);
         using(var fileHandle=(SimIdsFileHandle)GetOrOpenSaveFile(filePath)){
          using(var readerLease=fileHandle.AcquireReader(true)){
           var reader=readerLease.reader;
           using(var writerLease=fileHandle.AcquireWriter(readerLease)){
            var writer=writerLease.writer;
            Logs.Debug(()=>"'WriteToSimIdsFile':counter:"+counter);
            UpdateCounterId(fileHandle,writer,counter);
           }
          }
         }
        }
        internal void UpdateCounterId(SimIdsFileHandle fileHandle,BinaryWriter writer,ulong counter){
         if(writer==null){
          return;
         }
         int headerSize=fileHandle.GetHeaderSize();
         if(headerSize<0){
          return;
         }
         var stream=writer.BaseStream;
         stream.Seek(headerSize,SeekOrigin.Begin);
         writer.Write(counter);
        }
        internal bool ReadFromSimIdsFile(Type simObjectType,string variant,out ulong counter){
         var filePath=GetSimIdsSaveFilePath(simObjectType,variant);
         using(var fileHandle=(SimIdsFileHandle)GetOrOpenSaveFile(filePath,true)){
          using(var readerLease=fileHandle.AcquireReader(false)){
           var reader=readerLease.reader;
           return ReadSimIdsFileCounter(fileHandle,reader,out counter);
          }
         }
        }
        internal bool ReadSimIdsFileCounter(SimIdsFileHandle fileHandle,BinaryReader reader,out ulong counter){
         if(reader==null){
          counter=0;
          return false;
         }
         int headerSize=fileHandle.GetHeaderSize();
         if(headerSize<0){
          counter=0;
          return false;
         }
         var stream=reader.BaseStream;
         if(stream.Length<headerSize+sizeof(ulong)){
          counter=0;
          return false;
         }
         stream.Seek(headerSize,SeekOrigin.Begin);
         counter=reader.ReadUInt64();
         return true;
        }
    }
    internal class SimIdsFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         PersistentDataFileHeader.Return(typeof(SimIdsFileHeader),fileHeader);
         fileHeader=null;
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
           fileHeader=SimIdsFileHeader.serializer.Generate(SimIdsFileHeader.version);
           using(var writerLease=AcquireWriter()){
            var writer=writerLease.writer;
            SimIdsFileHeader.serializer.WriteTo(writer,(SimIdsFileHeader)fileHeader,SimIdsFileHeader.version);
           }
          }else{
           fileHeader=SimIdsFileHeader.serializer.ReadFrom(reader,SimIdsFileHeader.version);
           int oldHeaderVersion=fileHeader.headerVersion;
           int newHeaderVersion=SimIdsFileHeader.version;
           if(oldHeaderVersion!=newHeaderVersion){
            using(var writerLease=AcquireWriter()){
             var writer=writerLease.writer;
             if(writer==null){
              return;
             }
             var writerStream=writer.BaseStream;
             int newHeaderSize=SimIdsFileHeader.serializer.CalculateSerializedSize((SimIdsFileHeader)fileHeader,SimIdsFileHeader.version);
             EnsureHeaderSize((FileStream)writerStream,fileHeader,newHeaderSize,out bool sizeChanged);
             if(sizeChanged){
              Logs.Debug(()=>"'EnsureHeaderSize' size changed for file:"+saveFilePath);
             }
             SimIdsFileHeader.serializer.WriteTo(writer,(SimIdsFileHeader)fileHeader,SimIdsFileHeader.version);
            }
           }
          }
         }
        }
    }
    internal sealed class SimIdsFileHeader:PersistentDataFileHeader{
     internal override int magicBytes=>0x53494648; // SIFH
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
     internal static readonly SimIdsFileHeaderSerializer serializer=new();
        internal class SimIdsFileHeaderSerializer:PersistentDataFileHeaderSerializer<SimIdsFileHeader>{
            protected override int OnCalculateSerializedSize(SimIdsFileHeader value,int version,int supportedVersion){
             return value.CalculateSerializedSize(version,supportedVersion);
            }
            protected override void OnWriteTo(BinaryWriter writer,SimIdsFileHeader value,int requestedVersion,int supportedVersion){
             var stream=writer.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             value.WriteTo(writer,requestedVersion,supportedVersion);
            }
            protected override SimIdsFileHeader OnReadFrom(BinaryReader reader,int savedVersion,int supportedVersion){
             var stream=reader.BaseStream;
             stream.Seek(0,SeekOrigin.Begin);
             var fileHeader=(SimIdsFileHeader)PersistentDataFileHeader.Rent(typeof(SimIdsFileHeader));
             fileHeader.ReadFrom(reader,savedVersion,supportedVersion);
             return fileHeader;
            }
            protected override SimIdsFileHeader OnGenerate(int requestedVersion,int supportedVersion){
             var fileHeader=(SimIdsFileHeader)PersistentDataFileHeader.Rent(typeof(SimIdsFileHeader));
             fileHeader.Generate(requestedVersion,supportedVersion);
             return fileHeader;
            }
        }
    }
    internal class SimIdsFileHandle:PersistentDataFileHandle{
        protected SimIdsFile GetSimIdsFile(){
         return CurrentFile()as SimIdsFile;
        }
        internal int GetHeaderSize(){
         return GetSimIdsFile()?.GetHeaderSize()??-1;
        }
    }
}