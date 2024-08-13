#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain;
using AKCondinoO.Voxels.Terrain.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
namespace AKCondinoO.Voxels{
    internal partial class VoxelSystem{
     [SerializeField]VoxelTerrainChunkUnnamedMessageHandler _VoxelTerrainChunkUnnamedMessageHandlerPrefab;
     [SerializeField]VoxelTerrainChunkArraySync             _VoxelTerrainChunkArraySyncPrefab            ;
     [NonSerialized]internal ServerData asServer;
     [NonSerialized]internal ClientData asClient;
        internal void NetInit(){
         if(Core.singleton.isServer){
          asServer=new ServerData();
          asServer.NetServerSideInit();
         }
         if(Core.singleton.isClient){
          asClient=new ClientData();
          asClient.NetClientSideInit();
         }
        }
        internal void OnDestroyingCoreNetworkDestroy(){
         Log.DebugMessage("OnDestroyingCoreNetworkDestroy");
         if(Core.singleton.isServer){
          asServer.NetServerSideOnDestroyingCoreNetworkDestroy();
         }
         if(Core.singleton.isClient){
          asClient.NetClientSideOnDestroyingCoreNetworkDestroy();
         }
        }
        internal void OnDestroyingCoreNetworkDispose(){
         Log.DebugMessage("OnDestroyingCoreNetworkDispose");
         if(Core.singleton.isServer){
          asServer.NetServerSideOnDestroyingCoreNetworkDispose();
         }
         if(Core.singleton.isClient){
          asClient.NetClientSideOnDestroyingCoreNetworkDispose();
         }
        }
     [SerializeField]ulong DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0;
        internal void NetUpdate(){
         VoxelTerrainChunkUnnamedMessageHandler.StaticUpdate();
         VoxelTerrainChunkArraySync            .StaticUpdate();
         if(Core.singleton.isServer){
          asServer.NetServerSideNetUpdate();
         }
         if(Core.singleton.isClient){
          asClient.NetClientSideNetUpdate();
         }
        }
     [NonSerialized]internal readonly HashSet<Gameplayer>generationRequestedAssignMessageHandlers=new HashSet<Gameplayer>();
        [Serializable]internal partial class ServerData{
         [NonSerialized]internal readonly     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersPool    =new     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>();
         [NonSerialized]internal readonly Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersAssigned=new Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>();
          [NonSerialized]internal readonly          List<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlers=new List<VoxelTerrainChunkUnnamedMessageHandler>();
         [NonSerialized]internal readonly     LinkedList<VoxelTerrainChunkArraySync>terrainArraySyncsPool    =new     LinkedList<VoxelTerrainChunkArraySync>();
         [NonSerialized]internal readonly Dictionary<int,VoxelTerrainChunkArraySync>terrainArraySyncsAssigned=new Dictionary<int,VoxelTerrainChunkArraySync>();
          [NonSerialized]internal readonly          List<VoxelTerrainChunkArraySync>terrainArraySyncs=new List<VoxelTerrainChunkArraySync>();
           [NonSerialized]internal                             int netVoxelArraysMaxCount;
           [NonSerialized]internal                             int netVoxelArraysCount;
           [NonSerialized]internal readonly   Queue<VoxelArraySync>netVoxelArraysPool  =new   Queue<VoxelArraySync>();
           [NonSerialized]internal readonly HashSet<VoxelArraySync>netVoxelArraysActive=new HashSet<VoxelArraySync>();
            [NonSerialized]readonly         HashSet<VoxelArraySync>netVoxelArraysToPool=new HashSet<VoxelArraySync>();
             [NonSerialized]internal readonly HashSet<ulong>clientIdsRequestingNetVoxelArray            =new HashSet<ulong>();
              [NonSerialized]readonly         HashSet<ulong>clientIdsRequestingNetVoxelArrayDisconnected=new HashSet<ulong>();
            internal partial void NetServerSideInit();
            internal partial void NetServerSideOnDestroyingCoreNetworkDestroy();
            internal partial void NetServerSideOnDestroyingCoreNetworkDispose();
            internal partial void NetServerSideNetUpdate();
        }
        [Serializable]internal partial class ClientData{
          [NonSerialized]internal readonly          List<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlers=new List<VoxelTerrainChunkUnnamedMessageHandler>();
          [NonSerialized]internal readonly          List<VoxelTerrainChunkArraySync>terrainArraySyncs=new List<VoxelTerrainChunkArraySync>();
            internal partial void NetClientSideInit();
            internal partial void NetClientSideOnDestroyingCoreNetworkDestroy();
            internal partial void NetClientSideOnDestroyingCoreNetworkDispose();
            internal partial void NetClientSideNetUpdate();
        }
    }
}