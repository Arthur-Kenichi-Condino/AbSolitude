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
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(38,.0005f);
        protected virtual void OnATTACK_ST(){
         if(
          IsTraversingPath()
         ){
          navMeshAgent.destination=navMeshAgent.transform.position;
         }else{
          if(characterController!=null){
           Vector3 lookDir=MyEnemy.transform.position-transform.position;
           Vector3 planarLookDir=lookDir;
           planarLookDir.y=0f;
           onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
           characterController.character.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
           Debug.DrawRay(characterController.character.transform.position,characterController.character.transform.forward,Color.gray);
           if(simUMA!=null){
            Vector3 animatorLookDir=-simUMA.transform.parent.forward;
            Vector3 animatorLookEuler=simUMA.transform.parent.eulerAngles;
            animatorLookEuler.y+=180f;
            Vector3 animatorPlanarLookEuler=animatorLookEuler;
            animatorPlanarLookEuler.x=0f;
            animatorPlanarLookEuler.z=0f;
            Vector3 animatorPlanarLookDir=Quaternion.Euler(animatorPlanarLookEuler)*Vector3.forward;
            Debug.DrawRay(characterController.character.transform.position,animatorPlanarLookDir,Color.white);
            if(Vector3.Angle(characterController.character.transform.forward,animatorPlanarLookDir)<=5f){
             DoAttack();
            }
           }
          }
         }
        }
    }
}