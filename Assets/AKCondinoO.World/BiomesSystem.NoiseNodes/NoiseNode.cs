using AKCondinoO.Utilities;
using LibNoise;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class NoiseNode:ScriptableObject{
     public int seedOffset;
     public NodeMaterialTable[]materialTables;
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
    internal abstract class OperatorNoiseNode:NoiseNode{
     public NoiseNode input;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         OperatorNoiseNodesSnapshot snapshot=(OperatorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(OperatorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal abstract class SelectorNoiseNode:NoiseNode{
     public NoiseNode inputA;
     public NoiseNode inputB;
     public NoiseNode controller;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         SelectorNoiseNodesSnapshot snapshot=(SelectorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(SelectorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal abstract class MultiplierNoiseNode:NoiseNode{
     public NoiseNode lhs;
     public NoiseNode rhs;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         MultiplierNoiseNodesSnapshot snapshot=(MultiplierNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(MultiplierNoiseNodesSnapshot));
         return snapshot;
        }
    }
}