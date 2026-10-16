using AKCondinoO.Bootstrap;
using System;
using System.IO;
using UnityEngine;
namespace AKCondinoO.PersistentData{
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
    internal class SimObjectFiles:PersistentDataFileManager{
        internal SimObjectFiles(string saveFolderPath):base(saveFolderPath){
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
    }
    internal class SimObjectFile:PersistentDataFileStreaming{
        protected override void OnReturnToPoolRecycle(){
         PersistentDataFileHeader.Return(typeof(SimObjectFileHeader),fileHeader);
         fileHeader=null;
         base.OnReturnToPoolRecycle();
        }
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
    }
    internal class SimObjectFileHandle:PersistentDataFileHandle{
    }
}