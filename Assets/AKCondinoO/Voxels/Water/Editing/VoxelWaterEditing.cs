#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Water.Editing{
    internal class VoxelWaterEditing:MonoBehaviour,ISingletonInitialization{
     internal static VoxelWaterEditing singleton{get;set;}
     internal VoxelWaterEditingContainer waterEditingBG=new VoxelWaterEditingContainer();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("VoxelWaterEditing:OnDestroyingCoreEvent");
         waterEditingBG.IsCompleted(VoxelSystem.singleton.waterEditingBGThread.IsRunning,-1);
        }
        internal struct WaterEditRequest{
         internal Vector3 center;
        }
        internal void EditWater(
         Vector3 at
        ){
         editRequests.Enqueue(
          new WaterEditRequest{
           center=at
          }
         );
        }
     [SerializeField]bool    DEBUG_ADD_WATER_SOURCE=false;
     [SerializeField]Vector3 DEBUG_ADD_WATER_SOURCE_AT=new Vector3(0,60,0);
     readonly Queue<WaterEditRequest>editRequests=new Queue<WaterEditRequest>();
     bool applyingEdits;
        void Update(){
         if(DEBUG_ADD_WATER_SOURCE){
            DEBUG_ADD_WATER_SOURCE=false;
          Log.DebugMessage("DEBUG_ADD_WATER_SOURCE_AT:"+DEBUG_ADD_WATER_SOURCE_AT);
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