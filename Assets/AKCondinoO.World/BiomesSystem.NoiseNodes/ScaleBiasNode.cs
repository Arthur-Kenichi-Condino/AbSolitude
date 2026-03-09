using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/ScaleBias")]
    internal class ScaleBiasNode:NoiseNode{
     public NoiseNode input;
     public double scale;
     public double bias;
        public override ModuleBase Build(int worldSeed){
         return new ScaleBias(
          scale,
          bias,
          input.Build(worldSeed)
         );
        }
    }
}