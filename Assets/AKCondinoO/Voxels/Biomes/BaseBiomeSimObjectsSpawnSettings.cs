#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Trees;
using LibNoise.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Voxels.Biomes{
    internal class BaseBiomeSimObjectsSpawnSettings{
        internal struct SimObjectSettings{
         internal float chance;
         internal float inclination;
         internal Vector3 minScale;
         internal Vector3 maxScale;
         internal float depth;
         internal Vector3 minSpacing;
         internal Vector3 maxSpacing;
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
         if(!simObjectPicking.TryGetValue(1,out var typesAtPicking1)){
          typesAtPicking1=simObjectPicking[1]=new HashSet<Type>();
         }
         typesAtPicking1.Add(typeof(Pinus_elliottii_1));
         if(!allSettings.TryGetValue(typeof(Pinus_elliottii_1),out var Pinus_elliottii_1Settings)){
          Pinus_elliottii_1Settings=allSettings[typeof(Pinus_elliottii_1)]=new Dictionary<int,List<SimObjectSettings>>();
         }
         if(!Pinus_elliottii_1Settings.TryGetValue(1,out var Pinus_elliottii_1SettingsListAtPicking1)){
          Pinus_elliottii_1SettingsListAtPicking1=Pinus_elliottii_1Settings[1]=new List<SimObjectSettings>();
         }
         Pinus_elliottii_1SettingsListAtPicking1.Add(
          new SimObjectSettings{
           chance=.125f,
           inclination=.125f,
           minScale=Vector3.one*.5f,
           maxScale=Vector3.one*1.5f,
           depth=1.2f,
           minSpacing=Vector3.one*2.4f,
           maxSpacing=Vector3.one*4.8f,
          }
         );
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
          Log.DebugMessage("BaseBiomeSimObjectsSpawnSettings Set():settingsCountForSelection["+selection+"]="+settingsCount);
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