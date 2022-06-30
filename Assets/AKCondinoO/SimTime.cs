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
     [SerializeField]internal Light mainSun;
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
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         _MINUTE=60*_SECOND;
         _HOUR  =60*_MINUTE;
         _DAY   =24*_HOUR  ;
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SimTime:OnDestroyingCoreEvent");
        }
    }
}