using AKCondinoO.Utilities;
using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Turbulence")]
    internal class TurbulenceModuleNode:OperatorModuleNode{
     public double frequency;
     public double power;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var operatorSnapshot=(OperatorNoiseNodesSnapshot)snapshot;
         var inputSnapshot=input.DoSnapshot(worldSeed,operatorSnapshot);
         var inputModule=inputSnapshot.module;
         operatorSnapshot.input=inputSnapshot;
         int seed=SeedHash(worldSeed,seedOffset);
         var turbulence=new Turbulence(inputModule);
         turbulence.Frequency=frequency;
         turbulence.Power=power;
         turbulence.Seed=seed;
         return turbulence;
        }
    }
}