using AKCondinoO.Bootstrap;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World{
    internal class WorldChunkManager:MonoSingleton<WorldChunkManager>{
     public override int initOrder{get{return 9;}}
     [SerializeField]private WorldChunk chunkPrefab;
     private MonoPool<WorldChunk>chunkPool;
     [SerializeField]internal Vector2Int expropriationDistance=new Vector2Int(9,9);//  ...pool size
     [SerializeField]internal Vector2Int instantiationDistance=new Vector2Int(6,6);
     private readonly Dictionary<Vector2Int,int       >chunkRef=new();
     private readonly Dictionary<Vector2Int,WorldChunk>chunks  =new();
        public override void Initialize(){
         base.Initialize();
         chunkPool=new(chunkPrefab);
         if(this!=null){
         }
        }
        public override void Shutdown(){
         if(this!=null){
         }
         chunkPool.Destroy();
         base.Shutdown();
        }
        internal void AddRef(Vector2Int coord){
         //Logs.Message(Logs.LogType.Debug,"coord:"+coord);
         if(!chunkRef.TryGetValue(coord,out var cRef)){
          chunkRef.Add(coord,cRef=0);
         }
         chunkRef[coord]=cRef+1;
        }
        internal void RemoveRef(Vector2Int coord){
         //Logs.Message(Logs.LogType.Debug,"coord:"+coord);
         if(chunkRef.TryGetValue(coord,out var cRef)){
          cRef--;
          if(cRef<=0){
           chunkRef.Remove(coord);
           if(chunks.Remove(coord,out var cnk)){
            chunkPool.Return(cnk);
           }
          }
         }
        }
        internal void EnsureExists(Vector2Int coord){
         //Logs.Message(Logs.LogType.Debug,"coord:"+coord);
         if(!chunks.TryGetValue(coord,out var cnk)){
          chunks.Add(coord,cnk=chunkPool.Rent());
          cnk.Initialize();
         }
         cnk.OnEnsureExists(coord);
        }
    }
}