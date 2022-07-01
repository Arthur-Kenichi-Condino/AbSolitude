#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class SimTime:MonoBehaviour{   
     internal static SimTime singleton;
     [SerializeField]internal SunLight mainSun;
     internal const int   _YEAR  =12;
     internal const int   _MONTH =28;
     internal       float _DAY      {get;private set;}
     internal       float _HOUR     {get;private set;}
     internal       float _MINUTE   {get;private set;}
     internal const float _SECOND=1f;
     /// <summary>
     ///  Quantos minutos dura um ciclo dia-noite no jogo se useRealTime==false
     /// </summary>
     [SerializeField]internal float simDayInRealMinutes=.1f;
     internal uint  simDay  =1;
     internal uint  simMonth=1;
     internal ulong simYear =1;
     internal float simTimeOfDay;//  Contagem de segundos total no dia
     internal float simDayOfYear;//  Contagem de dias total no ano
     internal float simDayCourse;
     internal readonly AmbientLightIntensity ambientLightSettings=new AmbientLightIntensity();
     internal SunLight    []sunLights    ;
     internal SunTransform[]sunTransforms;
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         _MINUTE=60*_SECOND;
         _HOUR  =60*_MINUTE;
         _DAY   =24*_HOUR  ;
         sunLights    =GetComponentsInChildren<SunLight    >();
         sunTransforms=GetComponentsInChildren<SunTransform>();
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimTime:OnDestroyingCoreEvent");
        }
        internal enum DayTransitions{
         Day,
         DayToSunset,
         SunsetToNight,
         Night,
         NightToDawn,
         DawnToDay,
        }
     internal DayTransitions simDayTransition=DayTransitions.Day;
        void Update(){
            simTimeOfDay+=Time.deltaTime*_DAY/(simDayInRealMinutes*_MINUTE);
         if(simTimeOfDay>=_DAY){
            simTimeOfDay-=_DAY;
             simDay++;
          if(simDay>_MONTH){
             simDay=1;
              simMonth++;
           if(simMonth>_YEAR){
              simMonth=1;
            simYear++;
            Log.DebugMessage("ano passou, simYear:"+simYear);
           }
           Log.DebugMessage("mês passou, simMonth:"+simMonth);
          }
          Log.DebugMessage("dia passou, simDay:"+simDay);
         }
         simDayOfYear=(simMonth-1)*_MONTH+simDay;
         simDayCourse=simTimeOfDay/_DAY;
         float transitionLerp=1.0f;
         if(simDayCourse>=0.75f||simDayCourse<0.25f){      
          simDayTransition=DayTransitions.Night;
         }else if(simDayCourse>=0.67f&&simDayCourse<0.71f){
          float delta=0.71f-0.67f;
          transitionLerp=(simDayCourse-0.67f)/delta;
          simDayTransition=DayTransitions.DayToSunset;
         }else if(simDayCourse>=0.71f&&simDayCourse<0.75f){
          float delta=0.75f-0.71f;
          transitionLerp=(simDayCourse-0.71f)/delta;
          simDayTransition=DayTransitions.SunsetToNight;
         }else if(simDayCourse>=0.25f&&simDayCourse<0.29f){
          float delta=0.29f-0.25f;
          transitionLerp=(simDayCourse-0.25f)/delta;
          simDayTransition=DayTransitions.NightToDawn;
         }else if(simDayCourse>=0.29f&&simDayCourse<0.33f){
          float delta=0.33f-0.29f;
          transitionLerp=(simDayCourse-0.29f)/delta;
          simDayTransition=DayTransitions.DawnToDay;
         }else{                                            
          simDayTransition=DayTransitions.Day;
         }
         ambientLightSettings    .UpdateValues(transitionLerp);
         foreach(var sunLight     in sunLights    ){
                     sunLight    .UpdateValues(transitionLerp);
         }
         foreach(var sunTransform in sunTransforms){
                     sunTransform.UpdateValues();
         }
        }
    }
}