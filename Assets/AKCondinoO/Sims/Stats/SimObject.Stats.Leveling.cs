#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
        internal partial class Stats{
            protected virtual void OnGenerateValidation_Level(SimObject statsSim=null,bool reset=true){
             OnRefresh(statsSim);
             if(
              reset||
              simLevel_value<=0||
              ageLevel_value<=0
             ){
              IsTranscendentSet(math_random.CoinFlip(),statsSim,false);
              SimLevelSet(math_random.Next(1,201),statsSim,false);
              AgeLevelSet(math_random.Next(1,970),statsSim,false);
             }
             //Log.DebugMessage("statsSim:"+statsSim+":simLevel_value:"+simLevel_value);
            }
         #region IsTranscendent
         [NonSerialized]protected bool isTranscendent_value;
          internal bool IsTranscendentGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return isTranscendent_value;
          }
           internal void IsTranscendentSet(bool value,SimObject statsSim=null,bool forceRefresh=false){
            isTranscendent_value=value;
            updatedIsTranscendent=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedIsTranscendent;
             internal void OnRefresh_IsTranscendent(SimObject statsSim=null){
              if(onGeneration||
                 updatedIsTranscendent
              ){
               refreshedIsTranscendent=true;
              }
             }
              [NonSerialized]protected bool refreshedIsTranscendent;
         #endregion
         #region Experience
         /// <summary>
         ///  Experience
         /// </summary>
         [NonSerialized]protected float experience_value;
          internal float ExperienceGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return experience_value;
          }
           internal void ExperienceSet(float value,SimObject statsSim=null,bool forceRefresh=false){
            experience_value=value;
            updatedExperience=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedExperience;
             internal void OnRefresh_Experience(SimObject statsSim=null){
              if(onGeneration||
                 updatedExperience||
                 refreshedIsTranscendent
              ){
               bool isTranscendent=isTranscendent_value;
               int curLevel=simLevel_value;
               float simExpAtLevel=GetTotalExpPointsForLevel(curLevel,isTranscendent);
               if(experience_value<simExpAtLevel){
                experience_value=experience_value+simExpAtLevel;
               }
               refreshedExperience=true;
              }
             }
              [NonSerialized]protected bool refreshedExperience;
            // -----------------------------------------------------------------------------
            //  CurveFit v1.0 – Interpolador EXP por Hermite-PCHIP com suporte a pesos
            //  Desenvolvido com paciência, curiosidade e muito café por Arthur Kenichi
            //  Assistência técnica e matemática de ChatGPT (OpenAI, modelo GPT-4o)
            //  Preserva pontos de controle fornecidos com fidelidade e suavidade
            //  Que esta curva leve seu jogo ao próximo nível – sem grind injusto!
            // -----------------------------------------------------------------------------
            //  EXP progression developed with the assistance of AI (chatGPT, openAI, 2025-07):
            //  Curva de experiência gerada com paciência, suor e algumas lágrimas,
            // cuidadosamente calibrada por Arthur Kenichi & ChatGPT
            //  Interpolação Hermite-PCHIP implementada à mão, respeitando pesos e pontos de controle
            //  Ciência, precisão e um toque de carinho digital
            //  CurveFit v1.0 – Interpolador Hermite-PCHIP com suporte a pesos por ponto
            //  Desenvolvido por Arthur Kenichi com assistência técnica do modelo GPT-4o da OpenAI
            //  Gerado com foco em monotonicidade, fidelidade aos pontos e personalização total
            //  Esta curva foi gerada a duras penas entre sessões de café, reflexões matemáticas
            // e trocas existenciais entre Arthur e sua IA de estimação.
            //  Que esta EXP traga bons níveis e nenhum grind desnecessário!
            //  Obs.: Este código utiliza interpolação cúbica de Hermite (PCHIP) implementada manualmente.
            //  Ele respeita os pontos fornecidos (inclusive extremos), com preservação de monotonicidade.
            //  Para ajustes futuros, é possível modificar os pesos ou adicionar pontos de controle.
            internal struct ExpCurvePoint{
             public int level;
             public float totalExp;
             public float weight;
                public ExpCurvePoint(int level,float totalExp,float weight=1f){
                 this.level=level;
                 this.totalExp=totalExp;
                 this.weight=weight;
                }
            }
            internal class ExpCurve{
             public ExpCurvePoint this[int index]{
              get{return points[index];}
             }
             public readonly ExpCurvePoint[]points;
             public readonly ExpCurvePoint[]pointsSorted;
                public ExpCurve(ExpCurvePoint[]points){
                 this.points=points;
                 pointsSorted=new ExpCurvePoint[points.Length];
                 Array.Copy(points,pointsSorted,points.Length);
                 Array.Sort(pointsSorted,(p1,p2)=>p1.level.CompareTo(p2.level));
                 PrecomputeExpCurvePCHIPSlopes();
                }
             double[]m;
                internal void PrecomputeExpCurvePCHIPSlopes(){
                 var points=pointsSorted;
                 int n=points.Length;
                 double[]h=new double[n-1];
                 double[]delta=new double[n-1];
                 m=new double[n];
                 for(int i=0;i<n-1;i++){
                  h[i]=points[i+1].level-points[i].level;
                  delta[i]=(points[i+1].totalExp-points[i].totalExp)/h[i];
                 }
                 for(int i=1;i<n-1;i++){
                  if(Math.Sign(delta[i-1])*Math.Sign(delta[i])>0){
                   double w1=2*h[i]+h[i-1];
                   double w2=h[i]+2*h[i-1];
                   m[i]=(w1+w2==0)?0:(w1+w2)/((w1/delta[i-1])+(w2/delta[i]));
                  }else{
                   m[i]=0;
                  }
                 }
                 m[0]=delta[0];
                 m[n-1]=delta[n-2];
                }
                internal double GetTotalExpPointsForLevel(int level){
                 var points=pointsSorted;
                 if(level<=points[0].level)return points[0].totalExp;
                 foreach(var p in points){
                  if(p.level==level)
                   return p.totalExp;
                 }
                 if(level>=points[points.Length-1].level)return points[points.Length-1].totalExp;
                 int idx=0;
                 while(idx<points.Length-2&&level>points[idx+1].level)
                  idx++;
                 double x0=points[idx].level;
                 double x1=points[idx+1].level;
                 double y0=points[idx].totalExp;
                 double y1=points[idx+1].totalExp;
                 double h=x1-x0;
                 if(h==0)
                  return y0;
                 double t=(level-x0)/h;
                 double h00=(1+2*t)*(1-t)*(1-t);
                 double h10=t*(1-t)*(1-t);
                 double h01=t*t*(3-2*t);
                 double h11=t*t*(t-1);
                 return
                  h00*y0+
                  h10*h*m[idx]+
                  h01*y1+
                  h11*h*m[idx+1];
                }
            }
         [NonSerialized]static readonly Dictionary<(int currentLevel,bool transcendent),float>totalExpPointsAtLevel=new Dictionary<(int,bool),float>();
            internal static float GetTotalExpPointsForLevel(int level,bool transcendent,ExpCurve expCurve){
             if(level<=1)return 0f;
             float totalExp;
             lock(totalExpPointsAtLevel){
              if(totalExpPointsAtLevel.TryGetValue((level,transcendent),out totalExp)){
               return totalExp;
              }
             }
             double exp=expCurve.GetTotalExpPointsForLevel(level);
             //Log.DebugMessage("level:"+level+";exp:"+exp);
             totalExp=(float)exp;
             lock(totalExpPointsAtLevel){
              totalExpPointsAtLevel[(level,transcendent)]=totalExp;
             }
             return totalExp;
            }
         [NonSerialized]static readonly Dictionary<(int currentLevel,bool transcendent),float>expPointsForNextLevel=new Dictionary<(int,bool),float>();
            internal static float GetExpRequiredForNextLevel(int level,bool transcendent,ExpCurve expCurve){
             float expRequired;
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((level,transcendent),out expRequired)){
               return expRequired;
              }
             }
             expRequired=GetTotalExpPointsForLevel(level+1,transcendent,expCurve)-GetTotalExpPointsForLevel(level,transcendent,expCurve);
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(level,transcendent)]=expRequired;
             }
             return expRequired;
            }
         //  Pontos baseados no iRO Wiki
         static readonly ExpCurve expCurveFrom1To99=new(
          new ExpCurvePoint[]{
           new(  2,        548f,1f),
           new(  3,      1_442f,1f),
           new( 10,     25_404f,1f),
           new( 25,    146_821f,1f),
           new( 50,    773_336f,1f),
           new( 75,  2_563_170f,1f),
           new( 90,  6_583_073f,1f),
           new( 97, 11_923_376f,1f),
           new( 98, 13_053_008f,1f),
           new( 99, 14_305_769f,1f),
           new(100, 15_578_516f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom100To150=new(
          new ExpCurvePoint[]{
           new( 99,     14_305_769f,1f),
           new(100,     15_578_516f,1f),
           new(101,     16_932_718f,1f),
           new(102,     18_373_588f,1f),
           new(103,     19_906_673f,1f),
           new(110,     33_766_814f,1f),
           new(125,     95_515_147f,1f),
           new(150,    611_169_449f,1f),
           new(151,    659_677_653f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom151To200=new(
          new ExpCurvePoint[]{
           new(150,    611_169_449f,1f),
           new(151,    659_677_653f,1f),
           new(175,  4_182_888_718f,1f),
           new(190, 10_140_031_377f,1f),
           new(197, 13_963_672_526f,1f),
           new(198, 14_577_391_223f,1f),
           new(199, 15_209_521_480f,1f),
           new(200, 15_860_615_644f,1f),
           new(201, 16_513_663_090f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom201To260=new(
          new ExpCurvePoint[]{
           new(201,   16_513_663_090f,1f),
           new(202,   17_821_717_124f,1f),
           new(225,   35_162_890_397f,1f),
           new(250,  311_128_271_573f,1f),
           new(260,1_585_926_900_139f,1f),
          }
         );
         //  Pontos baseados no iRO Wiki (Transc)
         static readonly ExpCurve expCurveFrom1To99Transc=new(
          new ExpCurvePoint[]{
           new(  2,        658f,1f),
           new(  3,      1_731f,1f),
           new( 10,     30_486f,1f),
           new( 25,    181_409f,1f),
           new( 50,  1_029_641f,1f),
           new( 75,  3_665_151f,1f),
           new( 90,  9_032_253f,1f),
           new( 97, 14_919_408f,1f),
           new( 98, 16_077_379f,1f),
           new( 99, 17_336_093f,1f),
           new(100, 18_608_840f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom100To150Transc=new(
          new ExpCurvePoint[]{
           new( 99,     17_336_093f,1f),
           new(100,     18_608_840f,1f),
           new(101,     19_963_042f,1f),
           new(102,     21_403_912f,1f),
           new(103,     22_936_997f,1f),
           new(110,     36_797_138f,1f),
           new(125,    105_548_675f,1f),
           new(150,    614_199_773f,1f),
           new(151,    662_707_977f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom151To200Transc=new(
          new ExpCurvePoint[]{
           new(150,    614_199_773f,1f),
           new(151,    662_707_977f,1f),
           new(175,  4_185_419_042f,1f),
           new(190, 10_143_061_701f,1f),
           new(197, 13_966_702_850f,1f),
           new(198, 14_580_421_547f,1f),
           new(199, 15_212_551_804f,1f),
           new(200, 15_863_645_968f,1f),
           new(201, 16_516_693_414f,1f),
          }
         );
         static readonly ExpCurve expCurveFrom201To260Transc=new(
          new ExpCurvePoint[]{
           new(201,   16_516_693_414f,1f),
           new(202,   17_824_747_448f,1f),
           new(225,   35_165_920_721f,1f),
           new(250,  311_131_301_897f,1f),
           new(260,1_585_929_930_463f,1f),
          }
         );
            internal static float GetTotalExpPointsForLevel(int level,bool transcendent){
             if(level<=99){
              return GetTotalExpPointsForLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom1To99):
                (expCurveFrom1To99Transc)
              );
             }
             if(level<=150){
              return GetTotalExpPointsForLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom100To150):
                (expCurveFrom100To150Transc)
              );
             }
             if(level<=200){
              return GetTotalExpPointsForLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom151To200):
                (expCurveFrom151To200Transc)
              );
             }
             return GetTotalExpPointsForLevel(level,transcendent,
              (!transcendent)?
               (expCurveFrom201To260):
               (expCurveFrom201To260Transc)
             );
            }
            internal static float GetExpRequiredForNextLevel(int level,bool transcendent){
             if(level<=99){
              return GetExpRequiredForNextLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom1To99):
                (expCurveFrom1To99Transc)
              );
             }
             if(level<=150){
              return GetExpRequiredForNextLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom100To150):
                (expCurveFrom100To150Transc)
              );
             }
             if(level<=200){
              return GetExpRequiredForNextLevel(level,transcendent,
               (!transcendent)?
                (expCurveFrom151To200):
                (expCurveFrom151To200Transc)
              );
             }
             return GetExpRequiredForNextLevel(level,transcendent,
              (!transcendent)?
               (expCurveFrom201To260):
               (expCurveFrom201To260Transc)
             );
            }
            internal static float GetExpPointsForNextLevelFrom1To99(int currentLevel,bool transcendent){
             if(currentLevel>99){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             float result;
             if(currentLevel<=1){
              result=(!transcendent?expCurveFrom1To99[0].totalExp:expCurveFrom1To99Transc[0].totalExp);
             }else{
              float expRequired=GetExpRequiredForNextLevel(currentLevel,transcendent);
              result=expRequired;
             }
             return result;
            }
            internal static float GetExpPointsForNextLevelFrom100To150(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<=99){
              return GetExpPointsForNextLevelFrom1To99   (currentLevel,transcendent);
             }
             if(currentLevel>150){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result;
             float expRequired=GetExpRequiredForNextLevel(currentLevel,transcendent);
             result=expRequired;
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
            internal static float GetExpPointsForNextLevelFrom151To200(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<=150){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             if(currentLevel>200){
              return GetExpPointsForNextLevelFrom201To260(currentLevel,transcendent);
             }
             float result;
             float expRequired=GetExpRequiredForNextLevel(currentLevel,transcendent);
             result=expRequired;
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
            internal static float GetExpPointsForNextLevelFrom201To260(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<=200){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result;
             float expRequired=GetExpRequiredForNextLevel(currentLevel,transcendent);
             result=expRequired;
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
            internal static void ProcessExpPoints(SimObject simObject,SimObject fromDeadSim,float simExp){
             Stats stats=simObject.stats;
             if(stats!=null){
              float curSimExp=stats.ExperienceGet(simObject);
              curSimExp+=simExp;
              stats.ExperienceSet(curSimExp,simObject);
             }
            }
         #endregion
         #region SimLevel
         /// <summary>
         ///  Based on all character's interactions (which gives Exp)
         /// </summary>
         [NonSerialized]protected int simLevel_value;
          [NonSerialized]protected int totalStatPoints_value;
          [NonSerialized]protected int statPointsSpent_value;
          internal int SimLevelGet(SimObject statsSim=null){
           OnRefresh(statsSim);
           return simLevel_value;
          }
           internal void SimLevelSet(int value,SimObject statsSim=null,bool forceRefresh=false){
            simLevel_value=value;
            updatedSimLevel=true;
            SetPendingRefresh(statsSim,forceRefresh);
           }
            [NonSerialized]protected bool updatedSimLevel;
             internal void OnRefresh_SimLevel(SimObject statsSim=null){
              if(onGeneration||
                 updatedSimLevel||
                 refreshedIsTranscendent||
                  refreshedExperience
              ){
               bool isTranscendent=isTranscendent_value;
               int curLevel=simLevel_value;
               if(curLevel<=1){
                curLevel=1;
               }
               int nextLevel;
               float simExpAtNextLevel=-1f;
               while((nextLevel=curLevel+1)<=260&&experience_value>=(simExpAtNextLevel=GetTotalExpPointsForLevel(nextLevel,isTranscendent))){
                curLevel=nextLevel;
               }
               simLevel_value=curLevel;
               Log.DebugMessage("statsSim:"+statsSim+":simLevel_value:"+simLevel_value+":experience_value:"+experience_value+":simExpAtNextLevel:"+simExpAtNextLevel);
               totalStatPoints_value=AddStatPointsFrom201To260(simLevel_value,isTranscendent_value);
               //Log.DebugMessage("statsSim:"+statsSim+":totalStatPoints_value:"+totalStatPoints_value);
               refreshedSimLevel=true;
              }
             }
              [NonSerialized]protected bool refreshedSimLevel;
       [NonSerialized]static readonly Dictionary<(int currentLevel,bool transcendent),int>totalStatPointsAtLevel=new Dictionary<(int,bool),int>();
          internal static int AddStatPointsFrom1To99(int currentLevel,bool transcendent){
           lock(totalStatPointsAtLevel){
            if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
             return cached;
            }
           }
           int statPoints;
           if(transcendent){
            statPoints=100;
           }else{
            statPoints=48;
           }
           for(int level=2;level<=Math.Min(currentLevel,99);level++){
            statPoints+=Mathf.FloorToInt(
             (transcendent?((float)expCurveFrom1To99Transc[0].totalExp/(float)expCurveFrom1To99[0].totalExp):1)*
             (((level-1)/5f)+3)
            );
            lock(totalStatPointsAtLevel){
             totalStatPointsAtLevel[(level,transcendent)]=statPoints;
            }
           }
           return statPoints;
          }
          internal static int AddStatPointsFrom100To150(int currentLevel,bool transcendent){
           lock(totalStatPointsAtLevel){
            if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
             return cached;
            }
           }
           int statPoints=AddStatPointsFrom1To99(currentLevel,transcendent);
           for(int level=100;level<=Math.Min(currentLevel,150);level++){
            statPoints+=Mathf.FloorToInt(
             (transcendent?((float)expCurveFrom1To99Transc[0].totalExp/(float)expCurveFrom1To99[0].totalExp):1)*
             (Mathf.FloorToInt((level-1)/10f)+13)
            );
            lock(totalStatPointsAtLevel){
             totalStatPointsAtLevel[(level,transcendent)]=statPoints;
            }
           }
           return statPoints;
          }
          internal static int AddStatPointsFrom151To200(int currentLevel,bool transcendent){
           lock(totalStatPointsAtLevel){
            if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
             return cached;
            }
           }
           int statPoints=AddStatPointsFrom100To150(currentLevel,transcendent);
           for(int level=151;level<=Math.Min(currentLevel,200);level++){
            statPoints+=Mathf.FloorToInt(
             (transcendent?((float)expCurveFrom1To99Transc[0].totalExp/(float)expCurveFrom1To99[0].totalExp):1)*
             (Mathf.FloorToInt(((level-1)-150)/7f)+28)
            );
            lock(totalStatPointsAtLevel){
             totalStatPointsAtLevel[(level,transcendent)]=statPoints;
            }
           }
           return statPoints;
          }
          internal static int AddStatPointsFrom201To260(int currentLevel,bool transcendent){
           lock(totalStatPointsAtLevel){
            if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
             return cached;
            }
           }
           int statPoints=AddStatPointsFrom151To200(currentLevel,transcendent);
           for(int level=201;level<=Math.Min(currentLevel,260);level++){
            statPoints+=Mathf.FloorToInt(((level-1)-150)/7f)+28;
            lock(totalStatPointsAtLevel){
             totalStatPointsAtLevel[(level,transcendent)]=statPoints;
            }
           }
           return statPoints;
          }
         #endregion
         #region AgeLevel
         /// <summary>
         ///  Based on character's existence time (which raises Age)
         /// </summary>
         [NonSerialized]protected int ageLevel_value;
         internal int AgeLevelGet(SimObject statsSim=null){
          OnRefresh(statsSim);
          return ageLevel_value;
         }
          internal void AgeLevelSet(int value,SimObject statsSim=null,bool forceRefresh=false){
           ageLevel_value=value;
           updatedAgeLevel=true;
           SetPendingRefresh(statsSim,forceRefresh);
          }
           [NonSerialized]protected bool updatedAgeLevel;
            internal void OnRefresh_AgeLevel(SimObject statsSim=null){
             if(onGeneration||
                updatedAgeLevel
             ){
              refreshedAgeLevel=true;
             }
            }
             [NonSerialized]protected bool refreshedAgeLevel;
         #endregion
        }
    }
}