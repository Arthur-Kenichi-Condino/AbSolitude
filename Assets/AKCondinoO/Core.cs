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
     [SerializeField]Gameplayer GameplayerPrefab;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         QualitySettings.vSyncCount=1;
         Application.targetFrameRate=75;
         savePath=string.Format("{0}{1}/",saveLocation,saveName);
         Directory.CreateDirectory(savePath);
         Gameplayer.main=Instantiate(GameplayerPrefab);
        }
        private void Start(){
         VoxelSystem     .singleton.Init();
         SimObjectManager.singleton.Init();
         SimObjectSpawner.singleton.Init();
         Placeholder     .singleton.Init();
         Gameplayer.main.Init();
        }
        void OnDestroy(){
         if(singleton==this){
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
              SimObjectManager.singleton=null;
              SimObjectSpawner.singleton=null;
              VoxelSystem     .singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}