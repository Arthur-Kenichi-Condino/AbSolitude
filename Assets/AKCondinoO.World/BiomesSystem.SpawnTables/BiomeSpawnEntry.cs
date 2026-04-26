using AKCondinoO.SimObjects;
using LibNoise;
using System;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [Serializable]
    internal class BiomeSpawnEntry{
     public SimObject prefab;
     [Range(0,1)]public float chance;
     public SpawnVariations variations;
     [Serializable]
     internal class SpawnVariations{
      public Vector3 rotMin,rotMax;
      [Min(.01f)]public Vector3 scaleMin;
      [Min(.01f)]public Vector3 scaleMax;
      public bool alignToTerrain=true;
     }
    }
}