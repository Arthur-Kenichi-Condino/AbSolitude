using LibNoise;
using LibNoise.Operator;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/Noise/Multiply")]
    internal class MultiplyModuleNode:MultiplierModuleNode{
        protected override ModuleBase CreateModule(int worldSeed,NoiseNodesSnapshot snapshot){
         var multiplierSnapshot=(MultiplierNoiseNodesSnapshot)snapshot;
         var rhsSnapshot=rhs.DoSnapshot(worldSeed,multiplierSnapshot);
         var lhsSnapshot=lhs.DoSnapshot(worldSeed,rhsSnapshot);
         var rhsModule=rhsSnapshot.module;
         var lhsModule=lhsSnapshot.module;
         multiplierSnapshot.SetInput(lhsSnapshot,rhsSnapshot);
         return new Multiply(
          lhsModule,rhsModule
         );
        }
    }
    internal abstract class MultiplierModuleNode:ModuleNode{
     public ModuleNode lhs;
     public ModuleNode rhs;
        protected override NoiseNodesSnapshot CreateSnapshot(){
         MultiplierNoiseNodesSnapshot snapshot=(MultiplierNoiseNodesSnapshot)NoiseNodesSnapshot.Rent(typeof(MultiplierNoiseNodesSnapshot));
         return snapshot;
        }
    }
    internal class MultiplierNoiseNodesSnapshot:NoiseNodesSnapshot{
        internal override void OnReturnToPoolRecycle(){
         NoiseNodesSnapshot.Return(lhs.GetType(),lhs);lhs=null;
         NoiseNodesSnapshot.Return(rhs.GetType(),rhs);rhs=null;
         base.OnReturnToPoolRecycle();
        }
     public NoiseNodesSnapshot lhs;
     public NoiseNodesSnapshot rhs;
        internal virtual void SetInput(
         NoiseNodesSnapshot lhs,
         NoiseNodesSnapshot rhs
        ){
         this.lhs=lhs;
         this.rhs=rhs;
        }
        internal override void MergeCollection(){
         lhs.MergeCollection();
         rhs.MergeCollection();
        }
        internal override void SettingsPropagation(){
         lhs.SettingsPropagation();
         rhs.SettingsPropagation();
         spawnSettings=SnapshotSpawnSettings.pool.Rent();
         lhs.PropagateSpawnSettings(spawnSettings);
         rhs.PropagateSpawnSettings(spawnSettings);
        }
    }
}