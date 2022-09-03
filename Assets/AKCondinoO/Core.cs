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
using System.Reflection;
using UnityEngine;
namespace AKCondinoO{
    internal class Core:MonoBehaviour{
     internal static Core singleton;
     internal static int threadCount;
     internal static readonly string saveLocation=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/";
     internal static string saveName="terra";
     internal static string savePath;
     internal static string terrainEditingPath;
     internal static string terrainEditingFilenameFormat="{0}.{1}.txt";
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
          { 0,InputHandler       .singleton},
          { 1,MainCamera         .singleton},
          { 2,SimTime            .singleton},
          { 3,VoxelSystem        .singleton},
          { 4,VoxelTerrainEditing.singleton},
          { 5,SimObjectManager   .singleton},
          { 6,SimObjectSpawner   .singleton},
          { 7,SkillsManager      .singleton},
          { 8,SimsMachine        .singleton},
          { 9,AutonomyCore       .singleton},
          {10,GameMode           .singleton},
          {11,Placeholder        .singleton},
          {12,FixedUI            .singleton},
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