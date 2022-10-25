#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        private void OnClientReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment){
          //Log.DebugMessage("messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment");
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkEditDataSegment(clientId,reader);
          }
         }
        }
        void OnClientSideReceivedVoxelTerrainChunkEditDataSegment(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnClientSideReceivedVoxelTerrainChunkEditDataSegment");
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