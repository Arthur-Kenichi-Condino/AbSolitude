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
        protected virtual void OnStartUsingAI(){
         if(characterController!=null){
          transform.rotation=characterController.character.transform.rotation;
          characterController.character.transform.rotation=transform.rotation;
          characterController.isAiming=false;
         }
        }
        protected virtual void OnStopUsingAI(){
         if(characterController!=null){
          transform.rotation=characterController.character.transform.rotation;
          characterController.character.transform.rotation=transform.rotation;
         }
        }
     [SerializeField]internal bool sniper=false;
     [SerializeField]internal QuaternionRotLerpHelper aiRotTurnTo=new QuaternionRotLerpHelper(20,1.25f);
        internal partial class AI{
         BaseAI me;
            internal AI(BaseAI me){
             this.me=me;
              snipeSt=new  SNIPE_ST(me,this);
             attackSt=new ATTACK_ST(me,this);
              chaseSt=new  CHASE_ST(me,this);
            }
            internal class ST{
             protected BaseAI me;
             protected AI ai;
              protected State MyState{get{return ai.MyState;}}
              protected SimObject MyEnemy{get{return ai.MyEnemy;}}
                internal ST(BaseAI me,AI ai){
                 this.me=me;
                 this.ai=ai;
                }
            }
          bool isInAttackRange          =false;
          bool isInAttackRangeWithWeapon=false;
          Vector3 attackDistance          ;
          Vector3 attackDistanceWithWeapon;
            internal virtual void main(){
             //  ajustar corpo para virar para direção da mira com menos diferença de rotação
             //  detecção de objetos com mira: ignorar objeto morto ou desativar volume de objeto morto
             me.RenewTargets();
             me.stopPathfindingOnTimeout=true;
             //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
             isInAttackRange          =false;
             isInAttackRangeWithWeapon=false;
             if(MyEnemy!=null){
              isInAttackRange          =me.IsInAttackRange(MyEnemy,out attackDistance               );
              isInAttackRangeWithWeapon=me.IsInAttackRange(MyEnemy,out attackDistanceWithWeapon,true);
             }else{
              attackDistance          =me.AttackDistance(    );
              attackDistanceWithWeapon=me.AttackDistance(true);
             }
             UpdateMyState();
             ProcessStateRoutine();
            }
        }
        //protected virtual void mainAIFunction(){
        // if(resettingRotation){
        //  characterController.character.transform.rotation=resettingRotationRotLerp.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
        //  transform.rotation=characterController.character.transform.rotation;
        //  if(resettingRotationRotLerp.tgtRotLerpVal>=1f){
        //   AI_OnResetRotationEnd();
        //  }
        // }
        // bool callingSlaves=false;
        // foreach(var slave in slaves){
        //  if(!SimObjectManager.singleton.active.TryGetValue(slave,out SimObject slaveSimObject)){
        //   if(!callingSlaves){
        //    SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
        //    callingSlaves=true;
        //   }
        //  }
        // }
        // if(MyState==State.IDLE_ST){SetBestSkillToUse(Skill.SkillUseContext.OnIdle);}
        // if(MySkill!=null){
        //  DoSkill();
        // }
        //}
     //   void AI_ToResetRotation(out Vector3 dir){
     //    if(MyState==State.ATTACK_ST||
     //       MyState==State. CHASE_ST
     //    ){
     //     dir=(MyEnemy.transform.position-transform.position).normalized;
     //    //}else if(characterController!=null){
     //    // dir=characterController.character.transform.forward;
     //    //}else if(animatorController!=null){
     //    // dir=animatorController.animator.transform.forward;
     //    }else{
     //     dir=transform.forward;
     //    }
     //    if(dir.x==0f&&dir.z==0f){
     //     dir=transform.forward;
     //    }
     //    dir.y=0f;
     //   }
     //protected bool resettingRotation;
     // protected QuaternionRotLerpHelper resettingRotationRotLerp;
     //   void AI_ResetRotation(QuaternionRotLerpHelper rotLerp,bool instantly=false){
     //    if(characterController!=null){
     //     AI_ToResetRotation(out Vector3 dir);
     //     if(rotLerp==null){
     //      //transform.rotation=characterController.character.transform.rotation;
     //      characterController.character.transform.rotation=transform.rotation;
     //      return;
     //     }
     //     rotLerp.tgtRot=Quaternion.LookRotation(dir,Vector3.up);
     //     if(instantly){
     //      characterController.character.transform.rotation=rotLerp.EndRotation();
     //      transform.rotation=characterController.character.transform.rotation;
     //     }else{
     //      resettingRotationRotLerp=rotLerp;
     //      resettingRotation=true;
     //     }
     //    }
     //   }
     //   void AI_OnResetRotationEnd(bool end=true){
     //    if(!resettingRotation){
     //     return;
     //    }
     //    if(end){
     //     characterController.character.transform.rotation=resettingRotationRotLerp.EndRotation();
     //     //transform.rotation=characterController.character.transform.rotation;
     //    }
     //    resettingRotationRotLerp=null;
     //    resettingRotation=false;
     //   }
    }
}