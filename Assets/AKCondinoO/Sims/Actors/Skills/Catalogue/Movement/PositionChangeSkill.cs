#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class PositionChangeSkill:Skill{
     internal Vector3 targetDest;
        //  [https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html]
        protected static bool GetRandomPositionFor(SimObject simObject,Vector3 center,float maxDis,out Vector3 result){
         if(simObject is SimActor actor){
          for(int i=0;i<3;++i){
           Vector3 randomPoint=Util.GetRandomPosition(center,maxDis);
           if(NavMesh.SamplePosition(randomPoint,out NavMeshHit hit,Height,actor.navMeshQueryFilter)){
            result=hit.position;
            return true;
           }
          }
         }
         result=center;
         return false;
        }
    }
}