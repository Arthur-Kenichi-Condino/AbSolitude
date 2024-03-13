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
        protected void OnATTACK_ST_Reset(){
         ResetRotation(onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
        }
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
           Vector3 offset=-forward*(radius*2f)*scale.x;
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(characterController.character,transform.root,scale,offset);
           //Debug.DrawLine(values.point0,values.point1,Color.red,onAttackGetDataThrottlerInterval);
           //Debug.DrawRay(values.point0,rotation*Vector3.right*radius*scale.x,Color.red,onAttackGetDataThrottlerInterval);
           float maxDis=attackDistance.z+(radius*2f)*scale.x;
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
              if(IsInAttackRange(actorHit,out _)){
               bool isFriendly=actorHit.IsFriendlyTo(this);
               if(isFriendly){
                //Log.DebugMessage("I need to avoid hitting a friendly target:"+actorHit.name);
                onAttackHasFriendlyTargetsToAvoid.Add((actorHit,hit));
               }
              }
             }
            }
            Array.Sort(onAttackInTheWayColliderHits,OnAttackInTheWayColliderHitsArraySortComparer);
           }
          }
          onAttackTargetsToAvoidRefreshFlag=true;
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
     [SerializeField]internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,.5f);
     protected bool onAttackGetDestGoLeft;
     protected bool onAttackGetDestGoRight;
     protected bool onAttackGetDestGoRandom;
     protected float onAttackHasFriendlyTargetsToAvoidSubroutineMaxTime=8f;
     protected float onAttackHasFriendlyTargetsToAvoidSubroutineTime;
     protected float onAttackHasFriendlyTargetsToAvoidSubroutineCooldown=4f;
     protected float onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer;
     protected int onAttackHasFriendlyTargetsToAvoidSubroutineDestModifiersChangeAfterMoves=2;
     protected int onAttackHasFriendlyTargetsToAvoidSubroutineMoves;
        protected virtual void OnATTACK_ST_Routine(Vector3 attackDistance,Vector3 attackDistanceWithWeapon){
         if(MyEnemy==null){
          return;
         }
         //Log.DebugMessage("OnATTACK_ST_Routine(),this:"+this);
         bool canAttack=true;
         bool shouldAvoid=(onAttackHasFriendlyTargetsToAvoid.Count>0);
         if(onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer>0f){
          onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer-=Time.deltaTime;
          shouldAvoid=false;
         }
         if(!shouldAvoid){
          onAttackHasFriendlyTargetsToAvoidSubroutineTime=0f;
          onAttackHasFriendlyTargetsToAvoidSubroutineMoves=0;
         }
         if(shouldAvoid){
          canAttack=false;
          OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(canAttack,attackDistance);
          return;
         }
         if(
          IsTraversingPath()
         ){
          MoveStop();
         }
         OnATTACK_ST_Attack(canAttack);
        }
     protected bool onAttackTargetsToAvoidWaitingRefreshFlag;
     protected bool onAttackTargetsToAvoidRefreshFlag;
     protected bool onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=true;
        protected virtual void OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(bool canAttack,Vector3 attackDistance){
         onAttackHasFriendlyTargetsToAvoidSubroutineTime+=Time.deltaTime;
         if(onAttackHasFriendlyTargetsToAvoidSubroutineTime>=onAttackHasFriendlyTargetsToAvoidSubroutineMaxTime){
          OnATTACK_ST_Teleport(attackDistance);
          onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer=onAttackHasFriendlyTargetsToAvoidSubroutineCooldown;
          return;
         }
         if(!IsTraversingPath()){
          if(onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid){
           OnATTACK_ST_Attack(true);
           if(IsAttacking()){
            onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=false;
           }
          }else{
           if(!IsAttacking()){
            if(onAttackTargetsToAvoidWaitingRefreshFlag){
             if(onAttackTargetsToAvoidRefreshFlag){
              onAttackTargetsToAvoidWaitingRefreshFlag=false;
              onAttackHasFriendlyTargetsToAvoidSubroutineMoves++;
              if(onAttackHasFriendlyTargetsToAvoidSubroutineMoves%onAttackHasFriendlyTargetsToAvoidSubroutineDestModifiersChangeAfterMoves==0){
               OnATTACK_ST_DestModifiersNext();
              }
              Vector3 destDir=(transform.root.position-MyEnemy.transform.root.position).normalized;
              float destAngle=0f;
              for(int i=0;i<onAttackHasFriendlyTargetsToAvoid.Count;++i){
               //Log.DebugMessage("OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(),onAttackHasFriendlyTargetsToAvoid[i]:"+onAttackHasFriendlyTargetsToAvoid[i].sim.name);
               RaycastHit hit=onAttackHasFriendlyTargetsToAvoid[i].hit;
               SimObject simHit=onAttackHasFriendlyTargetsToAvoid[i].sim;
               Vector3 dirFromAllyToEnemy=(MyEnemy.transform.root.position-simHit.transform.root.position).normalized;
               dirFromAllyToEnemy.y=0f;
               //Debug.DrawRay(MyEnemy.transform.root.position,dirFromAllyToEnemy,Color.cyan,1f);
               Vector3 dirFromEnemyToMe=(transform.root.position-MyEnemy.transform.root.position).normalized;
               dirFromEnemyToMe.y=0f;
               //Debug.DrawRay(MyEnemy.transform.root.position,dirFromEnemyToMe,Color.cyan,1f);
               float angle=Vector3.SignedAngle(dirFromEnemyToMe,dirFromAllyToEnemy,Vector3.up);
               destAngle+=angle;
              }
              if(onAttackGetDestGoRandom){
               destAngle*=math_random.CoinFlip()?-1f:1f;
               destAngle+=(float)math_random.NextDouble(-90f,90f);
              }else if(onAttackGetDestGoLeft){
               destAngle=-Mathf.Abs(destAngle);
               destAngle-=(float)math_random.NextDouble(0f,90f);
              }else if(onAttackGetDestGoRight){
               destAngle=Mathf.Abs(destAngle);
               destAngle+=(float)math_random.NextDouble(0f,90f);
              }
              if(destAngle!=0f){
               destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
               Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.cyan,1f);
              }
              Vector3 dest=MyEnemy.transform.root.position+(destDir*attackDistance.z*1.1f);
              //Debug.DrawLine(transform.root.position,dest,Color.cyan,1f);
              Move(dest);
              onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=true;
             }
            }else{
             onAttackTargetsToAvoidWaitingRefreshFlag=true;
            }
           }
          }
         }
         onAttackTargetsToAvoidRefreshFlag=false;
        }
        protected virtual void OnATTACK_ST_DestModifiersNext(){
         if(onAttackGetDestGoLeft){
          onAttackGetDestGoLeft=false;
          onAttackGetDestGoRight=true;
         }else if(onAttackGetDestGoRight){
          onAttackGetDestGoRight=false;
          onAttackGetDestGoRandom=true;
         }else if(onAttackGetDestGoRandom){
          onAttackGetDestGoRandom=false;
         }else{
          onAttackGetDestGoLeft=true;
         }
        }
        protected virtual void OnATTACK_ST_Teleport(Vector3 attackDistance){
         if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
          teleport.targetDest=MyEnemy.transform.position;
          teleport.cooldown=0f;
          teleport.useRandom=true;
          teleport.randomMaxDis=attackDistance.z*1.1f;
          teleport.DoSkill(this,1);
         }
        }
        protected virtual void OnATTACK_ST_Attack(bool canAttack){
         if(LookToMyEnemy()){
          if(canAttack){
           DoAttackOnAnimationEvent();
          }
         }
        }
    }
}