using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Turbulence")]
    internal class TurbulenceNode:NoiseNode{
     public NoiseNode input;
     public double frequency;
     public double power;
        public override ModuleBase Build(int worldSeed){
         var turbulence=new Turbulence(
          input.Build(worldSeed)
         );
         int seed=SeedHash(worldSeed,seedOffset);
         turbulence.Frequency=frequency;
         turbulence.Power=power;
         turbulence.Seed=seed;
         return turbulence;
        }
    }
}