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
    }
}