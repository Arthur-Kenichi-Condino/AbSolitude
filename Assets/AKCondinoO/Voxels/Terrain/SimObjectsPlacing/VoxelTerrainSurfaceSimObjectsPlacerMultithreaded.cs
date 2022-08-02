#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Biomes.BaseBiomeSimObjectsSpawnSettings;
using static AKCondinoO.Voxels.Terrain.SimObjectsPlacing.VoxelTerrainSurfaceSimObjectsPlacerContainer;
namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal class VoxelTerrainSurfaceSimObjectsPlacerContainer:BackgroundContainer{
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
     internal readonly Dictionary<int,RaycastHit?>gotGroundHits=new Dictionary<int,RaycastHit?>(Width*Depth);
        internal enum Execution{
         GetGround,
         FillSpawnData,
        }
     internal Execution execution;
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerMultithreaded:BaseMultithreaded<VoxelTerrainSurfaceSimObjectsPlacerContainer>{
        protected override void Execute(){
         switch(container.execution){
          case Execution.GetGround:{
           Log.DebugMessage("Execution.GetGround");
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            Vector3 from=vCoord1;
                    from.x+=container.cnkRgn.x-Width/2f+.5f;
                    from.z+=container.cnkRgn.y-Depth/2f+.5f;
            container.GetGroundRays.AddNoResize(new RaycastCommand(from,Vector3.down,Height,VoxelSystem.voxelTerrainLayer,1));
            container.GetGroundHits.AddNoResize(new RaycastHit    ()                                                        );
           }}
           break;
          }
          case Execution.FillSpawnData:{
           Log.DebugMessage("Execution.FillSpawnData");
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
							     Vector3Int noiseInput=vCoord1;noiseInput.x+=container.cnkRgn.x;
                                          noiseInput.z+=container.cnkRgn.y;
							     (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput);
							     if(simObjectPicked!=null){
								     SimObjectSpawnModifiers modifiers=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput,simObjectPicked.Value.simObjectSettings);
								     Vector3 minSpacing=simObjectPicked.Value.simObjectSettings.minSpacing;
								     minSpacing=Vector3.Scale(minSpacing,modifiers.scale);
								     minSpacing.x=Mathf.Max(minSpacing.x,1f);
								     minSpacing.y=Mathf.Max(minSpacing.y,1f);
								     minSpacing.z=Mathf.Max(minSpacing.z,1f);
								     Vector3 maxSpacing=simObjectPicked.Value.simObjectSettings.maxSpacing;
								    }
           }}
           break;
          }
         }
        }
    }
}