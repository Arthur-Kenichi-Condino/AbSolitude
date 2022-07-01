using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class SunLight:MonoBehaviour{
     Light sunLightComponent;
        void Awake(){
         sunLightComponent=GetComponent<Light>();
        }
     [SerializeField]float sunLightIntensityClearNight =0f ;
     [SerializeField]float sunLightIntensityClearSunset=.5f;
     [SerializeField]float sunLightIntensityClearDawn  =.5f;
     [SerializeField]float sunLightIntensityClearDay   =1f ;
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
         sunLightComponent.intensity=sunLightIntensityClearNight;
        }
        void OnDayToSunsetTransition(float transitionLerp){
         sunLightComponent.intensity=Mathf.Lerp(sunLightIntensityClearDay   ,sunLightIntensityClearSunset,transitionLerp);
        }
        void OnSunsetToNightTransition(float transitionLerp){
         sunLightComponent.intensity=Mathf.Lerp(sunLightIntensityClearSunset,sunLightIntensityClearNight ,transitionLerp);
        }
        void OnNightToDawnTransition(float transitionLerp){
         sunLightComponent.intensity=Mathf.Lerp(sunLightIntensityClearNight ,sunLightIntensityClearDawn  ,transitionLerp);
        }
        void OnDawnToDayTransition(float transitionLerp){
         sunLightComponent.intensity=Mathf.Lerp(sunLightIntensityClearDawn  ,sunLightIntensityClearDay   ,transitionLerp);
        }
        void OnDayTime(float transitionLerp){
         sunLightComponent.intensity=sunLightIntensityClearDay;
        }
    }
}