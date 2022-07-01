#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class AmbientLightIntensity{        
     float ambientLightIntensityClearNight =0f  ;
     float ambientLightIntensityClearSunset=.25f;
     float ambientLightIntensityClearDawn  =.25f;
     float ambientLightIntensityClearDay   =1f  ;
      float ambientReflectionsIntensityClearNight =0f  ;
      float ambientReflectionsIntensityClearSunset=.25f;
      float ambientReflectionsIntensityClearDawn  =.25f;
      float ambientReflectionsIntensityClearDay   =1f  ;
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
         RenderSettings.ambientIntensity=ambientLightIntensityClearNight;
          RenderSettings.reflectionIntensity=ambientReflectionsIntensityClearNight;
        }
        void OnDayToSunsetTransition(float transitionLerp){
         RenderSettings.ambientIntensity=Mathf.Lerp(ambientLightIntensityClearDay   ,ambientLightIntensityClearSunset,transitionLerp);
          RenderSettings.reflectionIntensity=Mathf.Lerp(ambientReflectionsIntensityClearDay   ,ambientReflectionsIntensityClearSunset,transitionLerp);
        }
        void OnSunsetToNightTransition(float transitionLerp){
         RenderSettings.ambientIntensity=Mathf.Lerp(ambientLightIntensityClearSunset,ambientLightIntensityClearNight ,transitionLerp);
          RenderSettings.reflectionIntensity=Mathf.Lerp(ambientReflectionsIntensityClearSunset,ambientReflectionsIntensityClearNight ,transitionLerp);
        }
        void OnNightToDawnTransition(float transitionLerp){
         RenderSettings.ambientIntensity=Mathf.Lerp(ambientLightIntensityClearNight ,ambientLightIntensityClearDawn  ,transitionLerp);
          RenderSettings.reflectionIntensity=Mathf.Lerp(ambientReflectionsIntensityClearNight ,ambientReflectionsIntensityClearDawn  ,transitionLerp);
        }
        void OnDawnToDayTransition(float transitionLerp){
         RenderSettings.ambientIntensity=Mathf.Lerp(ambientLightIntensityClearDawn  ,ambientLightIntensityClearDay   ,transitionLerp);
          RenderSettings.reflectionIntensity=Mathf.Lerp(ambientReflectionsIntensityClearDawn  ,ambientReflectionsIntensityClearDay   ,transitionLerp);
        }
        void OnDayTime(float transitionLerp){
         RenderSettings.ambientIntensity=ambientLightIntensityClearDay;
          RenderSettings.reflectionIntensity=ambientReflectionsIntensityClearDay;
        }
    }
}