#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
     [NonSerialized]float clientSendMessageDelay=0.05f;
      [NonSerialized]float clientSendMessageTimer=10f;
      [NonSerialized]internal readonly Dictionary<int,FastBufferWriter>clientVoxelTerrainChunkEditDataRequestsToSend=new Dictionary<int,FastBufferWriter>();
     [NonSerialized]internal int clientMaxVoxelTerrainChunkEditDataRequestsPerFrame=8;
      [NonSerialized]internal int clientVoxelTerrainChunkEditDataRequestsSent;
       [NonSerialized]readonly List<int>clientVoxelTerrainChunkEditDataRequestsSentToRemove=new List<int>();
        internal void NetClientSideInit(){
         Log.DebugMessage("NetClientSideInit");
         Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage+=OnClientReceivedUnnamedMessage;
        }
        internal void NetClientSideOnDestroyingCoreNetworkDestroy(){
         if(Core.singleton.netManager.CustomMessagingManager!=null){
            Core.singleton.netManager.CustomMessagingManager.OnUnnamedMessage-=OnClientReceivedUnnamedMessage;
         }
        }
        internal void NetClientSideOnDestroyingCoreNetworkDispose(){
         foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
          FastBufferWriter request=clientSideRequestToSend.Value;
          request.Dispose();
         }
         clientVoxelTerrainChunkEditDataRequestsToSend.Clear();
         //  everything at client has been disposed
        }
        internal void NetClientSideNetUpdate(){
         if(clientSendMessageTimer<=0f){
            clientSendMessageTimer=clientSendMessageDelay;
          clientVoxelTerrainChunkEditDataRequestsSentToRemove.Clear();
          clientVoxelTerrainChunkEditDataRequestsSent=0;
          foreach(var clientSideRequestToSend in clientVoxelTerrainChunkEditDataRequestsToSend){
           FastBufferWriter request=clientSideRequestToSend.Value;
           if(Core.singleton.isClient){
            if(Core.singleton.netManager.IsConnectedClient){
             Core.singleton.netManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,request,NetworkDelivery.ReliableSequenced);
            }
           }
           request.Dispose();
           clientVoxelTerrainChunkEditDataRequestsSentToRemove.Add(clientSideRequestToSend.Key);
           clientVoxelTerrainChunkEditDataRequestsSent++;
           if(clientVoxelTerrainChunkEditDataRequestsSent>=clientMaxVoxelTerrainChunkEditDataRequestsPerFrame){
            break;
           }
          }
          foreach(int toRemove in clientVoxelTerrainChunkEditDataRequestsSentToRemove){
           clientVoxelTerrainChunkEditDataRequestsToSend.Remove(toRemove);
          }
         }else{
          clientSendMessageTimer-=Time.deltaTime;
         }
        }
    }
}