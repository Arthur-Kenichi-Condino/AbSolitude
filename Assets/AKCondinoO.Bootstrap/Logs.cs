using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class Logs{
     private static bool enableAll=true;
     private static readonly HashSet<string>enabledAt=new(){
      //"ActiveZone",
      "Main",
     };
     internal enum LogType{
      Debug,
      Error,
     }
        internal static void Enable (string className)=>enabledAt.Add   (className);
        internal static void Disable(string className)=>enabledAt.Remove(className);
        [HideInCallstack]
        internal static void Message(LogType logType,string logMsg,Object context=null,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         string className=System.IO.Path.GetFileNameWithoutExtension(file);
         if(logType!=LogType.Error){
          if(!enableAll&&enabledAt.Count>0&&!enabledAt.Contains(className))
           return;
         }
         string message=$"[{className}.{member}]:{logMsg}";
         switch(logType){
          case(LogType.Debug):{
           Logs.Debug(message,context);
           break;
          }
          case(LogType.Error):{
           Logs.Error(message,context);
           break;
          }
         }
        }
        [Conditional("ENABLE_LOG_DEBUG")]
        [HideInCallstack]
        private static void Debug(string message,Object context){
         UnityEngine.Debug.Log(message,context);
        }
        [HideInCallstack]
        private static void Error(string message,Object context){
         UnityEngine.Debug.LogError(message,context);
        }
    }
}