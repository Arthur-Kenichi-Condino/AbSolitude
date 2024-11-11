#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;
namespace AKCondinoO{
    internal static class MemoryManagement{
     const long kCollectAfterAllocating=85L*1024L;
      const float kCollectAfterAllocatingCollectionTimeInterval=60.0f;
     static long nextCollectAt=0L;
     const long kHighWater=2L*1024L*1024L*1024L;
      const float kHighWaterCollectionTimeInterval=30.0f;
     const float manualGarbageCollectionTimeInterval=90.0f;
     internal static float lastManualGarbageCollectionTime=-1;
     static long lastFrameMemory=0L;
        internal static void CallGC(float time){
         bool collectNow=false;
         bool immediate=false;
         long mem=Profiler.GetMonoUsedSizeLong();
         if(mem<lastFrameMemory){
          Log.DebugMessage("GC happened mem:"+mem);
          nextCollectAt=mem+kCollectAfterAllocating;
         }
         if(mem>=nextCollectAt){
          if(!(time-lastManualGarbageCollectionTime<=kCollectAfterAllocatingCollectionTimeInterval)){
           Log.DebugMessage("mem>=nextCollectAt:Trigger GC");
           collectNow|=true;
          }
         }
         if(mem>kHighWater){
          if(!(time-lastManualGarbageCollectionTime<=kHighWaterCollectionTimeInterval)){
           Log.Warning("mem>kHighWater:Trigger immediate GC");
           collectNow|=true;
           immediate|=true;
          }
         }
         if(lastManualGarbageCollectionTime>=0f){
          if(!(time-lastManualGarbageCollectionTime<=manualGarbageCollectionTimeInterval)){
           Log.DebugMessage("manualGarbageCollectionTimeInterval:Trigger GC");
           collectNow|=true;
          }
         }
         if(collectNow){
          Log.DebugMessage("call GC mem:"+mem);
          if(immediate||!GarbageCollector.isIncremental){
           GCSettings.LargeObjectHeapCompactionMode=GCLargeObjectHeapCompactionMode.CompactOnce;
           GC.Collect(GC.MaxGeneration,GCCollectionMode.Forced,true,true);
           GC.WaitForPendingFinalizers();
          }else{
           GarbageCollector.CollectIncremental();
          }
          Log.DebugMessage("post GC call mem:"+mem);
          lastManualGarbageCollectionTime=time;
          nextCollectAt=mem+kCollectAfterAllocating;
         }
         lastFrameMemory=mem;
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