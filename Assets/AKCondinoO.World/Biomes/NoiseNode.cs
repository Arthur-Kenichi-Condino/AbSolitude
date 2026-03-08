using LibNoise;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class NoiseNode:ScriptableObject{
     public NoiseNode[]inputs;
        public abstract ModuleBase Build(int seed);
    }
}