#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     [NonSerialized]internal PersistentStats persistentStats;
        internal struct PersistentStats{
         [NonSerialized]public ListWrapper<StatsFloatData>statsFloats;
            public struct StatsFloatData{
             public Type simStatsType;public string fieldName;public float fieldValue;
            }
         [NonSerialized]public ListWrapper<StatsIntData  >statsInts  ;
            public struct StatsIntData  {
             public Type simStatsType;public string fieldName;public int   fieldValue;
            }
         [NonSerialized]public ListWrapper<StatsBoolData >statsBools ;
            public struct StatsBoolData {
             public Type simStatsType;public string fieldName;public bool  fieldValue;
            }
         [NonSerialized]static readonly object fieldsSync=new object();
          [NonSerialized]static readonly Type[]fieldTypesToScan=new Type[]{typeof(float),typeof(int),typeof(bool),};
           [NonSerialized]static readonly HashSet<Type>fieldTypesMissing=new HashSet<Type>();
            [NonSerialized]static readonly Dictionary<Type,Dictionary<Type,FieldInfo[]>>fieldTypesFound=new();
          [NonSerialized]static readonly Dictionary<Type,List<FieldInfo>>statsFieldsGotten=new();
          [NonSerialized]static readonly Dictionary<Type,Dictionary<Type,Dictionary<Type,FieldInfo[]>>>statsFields=new();
            internal void UpdateData(SimObject simObject){
             //Log.DebugMessage("PersistentStats:UpdateData");
             if(simObject==null){
              //Log.DebugMessage("PersistentStats:'collecting persistent stats from destroyed sim on exit save'");
             }
             simObject.stats.OnRefresh(simObject);
             Type statsType=simObject.stats.GetType();
             Dictionary<Type,FieldInfo[]>floats;
             Dictionary<Type,FieldInfo[]>ints  ;
             Dictionary<Type,FieldInfo[]>bools ;
             lock(fieldsSync){
              if(statsFieldsGotten.Count<=0){
               foreach(Type typeToScan in fieldTypesToScan){
                statsFieldsGotten.Add(typeToScan,new());
               }
              }
              if(!statsFields.TryGetValue(statsType,out var fieldsByType)){
               statsFields.Add(statsType,fieldsByType=new());
               fieldTypesMissing.UnionWith(fieldTypesToScan);
              }else{
               foreach(Type typeToScan in fieldTypesToScan){
                if(!fieldsByType.TryGetValue(typeToScan,out var fields)){
                 fieldTypesMissing.Add(typeToScan);
                }else{
                 fieldTypesFound.Add(typeToScan,fields);
                }
               }
              }
              if(fieldTypesMissing.Count>0){
               Type derived=statsType;
               do{
                FieldInfo[]fieldsGotten=derived.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
                foreach(FieldInfo field in fieldsGotten){
                 foreach(Type fieldTypeMissing in fieldTypesMissing){
                  if(field.FieldType==fieldTypeMissing&&(field.Name.EndsWith("_value")||field.Name.EndsWith("_stats"))){
                   String.Intern(field.Name);
                   //Log.DebugMessage("fieldTypeMissing:"+fieldTypeMissing+"..."+derived+":'found stats field':field.Name:"+field.Name);
                   statsFieldsGotten[fieldTypeMissing].Add(field);
                  }
                 }
                }
                foreach(var fieldsTypeStatsFieldsToAddPair in statsFieldsGotten){
                 Type fieldsType=fieldsTypeStatsFieldsToAddPair.Key;
                 var fieldsToAdd=fieldsTypeStatsFieldsToAddPair.Value;
                 if(!fieldsByType.TryGetValue(fieldsType,out var fieldsOfType)){
                  fieldsByType.Add(fieldsType,fieldsOfType=new());
                 }
                 if(fieldTypesMissing.Contains(fieldsType)){
                  fieldsOfType.Add(derived,fieldsToAdd.ToArray());
                 }
                 fieldsToAdd.Clear();
                }
                derived=derived.BaseType;
               }while(derived!=null&&derived!=typeof(object));
              }
              fieldTypesMissing.Clear();
               fieldTypesFound.Clear();
              statsFields[statsType].TryGetValue(typeof(float),out floats);
              statsFields[statsType].TryGetValue(typeof(int  ),out ints  );
              statsFields[statsType].TryGetValue(typeof(bool ),out bools );
             }
             statsFloats=new ListWrapper<StatsFloatData>(
              floats.SelectMany(
               kvp=>{
                return kvp.Value.Select(
                 field=>{
                  //Log.DebugMessage("simStatsType:"+kvp.Key+":field.Name:"+field.Name+":'field value':"+((float)field.GetValue(simObject.stats)));
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
             statsInts  =new ListWrapper<StatsIntData  >(
              ints  .SelectMany(
               kvp=>{
                return kvp.Value.Select(
                 field=>{
                  //Log.DebugMessage("simStatsType:"+kvp.Key+":field.Name:"+field.Name+":'field value':"+((int  )field.GetValue(simObject.stats)));
                  return new StatsIntData  {
                   simStatsType=kvp.Key,
                   fieldName=field.Name,
                   fieldValue=(int  )field.GetValue(simObject.stats),
                  };
                 }
                );
               }
              ).ToList()
             );
             statsBools =new ListWrapper<StatsBoolData >(
              bools .SelectMany(
               kvp=>{
                return kvp.Value.Select(
                 field=>{
                  //Log.DebugMessage("simStatsType:"+kvp.Key+":field.Name:"+field.Name+":'field value':"+((bool )field.GetValue(simObject.stats)));
                  return new StatsBoolData {
                   simStatsType=kvp.Key,
                   fieldName=field.Name,
                   fieldValue=(bool )field.GetValue(simObject.stats),
                  };
                 }
                );
               }
              ).ToList()
             );
            }
         private static readonly ConcurrentQueue<StringBuilder>stringBuilderPool=new ConcurrentQueue<StringBuilder>();
            public override string ToString(){
             if(!stringBuilderPool.TryDequeue(out StringBuilder stringBuilder)){
              stringBuilder=new StringBuilder();
             }
             stringBuilder.Clear();
             string result=string.Format(CultureInfoUtil.en_US,"persistentStats={{ {0}, }}",stringBuilder.ToString());
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
        }
     [NonSerialized]internal static readonly Dictionary<Type,Queue<Stats>>statsPool=new Dictionary<Type,Queue<Stats>>();
     [NonSerialized]internal Stats stats;
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
             //Log.DebugMessage("static Stats():'cache values'");
             int statPointsFor99=GetStatPointsSpentFor(99);
             //Log.DebugMessage("static Stats():GetStatPointsSpentFor(99):"+statPointsFor99);
             //Debug.Assert(GetStatPointsSpentFor(99)==statPointsFor99,"static Stats():GetStatPointsSpentFor(99):'wrong cached value'");
             int statPoints=GetStatPointsSpentFor(130);
             //Log.DebugMessage("static Stats():GetStatPointsSpentFor(130):"+statPoints);
             //Debug.Assert(GetStatPointsSpentFor(130)==statPoints,"static Stats():GetStatPointsSpentFor(130):'wrong cached value'");
             for(int statLevel=1;statLevel<130;++statLevel){
              statPoints=GetStatPointsRequired(statLevel,statLevel+1);
              //Log.DebugMessage("static Stats():GetStatPointsRequired("+statLevel+","+(statLevel+1)+"):"+statPoints);
              //Debug.Assert(GetStatPointsRequired(statLevel,statLevel+1)==statPoints,"static Stats():GetStatPointsRequired("+statLevel+","+(statLevel+1)+"):'wrong cached value'");
             }
             int totalStatPoints=AddStatPointsFrom201To250(250,false);
             Log.DebugMessage("static Stats():AddStatPointsFrom201To250(250,false):"+totalStatPoints);
             //Debug.Assert(AddStatPointsFrom201To250(250,false)==totalStatPoints,"static Stats():AddStatPointsFrom201To250(250,false):'wrong cached value'");
             totalStatPoints=AddStatPointsFrom201To250(250,true);
             Log.DebugMessage("static Stats():AddStatPointsFrom201To250(250,true):"+totalStatPoints);
             //Debug.Assert(AddStatPointsFrom201To250(250,true)==totalStatPoints,"static Stats():AddStatPointsFrom201To250(250,true):'wrong cached value'");
            }
         [NonSerialized]internal static System.Random seedGenerator;
         [NonSerialized]internal readonly System.Random math_random;
            internal Stats(){
             math_random=new System.Random(seedGenerator.Next());
            }
            internal virtual void InitFrom(PersistentStats persistentStats,SimObject statsSim=null){
             //Log.DebugMessage("Stats:InitFrom");
            }
            internal virtual void Generate(SimObject statsSim=null,bool reset=true){
             //Log.DebugMessage("Stats:Generate");
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