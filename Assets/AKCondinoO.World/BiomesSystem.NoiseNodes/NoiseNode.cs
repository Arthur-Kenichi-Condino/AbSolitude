using AKCondinoO.Utilities;
using LibNoise;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class NoiseNode:ScriptableObject{
     public int seedOffset;
     public BiomeSpawnTable[]spawnTables;
        public virtual ModuleBase Build(int worldSeed,
         NoiseNodesSnapshot inherited,out NoiseNodesSnapshot snapshot,
         out NoiseNodesSnapshot root
        ){
         snapshot=CreateSnapshot();
         snapshot.SetFrom(this,inherited);
         ModuleBase module=CreateModule(worldSeed,snapshot);
         snapshot.SetModule(module);
         root=snapshot.root;
         return module;
        }
        protected virtual NoiseNodesSnapshot CreateSnapshot(){
         NoiseNodesSnapshot snapshot=NoiseNodesSnapshot.Rent(typeof(NoiseNodesSnapshot));
         return snapshot;
        }
        protected abstract ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot);
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