using LibNoise;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class NoiseNode:ScriptableObject{
     public int seedOffset;
        public abstract ModuleBase Build(int worldSeed);
        protected static int SeedHash(int seed,int id){
         unchecked{
          int h=seed;
          h=h*374761393+id;
          h=(h<<13)^h;
          return h*1274126177;
         }
        }
    }
}