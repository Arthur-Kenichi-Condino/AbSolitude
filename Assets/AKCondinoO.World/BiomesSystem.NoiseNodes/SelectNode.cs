using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Select")]
    internal class SelectNode:SelectorNoiseNode{
     public double min;
     public double max;
     public double fallOff;
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var selectorSnapshot=(SelectorNoiseNodesSnapshot)snapshot;
         controller.Build(
          worldSeed,
          selectorSnapshot,
          out var controllerSnapshot,
          out _
         );
         inputA.Build(
          worldSeed,
          controllerSnapshot,
          out var inputASnapshot,
          out _
         );
         inputB.Build(
          worldSeed,
          controllerSnapshot,
          out var inputBSnapshot,
          out _
         );
         var inputAModule=inputASnapshot.GetModule(channel);
         var inputBModule=inputBSnapshot.GetModule(channel);
         var controllerModule=controllerSnapshot.GetModule(channel);
         selectorSnapshot.SetInput(inputASnapshot,inputBSnapshot,controllerSnapshot);
         var select=new Select(inputAModule,inputBModule,controllerModule);
         select.SetBounds(min:min,max:max);
         select.FallOff=fallOff;
         return select;
        }
    }
}