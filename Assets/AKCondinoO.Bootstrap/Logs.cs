using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class Logs{
     internal static bool enableAll=false;
     private static readonly HashSet<string>enabledAt=new(){
      "Main",
     };
     internal enum LogType{
      Debug,
      Error,
     }
        internal static void Enable (string className)=>enabledAt.Add   (className);
        internal static void Disable(string className)=>enabledAt.Remove(className);
        [Conditional("ENABLE_LOG_DEBUG")]
        internal static void Debug(string logMsg,Object context=null,bool condition=true,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         WriteMessage(LogType.Debug,
          logMsg,context,condition,
          file,
          member
         );
        }
        internal static void Error(string logMsg,Object context=null,bool condition=true,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         WriteMessage(LogType.Error,
          logMsg,context,condition,
          file,
          member
         );
        }
        private static void WriteMessage(LogType logType,string logMsg,Object context=null,bool condition=true,string file="",string member=""
        ){
         string message=null;
         if(logType==LogType.Error){
          message=logMsg;
         }else{
          if(!condition){return;}
          string className=System.IO.Path.GetFileNameWithoutExtension(file);
          if(!enableAll&&enabledAt.Count>0&&!enabledAt.Contains(className))
           return;
          message=$"[{className}.{member}]:{logMsg}";
         }
         if(message==null){return;}
         switch(logType){
          case(LogType.Debug):{
           UnityEngine.Debug.Log(message,context);
           break;
          }
          case(LogType.Error):{
           UnityEngine.Debug.LogError(message,context);
           break;
          }
         }
        }
    }
}