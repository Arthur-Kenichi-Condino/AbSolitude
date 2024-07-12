#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
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
     [NonSerialized]internal VoxelTerrainSendEditDataToServerContainer terrainSendEditDataToServerBG=new VoxelTerrainSendEditDataToServerContainer();
     [NonSerialized]internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     [NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
        }
        internal void OnInstantiated(){
         if(Core.singleton.isServer){
          int voxelsPerSegment=(VoxelsPerChunk/splits);
          terrainSendEditDataToServerBG.voxelsPerSegment=voxelsPerSegment;
          segmentSize=terrainSendEditDataToServerBG.segmentSize=(voxelsPerSegment*voxelEditSize+headerSize);
          int voxelsInLastSegment=(VoxelsPerChunk/splits)+(VoxelsPerChunk%splits);
          terrainSendEditDataToServerBG.voxelsInLastSegment=voxelsInLastSegment;
          lastSegmentSize=terrainSendEditDataToServerBG.lastSegmentSize=(voxelsInLastSegment*voxelEditSize+headerSize);
         }
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
         }
         if(Core.singleton.isServer){
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
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          netcnkIdx.Value=id.Value.cnkIdx;
          pendingGetFileEditData=true;
         }
        }
        internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
         //Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:'cnkIdx':"+id.Value.cnkIdx);
         clientIdsRequestingData.Add(clientId);
         pendingGetFileEditData=true;
        }
     [NonSerialized]internal static int maxMessagesPerFrame=2;
      [NonSerialized]internal static int messagesSent;
     [NonSerialized]internal static double sendingMaxExecutionTime=1.0;
      [NonSerialized]internal static double sendingExecutionTime;
     [NonSerialized]internal static float segmentSizeToTimeInSecondsDelayRatio=.1f/VoxelsPerChunk;//  turns segment Length into seconds to wait
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
     [SerializeField]bool DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA=false;
     [NonSerialized]bool waitingGetFileEditData;
     [NonSerialized]bool pendingGetFileEditData;
        internal void ManualUpdate(){
            if(Core.singleton.isServer){
             if(netObj.IsSpawned){
              if(waitingGetFileEditData){
                  if(OnGotFileEditData()){
                      waitingGetFileEditData=false;
                  }
              }else{
                  if(pendingGetFileEditData){
                      if(CanGetFileEditData()){
                          pendingGetFileEditData=false;
                          waitingGetFileEditData=true;
                      }
                  }
              }
             }
            }
        }
     //  TO DO: send interval
     readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
        bool CanGetFileEditData(){
         if((DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA)&&terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning)){
          terrainSendEditDataToServerBG.DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA=DEBUG_FORCE_SEND_WHOLE_CHUNK_DATA;
          terrainSendEditDataToServerBG.cCoord=id.Value.cCoord;
          terrainSendEditDataToServerBG.cnkRgn=id.Value.cnkRgn;
          terrainSendEditDataToServerBG.cnkIdx=id.Value.cnkIdx;
          VoxelTerrainSendEditDataToServerMultithreaded.Schedule(terrainSendEditDataToServerBG);
          return true;
         }
         return false;
        }
        bool OnGotFileEditData(){
         if(!sending&&terrainSendEditDataToServerBG.IsCompleted(VoxelSystem.singleton.terrainSendEditDataToServerBGThreads[0].IsRunning)){
          Log.DebugMessage("OnGotFileEditData");
          clientIdsToSendData.AddRange(clientIdsRequestingData);
          clientIdsRequestingData.Clear();
          sendingcnkIdx      =terrainSendEditDataToServerBG.cnkIdx;
          sendingDataToServer=terrainSendEditDataToServerBG.dataToSendToServer;
          terrainSendEditDataToServerBG.dataToSendToServer=null;
          segmentCount=sendingDataToServer.Count;
          sending=true;
          Log.DebugMessage("segmentCount:"+segmentCount);
          return true;
         }
         return false;
        }
     [NonSerialized]Coroutine clientSideSendVoxelTerrainChunkEditDataFileCoroutine;
     [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=.05f;
      [NonSerialized]internal float delayToSendNewMessages;//  writer.Length * segmentSizeToTimeInSecondsDelayRatio
     [NonSerialized]readonly List<ulong>clientIdsToSendData=new List<ulong>();
     [NonSerialized]bool sending;
      [NonSerialized]int sendingcnkIdx;
      [NonSerialized]Dictionary<int,FastBufferWriter>sendingDataToServer;
      [NonSerialized]int segmentSize;
       [NonSerialized]int lastSegmentSize;
      [NonSerialized]int segmentCount=-1;
      [NonSerialized]readonly List<int>sentSegments=new List<int>();
        internal IEnumerator ClientSideSendVoxelTerrainChunkEditDataFileCoroutine(){
         yield return null;
         WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
         WaitUntil waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});//  delay with WaitUntil and a cooldown
            //Log.DebugMessage("writingMaxExecutionTime:"+writingMaxExecutionTime);
            bool LimitMessagesSentPerFrame(){
             if(messagesSent>=maxMessagesPerFrame){
              return true;
             }
             messagesSent++;
             return false;
            }
            System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
            bool LimitExecutionTime(){
             sendingExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
             if(sendingExecutionTime>=sendingMaxExecutionTime){
              return true;
             }
             return false;
            }
            Loop:{
             yield return waitUntilGetFileData;
             Log.DebugMessage("ClientSideSendVoxelTerrainChunkEditDataFileCoroutine");
             sending=false;
             stopwatch.Restart();
             FastBufferWriter writer;
             foreach(var segmentBufferPair in sendingDataToServer){
              int segment=segmentBufferPair.Key;
              Log.DebugMessage("'send segment':"+segment);
              writer=segmentBufferPair.Value;
              if(writer.IsInitialized){
               //foreach(ulong clientId in clientIdsToSendData){
                //if(NetworkManager.ConnectedClientsIds.Contains(clientId)){
                 while(LimitExecutionTime()){
                  yield return null;
                  stopwatch.Restart();
                 }
                 while(LimitMessagesSentPerFrame()){
                  yield return null;
                 }
                 totalLengthOfDataSent+=writer.Length;
                 delayToSendNewMessages+=writer.Length*segmentSizeToTimeInSecondsDelayRatio;
                 Log.DebugMessage("sending segment FastBufferWriter writer.Length:"+writer.Length);
                 //NetworkManager.CustomMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId,writer,NetworkDelivery.ReliableFragmentedSequenced);
                 if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
                  Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
                  yield return waitForDelayToSendNewMessages;
                 }
                //}
               //}
               writer.Dispose();
              }
              sentSegments.Add(segment);
             }
             sendingDataToServer.Clear();
             dataToSendDictionaryPool.Enqueue(sendingDataToServer);
             sendingDataToServer=null;
             sentSegments.Clear();
             Log.DebugMessage("'sent all segments':"+segmentCount);
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
             clientIdsToSendData.Clear();
            }
            goto Loop;
        }
    }
}