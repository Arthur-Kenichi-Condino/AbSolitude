#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        protected virtual void OnSNIPE_ST_Routine(){
         Log.DebugMessage("OnSNIPE_ST_Routine()");
        }
    }
}