#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal float onSnipeRetreatTime=3f;
     internal float onSnipeRetreatDis;
     protected bool onSnipeAlternateRetreatShoot=false;
        protected virtual void OnSNIPE_ST_Routine(){
         Log.DebugMessage("OnSNIPE_ST_Routine()");
         Vector3 attackDistanceWithWeapon=AttackDistance(true);
         float myMoveSpeed=Mathf.Max(moveMaxVelocity.x,moveMaxVelocity.y,moveMaxVelocity.z);
         float myEnemyMoveSpeed=0f;
         if(MyEnemy is BaseAI myEnemyAI){
          myEnemyMoveSpeed=Mathf.Max(myEnemyAI.moveMaxVelocity.x,myEnemyAI.moveMaxVelocity.y,myEnemyAI.moveMaxVelocity.z);
         }
         float dis1=     myMoveSpeed*onSnipeRetreatTime;
         float dis2=myEnemyMoveSpeed*onSnipeRetreatTime;
         float ratio;
         if(dis2<=0f){
          ratio=1f;
         }else{
          ratio=dis1/dis2;
         }
         onSnipeRetreatDis=ratio*dis1;
         Log.DebugMessage("onSnipeRetreatDis:"+onSnipeRetreatDis);
         //
         bool alternateRoutineAction=false;
         bool moveToDestination=false;
         if(
          !IsTraversingPath()
         ){
          if(Vector3.Distance(transform.position,MyEnemy.transform.position)<=onSnipeRetreatDis){
           moveToDestination|=true;
          }
         }
         if(!onSnipeAlternateRetreatShoot){
          Log.DebugMessage("OnSNIPE_ST_Routine():move");
          if(moveToDestination){
           Vector3 dir=(transform.position-MyEnemy.transform.position).normalized;
           MyDest=MyEnemy.transform.position+dir*onSnipeRetreatDis;
           navMeshAgent.destination=MyDest;
          }
          if(
           !IsTraversingPath()
          ){
           alternateRoutineAction=true;
          }
         }
         if(onSnipeAlternateRetreatShoot){
          Log.DebugMessage("OnSNIPE_ST_Routine():shoot");
          if(OnSNIPE_ST_Shoot()){
           alternateRoutineAction=true;
          }
         }
         if(alternateRoutineAction){
          onSnipeAlternateRetreatShoot=!onSnipeAlternateRetreatShoot;
         }
        }
        protected virtual bool OnSNIPE_ST_Shoot(){
         return false;
        }
    }
}