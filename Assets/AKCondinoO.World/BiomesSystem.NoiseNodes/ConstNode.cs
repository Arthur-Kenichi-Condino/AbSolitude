using LibNoise;
using LibNoise.Generator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Const")]
    internal class ConstNode:NoiseNode{
     public double value;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         return new Const(value);
        }
    }
}