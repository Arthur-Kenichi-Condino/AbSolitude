#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class SunTransform:MonoBehaviour{
     internal Vector3 mainSunEulerAngles=new Vector3(50.0f,-90.0f,0.0f);
        void Update(){
        }
    }
}