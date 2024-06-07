#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
     internal SimObject enemy{get{return ai?.MyEnemy;}}
        internal partial class AI{
         internal SimObject MyEnemy=null;
        }
        internal enum GotTargetMode:int{
         FromMaster=0,
         FromSlave=1,
         FromFriends=2,
         Defensively=3,
         Aggressively=4,
        }
        internal enum EnemyPriority:int{
         High=0,
         Medium=1,
         Low=2,
        }
     internal readonly SortedList<GotTargetMode,SortedList<EnemyPriority,Dictionary<(Type simType,ulong number),SimObject>>>targetsGotten=new();
        internal virtual void InitTargets(){
         foreach(int i in Enum.GetValues(typeof(GotTargetMode))){
          targetsGotten.Add((GotTargetMode)i,new SortedList<EnemyPriority,Dictionary<(Type simType,ulong number),SimObject>>());
          foreach(int j in Enum.GetValues(typeof(EnemyPriority))){
           targetsGotten[(GotTargetMode)i].Add((EnemyPriority)j,new Dictionary<(Type simType,ulong number),SimObject>());
          }
         }
        }
        internal virtual void ReleaseTargets(){
         foreach(var kvp1 in targetsGotten){
          foreach(var kvp2 in kvp1.Value){
           foreach(var kvp3 in kvp2.Value){
            targetsToRemove.Add(kvp3.Key);
           }
           kvp2.Value.Clear();
          }
         }
         foreach((Type simType,ulong number)id in targetsToRemove){
          Log.DebugMessage("target to remove:"+id);
          OnRemoveTarget(id);
         }
         targetsToRemove.Clear();
         ai.MyEnemy=null;
        }
      internal readonly Dictionary<(Type simType,ulong number),(GotTargetMode mode,EnemyPriority priority)>targetsByPriority=new Dictionary<(Type,ulong),(GotTargetMode,EnemyPriority)>();
       internal readonly Dictionary<(Type simType,ulong number),float>targetTimeouts=new Dictionary<(Type,ulong),float>();
       internal readonly Dictionary<(Type simType,ulong number),float>targetDis     =new Dictionary<(Type,ulong),float>();
        internal readonly HashSet<(Type simType,ulong number)>targetsToRemove=new HashSet<(Type,ulong)>();
     [SerializeField]protected float renewEnemyInterval=3f;
      protected float renewEnemyTimer=3f;
        internal virtual void RenewTargets(){
         if(ai.MyEnemy!=null){
          if(ai.MyEnemy.id==null||(targetTimeouts.TryGetValue(ai.MyEnemy.id.Value,out float timeout)&&timeout-Time.deltaTime<=0f)){
           ai.MyEnemy=null;
          }else{
           renewEnemyTimer-=Time.deltaTime;
           if(renewEnemyTimer<=0f){
            renewEnemyTimer=renewEnemyInterval;
            ai.MyEnemy=null;
           }
          }
         }else{
          renewEnemyTimer=renewEnemyInterval;
         }
         SimObject myEnemy=ai.MyEnemy;
         foreach(var targetsByGottenMode in targetsGotten){
          GotTargetMode gotTargetMode=targetsByGottenMode.Key;
          foreach(var targetsByPriority in targetsByGottenMode.Value){
           EnemyPriority priority=targetsByPriority.Key;
           float closestDis=float.MaxValue;
           foreach(var idTargetPair in targetsByPriority.Value){
            (Type simType,ulong number)id=idTargetPair.Key;
            if(idTargetPair.Value==null||idTargetPair.Value.id==null||idTargetPair.Value.id.Value!=id){
             targetsToRemove.Add(id);
             continue;
            }
            if(idTargetPair.Value.IsDead()){
             targetsToRemove.Add(id);
             continue;
            }
            if(targetTimeouts[id]>=0f){
             targetTimeouts[id]-=Time.deltaTime;
             if(targetTimeouts[id]<=0f){
              targetsToRemove.Add(id);
              continue;
             }
            }
            SimObject target=idTargetPair.Value;
            targetDis[id]=Vector3.Distance(transform.position,target.transform.position);
            if(myEnemy==null&&targetDis[id]<closestDis){
             closestDis=targetDis[id];
             ai.MyEnemy=target;
            }
           }
           myEnemy=ai.MyEnemy;
          }
         }
         foreach((Type simType,ulong number)id in targetsToRemove){
          Log.DebugMessage("target to remove:"+id);
          OnRemoveTarget(id);
         }
         targetsToRemove.Clear();
        }
        protected void OnAddTarget(SimObject target,GotTargetMode gotTargetMode,EnemyPriority enemyPriority){
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          OnRemoveTarget(target.id.Value,false);
         }
         targetsGotten[gotTargetMode][enemyPriority].Add(target.id.Value,target);
         targetsByPriority.Add(target.id.Value,(gotTargetMode,enemyPriority));
         targetTimeouts[target.id.Value]=-1;
         targetDis     [target.id.Value]=Vector3.Distance(transform.position,target.transform.position);
         if(ai!=null){
          ai.damageSources[target]=ai.damageSourceForgiveTime;
         }
        }
        protected void OnRemoveTarget((Type simType,ulong number)id,bool validateMyEnemy=true){
         if(targetsByPriority.TryGetValue(id,out(GotTargetMode mode,EnemyPriority priority)modeAndPriority)){
          targetsGotten[modeAndPriority.mode][modeAndPriority.priority].Remove(id);
          targetsByPriority.Remove(id);
          targetTimeouts.Remove(id);
          targetDis     .Remove(id);
         }
         if(validateMyEnemy){
          if(ai.MyEnemy!=null){
           if(ai.MyEnemy.id==id){
            ai.MyEnemy=null;
           }
          }
         }
        }
        internal virtual void SetTargetToBeRemoved(SimObject target,float afterSeconds=30f){
         if(target.id==null){
          return;
         }
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          //Log.DebugMessage("target set to be removed:"+target.id.Value);
          targetTimeouts[target.id.Value]=afterSeconds;
         }
        }
        internal virtual void ApplyAggressionModeForThenAddTarget(SimObject target,SimObject targetOfTarget=null,bool hit=true){
         bool NoHitIfNotUsingAI(BaseAI ai){
          if(!ai.isUsingAI){
           if(hit){
            return false;
           }
          }
          return true;
         }
         if(target.id==null){
          return;
         }
         if(target.id==id){
          return;
         }
         if(target.id==masterId){
          if(masterSimObject is BaseAI masterAI&&(masterAI.enemy!=this||NoHitIfNotUsingAI(masterAI))){
           return;
          }
         }
         if(slaves.Contains(target.id.Value)){
          if(target is BaseAI targetAI&&(targetAI.enemy!=this||NoHitIfNotUsingAI(targetAI))){
           return;
          }
         }
         if(target.IsDead()){
          return;
         }
         void IfUsingAISetTimeout(){
          if(target is BaseAI targetAI&&targetAI.isUsingAI&&(!targetTimeouts.TryGetValue(target.id.Value,out float timeout)||timeout-Time.deltaTime<=0f)){
           SetTargetToBeRemoved(target,5f);
          }
         }
         if(MyAggressionMode==AggressionMode.AggressiveToAll){
          if(target is SimActor targetSimActor&&!target.IsMonster()){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Aggressively);
           return;
          }
         }else if(targetOfTarget!=null){
          if(targetOfTarget.id==null){
           return;
          }
          if(masterId==targetOfTarget.id){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromMaster);
           IfUsingAISetTimeout();
           return;
          }
          if(slaves.Contains(targetOfTarget.id.Value)){
           ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromSlave);
           IfUsingAISetTimeout();
           return;
          }
         }
         ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Defensively);
         IfUsingAISetTimeout();
        }
        internal virtual void ApplyEnemyPriorityForThenAddTarget(SimObject target,GotTargetMode gotTargetMode){
         if(target.id==null){
          return;
         }
         EnemyPriority enemyPriority=EnemyPriority.Low;
         //Log.DebugMessage("target to add:"+target.id.Value);
         OnAddTarget(target,gotTargetMode,enemyPriority);
        }
    }
}