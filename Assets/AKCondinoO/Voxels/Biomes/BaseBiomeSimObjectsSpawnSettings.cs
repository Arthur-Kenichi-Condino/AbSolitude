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
        }
     readonly protected Dictionary<int,Type[]>simObjectPicking;
        internal(Type simObject,SimObjectSettings simObjectSettings)?TrySpawnSimObject(Vector3Int noiseInputRounded){
         return null;
        }
    }
}