#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditing:MonoBehaviour,ISingletonInitialization{
     internal static VoxelTerrainEditing singleton{get;set;}
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("VoxelTerrainEditing:OnDestroyingCoreEvent");
        }
     [SerializeField]bool DEBUG_EDIT=false;
        void Update(){
        }
    }
}