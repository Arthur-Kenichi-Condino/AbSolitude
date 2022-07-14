using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Biomes{
    internal class BaseBiomeSimObjectsSpawnSettings{
     readonly BaseBiome biome;
        internal BaseBiomeSimObjectsSpawnSettings(BaseBiome biome){
         this.biome=biome;
        }
        internal struct SimObjectSettings{
         internal float chance;
         internal float inclination;
         internal Vector3 minScale;
         internal Vector3 maxScale;
         internal float depth;
         internal Vector3 minSpacing;
         internal Vector3 maxSpacing;
        }
        internal(Type simObject,SimObjectSettings simObjectSettings)?TrySpawnSimObject(Vector3Int noiseInputRounded){
         return null;
        }
    }
}