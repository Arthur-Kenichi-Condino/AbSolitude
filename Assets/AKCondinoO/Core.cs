#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.UI;
using AKCondinoO.UI.Fixed;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain.Editing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace AKCondinoO{
    internal class Core:MonoBehaviour{
     internal static Core singleton;
     internal static int threadCount;
     internal static readonly string saveLocation=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/";
     internal static string saveName="terra";
     internal static string savePath;
     [SerializeField]Gameplayer _GameplayerPrefab;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
                    Util.SetUtil();
         CultureInfoUtil.SetUtil();
         QualitySettings.vSyncCount=1;
         Application.targetFrameRate=75;
         savePath=string.Format("{0}{1}/",saveLocation,saveName);
         Directory.CreateDirectory(savePath);
         NavMeshHelper.SetNavMeshBuildSettings();
         Gameplayer.all.Add(Gameplayer.main=Instantiate(_GameplayerPrefab));
        }
     SortedDictionary<int,ISingletonInitialization>singletonInitOrder;
     IEnumerable<KeyValuePair<int,ISingletonInitialization>>singletonInitReversedOrder;
        private void Start(){
         singletonInitOrder=new SortedDictionary<int,ISingletonInitialization>{
          {0,MainCamera.singleton},
         };
         foreach(var singletonOrdered in singletonInitOrder){
          Log.DebugMessage("initialization at "+singletonOrdered.Key+":"+singletonOrdered.Value);
          singletonOrdered.Value.Init();
         }
         foreach(var singletonOrderedInReverse in singletonInitReversedOrder=singletonInitOrder.Reverse()){
          Log.DebugMessage("set deinitialization at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
          OnDestroyingCoreEvent+=singletonOrderedInReverse.Value.OnDestroyingCoreEvent;
         }
         //MainCamera         .singleton.Init();
         SimTime            .singleton.Init();
         VoxelSystem        .singleton.Init();
         VoxelTerrainEditing.singleton.Init();
         SimObjectManager   .singleton.Init();
         SimObjectSpawner   .singleton.Init();
         SkillsManager      .singleton.Init();
         SimsMachine        .singleton.Init();
         AutonomyCore       .singleton.Init();
         GameMode           .singleton.Init();
         Placeholder        .singleton.Init();
         FixedUI            .singleton.Init();
         Gameplayer.main.Init();
        }
        void OnDestroy(){
         if(singleton==this){
              foreach(Gameplayer gameplayer in Gameplayer.all){
               Log.DebugMessage("destroying core: disengage gameplayer (main:"+(gameplayer==Gameplayer.main)+")");
               gameplayer.OnRemove();
              }
              Gameplayer.all.Clear();
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
              FixedUI            .singleton=null;
              Placeholder        .singleton=null;
              GameMode           .singleton=null;
              AutonomyCore       .singleton=null;
              SimsMachine        .singleton=null;
              SkillsManager      .singleton=null;
              SimObjectSpawner   .singleton=null;
              SimObjectManager   .singleton=null;
              VoxelTerrainEditing.singleton=null;
              VoxelSystem        .singleton=null;
              SimTime            .singleton=null;
              MainCamera         .singleton=null;
                                  singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}