#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditing;
namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditingContainer:BackgroundContainer{
     internal object[]terrainSynchronization;
     internal readonly Queue<TerrainEditRequest>requests=new Queue<TerrainEditRequest>();
    }
    internal class VoxelTerrainEditingMultithreaded:BaseMultithreaded<VoxelTerrainEditingContainer>{
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainEditingMultithreaded:Execute()");
         foreach(object syn in container.terrainSynchronization){
          Monitor.Enter(syn);
         }
         try{
         }catch{
          throw;
         }finally{
          foreach(object syn in container.terrainSynchronization){
           Monitor.Exit(syn);
          }
         }
        }
    }
}