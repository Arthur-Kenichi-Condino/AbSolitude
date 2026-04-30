using LibNoise;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Node/Biome")]
    internal class BiomeGraphNode:GraphNode{
     public ModuleNode[]configs;
        internal override GraphNodeSnapshot DoSnapshot(int worldSeed,GraphNodeSnapshot parent){
         var snapshot=(BiomeGraphNodeSnapshot)base.DoSnapshot(worldSeed,parent);
         snapshot.BuildSnapshotConfigs(worldSeed,configs);
         return snapshot;
        }
        protected override GraphNodeSnapshot CreateSnapshot(){
         BiomeGraphNodeSnapshot snapshot=(BiomeGraphNodeSnapshot)GraphNodeSnapshot.Rent(typeof(BiomeGraphNodeSnapshot));
         return snapshot;
        }
    }
    internal class BiomeGraphNodeSnapshot:GraphNodeSnapshot{
     internal readonly Dictionary<NoiseChannel,NoiseNodesSnapshot>configs=new();
        protected override void OnReturnToPoolRecycle(){
         foreach(var kvp in configs){
          var config=kvp.Value;
          NoiseNodesSnapshot.Return(config.GetType(),config);
         }
         configs.Clear();
         base.OnReturnToPoolRecycle();
        }
        internal void BuildSnapshotConfigs(int worldSeed,ModuleNode[]configs){
         foreach(var config in configs){
          var channel=config.channel;
          var snapshot=config.DoSnapshot(worldSeed,null);
          this.configs.Add(channel,snapshot);
         }
        }
        internal override void BuildSnapshotResolution(){
         foreach(var kvp in configs){
          var config=kvp.Value;
          config.MergeCollection();
          config.SettingsPropagation();
          var spawnSettings=SnapshotSpawnSettings.pool.Rent();
          config.PropagateSpawnSettings(spawnSettings);
          this.spawnSettings.Add(kvp.Key,spawnSettings);
         }
        }
        internal override double GetValue(NoiseChannel channel,Vector3 noiseInput){
         if(configs.TryGetValue(channel,out var noiseSnapshot)){
          return noiseSnapshot.module.GetValue(noiseInput);
         }
         return 0d;
        }
        internal override SnapshotMaterialTable GetMaterialTable(NoiseChannel channel,Vector3 noiseInput){
         if(configs.TryGetValue(channel,out var noiseSnapshot)){
          return noiseSnapshot.GetMaterialTable(noiseInput);
         }
         return null;
        }
        internal override SnapshotBiomeSpawnTable GetBiomeSpawnTable(NoiseChannel channel,Vector3 noiseInput){
         if(configs.TryGetValue(channel,out var noiseSnapshot)){
          return noiseSnapshot.GetBiomeSpawnTable(noiseInput);
         }
         return null;
        }
    }
}