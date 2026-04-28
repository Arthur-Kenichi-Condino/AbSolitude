using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class GraphNode:ScriptableObject{
        internal virtual GraphNodeSnapshot DoSnapshot(int worldSeed,GraphNodeSnapshot parent){
         var snapshot=CreateSnapshot();
         snapshot.parent=parent;
         return snapshot;
        }
        protected virtual GraphNodeSnapshot CreateSnapshot(){
         GraphNodeSnapshot snapshot=GraphNodeSnapshot.Rent(typeof(GraphNodeSnapshot));
         return snapshot;
        }
    }
    internal class GraphNodeSnapshot{
     static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new(){
      {(typeof(      GraphNodeSnapshot),""),Pool.GetPool<      GraphNodeSnapshot>("",()=>new(),(      GraphNodeSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(  RootGraphNodeSnapshot),""),Pool.GetPool<  RootGraphNodeSnapshot>("",()=>new(),(  RootGraphNodeSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof(SelectGraphNodeSnapshot),""),Pool.GetPool<SelectGraphNodeSnapshot>("",()=>new(),(SelectGraphNodeSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
      {(typeof( BiomeGraphNodeSnapshot),""),Pool.GetPool< BiomeGraphNodeSnapshot>("",()=>new(),( BiomeGraphNodeSnapshot item)=>{item.OnReturnToPoolRecycle();},false)},
     };
        internal static GraphNodeSnapshot Rent(Type poolId){
         return(GraphNodeSnapshot)pool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,GraphNodeSnapshot snapshot){
         pool[(poolId,"")].ObjectReturn(snapshot);
        }
     internal GraphNodeSnapshot parent;
     internal readonly Dictionary<NoiseChannel,SnapshotSpawnSettings>spawnSettings=new();
        protected virtual void OnReturnToPoolRecycle(){
         foreach(var kvp in spawnSettings){
          var spawnSettings=kvp.Value;
          SnapshotSpawnSettings.pool.Return(spawnSettings);
         }
         spawnSettings.Clear();
         parent=null;
        }
        internal virtual void BuildSnapshotResolution(){
        }
        internal void PropagateSpawnSettings(Dictionary<NoiseChannel,SnapshotSpawnSettings>target){
         foreach(var kvp in spawnSettings){
          var channel=kvp.Key;
          var settings=kvp.Value;
          if(target.TryGetValue(channel,out var t)){
           t.MergeFrom(settings);
           SnapshotSpawnSettings.pool.Return(settings);
          }else{
           target[channel]=settings;
          }
         }
         spawnSettings.Clear();
        }
        internal virtual double GetValue(NoiseChannel channel,Vector3 noiseInput){
         return 0d;
        }
        internal virtual SnapshotMaterialTable GetMaterialTable(NoiseChannel channel,Vector3 noiseInput){
         return null;
        }
        internal virtual SnapshotSpawnSettings GetSpawnSettings(NoiseChannel channel){
         if(this.spawnSettings.TryGetValue(channel,out var spawnSettings)){
          return spawnSettings;
         }
         return null;
        }
        internal virtual SnapshotBiomeSpawnTable GetBiomeSpawnTable(NoiseChannel channel,Vector3 noiseInput){
         return null;
        }
    }
}