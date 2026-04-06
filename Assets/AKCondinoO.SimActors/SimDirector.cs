using AKCondinoO.Bootstrap;
using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System;
using UnityEngine;
using static AKCondinoO.SimObjects.SimObjectManager;
namespace AKCondinoO.SimActors{
    internal class SimDirector:MonoSingleton<SimDirector>{
     [SerializeField]private InteractablesRegistry[]interactablesRegistry;
        public override void Initialize(){
         base.Initialize();
         foreach(var r in interactablesRegistry){
          foreach(var interactableInteractions in r.interactables){
           interactableInteractions.Register();
          }
         }
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
        internal class SimSpawnJob:SpawnJob{
        }
    }
}