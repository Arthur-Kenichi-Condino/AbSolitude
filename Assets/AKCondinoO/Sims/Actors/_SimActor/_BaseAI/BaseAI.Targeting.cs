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
        protected void OnAddTarget(SimObject target,GotTargetMode gotTargetMode,EnemyPriority enemyPriority){
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
          OnRemoveTarget(target.id.Value,false);
         }
         targetsByPriority.Add(target.id.Value,(gotTargetMode,enemyPriority));
         targetsGotten[gotTargetMode][enemyPriority].Add(target.id.Value,target);
         targetTimeouts[target.id.Value]=-1;
         targetDis     [target.id.Value]=Vector3.Distance(transform.position,target.transform.position);
         //if(ai!=null){
         // ai.damageSources[target]=ai.damageSourceForgiveTime;
         //}
        }
        protected void OnRemoveTarget((Type simType,ulong number)id,bool validateMyEnemy=true){
         if(targetsByPriority.TryGetValue(id,out(GotTargetMode mode,EnemyPriority priority)modeAndPriority)){
          targetsGotten[modeAndPriority.mode][modeAndPriority.priority].Remove(id);
          targetTimeouts.Remove(id);
          targetDis     .Remove(id);
         // if(targetCooldowns.TryGetValue(id,out float cooldown)){
         //  if(cooldown>0f){
         //   targetsOnCooldown[id]=cooldown;
         //  }
         //  targetCooldowns.Remove(id);
         // }
          targetsByPriority.Remove(id);
         }
         //if(validateMyEnemy){
         // if(ai.MyEnemy!=null){
         //  if(ai.MyEnemy.id==id){
         //   ai.MyEnemy=null;
         //  }
         // }
         //}
        }
        internal virtual void SetTargetToBeRemoved(SimObject target,float afterSeconds=30f,float cooldown=1f){
         if(target.id==null){
          return;
         }
         if(targetsByPriority.TryGetValue(target.id.Value,out _)){
         // //Log.DebugMessage("target set to be removed:"+target.id.Value);
          targetTimeouts [target.id.Value]=afterSeconds;
         // targetCooldowns[target.id.Value]=cooldown    ;
         }
        }
        internal virtual void ApplyAggressionModeForThenAddTarget(SimObject target,SimObject allyToHelp=null,bool onHitDamaged=true){
         if(target.id==null){//  invalid
          //Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.id==null: it's invalid'");
          return;
         }
         float setTimeout=targetFastTimeout;
         float setCooldown=0f;
         if(target.IsDead()){
          Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.IsDead()'");
          return;
         }
         if(target.id==id){//  it's me
          Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.id==id: it's me'");
          return;
         }
         if(target.id==masterId){//  it's master
          if(masterSimObject is BaseAI masterAI){
           if(masterAI.isUsingAI){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.id is owner and is using AI'");
            return;
           }else if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.id is owner and it's not damaging me'");
            return;
           }
          }else{
           if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.id is owner and is an object that cannot cause damage'");
            return;
           }
          }
         }
         if(target.masterId!=null&&target.masterId==masterId){
          if(target is BaseAI targetAI){
           if(targetAI.isUsingAI){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.masterId is owner:we are siblings:and is using AI'");
            return;
           }else if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.masterId is owner:we are siblings:and it's not damaging me'");
            return;
           }
          }else{
           if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'target.masterId is owner:we are siblings:and it's an object that cannot cause damage'");
            return;
           }
          }
         }
         if(slaves.Contains(target.id.Value)){
          if(target is BaseAI targetAI){
           if(targetAI.isUsingAI){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'slaves.Contains(target.id.Value):it's my slave:and is using AI'");
            return;
           }else if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'slaves.Contains(target.id.Value):it's my slave:and it's not damaging me'");
            return;
           }
          }else{
           if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'slaves.Contains(target.id.Value):it's my slave:and it's an object that cannot cause damage'");
            return;
           }
          }
         }
         void SetTimeout(){
          if(!targetTimeouts.TryGetValue(target.id.Value,out float timeout)||timeout-Time.deltaTime<=0f){
           SetTargetToBeRemoved(target,setTimeout,setCooldown);
          }
         }
         if(MyAggressionMode==AggressionMode.AggressiveToAll){
          if(target is SimActor targetSimActor){
           if(!targetSimActor.IsMonster()){
            ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Aggressively);
            return;
           }
          }
         }else{
          if(allyToHelp!=null){
           if(allyToHelp.id==null){
            allyToHelp=null;
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'allyToHelp.id==null'");
           }
          }
          if(allyToHelp!=null){
           if(allyToHelp.id==masterId){
            ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromMaster);
            SetTimeout();
            return;
           }else if(slaves.Contains(allyToHelp.id.Value)){
            ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromSlave);
            SetTimeout();
            return;
           }else{
            ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromFriends);
            SetTimeout();
            return;
           }
          }else if(target is BaseAI targetAI){
           //  If Defensive-and-Defensive, also may ignore...
           if(targetAI.aggression!=AggressionMode.AggressiveToAll){
            setCooldown=targetCooldownAfterFastTimeout;
           }
           if(targetAI.isUsingAI){
            if(targetAI.enemy==null){
             if(targetAI.aggression!=AggressionMode.AggressiveToAll){
              if(!targetAI.IsMonster()){
               Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':AI:enemy is null:and not aggressive");
               return;
              }
             }
             if(!onHitDamaged){
              Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':AI:enemy is null:and no damage");
              return;
             }
            }else if(targetAI.enemy.id==null){
             if(targetAI.aggression!=AggressionMode.AggressiveToAll){
              if(!targetAI.IsMonster()){
               Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':AI:enemy is invalid:and not aggressive");
               return;
              }
             }
             if(!onHitDamaged){
              Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':AI:enemy is invalid:and no damage");
              return;
             }
            }else{
             if(targetAI.enemy.id==masterId){
              ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromMaster);
              SetTimeout();
              return;
             }else if(targetAI.enemy.masterId!=null&&targetAI.enemy.masterId==masterId){
              ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromFriends);
              SetTimeout();
              return;
             }else if(slaves.Contains(targetAI.enemy.id.Value)){
              ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromSlave);
              SetTimeout();
              return;
             }else{
              if(targetAI.enemy!=this){
               if(targetAI.enemy is BaseAI targetAIEnemyAI){
                if(targetAIEnemyAI.aggression!=AggressionMode.AggressiveToAll){
                 if(!targetAIEnemyAI.IsMonster()){
                  ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.FromFriends);
                  SetTimeout();
                  return;
                 }
                }
               }
               if(targetAI.aggression!=AggressionMode.AggressiveToAll){
                if(!targetAI.IsMonster()){
                 Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':I'm not its target:and it's not aggressive");
                 return;
                }
               }
               if(!onHitDamaged){
                Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':I'm not its target:and no damage");
                return;
               }
              }
             }
            }
           }else if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':user-controlled:no damage");
            return;
           }
          }else{
           if(!onHitDamaged){
            Log.DebugMessage("ApplyAggressionModeForThenAddTarget:'ignore non-aggressive target':object that cannot cause damage");
            return;
           }
          }
         }
         ApplyEnemyPriorityForThenAddTarget(target,GotTargetMode.Defensively);
         SetTimeout();
        }
        internal virtual void ApplyEnemyPriorityForThenAddTarget(SimObject target,GotTargetMode gotTargetMode){
         if(target.id==null){
          //Log.DebugMessage("ApplyEnemyPriorityForThenAddTarget:'target.id==null'");
          return;
         }
         //if(targetsOnCooldown.TryGetValue(target.id.Value,out _)){
         // Log.DebugMessage("ApplyEnemyPriorityForThenAddTarget:'on cooldown'");
         // return;
         //}
         EnemyPriority enemyPriority=EnemyPriority.Low;
         //Log.DebugMessage("target to add:"+target.id.Value);
         OnAddTarget(target,gotTargetMode,enemyPriority);
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
          //Log.DebugMessage("target to remove:"+id);
          OnRemoveTarget(id);
         }
         targetsToRemove.Clear();
         ai.MyEnemy=null;
        }
     [NonSerialized]internal float targetTimeout=30f;
      [NonSerialized]internal float targetCooldownAfterTimeout=3f;
      [NonSerialized]internal float targetFastTimeout=10f;
       [NonSerialized]internal float targetCooldownAfterFastTimeout=2f;
     internal readonly Dictionary<(Type simType,ulong number),(GotTargetMode mode,EnemyPriority priority)>targetsByPriority=new Dictionary<(Type,ulong),(GotTargetMode,EnemyPriority)>();
      internal readonly Dictionary<(Type simType,ulong number),float>targetTimeouts =new Dictionary<(Type,ulong),float>();
      internal readonly Dictionary<(Type simType,ulong number),float>targetCooldowns=new Dictionary<(Type,ulong),float>();
      internal readonly Dictionary<(Type simType,ulong number),float>targetDis      =new Dictionary<(Type,ulong),float>();
       internal readonly HashSet<(Type simType,ulong number)>targetsToRemove=new HashSet<(Type,ulong)>();
     internal readonly Dictionary<(Type simType,ulong number),float                                      >targetsOnCooldown=new Dictionary<(Type,ulong),float                        >();
      internal readonly List<(Type simType,ulong number)>targetsOnCooldownIterator=new();
     [NonSerialized]internal readonly Dictionary<SimObject,float>alliesInTrouble=new();
      [NonSerialized]internal readonly List<SimObject>alliesInTroubleIterator=new();
       [NonSerialized]internal float alliesTroubleForgetTimeout=30f;
     [SerializeField]protected float renewEnemyInterval=3f;
      protected float renewEnemyTimer=3f;
        internal virtual void RenewTargets(){
         targetsOnCooldownIterator.AddRange(targetsOnCooldown.Keys);
         foreach(var id in targetsOnCooldownIterator){
          var cooldown=targetsOnCooldown[id]-Time.deltaTime;
          if(cooldown<=0f){
           targetsOnCooldown.Remove(id);
          }else{
           targetsOnCooldown[id]=cooldown;
          }
         }
         targetsOnCooldownIterator.Clear();
         alliesInTroubleIterator.AddRange(alliesInTrouble.Keys);
         foreach(var id in alliesInTroubleIterator){
          var timeout=alliesInTrouble[id]-Time.deltaTime;
          if(timeout<=0f){
           alliesInTrouble.Remove(id);
          }else{
           alliesInTrouble[id]=timeout;
           //Log.Warning("TO DO: move to ally and search for a target");
          }
         }
         alliesInTroubleIterator.Clear();
         if(ai!=null){
          if(ai.MyEnemy!=null){
           if(ai.MyEnemy.id==null){
            ai.MyEnemy=null;
           }else if(
            (targetTimeouts.TryGetValue(ai.MyEnemy.id.Value,out float timeout)&&timeout-Time.deltaTime<=0f)
           ){
            ai.MyEnemy=null;
           }
          // }else{
          //  renewEnemyTimer-=Time.deltaTime;
          //  if(renewEnemyTimer<=0f){
          //   renewEnemyTimer=renewEnemyInterval;
          //   ai.MyEnemy=null;
          //  }
          }else{
          // renewEnemyTimer=renewEnemyInterval;
          }
         }
         SimObject myEnemy=null;
         SimObject newEnemy=null;
         if(ai!=null){
          newEnemy=myEnemy=ai.MyEnemy;
         }
         foreach(var targetsByGottenMode in targetsGotten){
          GotTargetMode gotTargetMode=targetsByGottenMode.Key;
          foreach(var targetsByPriority in targetsByGottenMode.Value){
           EnemyPriority priority=targetsByPriority.Key;
           float closestDis=float.MaxValue;
           foreach(var idTargetPair in targetsByPriority.Value){
            (Type simType,ulong number)id=idTargetPair.Key;
            if(idTargetPair.Value==null){
             targetsToRemove.Add(id);
             continue;
            }
            if(idTargetPair.Value.id==null){
             targetsToRemove.Add(id);
             continue;
            }
            if(idTargetPair.Value.id.Value!=id){
             targetsToRemove.Add(id);
             continue;
            }
         //   if(idTargetPair.Value==null||idTargetPair.Value.id==null||idTargetPair.Value.id.Value!=id){
         //    targetsToRemove.Add(id);
         //    continue;
         //   }
         //   if(idTargetPair.Value.IsDead()){
         //    targetsToRemove.Add(id);
         //    continue;
         //   }
         //   if(targetTimeouts[id]>=0f){
         //    targetTimeouts[id]-=Time.deltaTime;
         //    if(targetTimeouts[id]<=0f){
         //     targetsToRemove.Add(id);
         //     continue;
         //    }
         //   }
         //   SimObject target=idTargetPair.Value;
         //   targetDis[id]=Vector3.Distance(transform.position,target.transform.position);
         //   if(myEnemy==null&&targetDis[id]<closestDis){
         //    closestDis=targetDis[id];
         //    ai.MyEnemy=target;
         //   }
           }
           myEnemy=newEnemy;
           if(ai!=null){
            ai.MyEnemy=myEnemy;
           }
          }
         }
         foreach((Type simType,ulong number)id in targetsToRemove){
          //Log.DebugMessage("target to remove:"+id);
          OnRemoveTarget(id);
         }
         targetsToRemove.Clear();
        }
    }
}