#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class CallSlaveSkill:Skill{
        internal static void SetHomunToBeGenerated(SimActor actor,SpawnData spawnData){
         spawnData.Clear();
         //  add data to spawn
         if(actor is BaseAI baseAI){
          foreach(var requiredSlavesList in baseAI.requiredSlaves){
           Type requiredSlaveType=requiredSlavesList.Key;
           List<SlaveData>requiredSlavesOfType=requiredSlavesList.Value;
           foreach(SlaveData requiredSlaveOfType in requiredSlavesOfType){
            //  TO DO: if already spawned, teleport
            spawnData.at.Add((actor.transform.position,actor.transform.rotation.eulerAngles,Vector3.one,requiredSlaveType,null,new SimObject.PersistentData()));
            //  TO DO: fill SimActorPersistentData
            spawnData.masters[spawnData.at.Count-1]=actor.id.Value;
           }
          }
          Log.DebugMessage(actor+":SetHomunToBeGenerated");
          foreach(var slaveId in baseAI.slaves){
           if(SimObjectManager.singleton.active.TryGetValue(slaveId,out SimObject slaveSimObject)){
            if(slaveSimObject.masterId!=actor.id){
             slaveSimObject.masterId=actor.id;
            }
            Log.DebugMessage("teleport to master:"+slaveSimObject);
            //  TO DO: teleport
           }else{
            spawnData.at.Add((actor.transform.position,actor.transform.rotation.eulerAngles,Vector3.one,slaveId.simObjectType,slaveId.idNumber,new SimObject.PersistentData()));
            //  TO DO: fill SimActorPersistentData
            spawnData.masters[spawnData.at.Count-1]=actor.id.Value;
           }
          }
         }
         actor.requiredSlaves.Clear();
        }
    }
}