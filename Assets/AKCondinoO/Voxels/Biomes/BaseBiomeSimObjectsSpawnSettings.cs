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
     readonly protected Dictionary<Type,SimObjectSettings[]>allSettings=new Dictionary<Type,SimObjectSettings[]>();
     readonly BaseBiome biome;
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
         allSettings.Add(
          typeof(Pinus_elliottii_1),
          new SimObjectSettings[]{
           new SimObjectSettings{
            chance=.125f,
            inclination=.125f,
            minScale=Vector3.one*.5f,
            maxScale=Vector3.one*1.5f,
            depth=1.2f,
            minSpacing=Vector3.one*2.4f,
            maxSpacing=Vector3.one*4.8f,
           },
          }
         );
         if(!simObjectPicking.TryGetValue(1,out var types1)){
          types1=simObjectPicking[1]=new List<Type>();
         }
         types1.Add(typeof(Pinus_elliottii_1));
        }
     readonly protected Dictionary<int,List<Type>>simObjectPicking=new Dictionary<int,List<Type>>();
        internal(Type simObject,SimObjectSettings simObjectSettings)?TrySpawnSimObject(Vector3Int noiseInputRounded){
         Vector3 noiseInput=noiseInputRounded+biome.deround;
         int selection=biome.Selection(noiseInput);
         if(simObjectPicking.TryGetValue(selection,out var types)){
          //SimObjectSettings simObjectSettings=allSettings[];
         }
         return null;
        }
    }
}