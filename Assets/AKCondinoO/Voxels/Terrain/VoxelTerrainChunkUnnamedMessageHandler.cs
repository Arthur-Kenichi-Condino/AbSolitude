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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainChunkUnnamedMessageHandler;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     [SerializeField]VoxelTerrainChunkArraySync _VoxelTerrainChunkArraySyncPrefab;
     [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
     [NonSerialized]internal VoxelTerrainSendEditDataToServerContainer terrainSendEditDataToServerBG=new VoxelTerrainSendEditDataToServerContainer();
     [NonSerialized]internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     [NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
     /*
       sizeof(int)   : 4 bytes
       sizeof(double): 8 bytes
       sizeof(ushort): 2 bytes
     */
     /*
       headerSize: 5*4=20 bytes:
        ...add sizeof(int) for the message type
        ...add sizeof(int) for the cnkIdx
        ...add sizeof(int) for the current segment
        ...add sizeof(int) for the total segments (segment count)
        ...add sizeof(int) for the segment writes count
     */
     [NonSerialized]internal const int headerSize=sizeof(int)*5;
     /*
       voxelEditSize: 4+8+2=14 bytes:
        ...add sizeof(int) for voxel index
        ...add sizeof(double) for voxel density
        ...add sizeof(ushort) for voxel material id
     */
     [NonSerialized]internal const int voxelEditSize=sizeof(int)+sizeof(double)+sizeof(ushort);
     //  voxelsPerChunk: 16*16*256=65536 voxels...
     // ...voxelsPerChunk * VoxelEditDataSize is all edit data size if whole chunk is edited:
     // 65536*20=1310720 bytes: 1.310720 Megabytes
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         int voxelsPerSegment=(VoxelsPerChunk/splits);
         terrainSendEditDataToServerBG.voxelsPerSegment=voxelsPerSegment;
         segmentSize=terrainSendEditDataToServerBG.segmentSize=(voxelsPerSegment*voxelEditSize+headerSize);
         int voxelsInLastSegment=(VoxelsPerChunk/splits)+(VoxelsPerChunk%splits);
         terrainSendEditDataToServerBG.voxelsInLastSegment=voxelsInLastSegment;
         lastSegmentSize=terrainSendEditDataToServerBG.lastSegmentSize=(voxelsInLastSegment*voxelEditSize+headerSize);
         if(Core.singleton.isClient){
          VoxelSystem.singleton.terrainMessageHandlers.Add(this);
         }
        }
        internal void OnInstantiated(){
         cnkArraySync=Instantiate(_VoxelTerrainChunkArraySyncPrefab);
         cnkArraySync.cnkMsgr=this;
         cnkArraySync.OnInstantiated();
         VoxelSystem.singleton.terrainArraySyncs.Add(cnkArraySync);
        }
        internal void OnDestroyingCore(){
         terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning,-1);
         if(terrainSendEditDataToServerBG.dataToSendToServer!=null){
          foreach(var segmentBufferPair in terrainSendEditDataToServerBG.dataToSendToServer){
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          terrainSendEditDataToServerBG.dataToSendToServer.Clear();
          dataToSendDictionaryPool.Enqueue(terrainSendEditDataToServerBG.dataToSendToServer);
          terrainSendEditDataToServerBG.dataToSendToServer=null;
         }
        }
        internal void Dispose(){
         terrainSendEditDataToServerBG.Dispose();
        }
     [NonSerialized]internal NetworkObject netObj;
      private readonly NetworkVariable<int>netcnkIdx=new NetworkVariable<int>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         if(Core.singleton.isClient){
          OnClientSideNetcnkIdxValueChanged(netcnkIdx.Value,netcnkIdx.Value);//  update on spawn
          netcnkIdx.OnValueChanged+=OnClientSideNetcnkIdxValueChanged;
          clientSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ClientSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
        }
        public override void OnNetworkDespawn(){
         if(this!=null&&clientSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
          StopCoroutine(clientSideSendVoxelTerrainChunkEditDataFileCoroutine);
         }
         if(Core.singleton.isClient){
          netcnkIdx.OnValueChanged-=OnClientSideNetcnkIdxValueChanged;
         }
         base.OnNetworkDespawn();
         if(sendingDataToServer!=null){
          foreach(var segmentBufferPair in sendingDataToServer){
           int segment=segmentBufferPair.Key;
           if(sentSegments.Contains(segment)){
            continue;
           }
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          sendingDataToServer.Clear();
          dataToSendDictionaryPool.Enqueue(sendingDataToServer);
          sendingDataToServer=null;
          sentSegments.Clear();
         }
        }
     [NonSerialized]internal static int maxMessagesPerFrame=splits/8;
      [NonSerialized]internal static int messagesSent;
     [NonSerialized]internal static double sendingMaxExecutionTime=0.05;
      [NonSerialized]internal static double sendingExecutionTime;
     [NonSerialized]internal static float segmentSizeToTimeInSecondsDelayRatio=(1.25f/16f)/(VoxelsPerChunk);//  turns segment Length into seconds to wait
      [NonSerialized]internal static int totalLengthOfDataSent;
     [NonSerialized]internal static float globalCooldownToSendNewMessages;//  totalLengthOfDataSent * segmentSizeToTimeInSecondsDelayRatio
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