using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class SunLight:MonoBehaviour{
     float sunLightIntensityClearNight =0f ;
     float sunLightIntensityClearSunset=.5f;
     float sunLightIntensityClearDawn  =.5f;
     float sunLightIntensityClearDay   =1f ;
        internal void UpdateValues(float transitionLerp){
         if(SimTime.singleton.simDayTransition==SimTime.DayTransitions.Night){
          OnNightTime(transitionLerp);
         }else if(SimTime.singleton.simDayTransition==SimTime.DayTransitions.DayToSunset){
          OnDayToSunsetTransition(transitionLerp);
         }else if(SimTime.singleton.simDayTransition==SimTime.DayTransitions.SunsetToNight){
          OnSunsetToNightTransition(transitionLerp);
         }else if(SimTime.singleton.simDayTransition==SimTime.DayTransitions.NightToDawn){
          OnNightToDawnTransition(transitionLerp);
         }else if(SimTime.singleton.simDayTransition==SimTime.DayTransitions.DawnToDay){
          OnDawnToDayTransition(transitionLerp);
         }else{
          OnDayTime(transitionLerp);
         }
        }
        void OnNightTime(float transitionLerp){
        }
        void OnDayToSunsetTransition(float transitionLerp){
        }
        void OnSunsetToNightTransition(float transitionLerp){
        }
        void OnNightToDawnTransition(float transitionLerp){
        }
        void OnDawnToDayTransition(float transitionLerp){
        }
        void OnDayTime(float transitionLerp){
        }
    }
}