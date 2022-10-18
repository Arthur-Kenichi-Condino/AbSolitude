using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Voxels{
    //  is passive to changes by Voxel Chunks that update data to be used in background
    //  data should be changed in foreground and background only gets data
    //  background processing gets data from here
    //  should not change from the background processes because it can cause synchronization failures
    internal class VoxelSystemConcurrent{
    }
}