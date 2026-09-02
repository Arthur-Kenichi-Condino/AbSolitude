using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
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
        item.OnReturnToPoolRecycle();
       },
       false
      );
     internal NavMeshBuildSnapshot snapshot;
        internal NavMeshSourcesOrdered(){
         chunkComparer=new();
         chunksSourcesPendingAdd=new(chunkComparer);
         simObjectComparer=new();
         simObjectsSourcesPendingAdd=new(simObjectComparer);
         appendingChunksSources=new(chunkComparer);
         appendingSimObjectsSources=new(simObjectComparer);
        }
        internal void Init(NavMeshBuildSnapshot snapshot){
         this.snapshot=snapshot;
        }
        internal void OnReturnToPoolRecycle(){
         chunksSourcesPendingAdd.Clear();
         chunksSourcesPendingRemove.Clear();
         simObjectsSourcesPendingAdd.Clear();
         simObjectsSourcesPendingRemove.Clear();
         sourcesIndexMap.Clear();
         indexToSourceKey.Clear();
         added.Clear();
         appendingChunksSources.Clear();
         appendingSimObjectsSources.Clear();
         sourcesMarkedForRemoval.Clear();
         simObjectsSourcesMarkedForRemoval.Clear();
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
         chunkComparer.clusterBounds=snapshot.cluster.clusterBounds;
         chunksSourcesPendingAdd[chunk.cCoord]=chunk.terrain.navMeshBuildData.navMeshSource;
        }
     private readonly HashSet<Vector2Int>chunksSourcesPendingRemove=new();
        internal void UnregisterSource(WorldChunk chunk){
         chunksSourcesPendingAdd.Remove(chunk.cCoord);
         chunksSourcesPendingRemove.Add(chunk.cCoord);
         Logs.Debug(()=>"'unregister source':chunk.cCoord:"+chunk.cCoord);
        }
     private readonly SortedDictionary<SimObject,NavMeshBuildSource>simObjectsSourcesPendingAdd;
     readonly SimObjectComparer simObjectComparer;
        class SimObjectComparer:IComparer<SimObject>{
            internal Bounds clusterBounds;
            public int Compare(SimObject a,SimObject b){
             Vector3 posA=a.transform.position;
             Vector3 posB=b.transform.position;
             float dxA=posA.x-clusterBounds.center.x;
             float dyA=posA.y-clusterBounds.center.y;
             float dzA=posA.z-clusterBounds.center.z;
             float distA=dxA*dxA+dyA*dyA+dzA*dzA;
             float dxB=posB.x-clusterBounds.center.x;
             float dyB=posB.y-clusterBounds.center.y;
             float dzB=posB.z-clusterBounds.center.z;
             float distB=dxB*dxB+dyB*dyB+dzB*dzB;
             int c=distA.CompareTo(distB);
             if(c!=0)return c;
             c=posA.y.CompareTo(posB.y);
             if(c!=0)return c;
             c=posA.z.CompareTo(posB.z);
             if(c!=0)return c;
             c=posA.x.CompareTo(posB.x);
             if(c!=0)return c;
             return a.GetEntityId().CompareTo(b.GetEntityId());
            }
        }
        internal void RegisterSource(SimObject simObject){
         simObjectsSourcesPendingRemove.Remove(simObject.GetEntityId());
         simObjectComparer.clusterBounds=snapshot.cluster.clusterBounds;
         simObjectsSourcesPendingAdd[simObject]=simObject.simObjectNavMeshSource;
        }
     private readonly HashSet<int>simObjectsSourcesPendingRemove=new();
     private readonly Dictionary<SourceKey,int>sourcesIndexMap=new();
     private readonly List<SourceKey>indexToSourceKey=new();
        struct SourceKey{
         public int id;
         public SourceType type;
         public Vector2Int chunk;
            internal SourceKey(Vector2Int chunk){
             type=SourceType.WorldChunk;
             id=default;
             this.chunk=chunk;
            }
            internal SourceKey(EntityId gameObject){
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
     private readonly SortedDictionary<SimObject,NavMeshBuildSource>appendingSimObjectsSources;
     private IEnumerator<KeyValuePair<SimObject,NavMeshBuildSource>>appendingSimObjectsSourcesEnumerator;
     private readonly HashSet<Vector2Int>sourcesMarkedForRemoval=new();
     private readonly HashSet<int>simObjectsSourcesMarkedForRemoval=new();
     private readonly List<NavMeshBuildSource>ordered=new();
     bool removalInProgress;
     int removalReadIndex;
     int removalWriteIndex;
     bool rebuildingIndexMap;
     int indexMapRebuildIndex;
        internal void BeginOrderingSources(){
         added.Clear();
         added.AddRange(snapshot.asyncSources);
         appendingChunksSources.AddRange(chunksSourcesPendingAdd,DictionaryAddRangeHelper.DictionaryAddRangeMethod.Override);
         appendingChunksSourcesEnumerator=appendingChunksSources.GetEnumerator();
         appendingSimObjectsSources.AddRange(simObjectsSourcesPendingAdd,DictionaryAddRangeHelper.DictionaryAddRangeMethod.Override);
         appendingSimObjectsSourcesEnumerator=appendingSimObjectsSources.GetEnumerator();
         sourcesMarkedForRemoval.UnionWith(chunksSourcesPendingRemove);
         simObjectsSourcesMarkedForRemoval.UnionWith(simObjectsSourcesPendingRemove);
         chunksSourcesPendingAdd.Clear();
         chunksSourcesPendingRemove.Clear();
         simObjectsSourcesPendingAdd.Clear();
         simObjectsSourcesPendingRemove.Clear();
         removalInProgress=false;
         rebuildingIndexMap=false;
         if(sourcesMarkedForRemoval.Count>0||simObjectsSourcesMarkedForRemoval.Count>0){
          removalInProgress=true;
          removalReadIndex=0;
          removalWriteIndex=0;
         }
        }
        internal bool OrderingSourcesIncremental(){
         if(removalInProgress){
          int steps=0;
          int maxSteps=batchSize;
          while(removalReadIndex<ordered.Count&&steps<maxSteps){
           var key=indexToSourceKey[removalReadIndex];
           bool remove=false;
           if(key.type==SourceType.WorldChunk){
            Vector2Int cCoord=key.chunk;
            if(sourcesMarkedForRemoval.Contains(cCoord)){
             remove=true;
            }
           }
           if(!remove){
            if(removalWriteIndex!=removalReadIndex){
             ordered[removalWriteIndex]=ordered[removalReadIndex];
             indexToSourceKey[removalWriteIndex]=indexToSourceKey[removalReadIndex];
             if(removalWriteIndex<added.Count){
              added[removalWriteIndex]=added[removalReadIndex];
             }
            }
            removalWriteIndex++;
           }
           removalReadIndex++;
           steps++;
          }
          if(removalReadIndex>=ordered.Count){
           int finalCount=removalWriteIndex;
           if(finalCount<ordered.Count){
            int removeCount=ordered.Count-finalCount;
            ordered.RemoveRange(finalCount,removeCount);
            indexToSourceKey.RemoveRange(finalCount,removeCount);
            if(finalCount<added.Count){
             added.RemoveRange(finalCount,added.Count-finalCount);
            }
           }
           sourcesIndexMap.Clear();
           rebuildingIndexMap=true;
           indexMapRebuildIndex=0;
           removalReadIndex=0;
           removalWriteIndex=0;
           removalInProgress=false;
           sourcesMarkedForRemoval.Clear();
           simObjectsSourcesMarkedForRemoval.Clear();
           return true;
          }
          return true;
         }
         if(rebuildingIndexMap){
          int steps=0;
          int maxSteps=batchSize;
          while(indexMapRebuildIndex<indexToSourceKey.Count&&steps<maxSteps){
           sourcesIndexMap[indexToSourceKey[indexMapRebuildIndex]]=indexMapRebuildIndex;
           indexMapRebuildIndex++;
           steps++;
          }
          if(indexMapRebuildIndex>=indexToSourceKey.Count){
           rebuildingIndexMap=false;
           return true;
          }
          return true;
         }
         if(appendingChunksSourcesEnumerator.MoveNext()){
          var cCoord=appendingChunksSourcesEnumerator.Current.Key;
          SourceKey key=new(cCoord);
          if(!sourcesIndexMap.ContainsKey(key)){
           sourcesIndexMap[key]=ordered.Count;
           indexToSourceKey.Add(key);
           ordered.Add(appendingChunksSourcesEnumerator.Current.Value);
          }
          return true;
         }
         if(appendingSimObjectsSourcesEnumerator.MoveNext()){
          Logs.Debug(()=>"'adding to ordered':"+appendingSimObjectsSourcesEnumerator.Current.Key.name);
          var gameObject=appendingSimObjectsSourcesEnumerator.Current.Key.GetEntityId();
          SourceKey key=new(gameObject);
          if(!sourcesIndexMap.ContainsKey(key)){
           sourcesIndexMap[key]=ordered.Count;
           indexToSourceKey.Add(key);
           //Logs.Debug(()=>"'sourceObject':"+appendingSimObjectsSourcesEnumerator.Current.Value.sourceObject);
           Logs.Debug(()=>"'transform':"+appendingSimObjectsSourcesEnumerator.Current.Value.transform);
           ordered.Add(appendingSimObjectsSourcesEnumerator.Current.Value);
           //Logs.Debug(()=>"'added to ordered':"+key.id);
          }
          return true;
         }
         appendingChunksSources.Clear();
         appendingSimObjectsSources.Clear();
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