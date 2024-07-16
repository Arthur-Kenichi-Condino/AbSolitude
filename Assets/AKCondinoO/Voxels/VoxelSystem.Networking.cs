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
     [NonSerialized]internal readonly           List<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlers=new List<VoxelTerrainChunkUnnamedMessageHandler>();
     [NonSerialized]internal readonly     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersPool    =new     LinkedList<VoxelTerrainChunkUnnamedMessageHandler>();
     [NonSerialized]internal readonly Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>terrainMessageHandlersAssigned=new Dictionary<int,VoxelTerrainChunkUnnamedMessageHandler>();
      [NonSerialized]internal readonly List<VoxelTerrainChunkArraySync>terrainArraySyncs=new List<VoxelTerrainChunkArraySync>();
     [NonSerialized]internal                             int netVoxelArraysMaxCount;
     [NonSerialized]internal                             int netVoxelArraysCount;
     [NonSerialized]internal readonly   Queue<VoxelArraySync>netVoxelArraysPool  =new   Queue<VoxelArraySync>();
     [NonSerialized]internal readonly HashSet<VoxelArraySync>netVoxelArraysActive=new HashSet<VoxelArraySync>();
      [NonSerialized]readonly         HashSet<VoxelArraySync>netVoxelArraysToPool=new HashSet<VoxelArraySync>();
      [NonSerialized]internal readonly HashSet<ulong>clientIdsRequestingNetVoxelArray            =new HashSet<ulong>();
       [NonSerialized]readonly         HashSet<ulong>clientIdsRequestingNetVoxelArrayDisconnected=new HashSet<ulong>();
        internal void OnDestroyingCoreNetworkDestroy(){
         Log.DebugMessage("OnDestroyingCoreNetDestroy");
         if(Core.singleton.isServer){
          NetServerSideOnDestroyingCoreNetworkDestroy();
         }
         if(Core.singleton.isClient){
          NetClientSideOnDestroyingCoreNetworkDestroy();
         }
         for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
          if(i<terrainMessageHandlers.Count){
           VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
           cnkMsgr     .OnDestroyingCore();
          }
          if(i<terrainArraySyncs     .Count){
           VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
           cnkArraySync.OnDestroyingCore();
          }
         }
        }
        internal void OnDestroyingCoreNetworkDispose(){
         for(int i=0;i<Mathf.Max(terrainMessageHandlers.Count,terrainArraySyncs.Count);++i){
          if(i<terrainMessageHandlers.Count){
           VoxelTerrainChunkUnnamedMessageHandler cnkMsgr     =terrainMessageHandlers[i];
           cnkMsgr     .Dispose();
          }
          if(i<terrainArraySyncs     .Count){
           VoxelTerrainChunkArraySync             cnkArraySync=terrainArraySyncs     [i];
           cnkArraySync.Dispose();
          }
         }
         terrainMessageHandlers.Clear();
         terrainArraySyncs     .Clear();
         if(Core.singleton.isServer){
          NetServerSideOnDestroyingCoreNetworkDispose();
         }
         if(Core.singleton.isClient){
          NetClientSideOnDestroyingCoreNetworkDispose();
         }
        }
     [SerializeField]ulong DEBUG_SEND_VOXEL_TERRAIN_CHUNK_EDIT_DATA_TO_CLIENT=0;
        internal void NetUpdate(){
         VoxelTerrainChunkUnnamedMessageHandler.StaticUpdate();
         VoxelTerrainChunkArraySync            .StaticUpdate();
         if(Core.singleton.isServer){
          NetServerSideNetUpdate();
         }
         if(Core.singleton.isClient){
          NetClientSideNetUpdate();
         }
        }
    }
}