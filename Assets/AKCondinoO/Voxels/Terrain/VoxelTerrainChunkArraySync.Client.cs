#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkArraySync{
     [NonSerialized]int?clientSidecnkIdx=null;
        private void OnClientSideNetcnkIdxValueChanged(int previous,int current){
         if(Core.singleton.isClient){
          if(!IsOwner){
           if(clientSidecnkIdx==null||current!=clientSidecnkIdx.Value){
            clientSidecnkIdx=current;
            Log.DebugMessage("'ask server for chunk data'");
            /*
              add sizeof(int) for the message type
              add sizeof(int) for the cnkIdx
            */
            FastBufferWriter writer=new FastBufferWriter(sizeof(int)*2,Allocator.Persistent);
            if(writer.TryBeginWrite(sizeof(int)*2)){
             writer.WriteValue((int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest);
             writer.WriteValue((int)current);
            }
            if(VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend.TryGetValue(current,out FastBufferWriter oldRequest)){oldRequest.Dispose();}
            VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend[current]=writer;
           }
          }
         }
        }
    }
}