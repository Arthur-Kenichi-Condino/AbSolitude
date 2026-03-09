using LibNoise;
using LibNoise.Generator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Perlin")]
    internal class PerlinNode:NoiseNode{
     public double frequency;
     public double lacunarity;
     public double persistence;
     public int octaves;
     public QualityMode quality;
        public override ModuleBase Build(int worldSeed){
         return new Perlin(
          frequency,
          lacunarity,
          persistence,
          octaves,
          SeedHash(worldSeed,seedOffset),
          quality
         );
        }
    }
}