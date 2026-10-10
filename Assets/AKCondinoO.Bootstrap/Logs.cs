using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal static class Logs{
     internal static bool enableAll=false;
     internal static bool autoFlush=false;
     private static readonly HashSet<string>enabledAt=new(){
      "Main",
      //"InputHandler",
      //"InputHandler.TranslateInput",
      //"InputInterpreter",
      //"Window",
      //"Header",
      //"GameOrchestrator",
      //"RagnarokOnlineMode",
      //"MainCamera",
      //"OrbitalCameraMode",
      //"BiomesConfigurationSnapshot",
      //"ByChancePicker",
      //"WorldChunk",
      //"TerrainChunkBuilder",
      //"MarchingCubesHelper",
      //"NavMeshProvider",
      //"NavMeshSourcesOrdered",
      //"WorldChunkTerrain.Interactable",
      //"ChunkSimObjectSpawner",
      //"SimObjectManager",
      //"BiomesSimObjectSpawningSystem",
      //"SimObjectFactory",
      //"SimObject",
      //"SimObjectPart",
      //"SimObjectPart.Interactable",
      //"IInteractable",
      //"StateMachine",
      //"OpenObjectStateDefinition",
      //"CloseObjectStateDefinition",
      //"SimDirector",
      //"SimActor",
      //"SimMovement",
      //"SimBrain",
      //"SimInteractionResolver",
      //"SimInteractionQueue",
      //"GoHereDefinition",
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
         string message;
         try{
          message=$"[{className}.{member}]:{logMsg.Invoke()}";
         }catch(System.Exception e){
          message=$"[{className}.{member}]:logMsg caused an Exception! >:o";
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }
         ProcessMessage(LogType.Debug,
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
         ProcessMessage(LogType.Error,
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
         ProcessMessage(LogType.Warning,
          message,context,condition,
          file,
          member
         );
        }
        private static void ProcessMessage(LogType logType,string message,Object context=null,bool condition=true,string file="",string member=""
        ){
         if(message==null){return;}
         if(autoFlush){
          WriteMessage(logType,message,context,condition,file,member);
          return;
         }
         var logMessage=new LogMessage{
          logType=logType,
          message=message,
          context=context,
          condition=condition,
          file=file,
          member=member
         };
         logMessages.Enqueue(logMessage);
         if(logMessages.Count>=256){
          Flush();
         }
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
     internal static readonly ConcurrentQueue<LogMessage>logMessages=new();
        internal struct LogMessage{
         internal LogType logType;
         internal string message;
         internal Object context;
         internal bool condition;
         internal string file;
         internal string member;
        }
        private static void Flush(){
         while(logMessages.TryDequeue(out var logMessage)){
          WriteMessage(logMessage.logType,logMessage.message,logMessage.context,logMessage.condition,logMessage.file,logMessage.member);
         }
        }
    }
}