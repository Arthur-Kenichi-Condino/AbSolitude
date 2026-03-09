using LibNoise;
using LibNoise.Generator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/RidgedMultifractal")]
    internal class RidgedMultifractalNode:NoiseNode{
     public double frequency;
     public double lacunarity;
     public int octaves;
     public QualityMode quality;
        public override ModuleBase Build(int worldSeed){
         return new RidgedMultifractal(
          frequency,
          lacunarity,
          octaves,
          SeedHash(worldSeed,seedOffset),
          quality
         );
        }
    }
}