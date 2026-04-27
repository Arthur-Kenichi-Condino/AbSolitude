using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/ScaleBias")]
    internal class ScaleBiasModuleNode:OperatorModuleNode{
     public double scale;
     public double bias;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var operatorSnapshot=(OperatorNoiseNodesSnapshot)snapshot;
         var inputSnapshot=input.DoSnapshot(worldSeed,operatorSnapshot);
         operatorSnapshot.input=inputSnapshot;
         var inputModule=inputSnapshot.module;
         return new ScaleBias(
          scale,
          bias,
          inputModule
         );
        }
    }
    internal abstract class OperatorModuleNode:ModuleNode{
     public ModuleNode input;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         OperatorNoiseNodesSnapshot snapshot=(OperatorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(OperatorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal class OperatorNoiseNodesSnapshot:NoiseNodesSnapshot{
     internal NoiseNodesSnapshot input;
        internal override void OnReturnToPoolRecycle(){
         NoiseNodesSnapshot.Return(input.GetType(),input);input=null;
         base.OnReturnToPoolRecycle();
        }
        internal override void MergeCollection(){
         input.MergeCollection();
        }
        internal override void SettingsPropagation(){
         input.SettingsPropagation();
         spawnSettings=SnapshotSpawnSettings.pool.Rent();
         input.PropagateSpawnSettings(spawnSettings);
        }
    }
}