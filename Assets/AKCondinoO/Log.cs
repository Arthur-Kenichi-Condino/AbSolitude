using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class Log{
        internal static void Error(string logMsg){
            //  Always log errors
            Debug.LogError(logMsg);
        }
        internal static void Warning(string logMsg){
            //  Always log warnings
            Debug.LogWarning(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg){
            Debug.Log(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg,Object context){
            Debug.Log(logMsg,context);
        }
    }
}