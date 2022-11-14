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
     [SerializeField]bool    DEBUG_ADD_WATER_SOURCE=false;
     [SerializeField]Vector3 DEBUG_ADD_WATER_SOURCE_AT=new Vector3(0,60,0);
        void Update(){
        }
    }
}