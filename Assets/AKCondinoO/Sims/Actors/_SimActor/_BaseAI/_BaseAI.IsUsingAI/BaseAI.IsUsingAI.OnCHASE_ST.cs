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
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal partial class AI{
         [NonSerialized]internal CHASE_ST chaseSt;
            internal class CHASE_ST:ST{
                internal CHASE_ST(BaseAI me,AI ai):base(me,ai){
                }
             [NonSerialized]protected float travelMaxTime=3f;
              [NonSerialized]protected float travelTime;
               [NonSerialized]protected bool traveledForTooLong;
             [NonSerialized]protected bool consideringTeleport;
              [NonSerialized]protected float consideringTeleportTimeIntervalToTeleport=3f;
               [NonSerialized]protected float consideringTeleportTimerToTeleport;
             [NonSerialized]protected Vector3 myEnemyPos,previousMyEnemyPos;
              [NonSerialized]protected bool myEnemyMoved;
             [NonSerialized]protected bool predictMyEnemyDest;
              [NonSerialized]protected float predictMyEnemyDestDis;
             [NonSerialized]protected float stoppedPredictingMyEnemyDestCanPredictAgainTimeInterval=2f;
              [NonSerialized]protected float stoppedPredictingMyEnemyDestCanPredictAgainTimer;
             [NonSerialized]protected float renewDestinationTimeInterval=2f;
              [NonSerialized]protected float renewDestinationTimer;
             [NonSerialized]protected float renewDestinationMyEnemyIsMovingTimeInterval=.125f;
              [NonSerialized]protected float renewDestinationMyEnemyIsMovingTimer;
             [NonSerialized]AvoidMode avoidMode=AvoidMode.MoveRandom;
                enum AvoidMode:int{
                 MoveLeft=0,
                 MoveRight=1,
                 MoveRandom=2,
                }
                internal void Finish(){
                }
                internal void Start(){
                 if(MyEnemy!=null){
                  myEnemyPos=previousMyEnemyPos=MyEnemy.transform.position;
                 }
                 travelTime=0f;
                 traveledForTooLong=false;
                 consideringTeleport=false;
                 consideringTeleportTimerToTeleport=0f;
                 myEnemyMoved=false;
                 predictMyEnemyDest=false;
                 renewDestinationTimer=0f;
                 renewDestinationMyEnemyIsMovingTimer=0f;
                }
                internal void DoRoutine(){
                 if(MyEnemy==null){
                  me.MoveStop();
                  return;
                 }
                 if(me.IsTraversingPath()){
                  travelTime+=Time.deltaTime;
                 }
                 if(travelTime>=travelMaxTime){
                  travelTime=0f;
                  traveledForTooLong=true;
                 }
                 if(traveledForTooLong){
                  if(ai.MyPathfinding==PathfindingResult.UNREACHABLE){
                   Log.DebugMessage("'traveledForTooLong':'ai.MyPathfinding==PathfindingResult.UNREACHABLE':'try to evade'");
                   Log.Warning("'TO DO: set to evade state for a time'");
                  }else if(ai.damageSources.ContainsKey(MyEnemy)){
                   //Log.DebugMessage("'ai.damageSources.ContainsKey(MyEnemy)':'I can reach my enemy but the pathfinding isn't fast enough':'prepare for teleport to enemy'");
                   consideringTeleport=true;
                  }
                 }
                 if(consideringTeleport){
                  if(consideringTeleportTimerToTeleport<=0f){
                   consideringTeleportTimerToTeleport=consideringTeleportTimeIntervalToTeleport;
                  }
                  if(consideringTeleportTimerToTeleport>0f){
                   consideringTeleportTimerToTeleport-=Time.deltaTime;
                   if(consideringTeleportTimerToTeleport<=0f){
                    //Log.DebugMessage("'consideringTeleportTimerToTeleport<=0f':'teleport to enemy!'");
                    consideringTeleport=false;
                    Log.Warning("'TO DO: teleport to enemy on chase'");
                   }
                  }
                 }
                 BaseAI myEnemyBaseAI=MyEnemy as BaseAI;
                 bool myEnemyIsMoving=false;
                 myEnemyPos=MyEnemy.transform.position;
                 bool myEnemyChangedPos=myEnemyPos!=previousMyEnemyPos;
                 myEnemyMoved|=myEnemyChangedPos;
                 bool shouldPredictMyEnemyDest=false;
                 shouldPredictMyEnemyDest|=myEnemyChangedPos;
                 if(myEnemyBaseAI!=null){
                  myEnemyIsMoving=myEnemyBaseAI.IsMoving();
                  myEnemyMoved|=myEnemyIsMoving;
                  shouldPredictMyEnemyDest|=myEnemyIsMoving;
                 }
                 if(shouldPredictMyEnemyDest){
                  //Log.Warning("TO DO: do not predict dest on chase if already in front of enemy");
                  if(myEnemyBaseAI==null||myEnemyBaseAI.characterController==null){
                   shouldPredictMyEnemyDest=false;
                  }else{
                   Vector3 myEnemyForward=myEnemyBaseAI.characterController.transform.forward;
                   myEnemyForward.y=0f;
                   Vector3 dirFromMyEnemyToMe=(ai.me.transform.position-ai.MyEnemy.transform.position).normalized;
                   dirFromMyEnemyToMe.y=0f;
                   if(Vector3.Angle(myEnemyForward,dirFromMyEnemyToMe)<90f){
                    shouldPredictMyEnemyDest=false;
                   }
                  }
                 }
                 float myMoveSpeed=Mathf.Max(
                  me.moveMaxVelocity.x,
                  me.moveMaxVelocity.y,
                  me.moveMaxVelocity.z
                 );
                 float myEnemyMoveSpeed=0f;
                 if(myEnemyBaseAI!=null){
                  myEnemyMoveSpeed=Mathf.Max(
                   myEnemyBaseAI.moveMaxVelocity.x,
                   myEnemyBaseAI.moveMaxVelocity.y,
                   myEnemyBaseAI.moveMaxVelocity.z
                  );
                 }
                 float dis1=     myMoveSpeed*renewDestinationTimeInterval;
                 float dis2=myEnemyMoveSpeed*renewDestinationTimeInterval;
                 float ratio;
                 if(dis2<=0f){
                  ratio=1f;
                 }else{
                  ratio=dis1/dis2;
                 }
                 predictMyEnemyDestDis=ratio*dis1;
                 bool getDestination=false;
                 if(ai.damageSources.ContainsKey(MyEnemy)){
                 }
                 if(renewDestinationTimer>0f){
                  renewDestinationTimer-=Time.deltaTime;
                 }
                 if(renewDestinationTimer<=0f){
                  renewDestinationTimer=renewDestinationTimeInterval;
                  getDestination|=true;
                 }
                 if(renewDestinationMyEnemyIsMovingTimer>0f){
                  renewDestinationMyEnemyIsMovingTimer-=Time.deltaTime;
                 }
                 if(renewDestinationMyEnemyIsMovingTimer<=0f){
                  if(myEnemyMoved){
                   renewDestinationMyEnemyIsMovingTimer=renewDestinationMyEnemyIsMovingTimeInterval;
                   myEnemyMoved=false;
                   getDestination|=true;
                  }
                 }
                 if(stoppedPredictingMyEnemyDestCanPredictAgainTimer>0f){
                  stoppedPredictingMyEnemyDestCanPredictAgainTimer-=Time.deltaTime;
                 }
                 if(getDestination){
                  if(shouldPredictMyEnemyDest){
                   if(stoppedPredictingMyEnemyDestCanPredictAgainTimer<=0f){
                    predictMyEnemyDest=true;
                   }
                  }
                  if(predictMyEnemyDest){
                   if(!shouldPredictMyEnemyDest){
                    predictMyEnemyDest=false;
                    stoppedPredictingMyEnemyDestCanPredictAgainTimer=stoppedPredictingMyEnemyDestCanPredictAgainTimeInterval;
                   }
                  }
                  ai.MyDest=ai.MyEnemy.transform.position;
                  if(predictMyEnemyDest){
                   ai.MyDest=ai.MyEnemy.transform.position+myEnemyBaseAI.characterController.transform.forward*predictMyEnemyDestDis;
                  }else{
                   if(me.characterController!=null){
                    if(inTheWayColliderHitsCount>0){
                     for(int i=0;i<inTheWayColliderHitsCount;++i){
                      RaycastHit hit=inTheWayColliderHits[i];
                      if(
                       hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit&&
                       actorHit.characterController!=null&&
                       (actorHit.transform.root.position-me.transform.root.position).sqrMagnitude<(MyEnemy.transform.root.position-me.transform.root.position).sqrMagnitude
                      ){
                       Log.DebugMessage("'there's someone between me and my enemy':"+actorHit.name);
                       Vector3 cross=Vector3.Cross(me.transform.root.position,actorHit.transform.root.position);
                       Debug.DrawLine(actorHit.transform.root.position,me.transform.root.position,Color.gray,1f);
                       Debug.DrawRay(actorHit.transform.root.position,cross,Color.gray,1f);
                       Vector3 right=cross;
                       right=Vector3.ProjectOnPlane(right,Vector3.up);
                       right.Normalize();
                       Debug.DrawRay(actorHit.transform.root.position,right,Color.gray,1f);
                       Vector3 cross2=Vector3.Cross(actorHit.transform.root.position+right,actorHit.transform.root.position+Vector3.up);
                       Vector3 forward=cross2;
                       forward=Vector3.ProjectOnPlane(forward,Vector3.up);
                       forward.Normalize();
                       Debug.DrawRay(actorHit.transform.root.position,forward,Color.gray,1f);
                       int sign=1;
                       float disFromActor1=3.0f;
                       float disFromActor2=1.5f;
                       switch(avoidMode){
                        default:{
                         sign=me.math_random.CoinFlip()?-1:1;
                         disFromActor1=(float)me.math_random.NextDouble(2.0d,6d);
                         disFromActor2=(float)me.math_random.NextDouble(1.0d,6d);
                         avoidMode=me.math_random.CoinFlip()?AvoidMode.MoveLeft:AvoidMode.MoveRight;
                         break;
                        }
                        case(AvoidMode.MoveLeft):{
                         sign=-1;
                         disFromActor1=(float)me.math_random.NextDouble(3.0d,6.0d);
                         disFromActor2=(float)me.math_random.NextDouble(1.5d,3.0d);
                         avoidMode=me.math_random.CoinFlip()?AvoidMode.MoveLeft:AvoidMode.MoveRandom;
                         break;
                        }
                        case(AvoidMode.MoveRight):{
                         sign=1;
                         disFromActor1=(float)me.math_random.NextDouble(3.0d,6.0d);
                         disFromActor2=(float)me.math_random.NextDouble(1.5d,3.0d);
                         avoidMode=me.math_random.CoinFlip()?AvoidMode.MoveRight:AvoidMode.MoveRandom;
                         break;
                        }
                       }
                       ai.MyDest=
                        actorHit.transform.root.position+
                         ((right*sign)*disFromActor1-forward*disFromActor2)*
                          (actorHit.characterController.character.radius+me.characterController.character.radius)+
                           Vector3.down*(me.height/2f);
                       break;
                      }
                     }
                    }
                   }
                  }
                 }
                 ai.DoSkill();
                 me.Move(ai.MyDest);
                 previousMyEnemyPos=myEnemyPos;
                }
             internal Coroutine getDataCoroutine;
             protected WaitUntil getDataThrottler;
              protected float getDataThrottlerInterval=.125f;
               protected float getDataThrottlerTimer;
             protected RaycastHit[]inTheWayColliderHits=new RaycastHit[8];
              protected int inTheWayColliderHitsCount=0;
                internal virtual IEnumerator GetDataCoroutine(){
                 getDataThrottler=new WaitUntil(
                  ()=>{
                   if(getDataThrottlerTimer>0f){
                    getDataThrottlerTimer-=Time.deltaTime;
                   }
                   if(MyState==State.CHASE_ST){
                    if(MyEnemy==null){
                     return false;
                    }
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
                  //Log.DebugMessage("OnChaseGetDataCoroutine");
                  if(me.characterController!=null&&MyEnemy!=null){
                   var values=me.simCollisions.GetCapsuleValuesForCollisionTesting(me.characterController.character,me.transform.root);
                   float maxDis=Vector3.Distance(MyEnemy.transform.position,me.transform.root.position);
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
                    }
                    Array.Sort(inTheWayColliderHits,InTheWayColliderHitsArraySortComparer);
                   }
                  }
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
    }
}