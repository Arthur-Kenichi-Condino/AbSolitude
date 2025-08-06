#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using UnityEngine;
namespace AKCondinoO.Voxels.Biomes{
    internal class BiomeBehaviour:MonoBehaviour{
     [SerializeField]internal bool useHardCodedSurfaceSpawnIfAvailable=true;
     [SerializeField]internal BiomeSettings settings;
    }
}