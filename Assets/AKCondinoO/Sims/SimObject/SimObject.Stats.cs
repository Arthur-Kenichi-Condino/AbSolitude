#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal PersistentStats persistentStats;
        internal struct PersistentStats{
         public ListWrapper<StatsFloatData>statsFloats;
            public struct StatsFloatData{
             public Type simStatsType;public string fieldName;public float fieldValue;
            }
            static readonly Dictionary<Type,Dictionary<Type,FieldInfo[]>>statsFloatFields=new Dictionary<Type,Dictionary<Type,FieldInfo[]>>();
            internal void UpdateData(SimObject simObject){
             //Log.DebugMessage("UpdateData");
             if(simObject==null){
              //Log.DebugMessage("collecting persistent stats from destroyed sim on exit save");
             }
             simObject.stats.OnRefresh(simObject);
             Type statsType=simObject.stats.GetType();
             Dictionary<Type,FieldInfo[]>floatFields;
             lock(statsFloatFields){
              if(!statsFloatFields.TryGetValue(statsType,out floatFields)){
               floatFields=new Dictionary<Type,FieldInfo[]>();
               Type derived=statsType;
               do{
                FieldInfo[]fieldsGotten=derived.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Where(field=>{if(field.FieldType==typeof(float)&&(field.Name.EndsWith("_value")||field.Name.EndsWith("_stats"))){String.Intern(field.Name);Log.DebugMessage(derived+": got field:"+field.Name);return true;}return false;}).ToArray();
                floatFields.Add(derived,fieldsGotten);
                derived=derived.BaseType;
               }while(derived!=null&&derived!=typeof(object));
               statsFloatFields.Add(statsType,floatFields);
              }
             }
             statsFloats=new ListWrapper<StatsFloatData>(
              floatFields.SelectMany(
               kvp=>{
                return kvp.Value.Select(
                 field=>{
                  //Log.DebugMessage("simStatsType:"+kvp.Key+";fieldName:"+field.Name+";fieldValue:"+((float)field.GetValue(simObject.stats)));
                  return new StatsFloatData{
                   simStatsType=kvp.Key,
                   fieldName=field.Name,
                   fieldValue=(float)field.GetValue(simObject.stats),
                  };
                 }
                );
               }
              ).ToList()
             );
            }
        }
     internal static readonly Dictionary<Type,Queue<Stats>>statsPool=new Dictionary<Type,Queue<Stats>>();
     internal Stats stats;
        internal virtual void RenewStats(){
         //Log.DebugMessage("RenewStats");
         if(stats==null){
          if(statsPool.TryGetValue(typeof(Stats),out Queue<Stats>pool)&&pool.Count>0){
           stats=pool.Dequeue();
          }else{
           stats=new Stats();
          }
         }
        }
        internal virtual void ReleaseStats(){
         //Log.DebugMessage("ReleaseStats");
         if(stats!=null){
          Type statsType=stats.GetType();
          if(!statsPool.TryGetValue(statsType,out Queue<Stats>pool)){
           statsPool.Add(statsType,pool=new Queue<Stats>());
          }
          pool.Enqueue(stats);
         }
         stats=null;
        }
        internal partial class Stats{
         internal static System.Random seedGenerator;
         internal readonly System.Random math_random;
            internal Stats(){
             math_random=new System.Random(seedGenerator.Next());
            }
            internal virtual void InitFrom(PersistentStats persistentStats,SimObject statsSim=null){
             //Log.DebugMessage("Stats InitFrom");
            }
            internal virtual void Generate(SimObject statsSim=null){
             //Log.DebugMessage("Stats Generate");
            }
            internal void OnAppliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
            internal void OnUnappliedSkillBuff(SkillBuff skillBuff){
             pendingRefresh=true;
            }
        }
    }
}