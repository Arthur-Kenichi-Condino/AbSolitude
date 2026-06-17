using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using static AKCondinoO.PhysicsUtil;
using static AKCondinoO.World.SimObjects.ChunkSimObjectSpawner.BiomesSimObjectSpawnerJob;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal class WorldSimObjectSpatialMap:MonoSingleton<WorldSimObjectSpatialMap>{
     private static readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
     internal readonly Dictionary<int,HashSet<OrientedBounds>>registering=new();
     private static readonly ConcurrentDictionary<Vector2Int,SpawnSpatialMap>spawnSpatialMapsBycCoord=new();
        internal static void RegisterSimObjectSpawn(int layer,Vector2Int cCoord,SpawnReserve spawn){
         //Logs.Debug(()=>"spawn:"+spawn.pos);
         var ownercCoord=cCoord;
         var entry=SpawnSpatialEntry.pool.Rent();
         entry.layer=layer;
         entry.obb=spawn.obb;
         entry.ownercCoord=ownercCoord;
         Bounds worldBounds=spawn.obb.ToAABB();
         Vector2Int minChunk=vecPosTocCoord(worldBounds.min);
         Vector2Int maxChunk=vecPosTocCoord(worldBounds.max);
         Vector2Int minOffset=minChunk-cCoord;
         Vector2Int maxOffset=maxChunk-cCoord;
         for(int x=minOffset.x;x<=maxOffset.x;x++){
         for(int z=minOffset.y;z<=maxOffset.y;z++){
          Vector2Int cCoord2=cCoord+new Vector2Int(x,z);
          entry.occupiedcCoords.Add(cCoord2);
          GetOrAddSpawnSpatialMap(cCoord2);
         }}
         bool registered=false;
         rwl.EnterWriteLock();
         try{
          var ownerMap=GetOrAddSpawnSpatialMap(ownercCoord);
          if(!(registered=ownerMap.TryRegister(layer,spawn,entry))){
           goto _End;
          }
          for(int x=minOffset.x;x<=maxOffset.x;x++){
          for(int z=minOffset.y;z<=maxOffset.y;z++){
           Vector2Int cCoord2=cCoord+new Vector2Int(x,z);
           var map=GetOrAddSpawnSpatialMap(cCoord2);
           map.Add(layer,spawn,entry);
          }}
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
         _End:{}
         if(!registered){
          SpawnSpatialEntry.pool.Return(entry);
         }
        }
     internal static readonly Utilities.ObjectPool<HashSet<SpawnSpatialEntry>>visitedPool=
      Pool.GetPool<HashSet<SpawnSpatialEntry>>(
       "",()=>new(),(HashSet<SpawnSpatialEntry>item)=>{item.Clear();}
      );
        internal static bool HasSpawnConflicts(int layer,Vector2Int cCoord,SpawnCandidate candidate){
         Bounds worldBounds=candidate.obb.ToAABB();
         Vector2Int minChunk=vecPosTocCoord(worldBounds.min);
         Vector2Int maxChunk=vecPosTocCoord(worldBounds.max);
         Vector2Int minOffset=minChunk-cCoord;
         Vector2Int maxOffset=maxChunk-cCoord;
         HashSet<SpawnSpatialEntry>visited=visitedPool.Rent();
         rwl.EnterReadLock();
         try{
          for(int x=minOffset.x;x<=maxOffset.x;x++){
          for(int z=minOffset.y;z<=maxOffset.y;z++){
           Vector2Int cCoord2=cCoord+new Vector2Int(x,z);
           if(!spawnSpatialMapsBycCoord.TryGetValue(cCoord2,out var map)){
            continue;
           }
           foreach(var kvp in map.occupantsByLayer){
            int otherLayer=kvp.Key;
            if(otherLayer>=layer)continue;
            foreach(var entry in kvp.Value.Values){
             if(!visited.Add(entry))continue;
             if(candidate.obb.Intersects(entry.obb)){
              return true;
             }
            }
           }
          }}
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitReadLock();
          visitedPool.Return(visited);
         }
         return false;
        }
        private static SpawnSpatialMap GetOrAddSpawnSpatialMap(Vector2Int cCoord){
         SpawnSpatialMap newMap=null;
         var map=spawnSpatialMapsBycCoord.GetOrAdd(
          cCoord,
          _=>{
           newMap=SpawnSpatialMap.pool.Rent();
           newMap.cCoord=cCoord;
           return newMap;
          }
         );
         if(newMap!=null&&!ReferenceEquals(map,newMap)){
          SpawnSpatialMap.pool.Return(newMap);
         }
         return map;
        }
    }
    internal class SpawnSpatialMap{
     internal static readonly Utilities.ObjectPool<SpawnSpatialMap>pool=
      Pool.GetPool<SpawnSpatialMap>(
       "",
       ()=>new(),
       (SpawnSpatialMap item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     internal Vector2Int cCoord;
     internal readonly Dictionary<int,HashSet<OrientedBounds>>registered=new();
     internal readonly Dictionary<int,Dictionary<OrientedBounds,SpawnSpatialEntry>>occupantsByLayer=new();
        internal void OnReturnToPoolRecycle(){
         foreach(var kvp1 in occupantsByLayer){
          foreach(var kvp2 in kvp1.Value){
           if(kvp2.Value.ownercCoord==cCoord){
            SpawnSpatialEntry.pool.Return(kvp2.Value);
           }
          }
          kvp1.Value.Clear();
         }
         foreach(var kvp1 in registered){
          kvp1.Value.Clear();
         }
         cCoord=default;
        }
        internal bool TryRegister(int layer,SpawnReserve spawn,SpawnSpatialEntry entry){
         if(!registered.TryGetValue(layer,out var registrations)){
          registered.Add(layer,registrations=new());
         }
         bool result=registrations.Add(spawn.obb);
         return result;
        }
        internal void Add(int layer,SpawnReserve spawn,SpawnSpatialEntry entry){
         if(!occupantsByLayer.TryGetValue(layer,out var entries)){
          occupantsByLayer.Add(layer,entries=new());
         }
         entries.Add(spawn.obb,entry);
        }
    }
    internal class SpawnSpatialEntry{
     internal static readonly Utilities.ObjectPool<SpawnSpatialEntry>pool=
      Pool.GetPool<SpawnSpatialEntry>(
       "",
       ()=>new(),
       (SpawnSpatialEntry item)=>{
        item.OnReturnToPoolRecycle();
       }
      );
     internal int layer;
     internal OrientedBounds obb;
     internal Vector2Int ownercCoord;
     internal readonly HashSet<Vector2Int>occupiedcCoords=new();
        internal void OnReturnToPoolRecycle(){
         occupiedcCoords.Clear();
         ownercCoord=default;
         obb=default;
         layer=default;
        }
    }
}