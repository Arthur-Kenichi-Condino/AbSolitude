#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     protected bool teleportedMove;
        internal override bool OnTeleportTo(Vector3 position,Quaternion rotation){
         if(navMeshAgent!=null&&navMeshAgent.enabled){
          if(NavMesh.SamplePosition(position,out NavMeshHit hitResult,Height,navMeshQueryFilter)){
           if(navMeshAgent.Warp(hitResult.position+Vector3.up*navMeshAgent.height/2f)){
            MoveStop();
            teleportedMove=true;
           }
           if(!navMeshAgent.isOnNavMesh){
            DisableNavMeshAgent();
           }
           //Log.DebugMessage("OnTeleportTo, navMeshAgent, position:"+position+", hitResult.position:"+hitResult.position+", transform.position:"+transform.position);
           return true;
          }
          //Log.DebugMessage("OnTeleportTo failed, navMeshAgent, position:"+position+", transform.position:"+transform.position);
          return false;
         }
         //Log.DebugMessage("OnTeleportTo success, position:"+position+", transform.position:"+transform.position);
         transform.position=position;
         teleportedMove=true;
         return true;
        }
    }
}