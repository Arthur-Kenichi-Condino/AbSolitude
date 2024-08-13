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
     [NonSerialized]internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     [NonSerialized]internal NetworkObject netObj;
     [NonSerialized]internal ServerData asServer;
     [NonSerialized]internal ClientData asClient;
        [Serializable]internal partial class ServerData{
         [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
            internal ServerData(VoxelTerrainChunkArraySync cnkArraySync){
             this.cnkArraySync=cnkArraySync;
            }
            internal partial void NetServerSideOnDestroyingCore();
            internal partial void NetServerSideDispose();
        }
        [Serializable]internal partial class ClientData{
         [NonSerialized]internal VoxelTerrainChunkArraySync cnkArraySync;
            internal ClientData(VoxelTerrainChunkArraySync cnkArraySync){
             this.cnkArraySync=cnkArraySync;
            }
            internal partial void NetClientSideOnDestroyingCore();
            internal partial void NetClientSideDispose();
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
        internal static void StaticUpdate(){
        }
     //[NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
     //   void Awake(){
     //    if(Core.singleton.isServer){
     //     serverSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
     //    }
     //    if(Core.singleton.isClient){
     //     VoxelSystem.singleton.terrainArraySyncs.Add(this);
     //    }
     //   }
     //   internal void OnInstantiated(){
     //   }
     //   internal void OnDestroyingCore(){
     //   }
     //   internal void Dispose(){
     //    terrainGetFileEditDataToNetSyncBG.Dispose();
     //   }
     //[NonSerialized]internal static int maxMessagesPerFrame=chunkVoxelArraySplits/8;
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
}