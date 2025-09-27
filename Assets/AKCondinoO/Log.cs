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
        //  tudo na frente de Log.DebugMessage deve ser vazio ou comentário, pois a linha
        // será constantemente 'comentada' (com barras //) após testes
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg){
            Debug.Log(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg,bool condition){
            if(!condition){return;}
            Debug.Log(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg,Object context){
            Debug.Log(logMsg,context);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(bool condition,System.Func<object>stringFunc){
            if(!condition){return;}
            Debug.Log(stringFunc.Invoke());
        }
 }
}