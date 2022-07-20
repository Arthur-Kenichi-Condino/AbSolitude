using AKCondinoO.Sims.Trees;
using System;
using System.Collections;
using System.Collections.Generic;
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
     readonly protected Dictionary<int,HashSet<Type>>simObjectPicking=new Dictionary<int,HashSet<Type>>();
     readonly protected Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>allSettings=new Dictionary<Type,Dictionary<int,List<SimObjectSettings>>>();
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
        internal(Type simObject,SimObjectSettings simObjectSettings)?TrySpawnSimObject(Vector3Int noiseInputRounded){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.Selection(noiseInput);
         if(simObjectPicking.TryGetValue(selection,out var types)){
          foreach(var type in types){
           if(allSettings.TryGetValue(type,out var typeSettings)){
            if(typeSettings.TryGetValue(selection,out var typeSettingsListForSelection)){
             foreach(SimObjectSettings setting in typeSettingsListForSelection){
             }
            }
           }
          }
         }
         return null;
        }
    }
}