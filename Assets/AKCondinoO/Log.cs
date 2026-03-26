using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class Log{
        internal static void Error(string logMsg){
         throw new System.Exception("deprecated");
            //  Always log errors
            Debug.LogError(logMsg);
        }
        internal static void Warning(string logMsg){
         throw new System.Exception("deprecated");
            //  Always log warnings
            Debug.LogWarning(logMsg);
        }
        //  tudo na frente de Log.DebugMessage deve ser vazio ou comentário, pois a linha
        // será constantemente 'comentada' (com barras //) após testes
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg){
         throw new System.Exception("deprecated");
            Debug.Log(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg,bool condition){
         throw new System.Exception("deprecated");
            if(!condition){return;}
            Debug.Log(logMsg);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(string logMsg,Object context){
         throw new System.Exception("deprecated");
            Debug.Log(logMsg,context);
        }
        [System.Diagnostics.Conditional("ENABLE_LOG_DEBUG")]
        internal static void DebugMessage(bool condition,System.Func<object>stringFunc){
         throw new System.Exception("deprecated");
            if(!condition){return;}
            Debug.Log(stringFunc.Invoke());
        }
 }
}