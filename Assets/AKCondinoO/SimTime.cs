#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class SimTime:MonoBehaviour{   
     internal const int   _YEAR  =12;
     internal const int   _MONTH =28;
     internal       float _DAY      {get;private set;}
     internal       float _HOUR     {get;private set;}
     internal       float _MINUTE   {get;private set;}
     internal const float _SECOND=1f;
        void Awake(){
        }
    }
}