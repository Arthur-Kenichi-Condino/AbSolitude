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
using static AKCondinoO.Voxels.Biomes.BiomeSettings;
namespace AKCondinoO.Voxels.Biomes{
    internal class BaseBiomeSimObjectsSpawnSettings{
     internal BiomeBehaviour biomeBehaviour{get{if(VoxelSystem.singleton!=null){return VoxelSystem.singleton.biomeBehaviour;}return null;}}
        internal enum SpawnedTypes:int{
         All=0,
         Grass=1,
         Trees=2,
         Bushes=3,
        }
        internal struct SimObjectSettings{
         internal Vector3 size;
         internal float chance;
         internal float inclination;
         internal Vector3 assetScale;
         internal Vector3 minScale;
         internal Vector3 maxScale;
         internal float depth;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>minSpacing;
         internal readonly ReadOnlyDictionary<SpawnedTypes,Vector3>maxSpacing;
         internal readonly ReadOnlyCollection<SpawnedTypes>blocksTypes;
         internal readonly ReadOnlyCollection<SpawnedTypes>isBlockedBy;
            internal SimObjectSettings(Vector3 size,float chance,float inclination,Vector3 assetScale,Vector3 minScale,Vector3 maxScale,float depth,Dictionary<SpawnedTypes,Vector3>minSpacing,Dictionary<SpawnedTypes,Vector3>maxSpacing,SpawnedTypes[]blocksTypes,SpawnedTypes[]isBlockedBy){
             this.size=size;
             this.chance=chance;
             this.inclination=inclination;
             this.assetScale=assetScale;
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
     internal static Vector3Int maxSpawnSize=new Vector3Int(60,60,60);
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
        }
        internal void Set(){
         if(biomeBehaviour.useHardCodedSurfaceSpawnIfAvailable){
          #region hard-coded settings
          //SetSimObjectTypeAtPicking(1,typeof(Pinus_elliottii_1),out List<SimObjectSettings>Pinus_elliottii_1SettingsListAtPicking1);
          //Pinus_elliottii_1SettingsListAtPicking1.Add(
          // new SimObjectSettings(
          //  chance:.5f,
          //  inclination:.125f,
          //  minScale:Vector3.one*.25f,
          //  maxScale:Vector3.one*.5f,
          //  depth:1.8f,
          //  minSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*1.0f},{SpawnedTypes.Trees,Vector3.one*2.0f}},
          //  maxSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*2.0f},{SpawnedTypes.Trees,Vector3.one*4.0f}},
          //  blocksTypes:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Trees},
          //  isBlockedBy:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Trees}
          // ){
          // }
          //);
          //SetSimObjectTypeAtPicking(1,typeof(Betula_occidentalis_1),out List<SimObjectSettings>Betula_occidentalis_1SettingsListAtPicking1);
          //Betula_occidentalis_1SettingsListAtPicking1.Add(
          // new SimObjectSettings(
          //  chance:.5f,
          //  inclination:.125f,
          //  minScale:Vector3.one*.125f,
          //  maxScale:Vector3.one*.25f,
          //  depth:-1.2f,
          //  minSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*1.0f},{SpawnedTypes.Bushes,Vector3.one*1.0f}},
          //  maxSpacing:new Dictionary<SpawnedTypes,Vector3>{{SpawnedTypes.All,Vector3.one*2.0f},{SpawnedTypes.Bushes,Vector3.one*2.0f}},
          //  blocksTypes:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Bushes},
          //  isBlockedBy:new SpawnedTypes[]{SpawnedTypes.All,SpawnedTypes.Bushes}
          // ){
          // }
          //);
          #endregion hard-coded settings
         }
         foreach(SurfaceSpawn surfaceSpawnSetting in biomeBehaviour.settings.biomeSurfaceSpawns){
          Log.DebugMessage("spawn setting added for:"+surfaceSpawnSetting.simObject.GetType());
          Log.Warning("TO DO: criar cooldown de spawn de objeto (e remover/refazer 'divisão de chance' no Selection) e lidar com objetos maiores que um chunk");
          Log.Warning("TO DO: rotacionar objeto para que não fique flutuando ao ser spawn'ado");
          Log.Warning("TO DO: limitar tamanho de objetos aqui pelo tamanho máximo de spawn menos 1");
          SetSimObjectTypeAtPicking(surfaceSpawnSetting.picking,surfaceSpawnSetting.simObject.GetType(),out List<SimObjectSettings>settingsListAtPicking);
          settingsListAtPicking.Add(
           new SimObjectSettings(
            size:       surfaceSpawnSetting.size,
            chance:     surfaceSpawnSetting.chance,
            inclination:surfaceSpawnSetting.inclination,
            assetScale: surfaceSpawnSetting.assetScale,
            minScale:   surfaceSpawnSetting.minScale,
            maxScale:   surfaceSpawnSetting.maxScale,
            depth:      surfaceSpawnSetting.depth,
            minSpacing:surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
            maxSpacing:surfaceSpawnSetting.minSpacing.ToDictionary(k=>k.spawnedType,v=>v.spacingDis),
            blocksTypes:surfaceSpawnSetting.blocksTypes,
            isBlockedBy:surfaceSpawnSetting.isBlockedBy
           ){
           }
          );
         }
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
        protected void SetSimObjectTypeAtPicking(int picking,Type simObjectType,out List<SimObjectSettings>simObjectTypeSettingsListAtPicking){
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
     internal Perlin simObjectSpawnChancePerlin;
        internal(Type simObject,SimObjectSettings simObjectSettings)?TryGetSettingsToSpawnSimObject(Vector3Int noiseInputRounded,out double selectionValue){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.GetSelectorValue(noiseInput,out selectionValue);
         if(simObjectPicking.TryGetValue(selection,out var types)){
          int count=0;
          foreach(var type in types){
           if(allSettings.TryGetValue(type,out var typeSettings)){
            if(typeSettings.TryGetValue(selection,out var typeSettingsListForSelection)){
             foreach(SimObjectSettings setting in typeSettingsListForSelection){
              float chance=setting.chance;//settingsCountForSelection[selection];
              float dicing=Mathf.Clamp01(((float)simObjectSpawnChancePerlin.GetValue(noiseInput.z,noiseInput.x,(count+1)*.5f)+1f)/2f);
              count++;
              if(dicing<chance){
               //Log.DebugMessage("'dicing<chance':dicing:"+dicing+";chance:"+chance+";(type,setting):"+(type,setting));
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