using AKCondinoO.SimObjects;
using System;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [Serializable]
    internal class BiomeSpawnEntry{
     public SimObject prefab;
     [Range(0,1)]public float chance;
    }
}