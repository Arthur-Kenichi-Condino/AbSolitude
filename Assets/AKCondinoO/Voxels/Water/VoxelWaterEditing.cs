#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.Editing{
    internal class VoxelWaterEditing:MonoBehaviour,ISingletonInitialization{
     internal static VoxelWaterEditing singleton{get;set;}
     internal VoxelWaterEditingContainer waterEditingBG=new VoxelWaterEditingContainer();
     internal static string waterEditingPath;
     internal static string waterEditingFileFormat="{0}waterChunkEdits.{1}.{2}.txt";
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         waterEditingPath=string.Format("{0}{1}",Core.savePath,"WaterChunkEdits/");
         Directory.CreateDirectory(waterEditingPath);
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("VoxelWaterEditing:OnDestroyingCoreEvent");
         waterEditingBG.IsCompleted(VoxelSystem.singleton.waterEditingBGThread.IsRunning,-1);
        }
        internal struct WaterEditRequest{
         internal Vector3 center;
         internal Vector3Int size;
         internal bool wakeUp;
         internal double density;
         internal double previousDensity;
         internal bool hasBlockage;
         internal float evaporateAfter;
        }
        internal void EditWater(
         Vector3 at,
          Vector3Int size,
           bool wakeUp,
            double density,
             double previousDensity,
              bool hasBlockage,
               float evaporateAfter
        ){
         editRequests.Enqueue(
          new WaterEditRequest{
           center=at,
           size=size,
           wakeUp=wakeUp,
           density=density,
           previousDensity=previousDensity,
           hasBlockage=hasBlockage,
           evaporateAfter=evaporateAfter
          }
         );
        }
     [SerializeField]bool       DEBUG_ADD_WATER_SOURCE=false;
     [SerializeField]Vector3    DEBUG_ADD_WATER_SOURCE_AT=new Vector3(0,60,0);
     [SerializeField]Vector3Int DEBUG_ADD_WATER_SOURCE_SIZE=new Vector3Int(1,1,1);
     [SerializeField]double     DEBUG_ADD_WATER_SOURCE_DENSITY=100.0;
     [SerializeField]double     DEBUG_ADD_WATER_SOURCE_PREVIOUS_DENSITY=0.0;
     [SerializeField]bool       DEBUG_ADD_WATER_SOURCE_WAKE_UP=true;
     [SerializeField]bool       DEBUG_ADD_WATER_SOURCE_HAS_BLOCKAGE=false;
     [SerializeField]float      DEBUG_ADD_WATER_SOURCE_EVAPORATE_AFTER=-1f;
     readonly Queue<WaterEditRequest>editRequests=new Queue<WaterEditRequest>();
     bool applyingEdits;
        void Update(){
         if(DEBUG_ADD_WATER_SOURCE){
            DEBUG_ADD_WATER_SOURCE=false;
          Log.DebugMessage("DEBUG_ADD_WATER_SOURCE_AT:"+DEBUG_ADD_WATER_SOURCE_AT);
          EditWater(
           DEBUG_ADD_WATER_SOURCE_AT,
           DEBUG_ADD_WATER_SOURCE_SIZE,
           DEBUG_ADD_WATER_SOURCE_WAKE_UP,
           DEBUG_ADD_WATER_SOURCE_DENSITY,
           DEBUG_ADD_WATER_SOURCE_PREVIOUS_DENSITY,
           DEBUG_ADD_WATER_SOURCE_HAS_BLOCKAGE,
           DEBUG_ADD_WATER_SOURCE_EVAPORATE_AFTER
          );
         }
         if(applyingEdits){
             if(OnWaterEditingApplied()){
                 applyingEdits=false;
             }
         }else{
             if(editRequests.Count>0){
                 Log.DebugMessage("editRequests.Count>0");
                 if(OnWaterEditRequestsPush()){
                     OnWaterEditRequestsPushed();
                 }
             }
         }
        }
        bool OnWaterEditRequestsPush(){
         if(waterEditingBG.IsCompleted(VoxelSystem.singleton.waterEditingBGThread.IsRunning)){
          while(editRequests.Count>0){
           waterEditingBG.requests.Enqueue(editRequests.Dequeue());
          }
          VoxelWaterEditingMultithreaded.Schedule(waterEditingBG);
          return true;
         }
         return false;
        }
        void OnWaterEditRequestsPushed(){
         applyingEdits=true;
        }
        bool OnWaterEditingApplied(){
         if(waterEditingBG.IsCompleted(VoxelSystem.singleton.waterEditingBGThread.IsRunning)){
          return true;
         }
         return false;
        }
    }
}