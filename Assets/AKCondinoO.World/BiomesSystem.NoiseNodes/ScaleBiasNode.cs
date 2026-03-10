using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/ScaleBias")]
    internal class ScaleBiasNode:NoiseNode{
     public NoiseNode input;
     public double scale;
     public double bias;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         NoiseNodesSnapshotOperator snapshot=(NoiseNodesSnapshotOperator)NoiseNodesSnapshot.Rent((typeof(NoiseNodesSnapshotOperator),""));
         return snapshot;
        }
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var operatorSnapshot=(NoiseNodesSnapshotOperator)snapshot;
         var inputModule=input.Build(
          worldSeed,
          operatorSnapshot,
          out var inputSnapshot,
          out _
         );
         operatorSnapshot.SetInput(inputSnapshot);
         return new ScaleBias(
          scale,
          bias,
          inputModule
         );
        }
    }
}