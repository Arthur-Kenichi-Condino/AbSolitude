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
            internal static float GetExpPointsForNextLevelFrom1To99(int currentLevel,bool transcendent){
             if(currentLevel>99){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             float result=(!transcendent?expCurveFrom1To99[0].totalExp:expCurveFrom1To99Transc[0].totalExp);
             float expRequired=GetExpRequiredForNextLevel(currentLevel,transcendent,
              (!transcendent)?expCurveFrom1To99:expCurveFrom1To99Transc
             );
             result=expRequired;
             return result;
            }
         [NonSerialized]const float expPointsForLevel100NonTransc       =1272747f;
          [NonSerialized]const float totalExpPointsFrom100To150NonTransc=596863680f;
         [NonSerialized]const float expPointsForLevel100Transc          =1528225f;
          [NonSerialized]const float totalExpPointsFrom100To150Transc   =981363532f;
            internal static float GetExpPointsForNextLevelFrom100To150(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<=99){
              return GetExpPointsForNextLevelFrom1To99   (currentLevel,transcendent);
             }
             if(currentLevel>=150){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result=(!transcendent?expPointsForLevel100NonTransc:expPointsForLevel100Transc);
             if(currentLevel>99){
              if(!transcendent){
               result=expPointsForLevel100NonTransc*Mathf.Pow(Mathf.Pow((totalExpPointsFrom100To150NonTransc/expPointsForLevel100NonTransc),1f/50f),Math.Min(currentLevel-100,150-100));
              }else{
               result=expPointsForLevel100Transc   *Mathf.Pow(Mathf.Pow((totalExpPointsFrom100To150Transc   /expPointsForLevel100Transc   ),1f/50f),Math.Min(currentLevel-100,150-100));
              }
             }
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
         [NonSerialized]const float expPointsForLevel151NonTransc       =645371884f;
          [NonSerialized]const float totalExpPointsFrom151To200NonTransc=15846309875f;
         [NonSerialized]const float expPointsForLevel151Transc          =774446260f;
          [NonSerialized]const float totalExpPointsFrom151To200Transc   =19015571850f;
            internal static float GetExpPointsForNextLevelFrom151To200(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<150){
              return GetExpPointsForNextLevelFrom100To150(currentLevel,transcendent);
             }
             if(currentLevel>=200){
              return GetExpPointsForNextLevelFrom201To250(currentLevel,transcendent);
             }
             float result=(!transcendent?expPointsForLevel151NonTransc:expPointsForLevel151Transc);
             if(currentLevel>150){
              if(!transcendent){
               result=expPointsForLevel151NonTransc*Mathf.Pow(Mathf.Pow((totalExpPointsFrom151To200NonTransc/expPointsForLevel151NonTransc),1f/50f),Math.Min(currentLevel-150,200-150));
              }else{
               result=expPointsForLevel151Transc   *Mathf.Pow(Mathf.Pow((totalExpPointsFrom151To200Transc   /expPointsForLevel151Transc   ),1f/50f),Math.Min(currentLevel-150,200-150));
              }
             }
             lock(expPointsForNextLevel){
              expPointsForNextLevel[(currentLevel,transcendent)]=result;
             }
             return result;
            }
         [NonSerialized]const float expPointsForLevel201       =653047446f;
          [NonSerialized]const float totalExpPointsFrom201To250=1503757559122f;
            internal static float GetExpPointsForNextLevelFrom201To250(int currentLevel,bool transcendent){
             lock(expPointsForNextLevel){
              if(expPointsForNextLevel.TryGetValue((currentLevel,transcendent),out float cached)){
               return cached;
              }
             }
             if(currentLevel<200){
              return GetExpPointsForNextLevelFrom151To200(currentLevel,transcendent);
             }
             float result=expPointsForLevel201;
             if(currentLevel>200){
              result=expPointsForLevel201*Mathf.Pow(Mathf.Pow((totalExpPointsFrom201To250/expPointsForLevel201),1f/50f),Math.Min(currentLevel-200,250-200));
             }
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
               int prevLevel=simLevel_value-1;
               if(prevLevel<=1){
                prevLevel=1;
               }
               int curLevel=simLevel_value;
               if(curLevel<=1){
                curLevel=1;
               }
               //float expForPreviousLevel=GetExpPointsForNextLevelFrom201To250(prevLevel,isTranscendent);
               //float expForNextLevel=GetExpPointsForNextLevelFrom201To250(curLevel,isTranscendent);
               //float totalExpForPreviousLevel=experience_value;
               //if(){
               //}
               //while((experience_value-)>=(expForNextLevel=GetExpPointsForNextLevelFrom201To250(curLevel,isTranscendent))){
               // int nextLevel=curLevel+1;
               // curLevel=nextLevel;
               //}
               //Log.DebugMessage("curLevel:"+curLevel+";expForNextLevel:"+expForNextLevel);
               totalStatPoints_value=AddStatPointsFrom201To250(simLevel_value,isTranscendent_value);
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
          internal static int AddStatPointsFrom201To250(int currentLevel,bool transcendent){
           lock(totalStatPointsAtLevel){
            if(totalStatPointsAtLevel.TryGetValue((currentLevel,transcendent),out int cached)){
             return cached;
            }
           }
           int statPoints=AddStatPointsFrom151To200(currentLevel,transcendent);
           for(int level=201;level<=Math.Min(currentLevel,250);level++){
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