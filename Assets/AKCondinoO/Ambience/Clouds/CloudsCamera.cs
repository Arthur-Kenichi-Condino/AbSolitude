#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Ambience.Clouds{
    internal class CloudsCamera:MonoBehaviour{
        void LateUpdate(){
         transform.rotation=Camera.main.transform.rotation;
        }
    }
}