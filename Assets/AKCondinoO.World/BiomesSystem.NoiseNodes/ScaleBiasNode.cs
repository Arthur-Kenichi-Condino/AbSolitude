using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/ScaleBias")]
    internal class ScaleBiasNode:OperatorNoiseNode{
     public NoiseNode input;
     public double scale;
     public double bias;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var operatorSnapshot=(OperatorNoiseNodesSnapshot)snapshot;
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