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
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainGetFileEditDataToNetSyncContainer;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainSendEditDataToServerContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     [NonSerialized]internal VoxelTerrainSendEditDataToServerContainer terrainSendEditDataToServerBG=new VoxelTerrainSendEditDataToServerContainer();
     [NonSerialized]internal NetworkObject netObj;
     [NonSerialized]internal ServerData asServer;
     [NonSerialized]internal ClientData asClient;
        [Serializable]internal partial class ServerData{
         [NonSerialized]internal VoxelTerrainChunkUnnamedMessageHandler cnkMsgr;
            internal ServerData(VoxelTerrainChunkUnnamedMessageHandler cnkMsgr){
             this.cnkMsgr=cnkMsgr;
            }
            internal partial void NetServerSideOnDestroyingCore();
            internal partial void NetServerSideDispose();
        }
        [Serializable]internal partial class ClientData{
         [NonSerialized]internal VoxelTerrainChunkUnnamedMessageHandler cnkMsgr;
            internal ClientData(VoxelTerrainChunkUnnamedMessageHandler cnkMsgr){
             this.cnkMsgr=cnkMsgr;
            }
            internal partial void NetClientSideOnDestroyingCore();
            internal partial void NetClientSideDispose();
        }
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         if(Core.singleton.isServer){
          asServer=new ServerData(this);
          VoxelSystem.singleton.asServer.terrainMessageHandlers.Add(this);
         }
         if(Core.singleton.isClient){
          asClient=new ClientData(this);
          VoxelSystem.singleton.asClient.terrainMessageHandlers.Add(this);
         }
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
        }
        internal static void StaticUpdate(){
        }
     ///*
     //  sizeof(int)   : 4 bytes
     //  sizeof(double): 8 bytes
     //  sizeof(ushort): 2 bytes
     //*/
     ///*
     //  headerSize: 5*4=20 bytes:
     //   ...add sizeof(int) for the message type
     //   ...add sizeof(int) for the cnkIdx
     //   ...add sizeof(int) for the current segment
     //   ...add sizeof(int) for the total segments (segment count)
     //   ...add sizeof(int) for the segment writes count
     //*/
     //[NonSerialized]internal const int headerSize=sizeof(int)*5;
     ///*
     //  voxelEditSize: 4+8+2=14 bytes:
     //   ...add sizeof(int) for voxel index
     //   ...add sizeof(double) for voxel density
     //   ...add sizeof(ushort) for voxel material id
     //*/
     //[NonSerialized]internal const int voxelEditSize=sizeof(int)+sizeof(double)+sizeof(ushort);
     ////  voxelsPerChunk: 16*16*256=65536 voxels...
     //// ...voxelsPerChunk * VoxelEditDataSize is all edit data size if whole chunk is edited:
     //// 65536*20=1310720 bytes: 1.310720 Megabytes
     //   void Awake(){
     //    netObj=GetComponent<NetworkObject>();
     //    int voxelsPerSegment=(VoxelsPerChunk/voxelDataToSendSplits);
     //    terrainSendEditDataToServerBG.voxelsPerSegment=voxelsPerSegment;
     //    segmentSize=terrainSendEditDataToServerBG.segmentSize=(voxelsPerSegment*voxelEditSize+headerSize);
     //    int voxelsInLastSegment=(VoxelsPerChunk/voxelDataToSendSplits)+(VoxelsPerChunk%voxelDataToSendSplits);
     //    terrainSendEditDataToServerBG.voxelsInLastSegment=voxelsInLastSegment;
     //    lastSegmentSize=terrainSendEditDataToServerBG.lastSegmentSize=(voxelsInLastSegment*voxelEditSize+headerSize);
     //    if(Core.singleton.isClient){
     //     VoxelSystem.singleton.terrainMessageHandlers.Add(this);
     //    }
     //   }
     //   internal void OnInstantiated(){
     //    cnkArraySync=Instantiate(_VoxelTerrainChunkArraySyncPrefab);
     //    cnkArraySync.cnkMsgr=this;
     //    cnkArraySync.OnInstantiated();
     //    VoxelSystem.singleton.terrainArraySyncs.Add(cnkArraySync);
     //   }
     //   internal void OnDestroyingCore(){
     //    if(terrainSendEditDataToServerBG.dataToSendToServer!=null){
     //     foreach(var segmentBufferPair in terrainSendEditDataToServerBG.dataToSendToServer){
     //      FastBufferWriter writer=segmentBufferPair.Value;
     //      if(writer.IsInitialized){
     //       writer.Dispose();
     //      }
     //     }
     //     terrainSendEditDataToServerBG.dataToSendToServer.Clear();
     //     dataToSendDictionaryPool.Enqueue(terrainSendEditDataToServerBG.dataToSendToServer);
     //     terrainSendEditDataToServerBG.dataToSendToServer=null;
     //    }
     //   }
     //[NonSerialized]internal NetworkObject netObj;
     // internal readonly NetworkVariable<NetChunkId>netChunkId=new NetworkVariable<NetChunkId>(default,
     //  NetworkVariableReadPermission.Everyone,
     //  NetworkVariableWritePermission.Server
     // );
     // internal NetworkList<bool>netTerrainChunkArrayHasChanges=new NetworkList<bool>(
     //  new bool[chunkVoxelArraySplits],
     //  NetworkVariableReadPermission.Everyone,
     //  NetworkVariableWritePermission.Server
     // );
     //   public override void OnNetworkSpawn(){
     //    if(Core.singleton.isClient){
     //     OnClientSideNetChunkIdValueChanged(netChunkId.Value,netChunkId.Value);//  update on spawn
     //     netChunkId.OnValueChanged+=OnClientSideNetChunkIdValueChanged;
     //     OnClientSideNetTerrainChunkArrayHasChangesValueChanged(default);
     //     netTerrainChunkArrayHasChanges.OnListChanged+=OnClientSideNetTerrainChunkArrayHasChangesValueChanged;
     //     clientSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ClientSideSendVoxelTerrainChunkEditDataFileCoroutine());
     //    }
     //   }
     //   public override void OnNetworkDespawn(){
     //    if(this!=null&&clientSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
     //     StopCoroutine(clientSideSendVoxelTerrainChunkEditDataFileCoroutine);
     //    }
     //    if(Core.singleton.isClient){
     //     netChunkId.OnValueChanged-=OnClientSideNetChunkIdValueChanged;
     //     netTerrainChunkArrayHasChanges.OnListChanged-=OnClientSideNetTerrainChunkArrayHasChangesValueChanged;
     //    }
     //    base.OnNetworkDespawn();
     //    if(sendingDataToServer!=null){
     //     foreach(var segmentBufferPair in sendingDataToServer){
     //      int segment=segmentBufferPair.Key;
     //      if(sentSegments.Contains(segment)){
     //       continue;
     //      }
     //      FastBufferWriter writer=segmentBufferPair.Value;
     //      if(writer.IsInitialized){
     //       writer.Dispose();
     //      }
     //     }
     //     sendingDataToServer.Clear();
     //     dataToSendDictionaryPool.Enqueue(sendingDataToServer);
     //     sendingDataToServer=null;
     //     sentSegments.Clear();
     //    }
     //   }
     //[NonSerialized]internal static int maxMessagesPerFrame=voxelDataToSendSplits/8;
     // [NonSerialized]internal static int messagesSent;
     //[NonSerialized]internal static double sendingMaxExecutionTime=0.05;
     // [NonSerialized]internal static double sendingExecutionTime;
     //[NonSerialized]internal static float segmentSizeToTimeInSecondsDelayRatio=(1.25f/16f)/(VoxelsPerChunk);//  turns segment Length into seconds to wait
     // [NonSerialized]internal static int totalLengthOfDataSent;
     //[NonSerialized]internal static float globalCooldownToSendNewMessages;//  totalLengthOfDataSent * segmentSizeToTimeInSecondsDelayRatio
     //   internal static void StaticUpdate(){
     //    if(globalCooldownToSendNewMessages>0f){
     //     globalCooldownToSendNewMessages-=Time.deltaTime;
     //     if(globalCooldownToSendNewMessages<=0f){
     //      messagesSent=0;
     //     }
     //    }else if(messagesSent>0){
     //     globalCooldownToSendNewMessages=totalLengthOfDataSent*segmentSizeToTimeInSecondsDelayRatio;
     //     totalLengthOfDataSent=0;
     //     Log.DebugMessage("StaticUpdate:globalCooldownToSendNewMessages:"+globalCooldownToSendNewMessages);
     //    }
     //    sendingExecutionTime=0d;
     //   }
    }
    public struct NetChunkId:IEquatable<NetChunkId>,INetworkSerializable{
     public Vector2Int cCoord;
     public Vector2Int cnkRgn;
     public int        cnkIdx;
        public NetChunkId(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx){
         this.cCoord=cCoord;this.cnkRgn=cnkRgn;this.cnkIdx=cnkIdx;
        }
        public void NetworkSerialize<T>(BufferSerializer<T>serializer)where T:IReaderWriter{
         if(serializer.IsWriter){
          serializer.GetFastBufferWriter().WriteValueSafe(cCoord);
          serializer.GetFastBufferWriter().WriteValueSafe(cnkRgn);
          serializer.GetFastBufferWriter().WriteValueSafe(cnkIdx);
         }else{
          serializer.GetFastBufferReader().ReadValueSafe(out cCoord);
          serializer.GetFastBufferReader().ReadValueSafe(out cnkRgn);
          serializer.GetFastBufferReader().ReadValueSafe(out cnkIdx);
         }
        }
        public static bool operator==(NetChunkId a,NetChunkId b){
         if(
          a.cCoord==b.cCoord&&
          a.cnkRgn==b.cnkRgn&&
          a.cnkIdx==b.cnkIdx
         ){
          return true;
         }
         return false;
        }
        public static bool operator!=(NetChunkId a,NetChunkId b){
         return!(a==b);
        }
        public override bool Equals(object obj){
         if(!(obj is NetChunkId netChunkId)){
          return false;
         }
         return this==netChunkId;
        }
        public override int GetHashCode(){
         return HashCode.Combine(
          cCoord,
          cnkRgn,
          cnkIdx
         );
        }
        public bool Equals(NetChunkId other){
         return this==other;
        }
    }
}