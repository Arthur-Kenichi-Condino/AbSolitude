using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.World{
    internal class NavMeshSourcesOrdered{
     internal static readonly Utilities.ObjectPool<NavMeshSourcesOrdered>pool=
      Pool.GetPool<NavMeshSourcesOrdered>(
       "",
       ()=>new(),
       (NavMeshSourcesOrdered item)=>{
        item.Reset();
       },
       false
      );
     internal NavMeshBuildSnapshot snapshot;
        internal NavMeshSourcesOrdered(){
         chunkComparer=new();
         chunksSourcesPendingAdd=new(chunkComparer);
         orderingChunksSources=new(chunkComparer);
        }
        internal void Init(NavMeshBuildSnapshot snapshot){
         this.snapshot=snapshot;
        }
        internal void Reset(){
         chunksSourcesPendingAdd.Clear();
         sourcesIndexMap.Clear();
         orderingChunksSources.Clear();
         ordered.Clear();
         snapshot=null;
        }
     private readonly SortedDictionary<Vector2Int,NavMeshBuildSource>chunksSourcesPendingAdd;
     readonly ChunkCoordComparer chunkComparer;
        class ChunkCoordComparer:IComparer<Vector2Int>{
            internal Bounds clusterBounds;
            public int Compare(Vector2Int a,Vector2Int b){
             float dxA=a.x-clusterBounds.center.x;
             float dzA=a.y-clusterBounds.center.z;
             float distA=dxA*dxA+dzA*dzA;
             float dxB=b.x-clusterBounds.center.x;
             float dzB=b.y-clusterBounds.center.z;
             float distB=dxB*dxB+dzB*dzB;
             int c=distA.CompareTo(distB);
             if(c!=0)return c;
             c=a.y.CompareTo(b.y);
             if(c!=0)return c;
             return a.x.CompareTo(b.x);
            }
        }
     private bool sourceAdded;
        internal void RegisterSource(WorldChunk chunk){
         SourceKey key=new(chunk.cCoord);
         if(sourcesIndexMap.ContainsKey(key)){
          return;
         }
         chunkComparer.clusterBounds=snapshot.cluster.clusterBounds;
         chunksSourcesPendingAdd[chunk.cCoord]=chunk.terrain.navMeshBuildData.navMeshSource;
         sourceAdded=true;
        }
     private bool sourceRemoved;
        internal void UnregisterSource(WorldChunk chunk){
         chunksSourcesPendingAdd.Remove(chunk.cCoord);
         SourceKey key=new(chunk.cCoord);
         if(!sourcesIndexMap.TryGetValue(key,out int index)){
          return;
         }
         sourceRemoved=true;
        }
     private readonly Dictionary<SourceKey,int>sourcesIndexMap=new();
        struct SourceKey{
         public int id;
         public SourceType type;
            internal SourceKey(Vector2Int chunk){
             type=SourceType.WorldChunk;
             id=HashCode.Combine(chunk.x,chunk.y);
            }
            internal SourceKey(int gameObject){
             type=SourceType.GameObject;
             id=gameObject;
            }
        }
        enum SourceType:byte{
         WorldChunk,
         GameObject,
        }
     private readonly SortedDictionary<Vector2Int,NavMeshBuildSource>orderingChunksSources;
     private IEnumerator<KeyValuePair<Vector2Int,NavMeshBuildSource>>orderingChunksSourcesEnumerator;
        internal void BeginOrderingSources(){
         orderingChunksSources.AddRange(chunksSourcesPendingAdd,DictionaryAddRangeHelper.DictionaryAddRangeMethod.Override);
         orderingChunksSourcesEnumerator=orderingChunksSources.GetEnumerator();
         chunksSourcesPendingAdd.Clear();
        }
        internal bool OrderingSourcesIncremental(){
         if(orderingChunksSourcesEnumerator.MoveNext()){
          SourceKey key=new(orderingChunksSourcesEnumerator.Current.Key);
          sourcesIndexMap[key]=ordered.Count;
          ordered.Add(orderingChunksSourcesEnumerator.Current.Value);
          return true;
         }
         orderingChunksSources.Clear();
         return false;
        }
     private readonly List<NavMeshBuildSource>ordered=new();
     private IEnumerator<NavMeshBuildSource>orderedEnumerator;
     private int batchSize=5;
        internal void BeginAddingSources(){
         snapshot.asyncSources.Clear();
         orderedEnumerator=ordered.GetEnumerator();
        }
        internal bool AddingSourcesIncremental(){
         int count=0;
         while(count<batchSize&&orderedEnumerator.MoveNext()){
          snapshot.asyncSources.Add(orderedEnumerator.Current);
          count++;
         }
         if(count>0){
          return true;
         }
         return false;
        }
    }
}