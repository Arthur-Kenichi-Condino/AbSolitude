using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Node/Root")]
    internal class RootGraphNode:GraphNode{
     public GraphNode graph;
        internal override GraphNodeSnapshot DoSnapshot(int worldSeed,GraphNodeSnapshot parent){
         var snapshot=(RootGraphNodeSnapshot)base.DoSnapshot(worldSeed,parent);
         snapshot.graph=graph.DoSnapshot(worldSeed,snapshot);
         return snapshot;
        }
        protected override GraphNodeSnapshot CreateSnapshot(){
         RootGraphNodeSnapshot snapshot=(RootGraphNodeSnapshot)GraphNodeSnapshot.Rent(typeof(RootGraphNodeSnapshot));
         return snapshot;
        }
    }
    internal class RootGraphNodeSnapshot:GraphNodeSnapshot{
     internal GraphNodeSnapshot graph;
        protected override void OnReturnToPoolRecycle(){
         Return(graph.GetType(),graph);graph=null;
         base.OnReturnToPoolRecycle();
        }
        internal override void BuildSnapshotResolution(){
         graph.BuildSnapshotResolution();
         graph.PropagateSpawnSettings(spawnSettings);
        }
        internal override double GetValue(NoiseChannel channel,Vector3 noiseInput){
         return graph.GetValue(channel,noiseInput);
        }
        internal override SnapshotMaterialTable GetMaterialTable(NoiseChannel channel,Vector3 noiseInput){
         return graph.GetMaterialTable(channel,noiseInput);
        }
        internal override SnapshotBiomeSpawnTable GetBiomeSpawnTable(NoiseChannel channel,Vector3 noiseInput){
         return graph.GetBiomeSpawnTable(channel,noiseInput);
        }
    }
}