using AKCondinoO.Bootstrap;
using UnityEngine;
namespace AKCondinoO.SimActors{
    internal class SimDirector:MonoSingleton<SimDirector>{
        public override void Initialize(){
         base.Initialize();
        }
        public override void PreShutdown(){
         base.PreShutdown();
        }
        public override void Shutdown(){
         base.Shutdown();
        }
        void EnsureActiveSimExists(){
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(ActiveZone.main!=null){
          EnsureActiveSimExists();
         }
        }
    }
}