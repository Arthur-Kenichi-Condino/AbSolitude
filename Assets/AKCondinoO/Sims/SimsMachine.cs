#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimsMachine:MonoBehaviour,ISingletonInitialization{
     internal static SimsMachine singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimsMachine:OnDestroyingCoreEvent");
        }
     Vector3 mainCamPos,lastMainCamPos;
      bool initMainCamPos=true;
      Vector3Int mainCamGetCurrentBiomeInputRounded;
       Vector3 mainCamGetCurrentBiomeInput;
        Biomes mainCamGetCurrentBiomeOutput;
     readonly(Type simType,ulong number)idArthurCondino=(typeof(ArthurCondinoAI),0);
     float specificSpawnRequestsDelay=5f;
     float specificSpawnRequestsCooldown=5f;
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
        }
    }
}