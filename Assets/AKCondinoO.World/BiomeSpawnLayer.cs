using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [Serializable]
    internal class BiomeSpawnLayer{
     public int layer;
     public int gridSize;
     public List<BiomeSpawnEntry>entries;
    }
}