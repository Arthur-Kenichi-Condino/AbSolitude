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
                FieldInfo[]fieldsGotten=derived.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Where(
                 field=>{
                  if(field.FieldType==typeof(float)&&(field.Name.EndsWith("_value")||field.Name.EndsWith("_stats"))){
                   String.Intern(field.Name);
                   //Log.DebugMessage(derived+": got field:"+field.Name);
                   return true;
                  }
                  return false;
                 }
                ).ToArray();
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
            static Stats(){
             //  Testando funcionamento de funções
             //int assertingStatPointsSpent=0;
             //int assertingtotalStatPoints=AddStatPointsFrom151To200(200,true);
             //float assertingStat=128;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=89;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //Log.DebugMessage("static Stats():assertingStat:"+assertingStat+";assertingStatPointsSpent:"+assertingStatPointsSpent);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //Log.DebugMessage("static Stats():assertingStat:"+assertingStat+";assertingStatPointsSpent:"+assertingStatPointsSpent);
             int statPointsFor99=GetStatPointsSpentFor(99);
             Log.DebugMessage("static Stats():GetStatPointsSpentFor(99):"+statPointsFor99);
             Debug.Assert(GetStatPointsSpentFor(99)==statPointsFor99,"static Stats():GetStatPointsSpentFor(99):wrong cached value");
             int statPoints=GetStatPointsSpentFor(130);
             Log.DebugMessage("static Stats():GetStatPointsSpentFor(130):"+statPoints);
             Debug.Assert(GetStatPointsSpentFor(130)==statPoints,"static Stats():GetStatPointsSpentFor(130):wrong cached value");
             for(int statLevel=1;statLevel<130;++statLevel){
              statPoints=GetStatPointsRequired(statLevel,statLevel+1);
              Log.DebugMessage("static Stats():GetStatPointsRequired("+statLevel+","+(statLevel+1)+"):"+statPoints);
              Debug.Assert(GetStatPointsRequired(statLevel,statLevel+1)==statPoints,"static Stats():GetStatPointsRequired("+statLevel+","+(statLevel+1)+"):wrong cached value");
             }
             int totalStatPoints=AddStatPointsFrom1To99(99,false);
             Log.DebugMessage("static Stats():AddStatPointsFrom1To99(99,false):"+totalStatPoints);
             Debug.Assert(AddStatPointsFrom1To99(99,false)==totalStatPoints,"static Stats():AddStatPointsFrom1To99(99,false):wrong cached value");
             totalStatPoints=AddStatPointsFrom151To200(200,true);
             Log.DebugMessage("static Stats():AddStatPointsFrom151To200(200,true):"+totalStatPoints);
             Debug.Assert(AddStatPointsFrom151To200(200,true)==totalStatPoints,"static Stats():AddStatPointsFrom151To200(200,true):wrong cached value");
             //  Testando funcionamento de funções (pós-cache)
             //int assertingStatPointsSpent=0;
             //int assertingtotalStatPoints=AddStatPointsFrom151To200(200,true);
             //float assertingStat=128;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=89;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //Log.DebugMessage("static Stats():assertingStat:"+assertingStat+";assertingStatPointsSpent:"+assertingStatPointsSpent);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //assertingStat=1;
             //SpendAllRemainingPointsOn(ref assertingStat,ref assertingStatPointsSpent,assertingtotalStatPoints);
             //Log.DebugMessage("static Stats():assertingStat:"+assertingStat+";assertingStatPointsSpent:"+assertingStatPointsSpent);
            }
         internal static System.Random seedGenerator;
         internal readonly System.Random math_random;
            internal Stats(){
             math_random=new System.Random(seedGenerator.Next());
            }
            internal virtual void InitFrom(PersistentStats persistentStats,SimObject statsSim=null){
             //Log.DebugMessage("Stats InitFrom");
            }
            internal virtual void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats Generate");
             if(reset){
              OnReset(statsSim);
             }
             OnGenerateValidation_Level(statsSim,reset);
             OnGenerateValidation_Stats(statsSim,reset);
             SetPendingRefresh(statsSim,false);
            }
            protected virtual void OnReset(SimObject statsSim=null){
             simLevel_value=0;
              statPointsSpent_value=0;
                 bodily_kinesthetic_value=0f;
                      interpersonal_value=0f;
                      intrapersonal_value=0f;
                         linguistic_value=0f;
               logical_mathematical_value=0f;
                            musical_value=0f;
                       naturalistic_value=0f;
                            spatial_value=0f;
                luck_value=0f;
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