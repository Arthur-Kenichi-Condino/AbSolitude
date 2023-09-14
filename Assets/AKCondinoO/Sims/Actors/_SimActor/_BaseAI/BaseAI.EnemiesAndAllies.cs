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
        internal enum GotTargetMode:int{
         FromOwner=0,
         FromFriends=1,
         Defensively=2,
         Aggressively=3,
        }
        internal enum EnemyPriority:int{
         High=0,
         Medium=1,
         Low=2,
        }
     internal readonly SortedList<GotTargetMode,SortedList<EnemyPriority,Dictionary<(Type simType,ulong number),SimObject>>>targetsGotten=new();
      internal readonly Dictionary<(Type simType,ulong number),(GotTargetMode mode,EnemyPriority priority)>targetsByPriority=new Dictionary<(Type,ulong),(GotTargetMode,EnemyPriority)>();
       internal readonly Dictionary<(Type simType,ulong number),float>targetTimeouts=new Dictionary<(Type,ulong),float>();
       internal readonly Dictionary<(Type simType,ulong number),float>targetDis     =new Dictionary<(Type,ulong),float>();
        internal readonly HashSet<(Type simType,ulong number)>targetsToRemove=new HashSet<(Type,ulong)>();
        protected void OnAddAsTarget(SimObject target,GotTargetMode gotTargetMode,EnemyPriority enemyPriority){
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          OnRemoveFromAsTarget(target.id.Value,false);
         }
         targetsGotten[gotTargetMode][enemyPriority].Add(target.id.Value,target);
         targetsByPriority.Add(target.id.Value,(gotTargetMode,enemyPriority));
         targetTimeouts[target.id.Value]=-1;
         targetDis     [target.id.Value]=Vector3.Distance(transform.position,target.transform.position);
        }
        protected void OnRemoveFromAsTarget((Type simType,ulong number)id,bool validateMyEnemy=true){
         if(targetsByPriority.TryGetValue(id,out(GotTargetMode mode,EnemyPriority priority)modeAndPriority)){
          targetsGotten[modeAndPriority.mode][modeAndPriority.priority].Remove(id);
          targetsByPriority.Remove(id);
          targetTimeouts.Remove(id);
          targetDis     .Remove(id);
         }
         if(validateMyEnemy){
          if(MyEnemy!=null){
           if(MyEnemy.id==id){
            MyEnemy=null;
           }
          }
         }
        }
     [SerializeField]protected float renewEnemyInterval=3f;
      protected float renewEnemyTimer=3f;
        internal virtual void RenewEnemiesAndAllies(){
         if(MyEnemy!=null){
          if(MyEnemy.id==null||(targetTimeouts.TryGetValue(MyEnemy.id.Value,out float timeout)&&timeout-Time.deltaTime<=0f)){
           MyEnemy=null;
          }else{
           renewEnemyTimer-=Time.deltaTime;
           if(renewEnemyTimer<=0f){
            renewEnemyTimer=renewEnemyInterval;
            MyEnemy=null;
           }
          }
         }else{
          renewEnemyTimer=renewEnemyInterval;
         }
         SimObject myEnemy=MyEnemy;
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
             MyEnemy=target;
            }
           }
           myEnemy=MyEnemy;
          }
         }
         foreach((Type simType,ulong number)id in targetsToRemove){
          Log.DebugMessage("target to remove:"+id);
          OnRemoveFromAsTarget(id);
         }
         targetsToRemove.Clear();
        }
        internal virtual void ReleaseEnemiesAndAllies(){
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
          OnRemoveFromAsTarget(id);
         }
         targetsToRemove.Clear();
         MyEnemy=null;
        }
        internal virtual void InitEnemiesAndAllies(){
         foreach(int i in Enum.GetValues(typeof(GotTargetMode))){
          targetsGotten.Add((GotTargetMode)i,new SortedList<EnemyPriority,Dictionary<(Type simType,ulong number),SimObject>>());
          foreach(int j in Enum.GetValues(typeof(EnemyPriority))){
           targetsGotten[(GotTargetMode)i].Add((EnemyPriority)j,new Dictionary<(Type simType,ulong number),SimObject>());
          }
         }
        }
    }
}