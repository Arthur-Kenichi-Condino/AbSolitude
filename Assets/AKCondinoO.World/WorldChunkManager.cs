using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldChunkManager:MonoSingleton<WorldChunkManager>{
     [SerializeField]private WorldChunk chunkPrefab;
     private MonoPool<WorldChunk>chunkPool;
     [SerializeField]internal Vector2Int expropriationDistance=new Vector2Int(9,9);//  ...pool size
     [SerializeField]internal Vector2Int instantiationDistance=new Vector2Int(6,6);
     private readonly Dictionary<Vector2Int,HashSet<ActiveZone>>chunkRef=new();
     private readonly Dictionary<Vector2Int,WorldChunk         >chunks  =new();
     [SerializeField]internal string[]navMeshLayerNames=new string[]{
      "WorldChunkTerrain",
      "SimStructure",
     };
     internal NavMeshProvider navMeshProvider;
     [SerializeField]internal bool debugForceRegenerate=false;
        public override void Initialize(){
         base.Initialize();
         MaterialAtlasHelper.SetAtlasData();
         chunkPool=new(chunkPrefab);
         navMeshProvider=new(this);
         if(this!=null){
         }
        }
     private readonly List<WorldChunk>toDestroyModules=new();
        public override void PreShutdown(){
         navMeshProvider.Destroy();
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
     internal static readonly Utilities.ObjectPool<HashSet<ActiveZone>>cRefPool=
      Pool.GetPool<HashSet<ActiveZone>>("",()=>new(),(HashSet<ActiveZone>item)=>{item.Clear();});
        internal void AddRef(Vector2Int cCoord,ActiveZone zone){
         if(!chunkRef.TryGetValue(cCoord,out var cRef)){
          chunkRef.Add(cCoord,cRef=cRefPool.Rent());
         }
         chunkRef[cCoord].Add(zone);
        }
        internal void RemoveRef(Vector2Int cCoord,ActiveZone zone){
         if(chunkRef.TryGetValue(cCoord,out var cRef)){
          cRef.Remove(zone);
          if(cRef.Count<=0){
           cRefPool.Return(cRef);
           chunkRef.Remove(cCoord);
           if(chunks.Remove(cCoord,out var cnk)){
            cnk.OnPool();
            chunkPool.Return(cnk);
           }
          }
         }
        }
        internal void EnsureExists(Vector2Int cCoord){
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
        void OnDrawGizmos(){
         if(navMeshProvider!=null){
          foreach(var cluster in navMeshProvider.clusters){
           DrawGizmos.Bounds(cluster.clusterBounds,Color.blue);
          }
         }
        }
 }
}