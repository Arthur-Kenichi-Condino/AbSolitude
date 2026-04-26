using AKCondinoO.Utilities;
using LibNoise;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    internal abstract class Node:ScriptableObject{
     public NoiseChannel channel;
     public int seedOffset;
     public NodeMaterialTable[]materialTables;
     public BiomeSpawnTable[]spawnTables;
        public virtual void Build(int worldSeed,
         NoiseNodesSnapshot inherited,out NoiseNodesSnapshot snapshot,
         out NoiseNodesSnapshot root
        ){
         snapshot=CreateSnapshot();
         snapshot.SetFrom(this,inherited);
         ModuleBase module=CreateModule(worldSeed,snapshot);
         snapshot.SetModule(channel,module);
         root=snapshot.root;
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
    internal abstract class GraphNode:Node{
     public Node[]branches;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         GraphNodesSnapshot snapshot=(GraphNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(GraphNodesSnapshot));
         return snapshot;
        }
    }
    internal abstract class OperatorNoiseNode:Node{
     public Node input;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         OperatorNoiseNodesSnapshot snapshot=(OperatorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(OperatorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal abstract class SelectorNoiseNode:Node{
     public Node inputA;
     public Node inputB;
     public Node controller;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         SelectorNoiseNodesSnapshot snapshot=(SelectorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(SelectorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal abstract class MultiplierNoiseNode:Node{
     public Node lhs;
     public Node rhs;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         MultiplierNoiseNodesSnapshot snapshot=(MultiplierNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(MultiplierNoiseNodesSnapshot));
         return snapshot;
        }
    }
}