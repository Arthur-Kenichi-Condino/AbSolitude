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
        internal partial class AI{
         internal ATTACK_ST attackSt;
            internal class ATTACK_ST:ST{
                internal ATTACK_ST(BaseAI me,AI ai):base(me,ai){
                }
                internal void Finish(){
                 firstAttack=false;
             //    AI_ResetRotation(onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy);
                       //if(MyState==State.CHASE_ST){
                       // me.onChaseAlternateMoveAttack=true;
                       //}
                }
                internal void Start(){
                 firstAttack=true;
                 targetsToAvoidRefreshedFlag=false;
                 avoidMode=AvoidMode.MoveAway;
                 shouldAvoid=false;
                 avoiding=false;
                 avoidingMoving=false;
                }
             internal bool firstAttack;
             AvoidMode avoidMode=AvoidMode.MoveAway;
                enum AvoidMode:int{
                 MoveAway=0,
                 MoveRandom=1,
                }
             bool shouldAvoid;
              bool avoiding;
               bool avoidingMoving;
                bool hasJustAvoided;
                 float hasFriendlyTargetsToAvoidMaxTime=8f;
                 float hasFriendlyTargetsToAvoidTimer;
                internal void DoRoutine(){
                 if(MyEnemy==null){
                  me.MoveStop();
                  return;
                 }
                 if(firstAttack){
                  me.MoveStop();
                  if(me.Attack(ai.MyEnemy)){
                   firstAttack=false;
                  }else{
                   return;
                  }
                 }
                 ai.DoSkill();
                 if(avoiding&&avoidingMoving){
                  if(!me.IsTraversingPath()){
                   Log.DebugMessage("stop avoiding:"+ai.MyPathfinding);
                   shouldAvoid=false;
                   avoiding=false;
                   avoidingMoving=false;
                   hasJustAvoided=true;
                  }else{
                   Log.DebugMessage("I am avoiding");
                   Debug.DrawLine(me.transform.root.position,ai.MyDest,Color.green,1f);
                  }
                 }
                 if(targetsToAvoidRefreshedFlag){
                  targetsToAvoidRefreshedFlag=false;
                  shouldAvoid=hasFriendlyTargetsToAvoid.Count>0;
                  //shouldAvoid=true;
                 }
                 if(!shouldAvoid){
                  avoiding=false;
                  avoidingMoving=false;
                  hasFriendlyTargetsToAvoidTimer=0f;
                 }else{
                  hasFriendlyTargetsToAvoidTimer+=Time.deltaTime;
                 }
                 Log.DebugMessage("shouldAvoid:"+shouldAvoid);
                 if(shouldAvoid&&!hasJustAvoided){
                  if(!me.IsAttacking()){
                   if(hasFriendlyTargetsToAvoidTimer>=hasFriendlyTargetsToAvoidMaxTime){
                    hasFriendlyTargetsToAvoidTimer=0f;
                    me.MoveStop();
                    Log.DebugMessage("TeleportToRandomNearMyEnemy");
                    me.TeleportToRandomNearMyEnemy(ai.attackDistance);
                   }else{
                    if(!me.IsTraversingPath()){
                     if(!avoiding){
                      switch(avoidMode){
                       default:{
                        Vector3 destDir=(me.transform.root.position-MyEnemy.transform.root.position).normalized;
                        destDir.y=0f;
                        float destAngle=0f;
                        for(int i=0;i<hasFriendlyTargetsToAvoid.Count;++i){
                         Log.DebugMessage("hasFriendlyTargetsToAvoid[i]:"+hasFriendlyTargetsToAvoid[i].sim.name);
                         RaycastHit hit=hasFriendlyTargetsToAvoid[i].hit;
                         SimObject simHit=hasFriendlyTargetsToAvoid[i].sim;
                         Vector3 dirFromEnemyToAlly=(simHit.transform.root.position-MyEnemy.transform.root.position).normalized;
                         dirFromEnemyToAlly.y=0f;
                         //Debug.DrawRay(MyEnemy.transform.root.position,dirFromAllyToEnemy,Color.cyan,1f);
                         Vector3 dirFromEnemyToMe=(me.transform.root.position-MyEnemy.transform.root.position).normalized;
                         dirFromEnemyToMe.y=0f;
                         //Debug.DrawRay(MyEnemy.transform.root.position,dirFromEnemyToMe,Color.cyan,1f);
                         float angle=180f-Vector3.Angle(dirFromEnemyToMe,dirFromEnemyToAlly);
                         destAngle+=angle;
                        }
                        if(destAngle!=0f){
                         destAngle/=hasFriendlyTargetsToAvoid.Count;
                         destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
                         Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.cyan,1f);
                        }
                        Vector3 dest=MyEnemy.transform.root.position+(destDir*ai.attackDistance.z*1.1f);
                        Debug.DrawLine(me.transform.root.position,dest,Color.cyan,1f);
                        ai.MyDest=dest;
                        Log.DebugMessage("avoiding, move away");
                        //avoidMode=AvoidMode.MoveRandom;
                        break;
                       }
                       case(AvoidMode.MoveRandom):{
                        Vector3 destDir=(me.transform.root.position-MyEnemy.transform.root.position).normalized;
                        destDir.y=0f;
                        float destAngle=0f;
                        destAngle+=(float)me.math_random.NextDouble(-180f,180f);
                        if(destAngle!=0f){
                         destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
                         Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.cyan,1f);
                        }
                        Vector3 dest=MyEnemy.transform.root.position+(destDir*ai.attackDistance.z*1.1f);
                        Debug.DrawLine(me.transform.root.position,dest,Color.cyan,1f);
                        ai.MyDest=dest;
                        Log.DebugMessage("avoiding, random move");
                        avoidMode=AvoidMode.MoveAway;
                        break;
                       }
                      }
                      avoiding=true;
                     }
                     if(me.Move(ai.MyDest)){
                      avoidingMoving=true;
                      Log.DebugMessage("avoiding, move!");
                     }
                    }
                   }
                  }
                 }else{
                  Log.DebugMessage("MoveStop()");
                  me.MoveStop();
                  if(me.Attack(ai.MyEnemy)){
                   hasJustAvoided=false;
                  }
                 }
                }
             internal Coroutine getDataCoroutine;
             protected WaitUntil getDataThrottler;
              protected float getDataThrottlerInterval=.125f;
               protected float getDataThrottlerTimer;
             protected RaycastHit[]inTheWayColliderHits=new RaycastHit[8];
              protected int inTheWayColliderHitsCount=0;
             readonly List<(SimObject sim,RaycastHit hit)>hasFriendlyTargetsToAvoid=new List<(SimObject,RaycastHit)>();
             bool targetsToAvoidRefreshedFlag;
                internal virtual IEnumerator GetDataCoroutine(){
                 getDataThrottler=new WaitUntil(
                  ()=>{
                   if(getDataThrottlerTimer>0f){
                    getDataThrottlerTimer-=Time.deltaTime;
                   }
                   if(MyEnemy==null){
                    return false;
                   }
                   if(MyState==State.ATTACK_ST){
                    if(getDataThrottlerTimer<=0f){
                     getDataThrottlerTimer=getDataThrottlerInterval;
                     return true;
                    }
                   }
                   return false;
                  }
                 );
                 Loop:{
                  yield return getDataThrottler;
                  Log.DebugMessage("GetDataCoroutine:Loop");
                  //  TO DO: don't attack allies, handle MOTION_ATTACK_RIFLE (or motions that are not processed or implemented),
                  if(me.characterController!=null){
                   Quaternion rotation=me.GetRotation();
                   float height=me.GetHeight();
                   float radius=me.GetRadius();
                   Vector3 attackDistance=me.AttackDistance();
                   Vector3 forward=(MyEnemy.transform.root.position-me.transform.root.position).normalized;
                   forward.y=0f;
                   Log.DebugMessage("attackDistance:"+attackDistance);
                   Vector3 scale=new Vector3(
                    (attackDistance.x)/(radius),
                    (attackDistance.y)/(height),
                    (attackDistance.z)/(radius)
                   );
                   Log.DebugMessage("scale:"+scale);
                   Vector3 offset=-forward*(radius*2f);
                   var values=me.simCollisions.GetCapsuleValuesForCollisionTesting(me.characterController.character,me.transform.root,scale,offset);
                   Debug.DrawLine(values.point0,values.point1,Color.red,getDataThrottlerInterval);
                   Debug.DrawRay(values.point0,rotation*Vector3.right*radius*scale.x,Color.red,getDataThrottlerInterval);
                   Debug.DrawRay(values.point0,rotation*Vector3.forward*radius*scale.z,Color.red,getDataThrottlerInterval);
                   float maxDis=radius*2f+attackDistance.z;
                   Debug.DrawLine(me.transform.position+offset,me.transform.position+offset+(forward*maxDis),Color.red,getDataThrottlerInterval);
                   int inTheWayLength=0;
                   _GetInTheWayColliderHits:{
                    inTheWayLength=Physics.CapsuleCastNonAlloc(
                     values.point0,
                     values.point1,
                     values.radius,
                     (MyEnemy.transform.position-me.transform.root.position).normalized,
                     inTheWayColliderHits,
                     maxDis,
                     PhysUtil.physObstaclesLayer
                    );
                   }
                   if(inTheWayLength>0){
                    if(inTheWayLength>=inTheWayColliderHits.Length){
                     Array.Resize(ref inTheWayColliderHits,inTheWayLength*2);
                     goto _GetInTheWayColliderHits;
                    }
                   }
                   inTheWayColliderHitsCount=inTheWayLength;
                   hasFriendlyTargetsToAvoid.Clear();
                   if(inTheWayColliderHitsCount>0){
                    for(int i=inTheWayColliderHits.Length-1;i>=0;--i){
                     if(i>=inTheWayColliderHitsCount){
                      inTheWayColliderHits[i]=default(RaycastHit);
                      continue;
                     }
                     RaycastHit hit=inTheWayColliderHits[i];
                     if(hit.collider.transform.root==me.transform.root){
                      inTheWayColliderHits[i]=default(RaycastHit);
                      inTheWayColliderHitsCount--;
                      continue;
                     }
                     if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit){
                      float actorHitRadius=actorHit.GetRadius();
                      if(me.IsInAttackRange(actorHit,out _)){
                       bool isFriendly=actorHit.IsFriendlyTo(me);
                       if(isFriendly){
                        Log.DebugMessage("I need to avoid hitting a friendly target:"+actorHit.name);
                        hasFriendlyTargetsToAvoid.Add((actorHit,hit));
                       }
                      }
                     }
                    }
                    Array.Sort(inTheWayColliderHits,InTheWayColliderHitsArraySortComparer);
                   }
                  }
                  targetsToAvoidRefreshedFlag=true;
                 }
                 goto Loop;
                }
                //  ordena 'a' relativo a 'b', e retorna 'a' antes de 'b' se 'a' for menor que 'b'
                private int InTheWayColliderHitsArraySortComparer(RaycastHit a,RaycastHit b){
                 if(a.collider==null&&b.collider==null){
                  return 0;
                 }
                 if(a.collider==null&&b.collider!=null){
                  return 1;
                 }
                 if(a.collider!=null&&b.collider==null){
                  return -1;
                 }
                 return Vector3.Distance(me.transform.root.position,a.point).CompareTo(Vector3.Distance(me.transform.root.position,b.point));
                }
            }
        }
     //[SerializeField]internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(10,.5f);
     //protected bool onAttackGetDestGoLeft;
     //protected bool onAttackGetDestGoRight;
     //protected bool onAttackGetDestGoRandom;
     //protected float onAttackHasFriendlyTargetsToAvoidSubroutineMaxTime=8f;
     //protected float onAttackHasFriendlyTargetsToAvoidSubroutineTime;
     //protected float onAttackHasFriendlyTargetsToAvoidSubroutineCooldown=4f;
     //protected float onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer;
     //protected int onAttackHasFriendlyTargetsToAvoidSubroutineDestModifiersChangeAfterMoves=2;
     //protected int onAttackHasFriendlyTargetsToAvoidSubroutineMoves;
     //   protected virtual void OnATTACK_ST_Routine(Vector3 attackDistance,Vector3 attackDistanceWithWeapon){
     //    if(MyEnemy==null){
     //     return;
     //    }
     //    //Log.DebugMessage("OnATTACK_ST_Routine(),this:"+this);
     //    bool canAttack=true;
     //    bool shouldAvoid=(onAttackHasFriendlyTargetsToAvoid.Count>0);
     //    if(onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer>0f){
     //     onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer-=Time.deltaTime;
     //     shouldAvoid=false;
     //    }
     //    if(!shouldAvoid){
     //     onAttackHasFriendlyTargetsToAvoidSubroutineTime=0f;
     //     onAttackHasFriendlyTargetsToAvoidSubroutineMoves=0;
     //    }
     //    if(shouldAvoid){
     //     canAttack=false;
     //     OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(canAttack,attackDistance);
     //     return;
     //    }
     //    if(
     //     IsTraversingPath()
     //    ){
     //     MoveStop();
     //    }
     //    OnATTACK_ST_Attack(canAttack);
     //   }
     //protected bool onAttackTargetsToAvoidWaitingRefreshFlag;
     //protected bool onAttackTargetsToAvoidRefreshFlag;
     //protected bool onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=true;
     //   protected virtual void OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(bool canAttack,Vector3 attackDistance){
     //    onAttackHasFriendlyTargetsToAvoidSubroutineTime+=Time.deltaTime;
     //    if(onAttackHasFriendlyTargetsToAvoidSubroutineTime>=onAttackHasFriendlyTargetsToAvoidSubroutineMaxTime){
     //     OnATTACK_ST_Teleport(attackDistance);
     //     onAttackHasFriendlyTargetsToAvoidSubroutineCooldownTimer=onAttackHasFriendlyTargetsToAvoidSubroutineCooldown;
     //     return;
     //    }
     //    if(!IsTraversingPath()){
     //     if(onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid){
     //      OnATTACK_ST_Attack(true);
     //      if(IsAttacking()){
     //       onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=false;
     //      }
     //     }else{
     //      if(!IsAttacking()){
     //       if(onAttackTargetsToAvoidWaitingRefreshFlag){
     //        if(onAttackTargetsToAvoidRefreshFlag){
     //         onAttackTargetsToAvoidWaitingRefreshFlag=false;
     //         onAttackHasFriendlyTargetsToAvoidSubroutineMoves++;
     //         if(onAttackHasFriendlyTargetsToAvoidSubroutineMoves%onAttackHasFriendlyTargetsToAvoidSubroutineDestModifiersChangeAfterMoves==0){
     //          OnATTACK_ST_DestModifiersNext();
     //         }
     //         Vector3 destDir=(transform.root.position-MyEnemy.transform.root.position).normalized;
     //         float destAngle=0f;
     //         for(int i=0;i<onAttackHasFriendlyTargetsToAvoid.Count;++i){
     //          //Log.DebugMessage("OnATTACK_ST_SubroutineHasFriendlyTargetsToAvoid(),onAttackHasFriendlyTargetsToAvoid[i]:"+onAttackHasFriendlyTargetsToAvoid[i].sim.name);
     //          RaycastHit hit=onAttackHasFriendlyTargetsToAvoid[i].hit;
     //          SimObject simHit=onAttackHasFriendlyTargetsToAvoid[i].sim;
     //          Vector3 dirFromAllyToEnemy=(MyEnemy.transform.root.position-simHit.transform.root.position).normalized;
     //          dirFromAllyToEnemy.y=0f;
     //          //Debug.DrawRay(MyEnemy.transform.root.position,dirFromAllyToEnemy,Color.cyan,1f);
     //          Vector3 dirFromEnemyToMe=(transform.root.position-MyEnemy.transform.root.position).normalized;
     //          dirFromEnemyToMe.y=0f;
     //          //Debug.DrawRay(MyEnemy.transform.root.position,dirFromEnemyToMe,Color.cyan,1f);
     //          float angle=Vector3.SignedAngle(dirFromEnemyToMe,dirFromAllyToEnemy,Vector3.up);
     //          destAngle+=angle;
     //         }
     //         if(onAttackGetDestGoRandom){
     //          destAngle*=math_random.CoinFlip()?-1f:1f;
     //          destAngle+=(float)math_random.NextDouble(-90f,90f);
     //         }else if(onAttackGetDestGoLeft){
     //          destAngle=-Mathf.Abs(destAngle);
     //          destAngle-=(float)math_random.NextDouble(0f,90f);
     //         }else if(onAttackGetDestGoRight){
     //          destAngle=Mathf.Abs(destAngle);
     //          destAngle+=(float)math_random.NextDouble(0f,90f);
     //         }
     //         if(destAngle!=0f){
     //          destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
     //          Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.cyan,1f);
     //         }
     //         Vector3 dest=MyEnemy.transform.root.position+(destDir*attackDistance.z*1.1f);
     //         //Debug.DrawLine(transform.root.position,dest,Color.cyan,1f);
     //         Move(dest);
     //         onAttackDoAttackEvenIfHasFriendlyTargetsToAvoid=true;
     //        }
     //       }else{
     //        onAttackTargetsToAvoidWaitingRefreshFlag=true;
     //       }
     //      }
     //     }
     //    }
     //    onAttackTargetsToAvoidRefreshFlag=false;
     //   }
     //   protected virtual void OnATTACK_ST_DestModifiersNext(){
     //    if(onAttackGetDestGoLeft){
     //     onAttackGetDestGoLeft=false;
     //     onAttackGetDestGoRight=true;
     //    }else if(onAttackGetDestGoRight){
     //     onAttackGetDestGoRight=false;
     //     onAttackGetDestGoRandom=true;
     //    }else if(onAttackGetDestGoRandom){
     //     onAttackGetDestGoRandom=false;
     //    }else{
     //     onAttackGetDestGoLeft=true;
     //    }
     //   }
     //   protected virtual void OnATTACK_ST_Teleport(Vector3 attackDistance){
     //    if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
     //     teleport.targetDest=MyEnemy.transform.position;
     //     teleport.cooldown=0f;
     //     teleport.useRandom=true;
     //     teleport.randomMaxDis=attackDistance.z*1.1f;
     //     teleport.DoSkill(this,1);
     //    }
     //   }
     //   protected virtual void OnCHASE_ST_Teleport(Vector3 attackDistance){
     //    if(this.skills.TryGetValue(typeof(Teleport),out Skill skill)&&skill is Teleport teleport){
     //     teleport.targetDest=MyEnemy.transform.position;
     //     teleport.cooldown=0f;
     //     teleport.useRandom=true;
     //     teleport.randomMaxDis=attackDistance.z*1.1f;
     //     teleport.DoSkill(this,1);
     //    }
     //   }
     //   protected virtual void OnATTACK_ST_Attack(bool canAttack){
     //    if(AI_LookToMyEnemy(onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy)){
     //     if(canAttack){
     //      DoAttackOnAnimationEvent();
     //     }
     //    }
     //   }
    }
}