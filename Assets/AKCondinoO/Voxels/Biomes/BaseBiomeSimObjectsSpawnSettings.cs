#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Trees;
using LibNoise.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Voxels.Biomes{
    internal class BaseBiomeSimObjectsSpawnSettings{
        internal enum SpawnedTypes:int{
         All=0,
         Grass=1,
         Trees=2,
         Bushes=3,
        }
        internal struct SimObjectSettings{
         internal float chance;
         internal float inclination;
         internal Vector3 minScale;
         internal Vector3 maxScale;
         internal float depth;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>minSpacing;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>maxSpacing;
         internal readonly ReadOnlyCollection<SpawnedTypes>blocksTypes;
         internal readonly ReadOnlyCollection<SpawnedTypes>isBlockedBy;
            internal SimObjectSettings(float chance,float inclination,Vector3 minScale,Vector3 maxScale,float depth,Dictionary<SpawnedTypes,Vector3>minSpacing,Dictionary<SpawnedTypes,Vector3>maxSpacing,SpawnedTypes[]blocksTypes,SpawnedTypes[]isBlockedBy){
             this.chance=chance;
             this.inclination=inclination;
             this.minScale=minScale;
             this.maxScale=maxScale;
             this.depth=depth;
             this.minSpacing=new ReadOnlyDictionary<SpawnedTypes,Vector3>(minSpacing);
             this.maxSpacing=new ReadOnlyDictionary<SpawnedTypes,Vector3>(maxSpacing);
             this.blocksTypes=new ReadOnlyCollection<SpawnedTypes>(blocksTypes);
             this.isBlockedBy=new ReadOnlyCollection<SpawnedTypes>(isBlockedBy);
            }
        }
        internal struct SimObjectSpawnModifiers{
         internal Vector3 scale;
         internal float rotation;
        }
     readonly protected Dictionary<int,HashSet<Type>>simObjectPicking=new Dictionary<int,HashSet<Type>>();
     readonly protected Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>allSettings=new Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>();
     readonly protected Dictionary<int,int>settingsCountForSelection=new Dictionary<int,int>();
     readonly BaseBiome biome;
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
         AddSimObjectTypeAtPicking(1,typeof(Pinus_elliottii_1),out List<SimObjectSettings>Pinus_elliottii_1SettingsListAtPicking1);
         Pinus_elliottii_1SettingsListAtPicking1.Add(
          new SimObjectSettings(
           chance:.5f,
           inclination:.125f,
           minScale:Vector3.one*.5f,
           maxScale:Vector3.one*.75f,
           depth:1.2f,
           minSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*1.0f},{SpawnedTypes.Trees,Vector3.one*2.0f}},
           maxSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*2.0f},{SpawnedTypes.Trees,Vector3.one*4.0f}},
           blocksTypes:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Trees},
           isBlockedBy:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Trees}
          ){
          }
         );
         AddSimObjectTypeAtPicking(1,typeof(Betula_occidentalis_1),out List<SimObjectSettings>Betula_occidentalis_1SettingsListAtPicking1);
         Betula_occidentalis_1SettingsListAtPicking1.Add(
          new SimObjectSettings(
           chance:.5f,
           inclination:.125f,
           minScale:Vector3.one*.5f,
           maxScale:Vector3.one*.75f,
           depth:1.2f,
           minSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*1.0f},{SpawnedTypes.Bushes,Vector3.one*1.0f}},
           maxSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*2.0f},{SpawnedTypes.Bushes,Vector3.one*2.0f}},
           blocksTypes:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Bushes},
           isBlockedBy:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Bushes}
          ){
          }
         );
        }
        protected void AddSimObjectTypeAtPicking(int picking,Type simObjectType,out List<SimObjectSettings>simObjectTypeSettingsListAtPicking){
         if(!simObjectPicking.TryGetValue(picking,out var typesAtPicking)){
          typesAtPicking=simObjectPicking[picking]=new HashSet<Type>();
         }
         typesAtPicking.Add(simObjectType);
         if(!allSettings.TryGetValue(simObjectType,out var simObjectTypeSettings)){
          simObjectTypeSettings=allSettings[simObjectType]=new Dictionary<int,List<SimObjectSettings>>();
         }
         if(!simObjectTypeSettings.TryGetValue(picking,out simObjectTypeSettingsListAtPicking)){
          simObjectTypeSettingsListAtPicking=simObjectTypeSettings[picking]=new List<SimObjectSettings>();
         }
        }
        internal void Set(){
         foreach(var selectionTypesPair in simObjectPicking){
          int selection=selectionTypesPair.Key;
          HashSet<Type>types=selectionTypesPair.Value;
          int settingsCount=0;
          foreach(var typeSettingsListByPickingPair in allSettings){
           Type type=typeSettingsListByPickingPair.Key;
           Dictionary<int,List<SimObjectSettings>>settingsListByPicking=typeSettingsListByPickingPair.Value;
           if(types.Contains(type)){
            if(settingsListByPicking.TryGetValue(selection,out List<SimObjectSettings>settingsList)){
             settingsCount+=settingsList.Count;
            }
           }
          }
          settingsCountForSelection[selection]=settingsCount;
          //Log.DebugMessage("BaseBiomeSimObjectsSpawnSettings Set():settingsCountForSelection["+selection+"]="+settingsCount);
         }
        }
     internal Perlin simObjectSpawnChancePerlin;
        internal(Type simObject,SimObjectSettings simObjectSettings)?TryGetSettingsToSpawnSimObject(Vector3Int noiseInputRounded){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.Selection(noiseInput);
         if(simObjectPicking.TryGetValue(selection,out var types)){
          int count=0;
          foreach(var type in types){
           if(allSettings.TryGetValue(type,out var typeSettings)){
            if(typeSettings.TryGetValue(selection,out var typeSettingsListForSelection)){
             foreach(SimObjectSettings setting in typeSettingsListForSelection){
              float chance=setting.chance/settingsCountForSelection[selection];
              float dicing=Mathf.Clamp01(((float)simObjectSpawnChancePerlin.GetValue(noiseInput.z,noiseInput.x,(count+1)*.5f)+1f)/2f);
              count++;
              if(dicing<chance){
               return(type,setting);
              }
             }
            }
           }
          }
         }
         return null;
        }
     internal Perlin  scaleModifierPerlin;
     internal Perlin rotationModifierPerlin;
        internal virtual SimObjectSpawnModifiers GetSimObjectSpawnModifiers(Vector3Int noiseInputRounded,SimObjectSettings simObjectSettings){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         SimObjectSpawnModifiers modifiers=new SimObjectSpawnModifiers{
          scale=Vector3.Lerp(
           simObjectSettings.minScale,
           simObjectSettings.maxScale,
           Mathf.Clamp01(
            ((float)scaleModifierPerlin.GetValue(noiseInput.z,noiseInput.x,0)+1f)/2f
           )
          ),
          rotation=(float)rotationModifierPerlin.GetValue(noiseInput.z,noiseInput.x,0)*720f,
         };
         return modifiers;
        }
    }
}