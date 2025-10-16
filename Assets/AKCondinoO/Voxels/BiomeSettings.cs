#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.Biomes.BaseBiomeSimObjectsSpawnSettings;
namespace AKCondinoO.Voxels.Biomes{
    internal class BiomeSettings:MonoBehaviour{
     [SerializeField]internal SurfaceSpawnLayer[]biomeSurfaceSpawnsByLayer;
        [Serializable]internal struct SurfaceSpawnLayer{
         public int layer;
         public SurfaceSpawn[]biomeSurfaceSpawns;
        }
        [Serializable]internal struct SurfaceSpawn{
         public int picking;
         public SimObject simObject;
         public Vector3 size;
         public float chance;
         public int priority;
         public float inclination;
         public Vector3 assetScale;
         public Vector3 minScale;
         public Vector3 maxScale;
         public Vector2 rotation;
         public float depth;
            [Serializable]internal struct SpacingData{
             public SpawnedTypes spawnedType;
             public Vector3 spacingDis;
            }
         public SpacingData[]minSpacing;
         public SpacingData[]maxSpacing;
         public SpawnedTypes[]blocksTypes;
         public SpawnedTypes[]isBlockedBy;
         public Vector3 pivot;
        }
    }
}