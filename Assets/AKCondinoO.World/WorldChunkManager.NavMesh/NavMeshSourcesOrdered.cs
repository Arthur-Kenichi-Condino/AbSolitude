using AKCondinoO.Bootstrap;
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
         appendingChunksSources=new(chunkComparer);
        }
        internal void Init(NavMeshBuildSnapshot snapshot){
         this.snapshot=snapshot;
        }
        internal void Reset(){
         chunksSourcesPendingAdd.Clear();
         chunksSourcesPendingRemove.Clear();
         minRemovalIndex=int.MaxValue;
         sourcesIndexMap.Clear();
         indexToSourceKey.Clear();
         added.Clear();
         appendingChunksSources.Clear();
         sourcesMarkedForRemoval.Clear();
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
        internal void RegisterSource(WorldChunk chunk){
         chunksSourcesPendingRemove.Remove(chunk.cCoord);
         SourceKey key=new(chunk.cCoord);
         if(sourcesIndexMap.ContainsKey(key)){
          return;
         }
         chunkComparer.clusterBounds=snapshot.cluster.clusterBounds;
         chunksSourcesPendingAdd[chunk.cCoord]=chunk.terrain.navMeshBuildData.navMeshSource;
        }
     private readonly HashSet<Vector2Int>chunksSourcesPendingRemove=new();
     private int minRemovalIndex=int.MaxValue;
        internal void UnregisterSource(WorldChunk chunk){
         chunksSourcesPendingAdd.Remove(chunk.cCoord);
         SourceKey key=new(chunk.cCoord);
         if(!sourcesIndexMap.TryGetValue(key,out int index)){
          return;
         }
         minRemovalIndex=Math.Min(minRemovalIndex,index);
         chunksSourcesPendingRemove.Add(chunk.cCoord);
         Logs.Debug(()=>"'unregister source':chunk.cCoord:"+chunk.cCoord);
        }
     private readonly Dictionary<SourceKey,int>sourcesIndexMap=new();
     private readonly List<SourceKey>indexToSourceKey=new();
        struct SourceKey{
         public int id;
         public SourceType type;
         public Vector2Int chunk;
            internal SourceKey(Vector2Int chunk){
             type=SourceType.WorldChunk;
             id=HashCode.Combine(chunk.x,chunk.y);
             this.chunk=chunk;
            }
            internal SourceKey(int gameObject){
             type=SourceType.GameObject;
             id=gameObject;
             this.chunk=default;
            }
            public override bool Equals(object obj){
             if(obj is not SourceKey other)return false;
             if(type!=other.type)return false;
             if(type==SourceType.WorldChunk)
              return chunk==other.chunk;
             return id==other.id;
            }
            public override int GetHashCode(){
             if(type==SourceType.WorldChunk)
              return HashCode.Combine((int)type,chunk);
             return HashCode.Combine((int)type,id);
            }
        }
        enum SourceType:byte{
         WorldChunk,
         GameObject,
        }
     private readonly List<NavMeshBuildSource>added=new();
     private readonly SortedDictionary<Vector2Int,NavMeshBuildSource>appendingChunksSources;
     private IEnumerator<KeyValuePair<Vector2Int,NavMeshBuildSource>>appendingChunksSourcesEnumerator;
     private readonly HashSet<Vector2Int>sourcesMarkedForRemoval=new();
     private readonly List<NavMeshBuildSource>ordered=new();
        internal void BeginOrderingSources(){
         added.Clear();
         added.AddRange(snapshot.asyncSources);
         appendingChunksSources.AddRange(chunksSourcesPendingAdd,DictionaryAddRangeHelper.DictionaryAddRangeMethod.Override);
         appendingChunksSourcesEnumerator=appendingChunksSources.GetEnumerator();
         chunksSourcesPendingAdd.Clear();
         sourcesMarkedForRemoval.UnionWith(chunksSourcesPendingRemove);
         chunksSourcesPendingRemove.Clear();
        }
        internal bool OrderingSourcesIncremental(){
         if(sourcesMarkedForRemoval.Count>0){
          int writeIndex=minRemovalIndex;
          for(int readIndex=minRemovalIndex;readIndex<ordered.Count;readIndex++){
           var key=indexToSourceKey[readIndex];
           bool remove=false;
           if(key.type==SourceType.WorldChunk){
            Vector2Int cCoord=key.chunk;
            if(sourcesMarkedForRemoval.Contains(cCoord)){
             remove=true;
            }
           }
           if(remove){
            continue;
           }
           if(writeIndex!=readIndex){
            ordered[writeIndex]=ordered[readIndex];
            indexToSourceKey[writeIndex]=indexToSourceKey[readIndex];
            if(writeIndex<added.Count){
             added[writeIndex]=added[readIndex];
            }
           }
           writeIndex++;
          }
          if(writeIndex<ordered.Count){
           int removeCount=ordered.Count-writeIndex;
           ordered.RemoveRange(writeIndex,removeCount);
           indexToSourceKey.RemoveRange(writeIndex,removeCount);
           if(writeIndex<added.Count){
            int addedRemoveCount=added.Count-writeIndex;
            added.RemoveRange(writeIndex,addedRemoveCount);
           }
          }
          sourcesIndexMap.Clear();
          for(int i=0;i<indexToSourceKey.Count;i++){
           sourcesIndexMap[indexToSourceKey[i]]=i;
          }
         }
         minRemovalIndex=int.MaxValue;
         sourcesMarkedForRemoval.Clear();
         if(appendingChunksSourcesEnumerator.MoveNext()){
          SourceKey key=new(appendingChunksSourcesEnumerator.Current.Key);
          sourcesIndexMap[key]=ordered.Count;
          indexToSourceKey.Add(key);
          ordered.Add(appendingChunksSourcesEnumerator.Current.Value);
          return true;
         }
         appendingChunksSources.Clear();
         Logs.Debug(()=>"ordered.Count:"+ordered.Count);
         return false;
        }
     private IEnumerator<NavMeshBuildSource>orderedEnumerator;
     private int batchSize=5;
     int addingIndex;
        internal void PrepareSourcesForAgentBuild(){
         snapshot.asyncSources.Clear();
         snapshot.asyncSources.AddRange(added);
         orderedEnumerator=ordered.GetEnumerator();
         addingIndex=0;
        }
        internal bool AppendSourcesForAgentIncremental(){
         int count=0;
         while(count<batchSize&&orderedEnumerator.MoveNext()){
          if(addingIndex++<added.Count){continue;}
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