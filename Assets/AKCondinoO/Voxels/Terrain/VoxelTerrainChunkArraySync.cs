#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Networking;
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkArraySync;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainGetFileEditDataToNetSyncContainer;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkArraySync:NetworkBehaviour{
     [SerializeField]internal VoxelArraySync _VoxelArraySyncPrefab;
     [NonSerialized]internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     [NonSerialized]internal NetworkObject netObj;
      internal readonly NetworkVariable<NetChunkId>netChunkId=new NetworkVariable<NetChunkId>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
      internal NetworkList<bool>netChunkHasChanges=new NetworkList<bool>(
       new bool[chunkVoxelArraySplits],
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
     [NonSerialized]internal ServerData asServer;
     [NonSerialized]internal ClientData asClient;
        [Serializable]internal partial class ServerData{
         [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
            internal ServerData(VoxelTerrainChunkArraySync cnkArraySync){
             this.cnkArraySync=cnkArraySync;
            }
         [NonSerialized]internal readonly Dictionary<int,VoxelArraySync>netVoxelArraysActive=new Dictionary<int,VoxelArraySync>();
         [NonSerialized]internal LinkedListNode<VoxelTerrainChunkArraySync>expropriated;
         [NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
            internal partial void OnInstantiated();
            internal partial void OnSpawn();
            internal partial void NetServerSideOnDestroyingCore();
            internal partial void NetServerSideDispose();
            internal partial void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall);
            internal partial void NetServerSideManualUpdate();
        }
        [Serializable]internal partial class ClientData{
         [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
            internal ClientData(VoxelTerrainChunkArraySync cnkArraySync){
             this.cnkArraySync=cnkArraySync;
            }
            internal partial void NetClientSideOnDestroyingCore();
            internal partial void NetClientSideDispose();
            internal partial void NetClientSideManualUpdate();
        }
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         if(Core.singleton.isServer){
          asServer=new ServerData(this);
          VoxelSystem.singleton.asServer.terrainArraySyncs.Add(this);
         }
         if(Core.singleton.isClient){
          asClient=new ClientData(this);
          VoxelSystem.singleton.asClient.terrainArraySyncs.Add(this);
         }
        }
     internal bool spawnInitialization;
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         spawnInitialization=true;
         if(Core.singleton.isServer){
          asServer.OnSpawn();
         }
         if(Core.singleton.isClient){
          asClient.OnClientSideNetChunkIdValueChanged(netChunkId.Value,netChunkId.Value);//  update on spawn
          netChunkId.OnValueChanged+=asClient.OnClientSideNetChunkIdValueChanged;
          asClient.OnClientSideNetChunkHasChangesValueChanged(default);
          netChunkHasChanges.OnListChanged+=asClient.OnClientSideNetChunkHasChangesValueChanged;
     //     clientSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ClientSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
         spawnInitialization=false;
        }
        public override void OnNetworkDespawn(){
         if(Core.singleton.isClient){
          netChunkId.OnValueChanged-=asClient.OnClientSideNetChunkIdValueChanged;
          netChunkHasChanges.OnListChanged-=asClient.OnClientSideNetChunkHasChangesValueChanged;
         }
         base.OnNetworkDespawn();
        }
     [NonSerialized]internal static float globalCooldownToSendNewMessages;//  totalLengthOfDataSent * segmentSizeToTimeInSecondsDelayRatio
      [NonSerialized]internal static float segmentSizeToTimeInSecondsDelayRatio=(1.25f/16f)/(VoxelsPerChunk);//  turns segment Length into seconds to wait
       [NonSerialized]internal static int totalLengthOfDataSent;
      [NonSerialized]internal static int maxMessagesPerFrame=chunkVoxelArraySplits/8;
       [NonSerialized]internal static int messagesSent;
      [NonSerialized]internal static double sendingMaxExecutionTime=0.05;
       [NonSerialized]internal static double sendingExecutionTime;
        internal static void StaticUpdate(){
         if(globalCooldownToSendNewMessages>0f){
          globalCooldownToSendNewMessages-=Time.deltaTime;
          if(globalCooldownToSendNewMessages<=0f){
           messagesSent=0;
          }
         }else if(messagesSent>0){
          globalCooldownToSendNewMessages=totalLengthOfDataSent*segmentSizeToTimeInSecondsDelayRatio;
          totalLengthOfDataSent=0;
          Log.DebugMessage("StaticUpdate:globalCooldownToSendNewMessages:"+globalCooldownToSendNewMessages);
         }
         sendingExecutionTime=0d;
        }
    }
}