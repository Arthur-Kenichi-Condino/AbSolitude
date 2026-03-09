using LibNoise;
using LibNoise.Generator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Const")]
    internal class ConstNode:NoiseNode{
     public double value;
        public override ModuleBase Build(int worldSeed){
         return new Const(value);
        }
    }
}