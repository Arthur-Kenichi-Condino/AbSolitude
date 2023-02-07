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
     internal static float lastManualGarbageCollectionTime;
        internal static void CallGC(float time){
         GCSettings.LargeObjectHeapCompactionMode=GCLargeObjectHeapCompactionMode.CompactOnce;
         GC.Collect(GC.MaxGeneration,GCCollectionMode.Forced,true,true);
         GC.WaitForPendingFinalizers();
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