#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Ambience.Clouds;
using AKCondinoO.Gameplaying;
using AKCondinoO.Music;
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.UI;
using AKCondinoO.UI.Context;
using AKCondinoO.UI.Fixed;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain.Editing;
using AKCondinoO.Voxels.Water.Editing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AKCondinoO{
    internal class Core:MonoBehaviour{
     internal static Core singleton;
     internal NetworkManager netManager;
      internal int maxConnections=112;
      //  do not test for !isServer or !isClient: this could also mean the game didn't start the netManager yet
      /// <summary>
      ///  Do not test for !isServer or !isClient: this could also mean the game didn't start the netManager yet
      /// </summary>
      internal bool isServer=false;
      /// <summary>
      ///  Do not test for !isServer or !isClient: this could also mean the game didn't start the netManager yet
      /// </summary>
      internal bool isClient=false;
     internal static int threadCount;
     internal static readonly string saveLocation=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/";
     internal static string saveName="terra";
     internal static string savePath;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         int procCt=Environment.ProcessorCount;
         Log.DebugMessage("Environment.ProcessorCount:"+procCt);
         ThreadPool.GetMinThreads(out int minWorkerThreads,out int minIOCThreads);
         Log.DebugMessage("minimum number of worker threads:"+minWorkerThreads+";minimum asynchronous I/O completion threads:"+minIOCThreads);
         ThreadPool.GetMaxThreads(out int maxWorkerThreads,out int maxIOCThreads);
         Log.DebugMessage("maximum number of worker threads:"+maxWorkerThreads+";maximum asynchronous I/O completion threads:"+maxIOCThreads);
         Thread.CurrentThread.Priority=System.Threading.ThreadPriority.Normal;
         GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
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
         isServer=false;
         isClient=false;
         GameObject netManagerGameObject;
         if((netManagerGameObject=GameObject.Find("NetworkManager"))==null||
          (netManager=netManagerGameObject.GetComponent<NetworkManager>())==null
         ){
          Log.Warning("NetworkManager not found: going to EntryScene");
          SceneManager.LoadScene("EntryScene",LoadSceneMode.Single);
         }else{
          isServer=netManager.IsServer||netManager.IsHost;
          isClient=netManager.IsClient&&!netManager.IsHost;
          Log.DebugMessage("MainMenu.netManagerInitialized:"+MainMenu.netManagerInitialized+";isServer:"+isServer+";isClient:"+isClient);
         }
         if(!isServer&&!isClient){
          //Log.Error("harmless error:!isServer&&!isClient:engine will reload the game entry scene now");
         }
         singletonInitOrder=new SortedDictionary<int,ISingletonInitialization>{
          { 0,GameplayerManagement.singleton},
          { 1,InputHandler        .singleton},
          { 2,ScreenInput         .singleton},
          { 3,BGM                 .singleton},
          { 4,MainCamera          .singleton},
          { 5,SimTime             .singleton},
          { 6,CloudParticleSystem .singleton},
          { 7,GameMode            .singleton},
          { 8,VoxelSystem         .singleton},
          { 9,VoxelTerrainEditing .singleton},
          {10,VoxelWaterEditing   .singleton},
          {11,SimObjectManager    .singleton},
          {12,SimObjectSpawner    .singleton},
          {13,SkillsManager       .singleton},
          {14,SimsMachine         .singleton},
          {15,AutonomyCore        .singleton},
          {16,Placeholder         .singleton},
          {17,FixedUI             .singleton},
          {18,ContextMenuUI       .singleton},
         };
         foreach(var singletonOrdered in singletonInitOrder){
          Log.DebugMessage("initialization at "+singletonOrdered.Key+":"+singletonOrdered.Value);
          singletonOrdered.Value.Init();
         }
         foreach(var singletonOrderedInReverse in singletonInitReversedOrder=singletonInitOrder.Reverse()){
          Log.DebugMessage("set deinitialization at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
          OnDestroyingCoreEvent+=singletonOrderedInReverse.Value.OnDestroyingCoreEvent;
         }
         if(Gameplayer.main!=null){
            Gameplayer.main.Init(netManager.LocalClientId);
         }
         MemoryManagement.CallGC(Time.time);
         Resources.UnloadUnusedAssets();
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
              Gameplayer.main=null;//  also unset current player prefab that is no more active
              //  game was deinitialized
              singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
    }
}