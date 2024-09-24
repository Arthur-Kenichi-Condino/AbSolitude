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
             for(int i=0;i<terrainMessageHandlers.Count;++i){
              VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
              cnkMsgr     .asClient.NetClientSideOnDestroyingCore();
             }
             for(int i=0;i<terrainArraySyncs     .Count;++i){
              VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
              cnkArraySync.asClient.NetClientSideOnDestroyingCore();
             }
            }
            internal partial void NetClientSideOnDestroyingCoreNetworkDispose(){
             for(int i=0;i<terrainMessageHandlers.Count;++i){
              VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
              cnkMsgr     .asClient.NetClientSideDispose();
             }
             for(int i=0;i<terrainArraySyncs     .Count;++i){
              VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
              cnkArraySync.asClient.NetClientSideDispose();
             }
             terrainMessageHandlers.Clear();
             terrainArraySyncs     .Clear();
             foreach(var clientSideRequestToSend in clientSideChunkEditDataRequestsToSend){
              FastBufferWriter request=clientSideRequestToSend.Value;
              request.Dispose();
             }
             clientSideChunkEditDataRequestsToSend.Clear();
             Log.DebugMessage("everything at client has been disposed");
            }
         [NonSerialized]float clientSideSendMessageDelay=1f;
          [NonSerialized]float clientSideSendMessageTimer=5f;
         [NonSerialized]internal int clientSideMaxChunkEditDataRequestsPerFrame=8;
          [NonSerialized]internal int clientSideChunkEditDataRequestsSent;
           [NonSerialized]readonly List<int>clientSideChunkEditDataRequestsSentToRemove=new List<int>();
            internal partial void NetClientSideNetUpdate(){
             for(int i=0;i<terrainMessageHandlers.Count;++i){
              VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
              cnkMsgr     .asClient.NetClientSideManualUpdate();
             }
             for(int i=0;i<terrainArraySyncs     .Count;++i){
              VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
              cnkArraySync.asClient.NetClientSideManualUpdate();
             }
             if(clientSideSendMessageTimer>0f){
                clientSideSendMessageTimer-=Time.deltaTime;
             }
             if(clientSideSendMessageTimer<=0f){
                clientSideSendMessageTimer=clientSideSendMessageDelay;
              clientSideChunkEditDataRequestsSent=0;
              foreach(var clientSideRequestToSend in clientSideChunkEditDataRequestsToSend){
               FastBufferWriter request=clientSideRequestToSend.Value;
               if(Core.singleton.isClient){
                if(Core.singleton.netManager.IsConnectedClient){
                 Core.singleton.netManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,request,NetworkDelivery.ReliableSequenced);
                }
               }
               request.Dispose();
               clientSideChunkEditDataRequestsSentToRemove.Add(clientSideRequestToSend.Key);
               clientSideChunkEditDataRequestsSent++;
               if(clientSideChunkEditDataRequestsSent>=clientSideMaxChunkEditDataRequestsPerFrame){
                break;
               }
              }
              foreach(int toRemove in clientSideChunkEditDataRequestsSentToRemove){
               clientSideChunkEditDataRequestsToSend.Remove(toRemove);
              }
              clientSideChunkEditDataRequestsSentToRemove.Clear();
             }
            }
        }
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