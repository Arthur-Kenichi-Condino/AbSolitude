#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.UI;
using AKCondinoO.UI.Fixed;
using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private void Start(){
         MainCamera      .singleton.Init();
         VoxelSystem     .singleton.Init();
         SimObjectManager.singleton.Init();
         SimObjectSpawner.singleton.Init();
         AICondinoCore   .singleton.Init();
         GameMode        .singleton.Init();
         Placeholder     .singleton.Init();
         FixedUI         .singleton.Init();
         Gameplayer.main.Init();
        }
        void OnDestroy(){
         if(singleton==this){
              foreach(Gameplayer gameplayer in Gameplayer.all){
               Log.DebugMessage("destroying core: disengage gameplayer (main:"+(gameplayer==Gameplayer.main)+")");
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
                               singleton=null;
              FixedUI         .singleton=null;
              Placeholder     .singleton=null;
              GameMode        .singleton=null;
              AICondinoCore   .singleton=null;
              SimObjectSpawner.singleton=null;
              SimObjectManager.singleton=null;
              VoxelSystem     .singleton=null;
              MainCamera      .singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}