#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
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
     internal readonly bool[]blocked=new bool[FlattenOffset];
     internal readonly SpawnData spawnData=new SpawnData(FlattenOffset);
        internal enum Execution{
         GetGround,
         FillSpawnData,
         SaveStateToFile,
        }
     internal Execution execution;
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerMultithreaded:BaseMultithreaded<VoxelTerrainSurfaceSimObjectsPlacerContainer>{
     static readonly object mutex=new object();
        protected override void Execute(){
         switch(container.execution){
          case Execution.GetGround:{
           //Log.DebugMessage("Execution.GetGround");
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
           //Log.DebugMessage("Execution.FillSpawnData");
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            int index=vCoord1.z+vCoord1.x*Depth;
            if(container.gotGroundHits[index]==null){
             continue;
            }
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
             maxSpacing=Vector3.Scale(maxSpacing,modifiers.scale);
             maxSpacing.x=MathF.Max(maxSpacing.x,1f);
             maxSpacing.y=MathF.Max(maxSpacing.y,1f);
             maxSpacing.z=MathF.Max(maxSpacing.z,1f);
             if(Width-1-vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
              continue;
             }
             if(vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
              continue;
             }
             if(Depth-1-vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
              continue;
             }
             if(vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
              continue;
             }
             RaycastHit floor=container.gotGroundHits[index].Value;
             Quaternion rotation=Quaternion.SlerpUnclamped(
              Quaternion.identity,
              Quaternion.FromToRotation(
               Vector3.up,
               floor.normal
              ),
              simObjectPicked.Value.simObjectSettings.inclination
             )*Quaternion.Euler(
              new Vector3(0f,modifiers.rotation,0f)
             );
             Vector3 position=new Vector3(
              floor.point.x,
              floor.point.y-modifiers.scale.y*simObjectPicked.Value.simObjectSettings.depth,
              floor.point.z
             )+rotation*(Vector3.down*modifiers.scale.y);
             for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<Mathf.CeilToInt(minSpacing.x);x2++){
             for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<Mathf.CeilToInt(minSpacing.z);z2++){
              Vector3Int vCoord2=vCoord1;
              vCoord2.x+=x2;
              vCoord2.z+=z2;
              int index2=vCoord2.z+vCoord2.x*Depth;
              if(container.blocked[index2]){
               goto _Continue;
              }
             }}
             for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<Mathf.CeilToInt(minSpacing.x);x2++){
             for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<Mathf.CeilToInt(minSpacing.z);z2++){
              Vector3Int vCoord2=vCoord1;
              vCoord2.x+=x2;
              vCoord2.z+=z2;
              if(vCoord2.x<0){
               continue;
              }
              if(vCoord2.x>=Width){
               continue;
              }
              if(vCoord2.z<0){
               continue;
              }
              if(vCoord2.z>=Depth){
               continue;
              }
              int index2=vCoord2.z+vCoord2.x*Depth;
              container.blocked[index2]=true;
             }}
             container.spawnData.at.Add((position,rotation.eulerAngles,modifiers.scale,simObjectPicked.Value.simObject,null,new SimObject.PersistentData()));
            }
            _Continue:{
             continue;
            }
           }}
           break;
          }
         }
        }
    }
}