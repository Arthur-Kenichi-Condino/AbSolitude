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
     protected Coroutine onAttackGetDataCoroutine;
     protected WaitUntil onAttackGetDataThrottler;
      protected float onAttackGetDataThrottlerInterval=.125f;
       protected float onAttackGetDataThrottlerTimer;
     protected RaycastHit[]onAttackInTheWayColliderHits=new RaycastHit[8];
      protected int onAttackInTheWayColliderHitsCount=0;
     readonly List<(SimObject sim,RaycastHit hit)>onAttackHasFriendlyTargetsToAvoid=new List<(SimObject,RaycastHit)>();
        protected virtual IEnumerator OnAttackGetDataCoroutine(){
         onAttackGetDataThrottler=new WaitUntil(
          ()=>{
           if(onAttackGetDataThrottlerTimer>0f){
            onAttackGetDataThrottlerTimer-=Time.deltaTime;
           }
           if(MyState==State.ATTACK_ST){
            if(MyEnemy==null){
             return false;
            }
            if(onAttackGetDataThrottlerTimer<=0f){
             onAttackGetDataThrottlerTimer=onAttackGetDataThrottlerInterval;
             return true;
            }
           }
           return false;
          }
         );
         Loop:{
          yield return onAttackGetDataThrottler;
          //Log.DebugMessage("OnAttackGetDataCoroutine:Loop");
          //  TO DO: don't attack allies, handle MOTION_ATTACK_RIFLE (or motions that are not processed or implemented),
          if(characterController!=null){
           Quaternion rotation=GetRotation();
           float height=GetHeight();
           float radius=GetRadius();
           Vector3 attackDistance=AttackDistance();
           Vector3 forward=(MyEnemy.transform.root.position-transform.root.position).normalized;
           forward.y=0f;
           Vector3 scale=new Vector3(attackDistance.x/radius,attackDistance.y/height,1f);
           Vector3 offset=-forward*radius*2f*scale.x;
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(characterController.character,transform.root,scale,offset);
           Debug.DrawLine(values.point0,values.point1,Color.red,onAttackGetDataThrottlerInterval);
           Debug.DrawRay(values.point0,rotation*Vector3.right*radius*scale.x,Color.red,onAttackGetDataThrottlerInterval);
           float maxDis=attackDistance.z+radius*2f*scale.x;
           //Debug.DrawLine(this.transform.position+offset,this.transform.position+forward*maxDis,Color.red,onAttackGetDataThrottlerInterval);
           int inTheWayLength=0;
           _GetInTheWayColliderHits:{
            inTheWayLength=Physics.CapsuleCastNonAlloc(
             values.point0,
             values.point1,
             values.radius,
             (MyEnemy.transform.position-transform.root.position).normalized,
             onAttackInTheWayColliderHits,
             maxDis,
             PhysUtil.physObstaclesLayer
            );
           }
           if(inTheWayLength>0){
            if(inTheWayLength>=onAttackInTheWayColliderHits.Length){
             Array.Resize(ref onAttackInTheWayColliderHits,inTheWayLength*2);
             goto _GetInTheWayColliderHits;
            }
           }
           onAttackInTheWayColliderHitsCount=inTheWayLength;
           onAttackHasFriendlyTargetsToAvoid.Clear();
           if(onAttackInTheWayColliderHitsCount>0){
            for(int i=onAttackInTheWayColliderHits.Length-1;i>=0;--i){
             if(i>=onAttackInTheWayColliderHitsCount){
              onAttackInTheWayColliderHits[i]=default(RaycastHit);
              continue;
             }
             RaycastHit hit=onAttackInTheWayColliderHits[i];
             if(hit.collider.transform.root==this.transform.root){
              onAttackInTheWayColliderHits[i]=default(RaycastHit);
              onAttackInTheWayColliderHitsCount--;
              continue;
             }
             if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit){
              float actorHitRadius=actorHit.GetRadius();
              if(Math.Abs(transform.root.position.x-(hit.collider.transform.root.position.x-actorHitRadius))<=attackDistance.x||
                 Math.Abs(transform.root.position.z-(hit.collider.transform.root.position.z-actorHitRadius))<=attackDistance.z
              ){
               bool isFriendly=actorHit.IsFriendlyTo(this);
               if(isFriendly){
                Log.DebugMessage("I need to avoid hitting a friendly target:"+actorHit.name);
                onAttackHasFriendlyTargetsToAvoid.Add((actorHit,hit));
               }
              }
             }
            }
            Array.Sort(onAttackInTheWayColliderHits,OnAttackInTheWayColliderHitsArraySortComparer);
           }
          }
         }
         goto Loop;
        }
        //  ordena 'a' relativo a 'b', e retorna 'a' antes de 'b' se 'a' for menor que 'b'
        private int OnAttackInTheWayColliderHitsArraySortComparer(RaycastHit a,RaycastHit b){
         if(a.collider==null&&b.collider==null){
          return 0;
         }
         if(a.collider==null&&b.collider!=null){
          return 1;
         }
         if(a.collider!=null&&b.collider==null){
          return -1;
         }
         return Vector3.Distance(transform.root.position,a.point).CompareTo(Vector3.Distance(transform.root.position,b.point));
        }
     [SerializeField]internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(38,.0005f);
        protected virtual void OnATTACK_ST(){
         //Log.DebugMessage("OnATTACK_ST(),this:"+this);
         bool canAttack=true;
         Quaternion rotation=GetRotation();
         Vector3 attackDistance=AttackDistance();
         Debug.DrawRay(transform.root.position,rotation*(Vector3.forward*attackDistance.z),Color.magenta);
         if(onAttackHasFriendlyTargetsToAvoid.Count>0){
          canAttack=false;
          //Log.DebugMessage("onAttackHasFriendlyTargetsToAvoid.Count>0");
          if(
           !IsTraversingPath()
          ){
           Vector3 destDir=(transform.root.position-MyEnemy.transform.root.position).normalized;
           float destAngle=0f;
           for(int i=0;i<onAttackHasFriendlyTargetsToAvoid.Count;++i){
            //Log.DebugMessage("OnATTACK_ST(),onAttackHasFriendlyTargetsToAvoid[i]:"+onAttackHasFriendlyTargetsToAvoid[i].sim.name);
            RaycastHit hit=onAttackHasFriendlyTargetsToAvoid[i].hit;
            SimObject simHit=onAttackHasFriendlyTargetsToAvoid[i].sim;
            Vector3 dirFromAllyToEnemy=(MyEnemy.transform.root.position-simHit.transform.root.position).normalized;
            dirFromAllyToEnemy.y=0f;
            Debug.DrawRay(MyEnemy.transform.root.position,dirFromAllyToEnemy,Color.cyan,1f);
            Vector3 dirFromEnemyToMe=(transform.root.position-MyEnemy.transform.root.position).normalized;
            dirFromEnemyToMe.y=0f;
            Debug.DrawRay(MyEnemy.transform.root.position,dirFromEnemyToMe,Color.cyan,1f);
            float angle=Vector3.SignedAngle(dirFromEnemyToMe,dirFromAllyToEnemy,Vector3.up);
            destAngle+=angle;
           }
           if(destAngle!=0f){
            destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
            Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.blue,1f);
           }
           Vector3 dest=MyEnemy.transform.root.position+(destDir*attackDistance.z);
           Debug.DrawLine(transform.root.position,dest,Color.blue,1f);
           MyDest=dest-(Vector3.up*(GetHeight()/2f));
           navMeshAgent.destination=MyDest;
          }
         }else{
          if(
           IsTraversingPath()
          ){
           navMeshAgent.destination=navMeshAgent.transform.position;
          }
         }
         if(characterController!=null){
          Vector3 lookDir=MyEnemy.transform.position-transform.position;
          Vector3 planarLookDir=lookDir;
          planarLookDir.y=0f;
          onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
          characterController.character.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
          //Debug.DrawRay(characterController.character.transform.position,characterController.character.transform.forward,Color.gray);
          if(simUMA!=null){
           Quaternion animatorAdjustmentsForUMARotation=Quaternion.identity;
           if(animatorController!=null&&animatorController.transformAdjustmentsForUMA!=null){
            animatorAdjustmentsForUMARotation=Quaternion.Inverse(animatorController.transformAdjustmentsForUMA.localRotation);
           }
           Vector3 animatorLookDir=animatorAdjustmentsForUMARotation*-simUMA.transform.parent.forward;
           Vector3 animatorLookEuler=simUMA.transform.parent.eulerAngles+animatorAdjustmentsForUMARotation.eulerAngles;
           animatorLookEuler.y+=180f;
           Vector3 animatorPlanarLookEuler=animatorLookEuler;
           animatorPlanarLookEuler.x=0f;
           animatorPlanarLookEuler.z=0f;
           Vector3 animatorPlanarLookDir=Quaternion.Euler(animatorPlanarLookEuler)*Vector3.forward;
           //Debug.DrawRay(characterController.character.transform.position,animatorPlanarLookDir,Color.white);
           if(Vector3.Angle(characterController.character.transform.forward,animatorPlanarLookDir)<=5f){
            if(canAttack){
             DoAttackOnAnimationEvent();
            }
           }
          }
         }
        }
    }
}