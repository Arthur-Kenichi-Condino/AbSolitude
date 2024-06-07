#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal partial class AI{
         internal SNIPE_ST snipeSt;
            internal class SNIPE_ST:ST{
                internal SNIPE_ST(BaseAI me,AI ai):base(me,ai){
                }
             internal float minTimeBeforeCanChase=8f;
             internal float timer;
             internal int maxTryCount=10;
              internal int tryCount;
               internal float tryCountToCooldownMultiplier=1f;
             internal float maxStateCooldown=10f;
              internal float onStateCooldown;
             internal bool alternateRetreatShoot=false;
             internal float retreatTime=3f;
             internal float retreatDis;
             bool moving;
              Vector3?movingDestSet;
             bool tryingShooting;
             bool reloading;
             bool shooting;
             float noDestMaxTime=1f;
              float noDestTimer;
             float tryMoveMaxTime=2f;
              float tryMoveTimer;
             float tryShootMaxTime=4f;
              float tryShootTimer;
                internal void Finish(){
                 if(me.characterController!=null){
                    me.characterController.isAiming=false;
                 }
                 onStateCooldown=tryCount*tryCountToCooldownMultiplier;
                 onStateCooldown=Mathf.Min(onStateCooldown,maxStateCooldown);
                 tryCount=0;
                 Log.DebugMessage("onStateCooldown:"+onStateCooldown);
                }
                internal void Start(){
                 timer=0f;
                 alternateRetreatShoot=false;
                 movingDestSet=null;
                 noDestTimer=0f;
                 tryMoveTimer=0f;
                 tryShootTimer=0f;
                }
                internal void DoRoutine(){
                 //  TO DO
                 //  se tentar mover várias vezes depois de um tempo e não conseguir ficar longe o suficiente, atirar
                 //  corrigir direção do tiro
                 timer+=Time.deltaTime;
                 if(MyEnemy==null){
                  me.MoveStop();
                  return;
                 }
                 float myMoveSpeed=Mathf.Max(
                  me.moveMaxVelocity.x,
                  me.moveMaxVelocity.y,
                  me.moveMaxVelocity.z
                 );
                 float myEnemyMoveSpeed=0f;
                 if(MyEnemy is BaseAI myEnemyBaseAI){
                  myEnemyMoveSpeed=Mathf.Max(
                   myEnemyBaseAI.moveMaxVelocity.x,
                   myEnemyBaseAI.moveMaxVelocity.y,
                   myEnemyBaseAI.moveMaxVelocity.z
                  );
                 }
                 float dis1=     myMoveSpeed*retreatTime;
                 float dis2=myEnemyMoveSpeed*retreatTime;
                 float ratio;
                 if(dis2<=0f){
                  ratio=1f;
                 }else{
                  ratio=dis1/dis2;
                 }
                 retreatDis=ratio*dis1;
                 Log.DebugMessage("retreatDis:"+retreatDis);
                 ai.DoSkill();
                 if(reloading){
                  if(!me.IsReloading()){
                   alternateRetreatShoot=true;
                   reloading=false;
                   if(me.characterController!=null){
                      me.characterController.isAiming=false;
                   }
                  }else{
                   if(
                    !me.IsTraversingPath()
                   ){
                    me.TurnToMyEnemy();
                   }
                  }
                 }
                 if(shooting){
                  if(
                   me.IsTraversingPath()
                  ){
                   me.MoveStop();
                  }
                  if(!me.IsShooting()){
                   alternateRetreatShoot=true;
                   shooting=false;
                   if(me.characterController!=null){
                      me.characterController.isAiming=false;
                   }
                  }else{
                   me.TurnToMyEnemy();
                  }
                 }
                 if(moving){
                  if(
                   !me.IsTraversingPath()
                  ){
                   Log.DebugMessage("not me.IsTraversingPath:moving=false");
                   alternateRetreatShoot=true;
                   moving=false;
                  }
                 }
                 Log.DebugMessage("ai.MyPathfinding:"+ai.MyPathfinding);
                 if(!reloading&&
                    !shooting&&
                    !moving
                 ){
                  if(alternateRetreatShoot){
                   alternateRetreatShoot=false;
                   movingDestSet=null;
                   noDestTimer=0f;
                   tryMoveTimer=0f;
                   tryShootTimer=0f;
                   tryingShooting=!tryingShooting;
                  }
                  if(tryingShooting){
                   me.MoveStop();
                   Log.DebugMessage("tryingShooting");
                   if(me.TryShoot(MyEnemy,out bool r,out bool s)){
                    if(s){
                     shooting=true;
                    }else if(r){
                     reloading=true;
                    }else{
                     alternateRetreatShoot=true;
                    }
                   }else{
                    tryShootTimer+=Time.deltaTime;
                    if(tryShootTimer>=tryShootMaxTime){
                     tryShootTimer=0f;
                     tryCount++;
                     //  timer to cancel shoot and try movement
                     // then total snipe try count ++
                     // then add cooldown to SNIPE_ST
                     alternateRetreatShoot=true;
                    }
                   }
                  }else{
                   Log.DebugMessage("!tryingShooting");
                   if(Vector3.Distance(me.transform.position,MyEnemy.transform.position)<=retreatDis){
                    if(movingDestSet==null){
                     Vector3 dir=(me.transform.position-MyEnemy.transform.position).normalized;
                     dir.y=0f;
                     Vector3 dest=MyEnemy.transform.position+dir*retreatDis+Vector3.down*(me.height/2f);
                     if(Vector3.Distance(ai.MyDest,me.transform.position)<=.125f){
                      if(me.GetRandomPosition(MyEnemy.transform.position,retreatDis,out Vector3 randomPos)){
                       dest=randomPos;
                       movingDestSet=dest;
                      }
                     }else{
                      movingDestSet=dest;
                     }
                    }
                    if(movingDestSet!=null){
                     ai.MyDest=movingDestSet.Value;
                     if(me.Move(ai.MyDest)){
                      Debug.DrawLine(MyEnemy.transform.position,ai.MyDest,Color.blue,5f);
                      moving=true;
                     }else{
                      tryMoveTimer+=Time.deltaTime;
                      if(tryMoveTimer>=tryMoveMaxTime){
                       tryMoveTimer=0f;
                       tryCount++;
                       //  timer to cancel movement and try shoot
                       // then total snipe try count ++
                       // then add cooldown to SNIPE_ST
                       alternateRetreatShoot=true;
                      }
                     }
                    }else{
                     noDestTimer+=Time.deltaTime;
                     if(noDestTimer>=noDestMaxTime){
                      noDestTimer=0f;
                      tryCount++;
                      //  try count ++
                      alternateRetreatShoot=true;
                     }
                    }
                   }else{
                    alternateRetreatShoot=true;
                   }
                  }
                 }
                }
            }
        }
     //internal float onSnipeRetreatTime=3f;
     //internal float onSnipeRetreatDis;
     //protected bool onSnipeAlternateRetreatShoot=false;
     //   protected virtual void OnSNIPE_ST_Start(){
     //    Log.DebugMessage("OnSNIPE_ST_Start()");
     //    onSnipeTime=0f;
     //    onSnipeAlternateRetreatShoot=false;
     //    if(
     //     IsTraversingPath()
     //    ){
     //     MoveStop();
     //    }
     //    onSnipeMoving=false;
     //    onSnipeTryingShooting=false;
     //    onSnipeReloading=false;
     //    onSnipeShooting=false;
     //    onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=characterController.character.transform.rotation;
     //    onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.EndRotation();
     //   }
     //[SerializeField]internal QuaternionRotLerpHelper onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,1.5f);
     //bool onSnipeMoving;
     //bool onSnipeTryingShooting;
     //bool onSnipeReloading;
     //bool onSnipeShooting;
     //   protected virtual void OnSNIPE_ST_Routine(Vector3 attackDistance,Vector3 attackDistanceWithWeapon){
     //    onSnipeTime+=Time.deltaTime;
     //    if(MyEnemy==null){
     //     return;
     //    }
     //    //Log.DebugMessage("OnSNIPE_ST_Routine()");
     //    float myMoveSpeed=Mathf.Max(moveMaxVelocity.x,moveMaxVelocity.y,moveMaxVelocity.z);
     //    float myEnemyMoveSpeed=0f;
     //    if(MyEnemy is BaseAI myEnemyAI){
     //     myEnemyMoveSpeed=Mathf.Max(myEnemyAI.moveMaxVelocity.x,myEnemyAI.moveMaxVelocity.y,myEnemyAI.moveMaxVelocity.z);
     //    }
     //    float dis1=     myMoveSpeed*onSnipeRetreatTime;
     //    float dis2=myEnemyMoveSpeed*onSnipeRetreatTime;
     //    float ratio;
     //    if(dis2<=0f){
     //     ratio=1f;
     //    }else{
     //     ratio=dis1/dis2;
     //    }
     //    onSnipeRetreatDis=ratio*dis1;
     //    //Log.DebugMessage("onSnipeRetreatDis:"+onSnipeRetreatDis);
     //    //
     //    if(onSnipeReloading){
     //     //Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeReloading");
     //     if(
     //      IsTraversingPath()
     //     ){
     //      MoveStop();
     //     }
     //     if(!IsReloading()){
     //      //OnSNIPE_ST_Reset();
     //      onSnipeAlternateRetreatShoot=false;
     //      onSnipeReloading=false;
     //     }else{
     //      AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
     //     }
     //    }else if(onSnipeShooting){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeShooting");
     //     if(
     //      IsTraversingPath()
     //     ){
     //      MoveStop();
     //     }
     //     if(!IsShooting()){
     //      //OnSNIPE_ST_Reset(); 
     //      onSnipeAlternateRetreatShoot=false;
     //      onSnipeShooting=false;
     //     }else{
     //      AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
     //     }
     //    }else if(onSnipeMoving){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeMoving");
     //     if(
     //      !IsTraversingPath()
     //     ){
     //      onSnipeAlternateRetreatShoot=true;
     //      onSnipeMoving=false;
     //     }else{
     //      if(!AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //       MovePause();
     //      }else{
     //       MoveResume();
     //      }
     //     }
     //    }
     //    //Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot:"+onSnipeAlternateRetreatShoot);
     //    if(!onSnipeReloading&&
     //       !onSnipeShooting&&
     //       !onSnipeMoving
     //    ){
     //     if(onSnipeAlternateRetreatShoot){
     //      Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot==true");
     //      onSnipeTryingShooting=true;
     //     }
     //    }
     //    if(onSnipeTryingShooting){
     //     Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeTryingShooting");
     //     if(
     //      !IsTraversingPath()
     //     ){
     //      if(!onSnipeReloading&&
     //         !onSnipeShooting
     //      ){
     //       if(OnSNIPE_ST_Shoot()){
     //        Log.DebugMessage("OnSNIPE_ST_Routine():shoot");
     //        onSnipeTryingShooting=false;
     //       }
     //      }
     //     }else{
     //      if(!AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //       MovePause();
     //      }else{
     //       MoveResume();
     //      }
     //     }
     //    }else{
     //     if(!onSnipeAlternateRetreatShoot){
     //      Log.DebugMessage("OnSNIPE_ST_Routine():onSnipeAlternateRetreatShoot==false");
     //      if(characterController!=null){
     //         characterController.isAiming=false;
     //      }
     //      bool moveToDestination=!onSnipeMoving;
     //      if(Vector3.Distance(transform.position,MyEnemy.transform.position)<=onSnipeRetreatDis){
     //       if(
     //        !IsTraversingPath()
     //       ){
     //        moveToDestination|=true;
     //       }
     //      }
     //      if(moveToDestination){
     //       Log.DebugMessage("OnSNIPE_ST_Routine():move");
     //       Vector3 dir=(transform.position-MyEnemy.transform.position).normalized;
     //       dir.y=0f;
     //       MyDest=MyEnemy.transform.position+dir*onSnipeRetreatDis+Vector3.down*(height/2f);
     //       if(AI_LookToMyDest(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //        Move(MyDest);
     //        Debug.DrawRay(MyEnemy.transform.position,dir*onSnipeRetreatDis,Color.blue,5f);
     //        onSnipeMoving=true;
     //       }
     //      }
     //     }
     //    }
     //    if(animatorController.animatorIKController!=null){
     //     Debug.DrawLine(GetHeadPosition(true),animatorController.animatorIKController.headLookAtPositionLerp.tgtPos,Color.yellow);
     //    }
     //   }
     //   protected virtual bool OnSNIPE_ST_Shoot(){
     //    if(AI_LookToMyEnemy(onSnipePlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //     //
     //     characterController.isAiming=true;
     //     if(animatorController.animatorIKController==null||(Vector3.Angle(animatorController.animatorIKController.headLookAtPositionLerped,animatorController.animatorIKController.headLookAtPositionLerp.tgtPos)<=.125f&&animatorController.animatorIKController.headLookAtPositionLerp.tgtPos==animatorController.animatorIKController.headLookAtPositionLerp.tgtPos_Last&&animatorController.animatorIKController.headLookAtPositionLerp.tgtPosLerpVal>=1f)){
     //      if(itemsEquipped!=null){
     //       if(itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
     //        if(simWeapon.ammoLoaded<=0){
     //         if(simWeapon.TryStartReloadingAction(simAiming:this)){
     //          characterController.weaponsReloading.Add(simWeapon);
     //          onSnipeReloading=true;
     //          return true;
     //         }
     //        }else{
     //         if(simWeapon.TryStartShootingAction(simAiming:this)){
     //          onSnipeShooting=true;
     //          Debug.DrawLine(GetHeadPosition(true),animatorController.animatorIKController.headLookAtPositionLerped,Color.blue,5f);
     //          return true;
     //         }
     //        }
     //       }
     //      }
     //     }
     //    }
     //    return false;
     //   }
    }
}