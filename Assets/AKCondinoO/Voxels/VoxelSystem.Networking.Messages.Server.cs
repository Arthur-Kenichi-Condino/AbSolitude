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
        private void OnServerReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest){
          //Log.DebugMessage("messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest");
          if(Core.singleton.isServer){
           OnServerSideReceivedVoxelTerrainChunkEditDataRequest(clientId,reader);
          }
         }
        }
        private void OnServerSideReceivedVoxelTerrainChunkEditDataRequest(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataRequest");
         int cnkIdx;
         reader.ReadValueSafe(out cnkIdx);
         Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataRequest:cnkIdx:"+cnkIdx);
         //clientIdsRequestingData.Add(clientId);
         //pendingGetFileEditData=true;
        }
    }
}