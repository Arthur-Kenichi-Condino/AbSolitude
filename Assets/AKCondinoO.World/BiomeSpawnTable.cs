using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/SpawnTable")]
    internal class BiomeSpawnTable:ScriptableObject{
     public List<BiomeSpawnLayer>layers;
    }
}