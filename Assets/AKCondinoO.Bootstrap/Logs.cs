using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class Logs{
     internal static bool enableAll=false;
     private static readonly HashSet<string>enabledAt=new(){
      "Main",
      "InputHandler",
      "InputHandler.TranslateInput",
      "InputInterpreter",
      "Window",
      "Header",
      "GameOrchestrator",
      "RagnarokOnlineMode",
      "MainCamera",
      //"NavMeshProvider",
      //"NavMeshSourcesOrdered",
      //"WorldChunk",
      "WorldChunkTerrain.Interactable",
      //"TerrainChunkBuilder",
      "SimObjectManager",
      "SimObjectFactory",
      "SimObject",
      "SimDirector",
      "SimActor",
      "SimBrain",
      "SimInteractionResolver",
      "SimInteractionQueue",
      "SimMovement",
      "GoHereDefinition",
     };
     internal enum LogType{
      Debug,
      Error,
      Warning,
     }
        internal static void Enable (string className)=>enabledAt.Add   (className);
        internal static void Disable(string className)=>enabledAt.Remove(className);
        [Conditional("ENABLE_LOG_DEBUG")]
        internal static void Debug(System.Func<object>logMsg,Object context=null,bool condition=true,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         if(!condition){return;}
         string className=System.IO.Path.GetFileNameWithoutExtension(file);
         if(!enableAll&&enabledAt.Count>0&&!enabledAt.Contains(className))
          return;
         string message=$"[{className}.{member}]:{logMsg.Invoke()}";
         WriteMessage(LogType.Debug,
          message,context,condition,
          file,
          member
         );
        }
        internal static void Error(string logMsg,Object context=null,bool condition=true,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         string className=System.IO.Path.GetFileNameWithoutExtension(file);
         string message=$"[{className}.{member}]:{logMsg}";
         WriteMessage(LogType.Error,
          message,context,condition,
          file,
          member
         );
        }
        internal static void Warning(string logMsg,Object context=null,bool condition=true,
         [CallerFilePath]string file="",
         [CallerMemberName]string member=""
        ){
         string className=System.IO.Path.GetFileNameWithoutExtension(file);
         string message=$"[{className}.{member}]:{logMsg}";
         WriteMessage(LogType.Warning,
          message,context,condition,
          file,
          member
         );
        }
        private static void WriteMessage(LogType logType,string message,Object context=null,bool condition=true,string file="",string member=""
        ){
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
          case(LogType.Warning):{
           UnityEngine.Debug.LogWarning(message,context);
           break;
          }
         }
        }
    }
}