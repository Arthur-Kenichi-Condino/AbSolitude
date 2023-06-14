#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Humanoid;
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimsMachine:MonoBehaviour,ISingletonInitialization{
     internal static SimsMachine singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Biomes,SimsMachineSpawnSettings>spawnSettingsByBiome=new Dictionary<Biomes,SimsMachineSpawnSettings>();
        public void Init(){
           SetDefaultSpawnSettings();
         SetWastelandSpawnSettings();
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimsMachine:OnDestroyingCoreEvent");
        }
     internal readonly Dictionary<(Type simType,ulong number),SimActor>actors=new Dictionary<(Type,ulong),SimActor>();
        internal void OnActorSpawn(SimActor simActor){
         Type simType=simActor.id.Value.simObjectType;
         if(spawnControl.TryGetValue(simType,out HashSet<(Type simType,ulong number)>spawned)){
          spawned.Add(simActor.id.Value);
          Log.DebugMessage(simType+" actor has been spawned;count:"+spawned.Count);
         }
         actors.Add(simActor.id.Value,simActor);
        }
        internal void OnActorDespawn(SimActor simActor){
         Type simType=simActor.id.Value.simObjectType;
         if(spawnControl.TryGetValue(simType,out HashSet<(Type simType,ulong number)>spawned)){
          spawned.Remove(simActor.id.Value);
          Log.DebugMessage(simType+" actor has been despawned;count:"+spawned.Count);
         }
         actors.Remove(simActor.id.Value);
        }
     Vector3 mainCamPos,lastMainCamPos;
      bool initMainCamPos=true;
      Vector3Int mainCamGetCurrentBiomeInputRounded;
       Vector3 mainCamGetCurrentBiomeInput;
        Biomes mainCamGetCurrentBiomeOutput;
     readonly(Type simType,ulong number)idArthurCondino=(typeof(ArthurCondinoAI),0);
     float spawningRequestsDelay=1f;
      float spawningRequestsCooldown=10f;
      float specificSpawnRequestsDelay=10f;
       float specificSpawnRequestsCooldown=10f;
        void Update(){
         if(initMainCamPos|(lastMainCamPos=mainCamPos)!=(mainCamPos=MainCamera.singleton.transform.position)){
          if(initMainCamPos){
           Log.DebugMessage("SimsMachine:mainCamPos init");
           lastMainCamPos=mainCamPos;
           initMainCamPos=false;
          }
          Log.DebugMessage("SimsMachine:mainCamPos Update");
          mainCamGetCurrentBiomeInputRounded=new Vector3Int(
           Mathf.RoundToInt(mainCamPos.x),
           Mathf.RoundToInt(mainCamPos.y),
           Mathf.RoundToInt(mainCamPos.z)
          );
          mainCamGetCurrentBiomeInput=mainCamGetCurrentBiomeInputRounded+VoxelSystem.biome.deround;
          mainCamGetCurrentBiomeOutput=VoxelSystem.biome.GetCurrent(mainCamGetCurrentBiomeInput);
          Log.DebugMessage("SimsMachine:mainCam Current Biome:"+mainCamGetCurrentBiomeOutput);
         }
         if(specificSpawnRequestsCooldown>0f){
            specificSpawnRequestsCooldown-=Core.magicDeltaTimeNumber;
         }
         if(specificSpawnRequestsCooldown<=0f){
            specificSpawnRequestsCooldown=specificSpawnRequestsDelay;
          if(Core.singleton.isServer){
           if(!SimObjectManager.singleton.active.ContainsKey(idArthurCondino)){
            Log.DebugMessage("SimsMachine:call to current location:idArthurCondino:"+idArthurCondino);
            SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(idArthurCondino,MainCamera.singleton.transform.position,Vector3.zero,Vector3.one);
           }
          }
         }
         if(spawningRequestsCooldown>0f){
            spawningRequestsCooldown-=Core.magicDeltaTimeNumber;
         }
         if(spawningRequestsCooldown<=0f){
            spawningRequestsCooldown=spawningRequestsDelay;
          switch(mainCamGetCurrentBiomeOutput){
           case Biomes.Wasteland:{
            OnWastelandSpawning();
            break;
           }
           default:{
              OnDefaultSpawning();
            break;
           }
          }
         }
        }
     internal readonly Dictionary<Type,HashSet<(Type simType,ulong number)>>spawnControl=new Dictionary<Type,HashSet<(Type simType,ulong number)>>();
        internal void SetDefaultSpawnSettings(){
         Log.DebugMessage("SetDefaultSpawnSettings()");
         spawnSettingsByBiome.Add(Biomes.Default,
          new SimsMachineSpawnSettings{
           spawns=new Dictionary<Type,SimsMachineSpawnSettings.SimObjectSettings>{
            {
             typeof(DisfiguringHomunculusAI),
              new SimsMachineSpawnSettings.SimObjectSettings{
               count=5,
               chance=1.0f,
              }
            },
           },
          }
         );
         spawnControl.Add(typeof(DisfiguringHomunculusAI),new HashSet<(Type simType,ulong number)>());
        }
        internal void OnDefaultSpawning(){
         //Log.DebugMessage("OnDefaultSpawning()");
         if(spawnSettingsByBiome.TryGetValue(Biomes.Default,out SimsMachineSpawnSettings spawnSettings)){
          foreach(var kvp in spawnSettings.spawns){
           Type simType=kvp.Key;
           SimsMachineSpawnSettings.SimObjectSettings simSpawnSettings=kvp.Value;
           if(spawnControl.TryGetValue(simType,out HashSet<(Type simType,ulong number)>spawned)){
            if(spawned.Count<simSpawnSettings.count){
             Log.DebugMessage("OnDefaultSpawning: needs new spawn of simType:"+simType);
             for(int i=0;i<simSpawnSettings.count;++i){
              (Type simType,ulong number)id=(simType,(ulong)i);
              if(!spawned.Contains(id)){
               Vector3 randomPoint=Util.GetRandomPosition(MainCamera.singleton.transform.position,48.0f);
               SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(id,randomPoint,Vector3.zero,Vector3.one);
              }
             }
            }
           }
          }
         }
        }
    }
}