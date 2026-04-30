using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Select")]
    internal class SelectModuleNode:SelectorModuleNode{
     public double min;
     public double max;
     public double fallOff;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var selectorSnapshot=(SelectorNoiseNodesSnapshot)snapshot;
         var controllerSnapshot=controller.DoSnapshot(worldSeed,selectorSnapshot);
         var inputASnapshot=inputA?.DoSnapshot(worldSeed,controllerSnapshot);
         var inputBSnapshot=inputB?.DoSnapshot(worldSeed,controllerSnapshot);
         var inputAModule=inputASnapshot?.module??new Const(0.0);
         var inputBModule=inputBSnapshot?.module??new Const(1.0);
         var controllerModule=controllerSnapshot.module;
         selectorSnapshot.inputA=inputASnapshot;
         selectorSnapshot.inputB=inputBSnapshot;
         selectorSnapshot.controller=controllerSnapshot;
         var select=new Select(inputAModule,inputBModule,controllerModule);
         select.SetBounds(min:min,max:max);
         select.FallOff=fallOff;
         return select;
        }
    }
    internal abstract class SelectorModuleNode:ModuleNode{
     public ModuleNode inputA;
     public ModuleNode inputB;
     public ModuleNode controller;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         SelectorNoiseNodesSnapshot snapshot=(SelectorNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(SelectorNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal class SelectorNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void OnReturnToPoolRecycle(){
         if(inputA!=null){NoiseNodesSnapshot.Return(inputA.GetType(),inputA);inputA=null;}
         if(inputB!=null){NoiseNodesSnapshot.Return(inputB.GetType(),inputB);inputB=null;}
         NoiseNodesSnapshot.Return(controller.GetType(),controller);controller=null;
         base.OnReturnToPoolRecycle();
        }
     internal NoiseNodesSnapshot inputA;
     internal NoiseNodesSnapshot inputB;
     internal NoiseNodesSnapshot controller;
        internal override void MergeCollection(){
         inputA?.MergeCollection();
         inputB?.MergeCollection();
        }
        internal override void SettingsPropagation(){
         inputA?.SettingsPropagation();
         inputB?.SettingsPropagation();
         spawnSettings=SnapshotSpawnSettings.pool.Rent();
         if(inputA!=null){inputA.PropagateSpawnSettings(spawnSettings);}
         if(inputB!=null){inputB.PropagateSpawnSettings(spawnSettings);}
        }
        internal override SnapshotMaterialTable GetMaterialTable(Vector3 noiseInput){
         return SelectNode(noiseInput).GetMaterialTable(noiseInput);
        }
        internal override SnapshotBiomeSpawnTable GetBiomeSpawnTable(Vector3 noiseInput){
         return SelectNode(noiseInput).GetBiomeSpawnTable(noiseInput);
        }
        private NoiseNodesSnapshot SelectNode(Vector3 noiseInput){
         var cv=controller.module.GetValue(noiseInput);
         var select=(Select)module;
         var max=select.Maximum;
         var min=select.Minimum;
         if(cv<min||cv>max){
          return inputA;
         }
         return inputB;
        }
    }
}