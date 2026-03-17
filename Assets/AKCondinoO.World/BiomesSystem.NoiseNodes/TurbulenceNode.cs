using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Turbulence")]
    internal class TurbulenceNode:OperatorNoiseNode{
     public double frequency;
     public double power;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var operatorSnapshot=(OperatorNoiseNodesSnapshot)snapshot;
         var inputModule=input.Build(
          worldSeed,
          operatorSnapshot,
          out var inputSnapshot,
          out _
         );
         operatorSnapshot.SetInput(inputSnapshot);
         int seed=SeedHash(worldSeed,seedOffset);
         var turbulence=new Turbulence(inputModule);
         turbulence.Frequency=frequency;
         turbulence.Power=power;
         turbulence.Seed=seed;
         return turbulence;
        }
    }
}