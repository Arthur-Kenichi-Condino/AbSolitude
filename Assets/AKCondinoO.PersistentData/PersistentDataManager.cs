using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataFileStreaming;
namespace AKCondinoO.PersistentData{
    internal class PersistentDataManager:MonoSingleton<PersistentDataManager>{
     private readonly Dictionary<Type,PersistenDatatInitialization>initOrderTable=new(){
      {typeof( SpawnMapFiles),
       new(   0, "SpawnMap/",path=>new  SpawnMapFiles(path))},
      {typeof(SimObjectFiles),
       new( 100,"SimObject/",path=>new SimObjectFiles(path))},
     };
        private readonly struct PersistenDatatInitialization{
         internal readonly int initOrder;
         internal readonly string subPath;
         internal readonly Func<string,PersistentDataFileManager>factory;
            internal PersistenDatatInitialization(int initOrder,string subPath,Func<string,PersistentDataFileManager>factory){
             this.initOrder=initOrder;
             this.subPath=subPath;
             this.factory=factory;
            }
        }
     private readonly Dictionary<Type,PersistentDataFileManager>fileManagers=new();
     internal string saveName;
     internal string saveFolderPath;
     internal bool canSave=>Volatile.Read(ref shuttingDown)==0&&Volatile.Read(ref saveFailure)==0;
     private int shuttingDown;
     private int saveFailure;
        public override void Initialize(){
         base.Initialize();
         if(string.IsNullOrEmpty(saveName)){
          saveName="terranova";
         }
         if(string.IsNullOrEmpty(saveFolderPath)){
          saveFolderPath=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\","/")+"/AbSolitude/"+saveName+"/";
         }
         try{
          Directory.CreateDirectory(saveFolderPath);
         }catch(Exception e){
          DisableSaving(e,"'failed to create save folder'");
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
         if(this!=null){
         }
         foreach(var entry in initOrderTable.OrderBy(o=>o.Value.initOrder)){
          fileManagers.Add(
           entry.Key,
           InitFileManager(entry.Value,saveFolderPath)
          );
         }
        }
        private PersistentDataFileManager InitFileManager(PersistenDatatInitialization initialization,string rootPath){
         return initialization.factory.Invoke($"{rootPath}{initialization.subPath}");
        }
        public override void Shutdown(){
         if(this!=null){
         }
         Interlocked.Exchange(ref shuttingDown,1);
         foreach(var entry in initOrderTable.OrderByDescending(o=>o.Value.initOrder)){
          if(fileManagers.TryGetValue(entry.Key,out var manager)){
           manager.CloseAll();
          }
         }
         base.Shutdown();
        }
        internal PersistentDataFileManager GetFileManager(Type dataType){
         if(fileManagers.TryGetValue(dataType,out var manager)){
          return manager;
         }
         Logs.Error("'file manager not found for type':"+dataType.Name);
         return null;
        }
        internal void DisableSaving(Exception e,string context){
         if(Interlocked.Exchange(ref saveFailure,1)==0){
          Logs.Debug(()=>"'persistent data system disabled':"+context);
          if(e!=null){
           Logs.Error("'persistent data system failure':"+"\n"+e?.Message+"\n"+e?.StackTrace);
          }
         }
        }
    }
    internal class PersistentDataFileManager{
     protected readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     internal readonly string saveFolderPath;
        internal PersistentDataFileManager(string saveFolderPath){
         this.saveFolderPath=saveFolderPath;
         if(PersistentDataManager.singleton.canSave){
          try{
           Directory.CreateDirectory(saveFolderPath);
          }catch(Exception e){
           PersistentDataManager.singleton.DisableSaving(e,"'failed to create save subfolder'");
           Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          }
         }else{
          Logs.Error("'persistent data system is in a inconsistent state, skip subfolder':..."+saveFolderPath);
         }
        }
     protected readonly Dictionary<string,PersistentDataFileStreaming>openFiles=new();
        internal virtual void OpenSaveFile(string saveFilePath){
         Logs.Debug(()=>"'try to open the save file:'"+saveFilePath);
         rwl.EnterUpgradeableReadLock();
         try{
          if(!PersistentDataManager.singleton.canSave){
           Logs.Error("'persistent data system is in a inconsistent state, skip save file':..."+saveFilePath);
           return;
          }
          if(!openFiles.TryGetValue(saveFilePath,out var file)){
           rwl.EnterWriteLock();
           try{
            if(!openFiles.TryGetValue(saveFilePath,out file)){
             file=OpenStream(saveFilePath);
            }
           }catch(Exception e){
            Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           }finally{
            rwl.ExitWriteLock();
           }
          }
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitUpgradeableReadLock();
         }
        }
        protected virtual PersistentDataFileStreaming OpenStream(string filePath){
         var file=PersistentDataFileStreaming.pool.Rent();
         file.Open(filePath);
         openFiles.Add(filePath,file);
         return file;
        }
        internal virtual void CloseAll(){
         rwl.EnterWriteLock();
         try{
          foreach(var kvp in openFiles){
           var file=kvp.Value;
           CloseStream(file);
          }
          openFiles.Clear();
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
        }
        protected virtual void CloseStream(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.pool.Return(file);
        }
        protected virtual PersistentDataFileStreaming GetSaveFile(string saveFilePath){
         rwl.EnterReadLock();
         try{
          if(openFiles.TryGetValue(saveFilePath,out var file)){
           return file;
          }
          return null;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return null;
         }finally{
          rwl.ExitReadLock();
         }
        }
        protected BinaryWriterLease AcquireWriter(PersistentDataFileStreaming file){
         return file.AcquireWriter();
        }
        protected BinaryReaderLease AcquireReader(PersistentDataFileStreaming file){
         return file.AcquireReader();
        }
    }
    internal class PersistentDataFileStreaming{
     internal static readonly Utilities.ObjectPool<PersistentDataFileStreaming>pool=
      Pool.GetPool<PersistentDataFileStreaming>(
       "",
       ()=>new(),
       (PersistentDataFileStreaming item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     private readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     protected string saveFilePath;
     protected FileStream fileBinaryWriterStream;
     protected BinaryWriter fileBinaryWriter;
     protected readonly ConcurrentBag<PersistentDataBinaryReader>fileBinaryReaders=new();
     private int binaryReadersInUse;
     private readonly ManualResetEventSlim binaryReadersFinished=new(true);
     protected bool open;
        internal class PersistentDataBinaryReader{
         internal FileStream stream;
         internal BinaryReader reader;
        }
        protected virtual void OnReturnToPoolRecycle(){
         Close();
         binaryReadersInUse=0;
         binaryReadersFinished.Set();
        }
        internal virtual void Open(string filePath){
         rwl.EnterWriteLock();
         try{
          saveFilePath=filePath;
          Logs.Debug(()=>"'try to open file stream for:'"+saveFilePath);
          fileBinaryWriterStream=new(filePath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
          fileBinaryWriter=new(fileBinaryWriterStream);
          open=true;
         }catch(Exception e){
          open=false;
          PersistentDataManager.singleton.DisableSaving(e,"'failed to open file stream for:'..."+saveFilePath);
          DisposeResources();
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
        }
        protected virtual PersistentDataBinaryReader RentBinaryReader(){
         rwl.EnterReadLock();
         try{
          if(!PersistentDataManager.singleton.canSave){
           Logs.Error("'persistent data system is in a inconsistent state, skip BinaryReader of':..."+saveFilePath);
           return null;
          }
          if(!open){
           Logs.Error("'file is closed, you should not try to use a BinaryReader of':..."+saveFilePath);
           return null;
          }
          if(Interlocked.Increment(ref binaryReadersInUse)>0){
           binaryReadersFinished.Reset();
          }
          if(fileBinaryReaders.TryTake(out var binReader)){
           return binReader;
          }
          FileStream fileBinaryReaderStream=null;
          BinaryReader fileBinaryReader=null;
          try{
           fileBinaryReaderStream=new(saveFilePath,FileMode.OpenOrCreate,FileAccess.Read,FileShare.ReadWrite);
           fileBinaryReader=new(fileBinaryReaderStream);
           binReader=new(){stream=fileBinaryReaderStream,reader=fileBinaryReader,};
           return binReader;
          }catch(Exception e){
           fileBinaryReader?.Dispose();
           fileBinaryReaderStream?.Dispose();
           if(Interlocked.Decrement(ref binaryReadersInUse)<=0){
            binaryReadersFinished.Set();
           }
           Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           return null;
          }
         }catch(Exception e){
          if(Interlocked.Decrement(ref binaryReadersInUse)<=0){
           binaryReadersFinished.Set();
          }
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return null;
         }finally{
          rwl.ExitReadLock();
         }
        }
        protected virtual void ReturnBinaryReader(PersistentDataBinaryReader binReader){
         fileBinaryReaders.Add(binReader);
         if(Interlocked.Decrement(ref binaryReadersInUse)<=0){
          binaryReadersFinished.Set();
         }
        }
        internal virtual void Close(){
         rwl.EnterWriteLock();
         try{
          open=false;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
         binaryReadersFinished.Wait();
         rwl.EnterWriteLock();
         try{
          DisposeResources();
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
        }
        protected virtual void DisposeResources(){
         try{
          fileBinaryWriter?.Dispose();
          fileBinaryWriterStream?.Dispose();
          fileBinaryWriter=null;
          fileBinaryWriterStream=null;
          while(fileBinaryReaders.TryTake(out var binReader)){
           binReader.reader?.Dispose();
           binReader.stream?.Dispose();
          }
          saveFilePath=null;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
        }
        internal BinaryWriterLease AcquireWriter(){
         return new(this);
        }
        internal struct BinaryWriterLease:IDisposable{
         private PersistentDataFileStreaming file;
            internal BinaryWriterLease(PersistentDataFileStreaming file){
             this.file=file;
             file.rwl.EnterWriteLock();
            }
            public BinaryWriter Writer=>file.fileBinaryWriter;
            public void Dispose(){
             if(file==null)return;
             file.rwl.ExitWriteLock();
             file=null;
            }
        }
        internal BinaryReaderLease AcquireReader(){
         var binReader=RentBinaryReader();
         return new(this,binReader);
        }
        internal struct BinaryReaderLease:IDisposable{
         private PersistentDataFileStreaming file;
         private PersistentDataBinaryReader reader;
            internal BinaryReaderLease(PersistentDataFileStreaming file,PersistentDataBinaryReader reader){
             this.file=file;
             this.reader=reader;
             file.rwl.EnterReadLock();
            }
            public BinaryReader Reader=>reader.reader;
            public void Dispose(){
             if(file==null)return;
             file.rwl.ExitReadLock();
             file.ReturnBinaryReader(reader);
             reader=null;
             file=null;
            }
        }
    }
}