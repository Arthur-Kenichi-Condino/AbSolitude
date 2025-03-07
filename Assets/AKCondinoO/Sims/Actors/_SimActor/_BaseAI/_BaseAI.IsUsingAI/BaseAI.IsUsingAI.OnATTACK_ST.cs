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
         [NonSerialized]internal ATTACK_ST attackSt;
            internal class ATTACK_ST:ST{
                internal ATTACK_ST(BaseAI me,AI ai):base(me,ai){
                }
             [NonSerialized]internal bool firstAttack;
             [NonSerialized]AvoidMode avoidMode=AvoidMode.MoveAway;
                enum AvoidMode:int{
                 MoveAway=0,
                 MoveRandom=1,
                }
             [NonSerialized]bool shouldAvoid;
              [NonSerialized]bool avoiding;
               [NonSerialized]bool avoidingMoving;
                [NonSerialized]bool hasJustAvoided;
                 [NonSerialized]float hasFriendlyTargetsToAvoidMaxTime=8f;
                 [NonSerialized]float hasFriendlyTargetsToAvoidTimer;
                internal void Finish(){
                 firstAttack=false;
                }
                internal void Start(){
                 firstAttack=true;
                 targetsToAvoidRefreshedFlag=false;
                 avoidMode=AvoidMode.MoveAway;
                 shouldAvoid=false;
                 avoiding=false;
                 avoidingMoving=false;
                }
                internal void DoRoutine(){
                 if(MyEnemy==null){
                  me.MoveStop();
                  return;
                 }
                 if(firstAttack){
                  //Log.DebugMessage("'firstAttack'");
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
                   //Log.DebugMessage("'stop avoiding':ai.MyPathfinding:"+ai.MyPathfinding);
                   shouldAvoid=false;
                   avoiding=false;
                   avoidingMoving=false;
                   hasJustAvoided=true;
                  }else{
                   //Log.DebugMessage("'I am avoiding'");
                   //Debug.DrawLine(me.transform.root.position,ai.MyDest,Color.gray,1f);
                  }
                 }
                 if(targetsToAvoidRefreshedFlag){
                  targetsToAvoidRefreshedFlag=false;
                  shouldAvoid=hasFriendlyTargetsToAvoid.Count>0;
                 }
                 if(!shouldAvoid){
                  avoiding=false;
                  avoidingMoving=false;
                  hasFriendlyTargetsToAvoidTimer=0f;
                 }else{
                  hasFriendlyTargetsToAvoidTimer+=Time.deltaTime;
                 }
                 //Log.DebugMessage("shouldAvoid:"+shouldAvoid);
                 if(shouldAvoid&&!hasJustAvoided){
                  if(!me.IsAttacking()){
                   if(hasFriendlyTargetsToAvoidTimer>=hasFriendlyTargetsToAvoidMaxTime){
                    hasFriendlyTargetsToAvoidTimer=0f;
                    me.MoveStop();
                    //Log.DebugMessage("'TeleportToRandomNearMyEnemy(ai.attackDistance)'");
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
                         //Log.DebugMessage("hasFriendlyTargetsToAvoid["+i+"].sim.name:"+hasFriendlyTargetsToAvoid[i].sim.name);
                         RaycastHit hit=hasFriendlyTargetsToAvoid[i].hit;
                         SimObject simHit=hasFriendlyTargetsToAvoid[i].sim;
                         Vector3 dirFromEnemyToAlly=(simHit.transform.root.position-MyEnemy.transform.root.position).normalized;
                         dirFromEnemyToAlly.y=0f;
                         //Debug.DrawRay(MyEnemy.transform.root.position,dirFromAllyToEnemy,Color.gray,1f);
                         Vector3 dirFromEnemyToMe=(me.transform.root.position-MyEnemy.transform.root.position).normalized;
                         dirFromEnemyToMe.y=0f;
                         //Debug.DrawRay(MyEnemy.transform.root.position,dirFromEnemyToMe,Color.gray,1f);
                         float angle=180f-Vector3.Angle(dirFromEnemyToMe,dirFromEnemyToAlly);
                         destAngle+=angle;
                        }
                        if(destAngle!=0f){
                         destAngle/=hasFriendlyTargetsToAvoid.Count;
                         destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
                         //Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.gray,1f);
                        }
                        Vector3 dest=MyEnemy.transform.root.position+(destDir*ai.attackDistance.z*1.1f);
                        //Debug.DrawLine(me.transform.root.position,dest,Color.gray,1f);
                        ai.MyDest=dest;
                        //Log.DebugMessage("'avoiding, move away'");
                        avoidMode=AvoidMode.MoveRandom;
                        break;
                       }
                       case(AvoidMode.MoveRandom):{
                        Vector3 destDir=(me.transform.root.position-MyEnemy.transform.root.position).normalized;
                        destDir.y=0f;
                        float destAngle=0f;
                        destAngle+=(float)me.math_random.NextDouble(-180f,180f);
                        if(destAngle!=0f){
                         destDir=Quaternion.AngleAxis(destAngle,Vector3.up)*destDir;
                         //Debug.DrawRay(MyEnemy.transform.root.position,destDir,Color.gray,1f);
                        }
                        Vector3 dest=MyEnemy.transform.root.position+(destDir*ai.attackDistance.z*1.1f);
                        //Debug.DrawLine(me.transform.root.position,dest,Color.gray,1f);
                        ai.MyDest=dest;
                        //Log.DebugMessage("'avoiding, random move'");
                        avoidMode=AvoidMode.MoveAway;
                        break;
                       }
                      }
                      avoiding=true;
                     }
                     if(me.Move(ai.MyDest)){
                      avoidingMoving=true;
                      //Log.DebugMessage("'avoiding, move!'");
                     }
                    }
                   }
                  }
                 }else{
                  //Log.DebugMessage("'MoveStop()'");
                  me.MoveStop();
                  if(me.Attack(ai.MyEnemy)){
                   hasJustAvoided=false;
                  }
                 }
                }
             [NonSerialized]internal Coroutine getDataCoroutine;
             [NonSerialized]protected WaitUntil getDataThrottler;
              [NonSerialized]protected float getDataThrottlerInterval=.125f;
               [NonSerialized]protected float getDataThrottlerTimer;
             [NonSerialized]protected RaycastHit[]inTheWayColliderHits=new RaycastHit[8];
              [NonSerialized]protected int inTheWayColliderHitsCount=0;
             [NonSerialized]readonly List<(SimObject sim,RaycastHit hit)>hasFriendlyTargetsToAvoid=new List<(SimObject,RaycastHit)>();
             [NonSerialized]bool targetsToAvoidRefreshedFlag;
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
                  //Log.DebugMessage("GetDataCoroutine:Loop");
                  if(me.characterController!=null&&MyEnemy!=null){
                   Quaternion rotation=me.GetRotation();
                   float height=me.GetHeight();
                   float radius=me.GetRadius();
                   Vector3 attackDistance=me.AttackDistance(out _);
                   Vector3 forward=(MyEnemy.transform.root.position-me.transform.root.position).normalized;
                   forward.y=0f;
                   //Log.DebugMessage("attackDistance:"+attackDistance);
                   Vector3 scale=new Vector3(
                    (attackDistance.x)/(radius),
                    (attackDistance.y)/(height),
                    (attackDistance.z)/(radius)
                   );
                   //Log.DebugMessage("scale:"+scale);
                   Vector3 offset=-forward*(radius*2f);
                   var values=me.simCollisions.GetCapsuleValuesForCollisionTesting(me.characterController.character,me.transform.root,scale,offset);
                   //Debug.DrawLine(values.point0,values.point1,Color.gray,getDataThrottlerInterval);
                   //Debug.DrawRay(values.point0,rotation*Vector3.right*radius*scale.x,Color.gray,getDataThrottlerInterval);
                   //Debug.DrawRay(values.point0,rotation*Vector3.forward*radius*scale.z,Color.gray,getDataThrottlerInterval);
                   float maxDis=radius*2f+attackDistance.z;
                   //Debug.DrawLine(me.transform.position+offset,me.transform.position+offset+(forward*maxDis),Color.gray,getDataThrottlerInterval);
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
                        //Log.DebugMessage("'I need to avoid hitting a friendly target':"+actorHit.name);
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
    }
}