using AKCondinoO.Utilities;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Node/Select")]
    internal class SelectGraphNode:GraphNode{
     public double min;
     public double max;
     public double fallOff;
     public GraphNode inputA;
     public GraphNode inputB;
     public ModuleNode controller;
        internal override GraphNodeSnapshot DoSnapshot(int worldSeed,GraphNodeSnapshot parent){
         var snapshot=(SelectGraphNodeSnapshot)base.DoSnapshot(worldSeed,parent);
         snapshot.min=min;
         snapshot.max=max;
         snapshot.fallOff=fallOff;
         snapshot.inputA=inputA.DoSnapshot(worldSeed,snapshot);
         snapshot.inputB=inputB.DoSnapshot(worldSeed,snapshot);
         snapshot.select=controller.DoSnapshot(worldSeed,null);
         return snapshot;
        }
        protected override GraphNodeSnapshot CreateSnapshot(){
         SelectGraphNodeSnapshot snapshot=(SelectGraphNodeSnapshot)GraphNodeSnapshot.Rent(typeof(SelectGraphNodeSnapshot));
         return snapshot;
        }
    }
    internal class SelectGraphNodeSnapshot:GraphNodeSnapshot{
     internal double min;
     internal double max;
     internal double fallOff;
     internal GraphNodeSnapshot inputA;
     internal GraphNodeSnapshot inputB;
     internal NoiseNodesSnapshot select;
        protected override void OnReturnToPoolRecycle(){
         min=0;
         max=0;
         fallOff=0;
         Return(inputA.GetType(),inputA);inputA=null;
         Return(inputB.GetType(),inputB);inputB=null;
         NoiseNodesSnapshot.Return(select.GetType(),select);select=null;
         base.OnReturnToPoolRecycle();
        }
        internal override void BuildSnapshotResolution(){
         inputA.BuildSnapshotResolution();
         inputB.BuildSnapshotResolution();
         inputA.PropagateSpawnSettings(spawnSettings);
         inputB.PropagateSpawnSettings(spawnSettings);
        }
        internal override double GetValue(NoiseChannel channel,Vector3 noiseInput){
         var select=((SelectorNoiseNodesSnapshot)this.select);
         double mask=((Select)select.module).GetValue(noiseInput);
         double a=inputA.GetValue(channel,noiseInput);
         double b=inputB.GetValue(channel,noiseInput);
         return Lerp(a,b,mask);
        }
        private static double Lerp(double a,double b,double t){
         return a+(b-a)*t;
        }
        internal override SnapshotMaterialTable GetMaterialTable(NoiseChannel channel,Vector3 noiseInput){
         return SelectNode(noiseInput).GetMaterialTable(channel,noiseInput);
        }
        internal override SnapshotBiomeSpawnTable GetBiomeSpawnTable(NoiseChannel channel,Vector3 noiseInput){
         return SelectNode(noiseInput).GetBiomeSpawnTable(channel,noiseInput);
        }
        private GraphNodeSnapshot SelectNode(Vector3 noiseInput){
         var cv=((SelectorNoiseNodesSnapshot)this.select).controller.module.GetValue(noiseInput);
         var select=(Select)this.select.module;
         var max=select.Maximum;
         var min=select.Minimum;
         if(cv<min||cv>max){
          return inputA;
         }
         return inputB;
        }
    }
}