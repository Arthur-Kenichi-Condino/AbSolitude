#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Voxels.Terrain.Networking;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
        internal partial class ClientData{
            internal partial void NetClientSideInit(){
            }
            internal partial void NetClientSideOnDestroyingCoreNetworkDestroy(){
             for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
              if(i<terrainMessageHandlers.Count){
               VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
               cnkMsgr     .asClient.NetClientSideOnDestroyingCore();
              }
              if(i<terrainArraySyncs     .Count){
               VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
               cnkArraySync.asClient.NetClientSideOnDestroyingCore();
              }
             }
            }
            internal partial void NetClientSideOnDestroyingCoreNetworkDispose(){
             for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
              if(i<terrainMessageHandlers.Count){
               VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
               cnkMsgr     .asClient.NetClientSideDispose();
              }
              if(i<terrainArraySyncs     .Count){
               VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
               cnkArraySync.asClient.NetClientSideDispose();
              }
             }
             terrainMessageHandlers.Clear();
             terrainArraySyncs     .Clear();
            }
            internal partial void NetClientSideNetUpdate(){
            }
        }
     //[NonSerialized]float clientSendMessageDelay=0.05f;
     // [NonSerialized]float clientSendMessageTimer=10f;
     // [NonSerialized]internal readonly Dictionary<int,FastBufferWriter>clientVoxelTerrainChunkEditDataRequestsToSend=new Dictionary<int,FastBufferWriter>();
     //[NonSerialized]internal int clientMaxVoxelTerrainChunkEditDataRequestsPerFrame=8;
     // [NonSerialized]internal int clientVoxelTerrainChunkEditDataRequestsSent;
     //  [NonSerialized]readonly List<int>clientVoxelTerrainChunkEditDataRequestsSentToRemove=new List<int>();
     //   internal void NetClientSideInit(){
     //    Log.DebugMessage("NetClientSideInit");
     //    Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage+=OnClientReceivedUnnamedMessage;
     //   }
     //   internal void NetClientSideOnDestroyingCoreNetworkDestroy(){
     //    if(Core.singleton.netManager.CustomMessagingManager!=null){
     //       Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage-=OnClientReceivedUnnamedMessage;
     //    }
     //   }
     //   internal void NetClientSideOnDestroyingCoreNetworkDispose(){
     //    foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
     //     FastBufferWriter request=clientSideRequestToSend.Value;
     //     request.Dispose();
     //    }
     //    clientVoxelTerrainChunkEditDataRequestsToSend.Clear();
     //    //  everything at client has been disposed
     //   }
     //   internal void NetClientSideNetUpdate(){
     //    for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
     //     if(i<terrainMessageHandlers.Count){
     //      VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
     //      cnkMsgr     .NetClientSideManualUpdate();
     //     }
     //     if(i<terrainArraySyncs     .Count){
     //      VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
     //      cnkArraySync.NetClientSideManualUpdate();
     //     }
     //    }
     //    if(clientSendMessageTimer<=0f){
     //       clientSendMessageTimer=clientSendMessageDelay;
     //     clientVoxelTerrainChunkEditDataRequestsSentToRemove.Clear();
     //     clientVoxelTerrainChunkEditDataRequestsSent=0;
     //     foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
     //      FastBufferWriter request=clientSideRequestToSend.Value;
     //      if(Core.singleton.isClient){
     //       if(Core.singleton.netManager.IsConnectedClient){
     //        Core.singleton.netManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,request,NetworkDelivery.ReliableSequenced);
     //       }
     //      }
     //      request.Dispose();
     //      clientVoxelTerrainChunkEditDataRequestsSentToRemove.Add(clientSideRequestToSend.Key);
     //      clientVoxelTerrainChunkEditDataRequestsSent++;
     //      if(clientVoxelTerrainChunkEditDataRequestsSent>=clientMaxVoxelTerrainChunkEditDataRequestsPerFrame){
     //       break;
     //      }
     //     }
     //     foreach(int toRemove in clientVoxelTerrainChunkEditDataRequestsSentToRemove){
     //      clientVoxelTerrainChunkEditDataRequestsToSend.Remove(toRemove);
     //     }
     //    }else{
     //     clientSendMessageTimer-=Time.deltaTime;
     //    }
     //   }
    }
}