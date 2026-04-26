using LibNoise;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Node/BiomeGraph")]
    internal class BiomeGraphNode:GraphNode{
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         return null;
        }
        public override void Build(int worldSeed,NoiseNodesSnapshot inherited,out NoiseNodesSnapshot snapshot,out NoiseNodesSnapshot root){
         var graphNodesSnapshot=(GraphNodesSnapshot)CreateSnapshot();
         snapshot=graphNodesSnapshot;
         snapshot.SetFrom(this,inherited);
         foreach(var branch in branches){
          branch.Build(
           worldSeed,
           snapshot,
           out var branchSnapshot,
           out _
          );
          snapshot.SetModule(branch.channel,branchSnapshot.GetModule(branch.channel));
          graphNodesSnapshot.branches.Add(branch.channel,branchSnapshot);
         }
         root=graphNodesSnapshot.root;
        }
    }
}