using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Multiply")]
    internal class MultiplyNode:MultiplierNoiseNode{
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var multiplierSnapshot=(MultiplierNoiseNodesSnapshot)snapshot;
         rhs.Build(
          worldSeed,
          multiplierSnapshot,
          out var rhsSnapshot,
          out _
         );
         lhs.Build(
          worldSeed,
          rhsSnapshot,
          out var lhsSnapshot,
          out _
         );
         var rhsModule=rhsSnapshot.GetModule(channel);
         var lhsModule=lhsSnapshot.GetModule(channel);
         multiplierSnapshot.SetInput(lhsSnapshot,rhsSnapshot);
         return new Multiply(
          lhsModule,rhsModule
         );
        }
    }
}