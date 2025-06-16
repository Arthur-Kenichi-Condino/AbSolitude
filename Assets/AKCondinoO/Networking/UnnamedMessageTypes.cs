using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Networking{
    internal enum UnnamedMessageTypes:int{
     Undefined=0,
     FromClientVoxelTerrainChunkEditDataRequest=100,
     FromClientVoxelTerrainChunkEditDataReceived=101,
     FromClientVoxelTerrainChunkEditDataSegment=102,
    }
}