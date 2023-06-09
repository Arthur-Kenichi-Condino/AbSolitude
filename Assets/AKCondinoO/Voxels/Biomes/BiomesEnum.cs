#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels.Biomes{
    internal enum Biomes:int{
     Wasteland=0,
    }
    internal partial class BaseBiome{
         internal virtual Biomes GetCurrent(Vector3 noiseInput){
          return Biomes.Wasteland;
         }
    }
}