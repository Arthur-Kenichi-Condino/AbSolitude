#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.UI;
using AKCondinoO.UI.Context;
using AKCondinoO.UI.Fixed;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain.Editing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO{
    internal class Core:MonoBehaviour{
     internal static Core singleton;
     internal static int threadCount;
     internal static readonly string saveLocation=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/";
     internal static string saveName="terra";
     internal static string savePath;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
                    Util.SetUtil();
         CultureInfoUtil.SetUtil();
         QualitySettings.vSyncCount=1;
         Application.targetFrameRate=75;
         savePath=string.Format("{0}{1}/",saveLocation,saveName);
         Directory.CreateDirectory(savePath);
         NavMeshHelper.SetNavMeshBuildSettings();
        }
     SortedDictionary<int,ISingletonInitialization>singletonInitOrder;
     IEnumerable<KeyValuePair<int,ISingletonInitialization>>singletonInitReversedOrder;
        private void Start(){
         singletonInitOrder=new SortedDictionary<int,ISingletonInitialization>{
          { 0,GameplayerManagement.singleton},
          { 1,InputHandler        .singleton},
          { 2,ScreenInput         .singleton},
          { 3,MainCamera          .singleton},
          { 4,SimTime             .singleton},
          { 5,VoxelSystem         .singleton},
          { 6,VoxelTerrainEditing .singleton},
          { 7,SimObjectManager    .singleton},
          { 8,SimObjectSpawner    .singleton},
          { 9,SkillsManager       .singleton},
          {10,SimsMachine         .singleton},
          {11,AutonomyCore        .singleton},
          {12,GameMode            .singleton},
          {13,Placeholder         .singleton},
          {14,FixedUI             .singleton},
          {15,ContextMenuUI       .singleton},
         };
         foreach(var singletonOrdered in singletonInitOrder){
          Log.DebugMessage("initialization at "+singletonOrdered.Key+":"+singletonOrdered.Value);
          singletonOrdered.Value.Init();
         }
         foreach(var singletonOrderedInReverse in singletonInitReversedOrder=singletonInitOrder.Reverse()){
          Log.DebugMessage("set deinitialization at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
          OnDestroyingCoreEvent+=singletonOrderedInReverse.Value.OnDestroyingCoreEvent;
         }
         Gameplayer.main.Init();
        }
        void OnDestroy(){
         if(singleton==this){
              foreach(var gameplayer in GameplayerManagement.singleton.all){
               Log.DebugMessage("destroying core: disengage gameplayer (main:"+(gameplayer.Value==Gameplayer.main)+")");
               gameplayer.Value.OnRemove();
              }
              GameplayerManagement.singleton.all.Clear();
              try{
               EventHandler handler=OnDestroyingCoreEvent;
               handler?.Invoke(this,
                new OnDestroyingCoreEventArgs(){
                }
               );
              }catch{
               throw;
              }finally{
               if(threadCount>0){
                Log.Error("ThreadCount>0(ThreadCount=="+threadCount+"):one or more threads weren't stopped nor waited for termination");
               }
              }
              foreach(var singletonOrderedInReverse in singletonInitReversedOrder){
               Log.DebugMessage("unset destroyed singleton at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
               Type singletonType=singletonOrderedInReverse.Value.GetType();
               PropertyInfo singletonPropertyInfo=singletonType.GetProperty("singleton",BindingFlags.Static|BindingFlags.NonPublic);
               Log.DebugMessage("singletonPropertyInfo:"+singletonPropertyInfo);
               singletonPropertyInfo.SetValue(null,null);
               Log.DebugMessage("singletonPropertyInfo.GetValue(null):"+singletonPropertyInfo.GetValue(null));
              }
              singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}