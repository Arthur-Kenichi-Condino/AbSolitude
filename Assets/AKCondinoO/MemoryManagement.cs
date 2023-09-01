#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;
using UnityEngine;
namespace AKCondinoO{
    internal static class MemoryManagement{
     internal static float lastManualGarbageCollectionTime=-1;
     const float manualGarbageCollectionTimeInterval=30.0f;
        internal static void CallGC(float time){
         if(lastManualGarbageCollectionTime>=0f){
          if(time-lastManualGarbageCollectionTime<=manualGarbageCollectionTimeInterval){
           return;
          }
         }
         Log.DebugMessage("CallGC");
         GCSettings.LargeObjectHeapCompactionMode=GCLargeObjectHeapCompactionMode.CompactOnce;
         GC.Collect(GC.MaxGeneration,GCCollectionMode.Forced,true,true);
         GC.WaitForPendingFinalizers();
         lastManualGarbageCollectionTime=time;
        }
        internal static void SuperDestroy(this object @this,BindingFlags flags){
         foreach(FieldInfo field in @this.GetType().GetFields(flags)){
          Log.DebugMessage("SuperDestroy:field:"+field);
          if(field.IsLiteral&&!field.IsInitOnly){
           Log.DebugMessage("SuperDestroy:ignore const: "+field.Name+" of type "+field.FieldType);
           continue;
          }
          if(field.FieldType.IsPrimitive){
           Log.DebugMessage("SuperDestroy:ignore primitive: "+field.Name+" of type "+field.FieldType);
           continue;
          }
         }
        }
    }
}