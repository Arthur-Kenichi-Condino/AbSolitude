using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler{
        void OnClientSideReceivedVoxelTerrainChunkEditDataSegment(ulong clientId,FastBufferReader reader){
         //Log.DebugMessage("OnClientSideReceivedVoxelTerrainChunkEditDataFileSegment");
          //testing, REMOVE:
         //FastBufferReader dataToReceivedFromServer=new FastBufferReader(reader,Allocator.Persistent,-1,0,Allocator.Persistent);
         //dataToReceivedFromServer.ReadValueSafe(out int readFirstValue);
         ////Log.DebugMessage("readFirstValue:"+readFirstValue);
         //Debug.Log("readFirstValue:"+readFirstValue);
         //dataToReceivedFromServer.Dispose();
         //  Validate segment with the cnkIdx in the message "header" by comparing it to the
         // current cnkIdx set to this MessageHandler
         //  if this message fails to be received, client may ask for it after detecting missing segments in the "dictionary" of
         // segment-data
        }
    }
}