using AKCondinoO.Bootstrap;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.Ambience{
    internal class SimulationTime:MonoSingleton<SimulationTime>{
     [SerializeField]internal bool recalculateSettings=false;
     [SerializeField]internal bool useRealTime=false;
     [SerializeField]internal float realTimeHoursInGameDay=4f;//  4f
     [SerializeField]internal float daysInMonth=28f;//  28f
     [SerializeField]internal float monthsInYear=12f;//  12f
     internal double gameSecond;
     internal double seconds;
     internal float dayTimeNormalized;
     internal double days;
     internal SeasonId currentSeasonId=SeasonId.Summer;
        internal enum SeasonId:byte{
         Spring,
         Summer,
         Autumn,
         Winter
        }
     internal Season currentSeason;
        [System.Serializable]
        internal struct TimeRange{
         [SerializeField]internal float start;
         [SerializeField]internal float end;
            internal TimeRange(float start,float end){
             this.start=start;
             this.end=end;
            }
        }
        [System.Serializable]
        internal class DayPeriod{
         [SerializeField]internal string periodName;
         [SerializeField]internal TimeRange timeRange;
         [Header("Ambient Lighting")]
         [SerializeField]internal Gradient ambientLightGradient;
        }
        [System.Serializable]
        internal class Season{
         [SerializeField]internal string seasonName;
         [SerializeField]internal SeasonId seasonId;
         [Header("Day Periods")]
         [SerializeField]internal DayPeriod[]dayPeriods;
         internal Gradient[]ambientLightGradients=new Gradient[0]{};
         internal Gradient[]skySphereGradient=new Gradient[0]{};
         internal static readonly Gradient defaultGradient=CreateDefaultGradient();
            internal static Gradient CreateDefaultGradient(){
             Gradient gradient=new();
             Color color=new Color32(128,128,128,255);
             gradient.SetKeys(
              new GradientColorKey[]{
               new(color,0f),
               new(color,1f)
              },
              new GradientAlphaKey[]{
               new(1f,0f),
               new(1f,1f)
              }
             );
             return gradient;
            }
            internal void BuildAmbientLightGradient(){
             //  Construir os gradientes finais.
             int count=dayPeriods.Length;
             Array.Resize(ref ambientLightGradients,count);
             //  Primeiro, copiamos todos os gradientes originais.
             for(int i=0;i<count;i++){
              Gradient source=dayPeriods[i].ambientLightGradient;
              Gradient result=new();
              result.SetKeys(source.colorKeys,source.alphaKeys);
              result.mode=GradientMode.Blend;
              ambientLightGradients[i]=result;
             }
             //  Depois, conciliamos as fronteiras entre os períodos.
             for(int i=0;i<count-1;i++){
              ReconcileBoundary(i,i+1);
             }
             //  Fronteira circular: Early Night -> Late Night.
             if(count>1){
              ReconcileBoundary(count-1,0);
             }
                //  Concilia as fronteiras entre os períodos.
                void ReconcileBoundary(int previousIndex,int nextIndex){
                 Gradient previous=ambientLightGradients[previousIndex];
                 Gradient next=ambientLightGradients[nextIndex];
                 Color boundary=Color.LerpUnclamped(
                  previous.Evaluate(1f),
                  next.Evaluate(0f),
                  0.5f
                 );
                 GradientColorKey[]previousColors=previous.colorKeys;
                 GradientColorKey[]nextColors=next.colorKeys;
                 GradientAlphaKey[]previousAlphas=previous.alphaKeys;
                 GradientAlphaKey[]nextAlphas=next.alphaKeys;
                 //  Extremidade final do período anterior.
                 int previousColorLast=previousColors.Length-1;
                 int previousAlphaLast=previousAlphas.Length-1;
                 Color previousColor=previousColors[previousColorLast].color;
                 previousColors[previousColorLast].color=new(boundary.r,boundary.g,boundary.b,previousColor.a);
                 previousAlphas[previousAlphaLast].alpha=boundary.a;
                 //  Extremidade inicial do próximo período.
                 Color nextColor=nextColors[0].color;
                 nextColors[0].color=new(boundary.r,boundary.g,boundary.b,nextColor.a);
                 nextAlphas[0].alpha=boundary.a;
                 previous.SetKeys(previousColors,previousAlphas);
                 next.SetKeys(nextColors,nextAlphas);
                }
            }
            internal void Update(){
             var simTime=SimulationTime.singleton;
             RenderSettings.ambientLight=EvaluateAmbientLight(simTime.dayTimeNormalized);
            }
            internal Color EvaluateAmbientLight(float dayTimeNormalized){
             float hour=dayTimeNormalized*24f;
             hour=Mathf.Repeat(hour,24f);
             for(int i=0;i<dayPeriods.Length;i++){
              TimeRange range=dayPeriods[i].timeRange;
              if(hour>=range.start&&hour<range.end){
               float duration=range.end-range.start;
               float localTime=(hour-range.start)/duration;
               return ambientLightGradients[i].Evaluate(localTime);
              }
             }
             return defaultGradient.Evaluate(0f);
            }
        }
     [SerializeField]internal Season[]seasons=new Season[]{
      new(){
       seasonName="Spring",
       seasonId=SeasonId.Spring,
       dayPeriods=new DayPeriod[]{
        new(){periodName="Late Night" ,timeRange=new( 0f, 6f)},
        new(){periodName="Dawn"       ,timeRange=new( 6f, 7f)},
        new(){periodName="Sunrise"    ,timeRange=new( 7f, 8f)},
        new(){periodName="Day"        ,timeRange=new( 8f,16f)},
        new(){periodName="Sunset"     ,timeRange=new(16f,17f)},
        new(){periodName="Dusk"       ,timeRange=new(17f,18f)},
        new(){periodName="Early Night",timeRange=new(18f,24f)},
       }
      },
      new(){
       seasonName="Summer",
       seasonId=SeasonId.Summer,
       dayPeriods=new DayPeriod[]{
        new(){periodName="Late Night" ,timeRange=new( 0f, 5f)},
        new(){periodName="Dawn"       ,timeRange=new( 5f, 6f)},
        new(){periodName="Sunrise"    ,timeRange=new( 6f, 7f)},
        new(){periodName="Day"        ,timeRange=new( 7f,17f)},
        new(){periodName="Sunset"     ,timeRange=new(17f,18f)},
        new(){periodName="Dusk"       ,timeRange=new(18f,19f)},
        new(){periodName="Early Night",timeRange=new(19f,24f)},
       }
      },
      new(){
       seasonName="Autumn",
       seasonId=SeasonId.Autumn,
       dayPeriods=new DayPeriod[]{
        new(){periodName="Late Night" ,timeRange=new( 0f, 6f)},
        new(){periodName="Dawn"       ,timeRange=new( 6f, 7f)},
        new(){periodName="Sunrise"    ,timeRange=new( 7f, 8f)},
        new(){periodName="Day"        ,timeRange=new( 8f,16f)},
        new(){periodName="Sunset"     ,timeRange=new(16f,17f)},
        new(){periodName="Dusk"       ,timeRange=new(17f,18f)},
        new(){periodName="Early Night",timeRange=new(18f,24f)},
       }
      },
      new(){
       seasonName="Winter",
       seasonId=SeasonId.Winter,
       dayPeriods=new DayPeriod[]{
        new(){periodName="Late Night" ,timeRange=new(   0f, 7.5f)},
        new(){periodName="Dawn"       ,timeRange=new( 7.5f, 8.5f)},
        new(){periodName="Sunrise"    ,timeRange=new( 8.5f, 9.5f)},
        new(){periodName="Day"        ,timeRange=new( 9.5f,15.5f)},
        new(){periodName="Sunset"     ,timeRange=new(15.5f,16.5f)},
        new(){periodName="Dusk"       ,timeRange=new(16.5f,17.5f)},
        new(){periodName="Early Night",timeRange=new(17.5f,24.0f)},
       }
      }
     };
     [SerializeField]Material skySphereMaterial;
     internal Dictionary<SeasonId,Season>seasonsById=new();
        public override void Initialize(){
         base.Initialize();
         RenderSettings.ambientMode=AmbientMode.Trilight;
         RenderSettings.ambientMode=AmbientMode.Flat;
         RecalculateSettings();
        }
        internal void RecalculateSettings(){
         recalculateSettings=false;
         if(realTimeHoursInGameDay<1f/3600f){
          Logs.Error("'realTimeHoursInGameDay cannot be less than 1f/3600f (1 second per game day).'");
          realTimeHoursInGameDay=1f/3600f;
         }
         gameSecond=86400d/(realTimeHoursInGameDay*3600d);
         SetSeasons();
         currentSeason=GetCurrentSeason();
        }
        internal void SetSeasons(){
         seasonsById.Clear();
         for(int i=0;i<seasons.Length;i++){
          Season season=seasons[i];
          if(seasonsById.ContainsKey(season.seasonId)){
           Logs.Error("'season with id "+season.seasonId+" is duplicated, please check your configuration.'");
          }
          season.BuildAmbientLightGradient();
          seasonsById[season.seasonId]=season;
         }
        }
        internal Season GetCurrentSeason(){
         return GetSeason(currentSeasonId);
        }
        internal Season GetSeason(SeasonId id){
         return seasonsById.TryGetValue(id,out var season)?season:null;
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         if(recalculateSettings){
          RecalculateSettings();
         }
         double elapsedGameSeconds=Time.deltaTime*gameSecond;
         seconds+=elapsedGameSeconds;
         double previousDay=Math.Floor(days);
         days+=elapsedGameSeconds/86400d;
         double currentDay=Math.Floor(days);
         seconds%=86400d;
         dayTimeNormalized=(float)(seconds/86400d);
         //Logs.Debug(()=>"seconds:"+seconds);
         //Logs.Debug(()=>"dayTimeNormalized:"+dayTimeNormalized);
         if(currentDay>previousDay){
          for(double i=previousDay;i<currentDay;i++){
           Logs.Debug(()=>"um dia se passou");
          }
         }
         if(currentSeason!=null){
          currentSeason.Update();
         }
        }
    }
}