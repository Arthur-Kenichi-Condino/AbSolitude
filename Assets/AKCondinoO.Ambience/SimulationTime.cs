using AKCondinoO.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
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
        internal class DirectionalLightSettings{
         [SerializeField]internal string id;
         [SerializeField]internal Gradient colorGradient;
         [SerializeField]internal AnimationCurve intensityCurve;
        }
        [System.Serializable]
        internal class DayPeriod{
         [SerializeField]internal string periodName;
         [SerializeField]internal TimeRange timeRange;
         [Header("Sun and Moon (Directional Lights)")]
         [SerializeField]internal DirectionalLightSettings[]directionalLightSettings;
         [Header("Ambient Lighting")]
         [SerializeField]internal Gradient ambientLightGradient;
         [Header("Sky Sphere")]
         [SerializeField]internal Gradient skySphereGradient;
         [Header("Sky Box (Reflections)")]
         [SerializeField]internal Gradient skyboxTintGradient;
         [SerializeField]internal Gradient skyboxGroundGradient;
        }
        [System.Serializable]
        internal class Season{
         [SerializeField]internal string seasonName;
         [SerializeField]internal SeasonId seasonId;
         [Header("Day Periods")]
         [SerializeField]internal DayPeriod[]dayPeriods;
         internal readonly Dictionary<string,SeasonDirectionalLightAttributes>seasonDirectionalLightAttributesById=new();
            internal class SeasonDirectionalLightAttributes{
             internal string id;
             internal Gradient[]colorGradients=new Gradient[0]{};
             internal AnimationCurve[]intensityCurves=new AnimationCurve[0]{};
            }
         internal Gradient[]ambientLightGradients=new Gradient[0]{};
         internal Gradient[]skySphereGradients=new Gradient[0]{};
         internal Gradient[]skyboxTintGradients=new Gradient[0]{};
         internal Gradient[]skyboxGroundGradients=new Gradient[0]{};
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
            internal void Build(){
             BuildGradients(ref ambientLightGradients,dayPeriods.Select(p=>p.ambientLightGradient).ToArray());
             BuildGradients(ref    skySphereGradients,dayPeriods.Select(p=>p.   skySphereGradient).ToArray());
             BuildGradients(ref   skyboxTintGradients,dayPeriods.Select(p=>p.  skyboxTintGradient).ToArray());
             BuildGradients(ref skyboxGroundGradients,dayPeriods.Select(p=>p.skyboxGroundGradient).ToArray());
             BuildDirectionalLights();
            }
            internal static void BuildGradients(ref Gradient[]gradients,Gradient[]sourceGradients){
             int count=sourceGradients.Length;
             Array.Resize(ref gradients,count);
             for(int i=0;i<count;i++){
              Gradient source=sourceGradients[i];
              Gradient result=new();
              result.SetKeys(source.colorKeys,source.alphaKeys);
              result.mode=GradientMode.Blend;
              gradients[i]=result;
             }
             for(int i=0;i<count-1;i++){
              ReconcileBoundary(i,i+1,ref gradients);
             }
             if(count>1){
              ReconcileBoundary(count-1,0,ref gradients);
             }
                //  Concilia as fronteiras entre os períodos.
                static void ReconcileBoundary(int previousIndex,int nextIndex,ref Gradient[]gradients){
                 Gradient previous=gradients[previousIndex];
                 Gradient next=gradients[nextIndex];
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
         readonly Dictionary<string,Gradient[]>buildingGradientsById=new();
         readonly Dictionary<string,AnimationCurve[]>buildingDirectionalLightIntensityCurvesById=new();
            void BuildDirectionalLights(){
             for(int i=0;i<dayPeriods.Length;i++){
              var period=dayPeriods[i];
              for(int j=0;j<period.directionalLightSettings.Length;j++){
               var lightSettings=period.directionalLightSettings[j];
               var id=lightSettings.id;
               if(!seasonDirectionalLightAttributesById.TryGetValue(id,out var lightAttributes)){
                seasonDirectionalLightAttributesById.Add(id,lightAttributes=new(){id=id,});
                buildingGradientsById[id]=new Gradient[dayPeriods.Length];
                buildingDirectionalLightIntensityCurvesById[id]=new AnimationCurve[dayPeriods.Length];
               }
               if(buildingGradientsById[id].Length<dayPeriods.Length){
                var gradients=buildingGradientsById[id];
                Array.Resize(ref gradients,dayPeriods.Length);
                buildingGradientsById[id]=gradients;
                var intensityCurves=buildingDirectionalLightIntensityCurvesById[id];
                Array.Resize(ref intensityCurves,dayPeriods.Length);
                buildingDirectionalLightIntensityCurvesById[id]=intensityCurves;
               }
               buildingGradientsById[id][i]=lightSettings.colorGradient;
              }
             }
             foreach(var kvp in seasonDirectionalLightAttributesById){
              var id=kvp.Key;
              var gradients=buildingGradientsById[id];
              BuildGradients(ref seasonDirectionalLightAttributesById[id].colorGradients,gradients);
             }
            }
         internal readonly Dictionary<string,DirectionalLightController>directionalLightsById=new();
            internal void OnStart(){
             var simTime=SimulationTime.singleton;
             foreach(var kvp in seasonDirectionalLightAttributesById){
              var id=kvp.Key;
              var lightAttributes=kvp.Value;
              var light=simTime.directionalLightPool.Rent();
              light.Set(lightAttributes);
              directionalLightsById.Add(id,light);
             }
            }
            internal void OnEnd(){
             var simTime=SimulationTime.singleton;
             foreach(var kvp in directionalLightsById){
              var light=kvp.Value;
              simTime.directionalLightPool.Return(light);
             }
             directionalLightsById.Clear();
            }
            bool hasSkyTintId;
            bool hasGroundColorId;
            internal void OnManualUpdate(){
             var simTime=SimulationTime.singleton;
             if(GetCurrentPeriod(simTime.dayTimeNormalized,dayPeriods,out var periodIndex,out var localTime)){
              RenderSettings.ambientLight=EvaluateAmbientLight  (periodIndex,localTime);
              simTime.skySphere.color    =EvaluateSkySphereColor(periodIndex,localTime);
              if(hasSkyTintId    ||(hasSkyTintId    =simTime.skyboxMaterial.HasProperty(    skyTintId))){
               simTime.skyboxMaterial.SetColor(    skyTintId,EvaluateSkyboxTint       (periodIndex,localTime));
              }
              if(hasGroundColorId||(hasGroundColorId=simTime.skyboxMaterial.HasProperty(groundColorId))){
               simTime.skyboxMaterial.SetColor(groundColorId,EvaluateSkyboxGroundColor(periodIndex,localTime));
              }
              foreach(var kvp in directionalLightsById){
               kvp.Value.OnManualUpdate(periodIndex,localTime);
              }
             }
            }
            internal Color EvaluateAmbientLight     (int periodIndex,float localTime){
             return EvaluateGradient(periodIndex,localTime,ambientLightGradients);
            }
            internal Color EvaluateSkySphereColor   (int periodIndex,float localTime){
             return EvaluateGradient(periodIndex,localTime,   skySphereGradients);
            }
            internal Color EvaluateSkyboxTint       (int periodIndex,float localTime){
             return EvaluateGradient(periodIndex,localTime,  skyboxTintGradients);
            }
            internal Color EvaluateSkyboxGroundColor(int periodIndex,float localTime){
             return EvaluateGradient(periodIndex,localTime,skyboxGroundGradients);
            }
            internal static Color EvaluateGradient(int periodIndex,float localTime,Gradient[]gradients){
             return periodIndex>=0&&periodIndex<gradients.Length
              ?gradients[periodIndex].Evaluate(localTime)
              :defaultGradient.Evaluate(0f);
            }
            internal static bool GetCurrentPeriod(float dayTimeNormalized,DayPeriod[]dayPeriods,out int periodIndex,out float localTime){
             float hour=Mathf.Repeat(dayTimeNormalized*24f,24f);
             for(int i=0;i<dayPeriods.Length;i++){
              TimeRange range=dayPeriods[i].timeRange;
              if(hour>=range.start&&hour<range.end){
               periodIndex=i;
               localTime=(hour-range.start)/(range.end-range.start);
               return true;
              }
             }
             periodIndex=-1;
             localTime=0f;
             return false;
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
     [SerializeField]internal DirectionalLightController directionalLightPrefab;
     [SerializeField]internal Material skyboxMaterial;
     internal static readonly int     skyTintId=Shader.PropertyToID("_SkyTint"    );
     internal static readonly int groundColorId=Shader.PropertyToID("_GroundColor");
     private MonoPool<DirectionalLightController>directionalLightPool;
     internal SkySphere skySphere;
     internal Dictionary<SeasonId,Season>seasonsById=new();
        public override void Initialize(){
         base.Initialize();
         directionalLightPool=new(directionalLightPrefab,transform);
         DirectionalLightController[]existingLights=GetComponentsInChildren<DirectionalLightController>(true);
         foreach(var light in existingLights){
          directionalLightPool.Return(light);
         }
         skySphere=GetComponentInChildren<SkySphere>();
         skySphere.OnInitialize();
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
         currentSeason?.OnEnd();
         SetSeasons();
         currentSeason=GetCurrentSeason();
         currentSeason.OnStart();
        }
        internal void SetSeasons(){
         seasonsById.Clear();
         for(int i=0;i<seasons.Length;i++){
          Season season=seasons[i];
          if(seasonsById.ContainsKey(season.seasonId)){
           Logs.Error("'season with id "+season.seasonId+" is duplicated, please check your configuration.'");
          }
          season.Build();
          seasonsById[season.seasonId]=season;
         }
        }
        internal Season GetCurrentSeason(){
         return GetSeason(currentSeasonId);
        }
        internal Season GetSeason(SeasonId id){
         return seasonsById.TryGetValue(id,out var season)?season:null;
        }
        public override void Shutdown(){
         base.Shutdown();
         directionalLightPool.Destroy();
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
          currentSeason.OnManualUpdate();
         }
        }
    }
}