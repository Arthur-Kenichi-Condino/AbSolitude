#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.Sims.SimObject.PersistentStats;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     [NonSerialized]internal PersistentStats persistentStats;
        internal struct PersistentStats{
         [NonSerialized]public ListWrapper<StatsFloatData>statsFloats;
            public struct StatsFloatData{
             public Type simStatsType;public string fieldName;public float fieldValue;
                public override string ToString(){
                 if(fieldValue==default){
                  return"";
                 }
                 return string.Format(CultureInfoUtil.en_US,"[{0},{1},{2}]",simStatsType.Name,fieldName,fieldValue);
                }
                internal static StatsFloatData Parse(string s){
                 StatsFloatData floatData=new StatsFloatData();
                 //Log.DebugMessage("s:"+s);
                 string[]dataString=s.Split(',');
                 Type simStatsType=ReflectionUtil.GetTypeByName(dataString[0],typeof(Stats));
                 //Log.DebugMessage("simStatsType:"+simStatsType);
                 string fieldName=dataString[1];
                 float fieldValue=float.Parse(dataString[2],NumberStyles.Any,CultureInfoUtil.en_US);
                 //Log.DebugMessage("field:"+fieldName+":"+fieldValue);
                 floatData.simStatsType=simStatsType;
                 floatData.fieldName=fieldName;
                 floatData.fieldValue=fieldValue;
                 return floatData;
                }
            }
         [NonSerialized]public ListWrapper<StatsIntData  >statsInts  ;
            public struct StatsIntData  {
             public Type simStatsType;public string fieldName;public int   fieldValue;
                public override string ToString(){
                 if(fieldValue==default){
                  return"";
                 }
                 return string.Format(CultureInfoUtil.en_US,"[{0},{1},{2}]",simStatsType.Name,fieldName,fieldValue);
                }
                internal static StatsIntData Parse(string s){
                 StatsIntData intData=new StatsIntData();
                 //Log.DebugMessage("s:"+s);
                 string[]dataString=s.Split(',');
                 Type simStatsType=ReflectionUtil.GetTypeByName(dataString[0],typeof(Stats));
                 //Log.DebugMessage("simStatsType:"+simStatsType);
                 string fieldName=dataString[1];
                 int fieldValue=int.Parse(dataString[2],NumberStyles.Any,CultureInfoUtil.en_US);
                 //Log.DebugMessage("field:"+fieldName+":"+fieldValue);
                 intData.simStatsType=simStatsType;
                 intData.fieldName=fieldName;
                 intData.fieldValue=fieldValue;
                 return intData;
                }
            }
         [NonSerialized]public ListWrapper<StatsBoolData >statsBools ;
            public struct StatsBoolData {
             public Type simStatsType;public string fieldName;public bool  fieldValue;
                public override string ToString(){
                 if(fieldValue==default){
                  return"";
                 }
                 return string.Format(CultureInfoUtil.en_US,"[{0},{1},{2}]",simStatsType.Name,fieldName,fieldValue);
                }
                internal static StatsBoolData Parse(string s){
                 StatsBoolData boolData=new StatsBoolData();
                 //Log.DebugMessage("s:"+s);
                 string[]dataString=s.Split(',');
                 Type simStatsType=ReflectionUtil.GetTypeByName(dataString[0],typeof(Stats));
                 //Log.DebugMessage("simStatsType:"+simStatsType);
                 string fieldName=dataString[1];
                 bool fieldValue=bool.Parse(dataString[2]);
                 //Log.DebugMessage("field:"+fieldName+":"+fieldValue);
                 boolData.simStatsType=simStatsType;
                 boolData.fieldName=fieldName;
                 boolData.fieldValue=fieldValue;
                 return boolData;
                }
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
                FieldInfo[]fieldsGotten=derived.GetFields(BindingFlags.DeclaredOnly|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
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
              statsFields[statsType].TryGetValue(typeof(float),out floats);//Log.DebugMessage("floats.Count:"+floats.Count);
              statsFields[statsType].TryGetValue(typeof(int  ),out ints  );//Log.DebugMessage(  "ints.Count:"+  ints.Count);
              statsFields[statsType].TryGetValue(typeof(bool ),out bools );//Log.DebugMessage( "bools.Count:"+ bools.Count);
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
                   fieldValue=(float)field.GetValueOptimized(simObject.stats),
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
                   fieldValue=(int  )field.GetValueOptimized(simObject.stats),
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
                   fieldValue=(bool )field.GetValueOptimized(simObject.stats),
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
             bool stringBuilderHasData=false;
             stringBuilderHasData|=buildStringStatsOfType(statsFloats,nameof(statsFloats));
             stringBuilderHasData|=buildStringStatsOfType(statsInts  ,nameof(statsInts  ));
             stringBuilderHasData|=buildStringStatsOfType(statsBools ,nameof(statsBools ));
             bool buildStringStatsOfType<T>(ListWrapper<T>statsList,string statsListName)where T:struct{
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,statsListName+"={{ ");
              statsList.Reset();
              bool appendedStatString=false;
              while(statsList.MoveNext()){
               T stat=statsList.Current;
               string s=stat.ToString();
               //Log.DebugMessage("stat string:"+s);
               if(string.IsNullOrEmpty(s)){
                //Log.DebugMessage("'string.IsNullOrEmpty(s)'");
                continue;
               }
               appendedStatString=true;
               stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{0}, ",s);
              }
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
              if(!appendedStatString){
               int lengthToRemove=statsListName.Length+7;
               //Log.DebugMessage("stringBuilder.Length:"+stringBuilder.Length+";lengthToRemove:"+lengthToRemove);
               stringBuilder.Remove(stringBuilder.Length-lengthToRemove,lengthToRemove);
               return false;
              }
              return true;
             }
             string result;
             if(!stringBuilderHasData){
              result="";
             }else{
              result=string.Format(CultureInfoUtil.en_US,"persistentStats={{ {0}, }}",stringBuilder.ToString());
             }
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
         private static readonly ConcurrentQueue<List<StatsFloatData>>parsingStatsFloatsListPool=new();
         private static readonly ConcurrentQueue<List<StatsIntData  >>parsingStatsIntsListPool  =new();
         private static readonly ConcurrentQueue<List<StatsBoolData >>parsingStatsBoolsListPool =new();
            internal static PersistentStats Parse(string s){
             PersistentStats persistentStats=new PersistentStats();
             if(!parsingStatsFloatsListPool.TryDequeue(out List<StatsFloatData>statsFloatsList)){
              statsFloatsList=new List<StatsFloatData>();
             }
             statsFloatsList.Clear();
             if(!parsingStatsIntsListPool  .TryDequeue(out List<StatsIntData  >statsIntsList  )){
              statsIntsList  =new List<StatsIntData  >();
             }
             statsIntsList  .Clear();
             if(!parsingStatsBoolsListPool .TryDequeue(out List<StatsBoolData >statsBoolsList )){
              statsBoolsList =new List<StatsBoolData >();
             }
             statsBoolsList .Clear();
             int statsFloatsStringStart=s.IndexOf("statsFloats={");
             if(statsFloatsStringStart>=0){
              Log.DebugMessage("statsFloatsStringStart:"+statsFloatsStringStart);
                statsFloatsStringStart+=13;
              int statsFloatsStringEnd=s.IndexOf("} , ",statsFloatsStringStart);
              Log.DebugMessage("statsFloatsStringEnd:"+statsFloatsStringEnd);
              string statsFloatsString=s.Substring(statsFloatsStringStart,statsFloatsStringEnd-statsFloatsStringStart);
              int statsFloatStringStart=0;
              while((statsFloatStringStart=statsFloatsString.IndexOf("[",statsFloatStringStart))>=0){
               //Log.DebugMessage("statsFloatStringStart:"+statsFloatStringStart);
               int floatDataStringStart=statsFloatStringStart+1;
               int floatDataStringEnd  =statsFloatsString.IndexOf("],",floatDataStringStart);
               //Log.DebugMessage("floatDataStringEnd:"+floatDataStringEnd);
               statsFloatStringStart=floatDataStringEnd+2;
               StatsFloatData floatData=StatsFloatData.Parse(statsFloatsString.Substring(floatDataStringStart,floatDataStringEnd-floatDataStringStart));
               statsFloatsList.Add(floatData);
              }
             }
             int statsIntsStringStart=s.IndexOf("statsInts={");
             if(statsIntsStringStart>=0){
              Log.DebugMessage("statsIntsStringStart:"+statsIntsStringStart);
                statsIntsStringStart+=11;
              int statsIntsStringEnd=s.IndexOf("} , ",statsIntsStringStart);
              Log.DebugMessage("statsIntsStringEnd:"+statsIntsStringEnd);
              string statsIntsString=s.Substring(statsIntsStringStart,statsIntsStringEnd-statsIntsStringStart);
              int statsIntStringStart=0;
              while((statsIntStringStart=statsIntsString.IndexOf("[",statsIntStringStart))>=0){
               //Log.DebugMessage("statsIntStringStart:"+statsIntStringStart);
               int intDataStringStart=statsIntStringStart+1;
               int intDataStringEnd  =statsIntsString.IndexOf("],",intDataStringStart);
               //Log.DebugMessage("intDataStringEnd:"+intDataStringEnd);
               statsIntStringStart=intDataStringEnd+2;
               StatsIntData intData=StatsIntData.Parse(statsIntsString.Substring(intDataStringStart,intDataStringEnd-intDataStringStart));
               statsIntsList.Add(intData);
              }
             }
             int statsBoolsStringStart=s.IndexOf("statsBools={");
             if(statsBoolsStringStart>=0){
              Log.DebugMessage("statsBoolsStringStart:"+statsBoolsStringStart);
                statsBoolsStringStart+=12;
              int statsBoolsStringEnd=s.IndexOf("} , ",statsBoolsStringStart);
              Log.DebugMessage("statsBoolsStringEnd:"+statsBoolsStringEnd);
              string statsBoolsString=s.Substring(statsBoolsStringStart,statsBoolsStringEnd-statsBoolsStringStart);
              int statsBoolStringStart=0;
              while((statsBoolStringStart=statsBoolsString.IndexOf("[",statsBoolStringStart))>=0){
               //Log.DebugMessage("statsBoolStringStart:"+statsBoolStringStart);
               int boolDataStringStart=statsBoolStringStart+1;
               int boolDataStringEnd  =statsBoolsString.IndexOf("],",boolDataStringStart);
               //Log.DebugMessage("boolDataStringEnd:"+boolDataStringEnd);
               statsBoolStringStart=boolDataStringEnd+2;
               StatsBoolData boolData=StatsBoolData.Parse(statsBoolsString.Substring(boolDataStringStart,boolDataStringEnd-boolDataStringStart));
               statsBoolsList.Add(boolData);
              }
             }
             persistentStats.statsFloats=new ListWrapper<StatsFloatData>(statsFloatsList);
             persistentStats.statsInts  =new ListWrapper<StatsIntData  >(statsIntsList  );
             persistentStats.statsBools =new ListWrapper<StatsBoolData >(statsBoolsList );
             parsingStatsFloatsListPool.Enqueue(statsFloatsList);
             parsingStatsIntsListPool  .Enqueue(statsIntsList  );
             parsingStatsBoolsListPool .Enqueue(statsBoolsList );
             return persistentStats;
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
             Type thisType=this.GetType();
             persistentStats.statsFloats.Reset();
             while(persistentStats.statsFloats.MoveNext()){
              StatsFloatData statsFloat=persistentStats.statsFloats.Current;
              Type simStatsType=statsFloat.simStatsType;
              if(ReflectionUtil.IsTypeDerivedFrom(thisType,simStatsType)){
               FieldInfo fieldInfo=thisType.GetField(statsFloat.fieldName,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
               if(fieldInfo!=null){
                fieldInfo.SetValueOptimized(this,statsFloat.fieldValue);
               }
              }
             }
             persistentStats.statsInts.Reset();
             while(persistentStats.statsInts.MoveNext()){
              StatsIntData statsInt=persistentStats.statsInts.Current;
              Type simStatsType=statsInt.simStatsType;
              if(ReflectionUtil.IsTypeDerivedFrom(thisType,simStatsType)){
               FieldInfo fieldInfo=thisType.GetField(statsInt.fieldName,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
               if(fieldInfo!=null){
                fieldInfo.SetValueOptimized(this,statsInt.fieldValue);
               }
              }
             }
             persistentStats.statsBools.Reset();
             while(persistentStats.statsBools.MoveNext()){
              StatsBoolData statsBool=persistentStats.statsBools.Current;
              Type simStatsType=statsBool.simStatsType;
              if(ReflectionUtil.IsTypeDerivedFrom(thisType,simStatsType)){
               FieldInfo fieldInfo=thisType.GetField(statsBool.fieldName,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
               if(fieldInfo!=null){
                fieldInfo.SetValueOptimized(this,statsBool.fieldValue);
               }
              }
             }
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