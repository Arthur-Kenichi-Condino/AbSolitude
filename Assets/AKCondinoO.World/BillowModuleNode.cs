using LibNoise;
using LibNoise.Generator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Billow")]
    internal class BillowModuleNode:ModuleNode{
     public double frequency;
     public double lacunarity;
     public double persistence;
     public int octaves;
     public QualityMode quality;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         return new Billow(
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