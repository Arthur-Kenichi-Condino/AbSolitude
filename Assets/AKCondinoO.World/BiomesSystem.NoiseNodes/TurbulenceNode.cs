using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Turbulence")]
    internal class TurbulenceNode:NoiseNode{
     public NoiseNode input;
     public double frequency;
     public double power;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         NoiseNodesSnapshotOperator snapshot=(NoiseNodesSnapshotOperator)NoiseNodesSnapshot.Rent(typeof(NoiseNodesSnapshotOperator));
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
         int seed=SeedHash(worldSeed,seedOffset);
         var turbulence=new Turbulence(inputModule);
         turbulence.Frequency=frequency;
         turbulence.Power=power;
         turbulence.Seed=seed;
         return turbulence;
        }
    }
}