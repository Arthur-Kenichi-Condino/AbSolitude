using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
          Logs.Error("'persistent data system disabled':"+context);
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
        protected virtual PersistentDataFileHandle GetOrOpenSaveFile(string saveFilePath){
         Logs.Debug(()=>"'try to open the save file:'"+saveFilePath);
         rwl.EnterUpgradeableReadLock();
         try{
          if(!PersistentDataManager.singleton.canSave){
           Logs.Error("'persistent data system is in a inconsistent state, skip save file':..."+saveFilePath);
           return null;
          }
          if(!openFiles.TryGetValue(saveFilePath,out var file)){
           rwl.EnterWriteLock();
           try{
            if(!openFiles.TryGetValue(saveFilePath,out file)){
             file=OpenStream(saveFilePath);
             openFiles.Add(saveFilePath,file);
            }
           }catch(Exception e){
            Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
            return null;
           }finally{
            rwl.ExitWriteLock();
           }
          }
          var handle=CreateHandle(saveFilePath);
          return handle;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return null;
         }finally{
          rwl.ExitUpgradeableReadLock();
         }
        }
        protected virtual PersistentDataFileStreaming OpenStream(string filePath){
         var file=Rent();
         if(!file.Open(filePath)){
          PersistentDataManager.singleton.DisableSaving(null,"'failed to open stream of':"+filePath);
          Return(file);
          return null;
         }
         return file;
        }
        protected virtual PersistentDataFileStreaming Rent(){
         return PersistentDataFileStreaming.Rent(typeof(PersistentDataFileStreaming));
        }
        protected virtual PersistentDataFileHandle CreateHandle(string filePath){
         var handle=PersistentDataFileHandle.Rent(typeof(PersistentDataFileHandle));
         handle.Create(this,filePath);
         return handle;
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
         Return(file);
        }
        protected virtual void Return(PersistentDataFileStreaming file){
         PersistentDataFileStreaming.Return(typeof(PersistentDataFileStreaming),file);
        }
        internal virtual PersistentDataFileStreaming GetFile(string saveFilePath){
         rwl.EnterReadLock();
         try{
          if(!PersistentDataManager.singleton.canSave){
           return null;
          }
          if(!openFiles.TryGetValue(saveFilePath,out var file)){
           return null;
          }
          return file;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return null;
         }finally{
          rwl.ExitReadLock();
         }
        }
        internal BinaryWriterLease AcquireWriter(string saveFilePath){
         rwl.EnterReadLock();
         try{
          var file=GetFile(saveFilePath);
          if(file!=null){
           return file.AcquireWriter();
          }
          return default;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return default;
         }finally{
          rwl.ExitReadLock();
         }
        }
        internal BinaryReaderLease AcquireReader(string saveFilePath){
         rwl.EnterReadLock();
         try{
          var file=GetFile(saveFilePath);
          if(file!=null){
           return file.AcquireReader();
          }
          return default;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return default;
         }finally{
          rwl.ExitReadLock();
         }
        }
    }
    internal class PersistentDataFileStreaming{
     static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new(){
      {(typeof(PersistentDataFileStreaming),""),Pool.GetPool<PersistentDataFileStreaming>("",()=>new(),(PersistentDataFileStreaming item)=>{item.OnReturnToPoolRecycle();},true)},
      {(typeof(SpawnMapFile               ),""),Pool.GetPool<SpawnMapFile               >("",()=>new(),(SpawnMapFile                item)=>{item.OnReturnToPoolRecycle();},true)},
     };
        internal static PersistentDataFileStreaming Rent(Type poolId){
         return(PersistentDataFileStreaming)pool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,PersistentDataFileStreaming file){
         pool[(poolId,"")].ObjectReturn(file);
        }
     private readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     protected string saveFilePath;
     protected PersistentDataBinaryWriter fileBinaryWriter;
        internal class PersistentDataBinaryWriter{
         private FileStream stream;
         internal BinaryWriter writer{get;private set;}
         private int disposed;
            internal PersistentDataBinaryWriter(FileStream stream){
             this.stream=stream;
             writer=new(stream);
             Interlocked.Exchange(ref disposed,0);
            }
            internal void OnDispose(){
             Interlocked.Exchange(ref disposed,1);
             writer?.Dispose();
             stream?.Dispose();
             writer=null;
             stream=null;
            }
            internal bool IsDisposed(){
             return Volatile.Read(ref disposed)==1;
            }
        }
     private int binaryReadersInUse;
     private readonly ManualResetEventSlim binaryReadersFinished=new(true);
     protected readonly ConcurrentBag<PersistentDataBinaryReader>fileBinaryReaders=new();
        internal class PersistentDataBinaryReader{
         private FileStream stream;
         internal BinaryReader reader{get;private set;}
         private int disposed;
            internal PersistentDataBinaryReader(FileStream stream){
             this.stream=stream;
             reader=new(stream);
             Interlocked.Exchange(ref disposed,0);
            }
            internal void OnDispose(){
             Interlocked.Exchange(ref disposed,1);
             reader?.Dispose();
             stream?.Dispose();
             reader=null;
             stream=null;
            }
            internal bool IsDisposed(){
             return Volatile.Read(ref disposed)==1;
            }
        }
     protected int open;
     internal bool isOpen=>Volatile.Read(ref open)==1;
        protected virtual void OnReturnToPoolRecycle(){
         Close();
        }
        internal virtual bool Open(string filePath){
         rwl.EnterWriteLock();
         try{
          saveFilePath=filePath;
          Logs.Debug(()=>"'try to open file stream for:'"+saveFilePath);
          fileBinaryWriter=new(new(filePath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite));
          Interlocked.Exchange(ref open,1);
          return true;
         }catch(Exception e){
          Interlocked.Exchange(ref open,0);
          PersistentDataManager.singleton.DisableSaving(e,"'failed to open file stream for:'..."+saveFilePath);
          DisposeResources();
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return false;
         }finally{
          rwl.ExitWriteLock();
         }
        }
        internal virtual void Close(){
         rwl.EnterWriteLock();
         try{
          Interlocked.Exchange(ref open,0);
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
          fileBinaryWriter?.OnDispose();
          fileBinaryWriter=null;
          while(fileBinaryReaders.TryTake(out var binReader)){
           binReader?.OnDispose();
          }
          saveFilePath=null;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
        }
        internal BinaryWriterLease AcquireWriter(){
         return new(this,fileBinaryWriter);
        }
        internal struct BinaryWriterLease:IDisposable{
         private PersistentDataFileStreaming file;
         private PersistentDataBinaryWriter binWriter;
            internal BinaryWriterLease(PersistentDataFileStreaming file,PersistentDataBinaryWriter binWriter){
             this.file=file;
             this.binWriter=binWriter;
             if(file==null)return;
             file.rwl.EnterWriteLock();
             if(!PersistentDataManager.singleton.canSave){
              Exit();
              return;
             }
             if(!file.isOpen){
              Exit();
              return;
             }
            }
            void Exit(){
             file.rwl.ExitWriteLock();
             file=null;
            }
            public BinaryWriter writer{
             get{
              if(file!=null&&file.isOpen&&binWriter!=null&&!binWriter.IsDisposed()){
               return binWriter.writer;
              }
              return null;
             }
            }
            public void Dispose(){
             if(file==null)return;
             Exit();
            }
        }
        internal BinaryReaderLease AcquireReader(){
         return new(this,RentBinaryReader,ReturnBinaryReader);
        }
        protected virtual PersistentDataBinaryReader RentBinaryReader(){
         if(!PersistentDataManager.singleton.canSave){
          Logs.Error("'persistent data system is in a inconsistent state, skip BinaryReader of':..."+saveFilePath);
          return null;
         }
         if(!isOpen){
          Logs.Error("'file is closed, you should not try to use a BinaryReader of':..."+saveFilePath);
          return null;
         }
         Interlocked.Increment(ref binaryReadersInUse);
         binaryReadersFinished.Reset();
         if(fileBinaryReaders.TryTake(out var binReader)){
          return binReader;
         }
         try{
          binReader=new(new(saveFilePath,FileMode.OpenOrCreate,FileAccess.Read,FileShare.ReadWrite));
          return binReader;
         }catch(Exception e){
          if(Interlocked.Decrement(ref binaryReadersInUse)<=0){
           binaryReadersFinished.Set();
          }
          binReader?.OnDispose();
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
          return null;
         }
        }
        protected virtual void ReturnBinaryReader(PersistentDataBinaryReader binReader){
         fileBinaryReaders.Add(binReader);
         if(Interlocked.Decrement(ref binaryReadersInUse)<=0){
          binaryReadersFinished.Set();
         }
        }
        internal struct BinaryReaderLease:IDisposable{
         private PersistentDataFileStreaming file;
         private readonly Action<PersistentDataBinaryReader>returnBinaryReader;
         private PersistentDataBinaryReader binReader;
            internal BinaryReaderLease(PersistentDataFileStreaming file,Func<PersistentDataBinaryReader>rentBinaryReader,Action<PersistentDataBinaryReader>returnBinaryReader){
             this.file=file;
             binReader=null;
             this.returnBinaryReader=returnBinaryReader;
             if(file==null)return;
             file.rwl.EnterReadLock();
             if(!PersistentDataManager.singleton.canSave){
              Exit();
              return;
             }
             if(!file.isOpen){
              Exit();
              return;
             }
             try{
              binReader=rentBinaryReader.Invoke();
             }catch(Exception e){
              binReader=null;
              Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
             }
             if(binReader==null){
              Exit();
             }
            }
            void Exit(){
             file.rwl.ExitReadLock();
             file=null;
            }
            public BinaryReader reader{
             get{
              if(file!=null&&file.isOpen&&binReader!=null&&!binReader.IsDisposed()){
               return binReader.reader;
              }
              return null;
             }
            }
            public void Dispose(){
             if(file==null)return;
             var reader=binReader;
             binReader=null;
             Exit();
             if(reader!=null){
              returnBinaryReader.Invoke(reader);
             }
             binReader=null;
            }
        }
        internal static void WriteVector3(BinaryWriter writer,Vector3 value){
         writer.Write(value.x);
         writer.Write(value.y);
         writer.Write(value.z);
        }
        internal static Vector3 ReadVector3(BinaryReader reader){
         return new Vector3(
          reader.ReadSingle(),
          reader.ReadSingle(),
          reader.ReadSingle()
         );
        }
        internal static void WriteQuaternion(BinaryWriter writer,Quaternion value){
         writer.Write(value.x);
         writer.Write(value.y);
         writer.Write(value.z);
         writer.Write(value.w);
        }
        internal static Quaternion ReadQuaternion(BinaryReader reader){
         return new Quaternion(
          reader.ReadSingle(),
          reader.ReadSingle(),
          reader.ReadSingle(),
          reader.ReadSingle()
         );
        }
        internal static int GetStringSerializedSize(string value){
         int byteCount=Encoding.UTF8.GetByteCount(value);
         int lengthBytes=1;
         while(byteCount>=0x80){
          byteCount>>=7;
          lengthBytes++;
         }
         return lengthBytes+Encoding.UTF8.GetByteCount(value);
        }
    }
    internal class PersistentDataFileHandle:IDisposable{

     static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new(){
      {(typeof(PersistentDataFileHandle),""),Pool.GetPool<PersistentDataFileHandle>("",()=>new(),(PersistentDataFileHandle item)=>{item.Reset();},true)},
      {(typeof(SpawnMapFileHandle      ),""),Pool.GetPool<SpawnMapFileHandle      >("",()=>new(),(SpawnMapFileHandle       item)=>{item.Reset();},true)},
     };
        internal static PersistentDataFileHandle Rent(Type poolId){
         return(PersistentDataFileHandle)pool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,PersistentDataFileHandle file){
         pool[(poolId,"")].ObjectReturn(file);
        }
     protected PersistentDataFileManager fileManager;
     protected string saveFilePath;
        protected virtual void Reset(){
         saveFilePath=null;
         fileManager=null;
        }
        internal virtual void Create(PersistentDataFileManager fileManager,string filePath){
         this.fileManager=fileManager;
         saveFilePath=filePath;
        }
        public void Dispose(){
         Return(GetType(),this);
        }
        protected PersistentDataFileStreaming GetFile(){
         if(saveFilePath==null){
          return null;
         }
         return fileManager.GetFile(saveFilePath);
        }
        internal BinaryWriterLease AcquireWriter(){
         if(saveFilePath==null){
          return default;
         }
         return fileManager.AcquireWriter(saveFilePath);
        }
        internal BinaryReaderLease AcquireReader(){
         if(saveFilePath==null){
          return default;
         }
         return fileManager.AcquireReader(saveFilePath);
        }
    }
}