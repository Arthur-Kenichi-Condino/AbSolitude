#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Ambience.Clouds;
using AKCondinoO.Gameplaying;
using AKCondinoO.Music;
using AKCondinoO.Sims;
using AKCondinoO.Sims.Actors.Pathfinding;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillVisualEffects;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.UI;
using AKCondinoO.UI.Context;
using AKCondinoO.UI.Fixed;
using AKCondinoO.UI.WorldSpace;
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
using UnityEngine.Scripting;
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
     internal static Resolution[]resolutions;
      internal static float magicDeltaTimeNumber=.0060f;
        internal void SetMagicDeltaTimeNumber(int fps=-1){
         if(fps>0){
          magicDeltaTimeNumber=(1f/(float)fps)/((164.6f*2f)/(float)fps);
         }else{
          magicDeltaTimeNumber=(1f/(float)Screen.currentResolution.refreshRateRatio.value)/((164.6f*2f)/(float)Screen.currentResolution.refreshRateRatio.value);
         }
        }
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         LoadSceneParameters simObjectToSpriteToolLoadSceneParameters=new LoadSceneParameters();
         simObjectToSpriteToolLoadSceneParameters.loadSceneMode=LoadSceneMode.Additive;
         simObjectToSpriteToolLoadSceneParameters.localPhysicsMode=LocalPhysicsMode.Physics3D;
         SceneManager.LoadScene("SimObjectToSpriteTool",simObjectToSpriteToolLoadSceneParameters);
         int procCt=Environment.ProcessorCount;
         //Log.DebugMessage("Environment.ProcessorCount:"+procCt);
         ThreadPool.GetMinThreads(out int minWorkerThreads,out int minIOCThreads);
         //Log.DebugMessage("minimum number of worker threads:"+minWorkerThreads+";minimum asynchronous I/O completion threads:"+minIOCThreads);
         ThreadPool.GetMaxThreads(out int maxWorkerThreads,out int maxIOCThreads);
         //Log.DebugMessage("maximum number of worker threads:"+maxWorkerThreads+";maximum asynchronous I/O completion threads:"+maxIOCThreads);
         Thread.CurrentThread.Priority=System.Threading.ThreadPriority.Normal;
         #if !UNITY_EDITOR
          GarbageCollector.GCMode=GarbageCollector.Mode.Manual;
         #endif
         GCSettings.LatencyMode=GCLatencyMode.SustainedLowLatency;
                    Util.SetUtil();
         CultureInfoUtil.SetUtil();
                PhysUtil.SetUtil();
           RenderingUtil.SetUtil();
         savePath=string.Format("{0}{1}/",saveLocation,saveName);
         Directory.CreateDirectory(savePath);
         AStarPathfindingHelper.SetAStarPathfindingSettings();
         NavMeshHelper.SetNavMeshBuildSettings();
         resolutions=Screen.resolutions;
         //Log.DebugMessage("Screen.currentResolution.refreshRateRatio:"+Screen.currentResolution.refreshRateRatio);
         #if UNITY_STANDALONE_WIN
          if(Screen.currentResolution.refreshRateRatio.value>=144){
           QualitySettings.vSyncCount=2;
          }else{
           QualitySettings.vSyncCount=1;
          }
         #else
          Application.targetFrameRate=74;
         #endif
         SetMagicDeltaTimeNumber();
        }
     SortedDictionary<int,ISingletonInitialization>singletonInitOrder;
     IEnumerable<KeyValuePair<int,ISingletonInitialization>>singletonInitReversedOrder;
        private void Start(){
         fpsNextMeasurementRefresh=Time.realtimeSinceStartup+fpsMeasurementTimeInterval;
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
         int o=0;
         singletonInitOrder=new SortedDictionary<int,ISingletonInitialization>{
          {o++,GameplayerManagement     .singleton},
          {o++,InputHandler             .singleton},
          {o++,ScreenInput              .singleton},
          {o++,BGM                      .singleton},
          {o++,MainCamera               .singleton},
          {o++,SimTime                  .singleton},
          {o++,WindZoneControl          .singleton},
          {o++,CloudParticleSystem      .singleton},
          {o++,GameMode                 .singleton},
          {o++,VoxelSystem              .singleton},
          {o++,VoxelTerrainEditing      .singleton},
          {o++,VoxelWaterEditing        .singleton},
          {o++,AnimationManager         .singleton},
          {o++,AStarPathfinding         .singleton},
          {o++,SimInventoryManager      .singleton},
          {o++,SimObjectManager         .singleton},
          {o++,SimObjectSpawner         .singleton},
          {o++,SkillVisualEffectsManager.singleton},
          {o++,SkillAoEManager          .singleton},
          {o++,SkillsManager            .singleton},
          {o++,SimsMachine              .singleton},
          {o++,AutonomyCore             .singleton},
          {o++,Placeholder              .singleton},
          {o++,FixedUI                  .singleton},
          {o++,ContextMenuUI            .singleton},
          {o++,WorldSpaceUI             .singleton},
         };
         foreach(var singletonOrdered in singletonInitOrder){
          //Log.DebugMessage("initialization at "+singletonOrdered.Key+":"+singletonOrdered.Value);
          singletonOrdered.Value.Init();
         }
         foreach(var singletonOrderedInReverse in singletonInitReversedOrder=singletonInitOrder.Reverse()){
          //Log.DebugMessage("set deinitialization at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
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
               //Log.DebugMessage("unset destroyed singleton at "+singletonOrderedInReverse.Key+":"+singletonOrderedInReverse.Value);
               Type singletonType=singletonOrderedInReverse.Value.GetType();
               PropertyInfo singletonPropertyInfo=singletonType.GetProperty("singleton",BindingFlags.Static|BindingFlags.NonPublic);
               //Log.DebugMessage("singletonPropertyInfo:"+singletonPropertyInfo);
               singletonPropertyInfo.SetValue(null,null);
               //Log.DebugMessage("singletonPropertyInfo.GetValue(null):"+singletonPropertyInfo.GetValue(null));
              }
              Gameplayer.main=null;//  also unset current player prefab that is no more active
              //  game was deinitialized
              singleton=null;
         }
        }
     internal event EventHandler OnDestroyingCoreEvent;
        internal class OnDestroyingCoreEventArgs:EventArgs{
        }
     internal static int currentFps;
     const float fpsMeasurementTimeInterval=0.01f;
      private static float fpsNextMeasurementRefresh=0;
       private static int framesAccumulator=0;
     internal Camera currentRenderingTargetCamera{get;private set;}
      Vector3 currentRenderingTargetCameraRotation;
      Vector3 currentRenderingTargetCameraPosition;
       internal bool currentRenderingTargetCameraHasTransformChanges{get;private set;}
     internal bool currentRenderingTargetCameraChanged{get;private set;}
        void Update(){
         MemoryManagement.CallGC(Time.time);
         framesAccumulator++;
         if(Time.realtimeSinceStartup>fpsNextMeasurementRefresh){
          currentFps=(int)(framesAccumulator/fpsMeasurementTimeInterval);
          framesAccumulator=0;
          fpsNextMeasurementRefresh+=fpsMeasurementTimeInterval;
          SetMagicDeltaTimeNumber(currentFps);
         }
         currentRenderingTargetCameraChanged=false;
         if(Camera.main!=null&&currentRenderingTargetCamera!=(currentRenderingTargetCamera=Camera.main)){
          currentRenderingTargetCameraChanged=true;
          //Log.DebugMessage("currentRenderingTargetCameraChanged changed to Camera.main:"+currentRenderingTargetCamera);
         }
         if(currentRenderingTargetCamera==null){
          if(Camera.current!=null&&currentRenderingTargetCamera!=(currentRenderingTargetCamera=Camera.current)){
           currentRenderingTargetCameraChanged=true;
           Log.DebugMessage("currentRenderingTargetCameraChanged changed to Camera.current:"+currentRenderingTargetCamera);
          }
         }
         currentRenderingTargetCameraHasTransformChanges=false;
         if(currentRenderingTargetCamera!=null){
          if(currentRenderingTargetCamera==Camera.main){
           currentRenderingTargetCameraHasTransformChanges=MainCamera.singleton.hasTransformChanges;
           //Log.DebugMessage("does Camera.main transform has changes?"+currentCameraHasTransformChanges);
          }else{
           if(
            currentRenderingTargetCameraRotation!=(currentRenderingTargetCameraRotation=currentRenderingTargetCamera.transform.eulerAngles)||
            currentRenderingTargetCameraPosition!=(currentRenderingTargetCameraPosition=currentRenderingTargetCamera.transform.position)
           ){
            currentRenderingTargetCameraHasTransformChanges=true;
           }
           //Log.DebugMessage("does Camera.current transform has changes?"+currentCameraHasTransformChanges);
          }
         }
        }
    }
}