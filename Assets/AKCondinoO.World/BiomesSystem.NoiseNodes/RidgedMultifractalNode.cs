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
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
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