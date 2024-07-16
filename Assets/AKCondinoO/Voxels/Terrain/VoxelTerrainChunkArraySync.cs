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
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkArraySync:MonoBehaviour{
     [NonSerialized]internal VoxelTerrainChunkUnnamedMessageHandler cnkMsgr;
     [NonSerialized]internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     [NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         if(Core.singleton.isServer){
          serverSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
         if(Core.singleton.isClient){
          VoxelSystem.singleton.terrainArraySyncs.Add(this);
         }
        }
        internal void OnInstantiated(){
        }
        internal void OnDestroyingCore(){
         if(this!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
          StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
         }
         terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
        }
        internal void Dispose(){
         terrainGetFileEditDataToNetSyncBG.Dispose();
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