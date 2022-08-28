#if UNITY_EDITOR
    #define ENABLE_DEBUG_GIZMOS
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;

namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditing:MonoBehaviour,ISingletonInitialization{
     internal static VoxelTerrainEditing singleton{get;set;}
     internal VoxelTerrainEditingContainer terrainEditingBG=new VoxelTerrainEditingContainer();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("VoxelTerrainEditing:OnDestroyingCoreEvent");
         terrainEditingBG.IsCompleted(VoxelSystem.singleton.terrainEditingBGThread.IsRunning,-1);
        }
        internal enum EditMode{
         PlaceCube,
        }
        internal struct TerrainEditRequest{
         internal Vector3 center;
         internal EditMode mode;
         internal Vector3Int size;
         internal double density;
         internal MaterialId material;
         internal int smoothness;
        }
        internal void EditTerrain(
         Vector3 at,
          EditMode mode,
           Vector3Int size,
            double density,
             MaterialId material,
              int smoothness
        ){
         editRequests.Enqueue(
          new TerrainEditRequest{
           center=at,
           mode=mode,
           size=size,
           density=density,
           material=material,
           smoothness=smoothness
          }
         );
        }
     [SerializeField]bool       DEBUG_EDIT=false;
     [SerializeField]Vector3Int DEBUG_EDIT_AT=new Vector3Int(0,40,40);
     [SerializeField]EditMode   DEBUG_EDIT_MODE=EditMode.PlaceCube;
     [SerializeField]Vector3Int DEBUG_EDIT_SIZE=new Vector3Int(3,3,3);
     [SerializeField]double     DEBUG_EDIT_DENSITY=100.0;
     [SerializeField]MaterialId DEBUG_EDIT_MATERIAL_ID=MaterialId.Dirt;
     [SerializeField]int        DEBUG_EDIT_SMOOTHNESS=5;
     readonly Queue<TerrainEditRequest>editRequests=new Queue<TerrainEditRequest>();
     bool applyingEdits;
        void Update(){
         if(DEBUG_EDIT){
            DEBUG_EDIT=false;
          Log.DebugMessage("DEBUG_EDIT_AT:"+DEBUG_EDIT_AT);
          EditTerrain(
           DEBUG_EDIT_AT,
           DEBUG_EDIT_MODE,
           DEBUG_EDIT_SIZE,
           DEBUG_EDIT_DENSITY,
           DEBUG_EDIT_MATERIAL_ID,
           DEBUG_EDIT_SMOOTHNESS
          );
         }
         if(applyingEdits){
             if(OnTerrainEditingApplied()){
                 applyingEdits=false;
             }
         }else{
             if(editRequests.Count>0){
                 Log.DebugMessage("editRequests.Count>0");
                 if(OnTerrainEditingRequestsPush()){
                     OnTerrainEditingRequestsPushed();
                 }
             }
         }
        }
        bool OnTerrainEditingRequestsPush(){
         if(terrainEditingBG.IsCompleted(VoxelSystem.singleton.terrainEditingBGThread.IsRunning)){
          while(editRequests.Count>0){
           terrainEditingBG.requests.Enqueue(editRequests.Dequeue());
          }
          VoxelTerrainEditingMultithreaded.Schedule(terrainEditingBG);
          return true;
         }
         return false;
        }
        void OnTerrainEditingRequestsPushed(){
         applyingEdits=true;
        }
        bool OnTerrainEditingApplied(){
         if(terrainEditingBG.IsCompleted(VoxelSystem.singleton.terrainEditingBGThread.IsRunning)){
          //  TO DO: refresh chunks...
          return true;
         }
         return false;
        }
    }
}