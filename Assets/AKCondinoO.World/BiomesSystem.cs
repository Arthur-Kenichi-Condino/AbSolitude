using AKCondinoO.Bootstrap;
using AKCondinoO.World.Biomes;
using UnityEngine;
namespace AKCondinoO.World{
    internal class BiomesSystem:MonoSingleton<BiomesSystem>{
     [SerializeField]internal int seed=0;
     [SerializeField]internal float terrainSmoothingHeight=4f;
     [SerializeField]internal GraphNode biomeGraph;
     [SerializeField]private bool debugForceRebuild=false;
        public override void Initialize(){
         base.Initialize();
         BuildConfiguration();
         if(this!=null){
         }
        }
        public override void PreShutdown(){
         if(this!=null){
         }
         base.PreShutdown();
        }
        public override void Shutdown(){
         if(this!=null){
         }
         BiomesConfigurationSnapshot.DisposeAll();
         base.Shutdown();
        }
        internal void BuildConfiguration(){
         BiomesConfigurationSnapshot.Build();
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(debugForceRebuild){
          debugForceRebuild=false;
          BuildConfiguration();
          WorldChunkManager.singleton.debugForceRegenerate=true;
         }
        }
    }
}