using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunkManager:MonoSingleton<WorldChunkManager>{
     public override int initOrder{get{return 9;}}
     [SerializeField]private WorldChunk chunkPrefab;
     private MonoPool<WorldChunk>chunkPool;
     [SerializeField]internal Vector2Int expropriationDistance=new Vector2Int(9,9);//  ...pool size
     [SerializeField]internal Vector2Int instantiationDistance=new Vector2Int(6,6);
     private readonly Dictionary<Vector2Int,int       >chunkRef=new();
     private readonly Dictionary<Vector2Int,WorldChunk>chunks  =new();
     [SerializeField]internal bool debugForceRegenerate=false;
        public override void Initialize(){
         base.Initialize();
         MaterialAtlasHelper.SetAtlasData();
         chunkPool=new(chunkPrefab);
         if(this!=null){
         }
        }
     private readonly List<WorldChunk>toDestroyModules=new();
        public override void PreShutdown(){
         foreach(var kvp in chunks){
          var cnk=kvp.Value;
          cnk.OnPool();
         }
         toDestroyModules.AddRange(chunks.Values);
         chunkRef.Clear();
         chunks  .Clear();
         base.PreShutdown();
        }
        public override void Shutdown(){
         if(this!=null){
         }
         foreach(var chunk in toDestroyModules){
          chunk.ManualDestroy();
         }
         toDestroyModules.Clear();
         chunkPool.Destroy();
         base.Shutdown();
        }
        internal void AddRef(Vector2Int cCoord){
         //Logs.Message(Logs.LogType.Debug,"cCoord:"+cCoord);
         if(!chunkRef.TryGetValue(cCoord,out var cRef)){
          chunkRef.Add(cCoord,cRef=0);
         }
         chunkRef[cCoord]=cRef+1;
        }
        internal void RemoveRef(Vector2Int cCoord){
         //Logs.Message(Logs.LogType.Debug,"cCoord:"+cCoord);
         if(chunkRef.TryGetValue(cCoord,out var cRef)){
          cRef--;
          if(cRef<=0){
           chunkRef.Remove(cCoord);
           if(chunks.Remove(cCoord,out var cnk)){
            cnk.OnPool();
            chunkPool.Return(cnk);
           }
          }
         }
        }
        internal void EnsureExists(Vector2Int cCoord){
         //Logs.Message(Logs.LogType.Debug,"cCoord:"+cCoord);
         if(!chunks.TryGetValue(cCoord,out var cnk)){
          chunks.Add(cCoord,cnk=chunkPool.Rent());
          cnk.Initialize();
         }
         cnk.OnEnsureExists(cCoord);
        }
        internal bool OnAddSimObjectAt(Vector3 pos,SimObject simObject){
         Vector2Int cCoord=vecPosTocCoord(pos);
         if(chunks.TryGetValue(cCoord,out var cnk)){
          cnk.AddSimObject(simObject);
          return true;
         }
         return false;
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(debugForceRegenerate){
          debugForceRegenerate=false;
          foreach(var kvp in chunks){
           var cnk=kvp.Value;
           cnk.Generate();
          }
         }
        }
    }
}