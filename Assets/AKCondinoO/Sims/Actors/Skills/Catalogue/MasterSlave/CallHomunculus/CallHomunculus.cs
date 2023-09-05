#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class CallHomunculus:CallSlaveSkill{
        internal override bool IsAvailable(SimObject target,int useLevel){
         if(base.IsAvailable(target,useLevel)){
          //  do more tests here
          return true;
         }
         //  oops, it's not the time to use the skill, and no more tests required
         return false;
        }
        internal override bool DoSkill(SimObject target,int useLevel){
         if(base.DoSkill(target,useLevel)){
          //  do any other skill setting needed here
          GenerateHomunculus.SetHomunToBeGenerated(actor,spawnData);
          return true;
         }
         //  the skill cannot be used!
         return false;
        }
     readonly SpawnData spawnData=new SpawnData();
    }
}