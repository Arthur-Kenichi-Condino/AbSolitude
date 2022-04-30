using AKCondinoO.Sims;
using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO{
    internal class Core:MonoBehaviour{
     internal static Core Singleton;
     internal static int ThreadCount;
     internal static readonly string saveLocation=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/";
     internal static string saveName="terra";
     internal static string savePath;
     [SerializeField]Gameplayer GameplayerPrefab;
        private void Awake(){
         if(Singleton==null){Singleton=this;}else{DestroyImmediate(this);return;}
         QualitySettings.vSyncCount=1;
         Application.targetFrameRate=75;
         savePath=string.Format("{0}{1}/",saveLocation,saveName);
         Directory.CreateDirectory(savePath);
         Gameplayer.main=Instantiate(GameplayerPrefab);
        }
        private void Start(){
         VoxelSystem     .Singleton.Init();
         SimObjectManager.Singleton.Init();
         SimObjectSpawner.Singleton.Init();
         Gameplayer.main.Init();
        }
        void OnDestroy(){
         if(Singleton==this){
              try{
               EventHandler handler=OnDestroyingCoreEvent;
               handler?.Invoke(this,
                new OnDestroyingCoreEventArgs(){
                }
               );
              }catch{
               throw;
              }finally{
               if(ThreadCount>0){
                Log.Error("ThreadCount>0(ThreadCount=="+ThreadCount+"):one or more threads weren't stopped nor waited for termination");
               }
              }
                               Singleton=null;
              SimObjectManager.Singleton=null;
              SimObjectSpawner.Singleton=null;
              VoxelSystem     .Singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}