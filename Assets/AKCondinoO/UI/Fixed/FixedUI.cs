#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed{
    internal class FixedUI:MonoBehaviour{
        void Awake(){
        }
     [SerializeField]SimObject DEBUG_SET_PLACEHOLDER=null;
        void Update(){
         if(DEBUG_SET_PLACEHOLDER!=null){
          Log.DebugMessage("DEBUG_SET_PLACEHOLDER:"+DEBUG_SET_PLACEHOLDER);
          Placeholder.singleton.GetPlaceholderFor(DEBUG_SET_PLACEHOLDER.GetType());
            DEBUG_SET_PLACEHOLDER=null;
         }
        }
    }
}