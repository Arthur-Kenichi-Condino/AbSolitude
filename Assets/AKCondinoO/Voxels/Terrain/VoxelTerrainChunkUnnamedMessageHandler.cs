#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     internal static readonly ConcurrentQueue<Dictionary<int,FastBufferWriter>>dataToSendDictionaryPool=new ConcurrentQueue<Dictionary<int,FastBufferWriter>>();
     //  sizeof(int): 32 bits
     //  sizeof(double): 64 bits
     //  sizeof(ushort): 16 bits
     //add sizeof(int) for the message type
     //add sizeof(int) for the cnkIdx
     //add sizeof(int) for the current segment
     //add sizeof(int) for the total segments (segment count)
     //add sizeof(int) for the segment writes count
     //add sizeof(int) for voxel index
     //add sizeof(double) for voxel density
     //add sizeof(ushort) for voxel material id
     //  HeaderSize plus VoxelEditDataSize: 5*32+32+64+16=272 bits
     //  VoxelsPerChunk: 16*16*256=65536 voxels
     //  VoxelsPerChunk*VoxelEditDataSize is all edit data size if whole chunk is edited:
     //  65536*272=17825792 bits: 2228224 bytes: 2.228224 Megabytes
     internal const int HeaderSize=sizeof(int)*5;
     internal const int VoxelEditDataSize=sizeof(int)+sizeof(double)+sizeof(ushort);
     //  
     internal const int Splits=8;
     internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     internal NetworkObject netObj;
      private readonly NetworkVariable<int>netcnkIdx=new NetworkVariable<int>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
       private void OnClientSideNetcnkIdxValueChanged(int previous,int current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          if(clientSidecnkIdx==null||current!=clientSidecnkIdx.Value){
           clientSidecnkIdx=current;
           //Log.DebugMessage("ask server for chunk data");
           //add sizeof(int) for the message type
           //add sizeof(int) for the cnkIdx
           FastBufferWriter writer=new FastBufferWriter(sizeof(int)*2,Allocator.Persistent);
           if(writer.TryBeginWrite(sizeof(int)*2)){
            writer.WriteValue((int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest);
            writer.WriteValue((int)current);
           }
           if(VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend.TryGetValue(current,out FastBufferWriter oldRequest)){oldRequest.Dispose();}
           VoxelSystem.singleton.clientVoxelTerrainChunkEditDataRequestsToSend[current]=writer;
          }
         }
        }
       }
     internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         waitUntilGetFileData=new WaitUntil(()=>{return segmentCount>=0;});
         waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});
        }
        internal void OnInstantiated(){
         if(Core.singleton.isServer){
          segmentSize=terrainGetFileEditDataToNetSyncBG.segmentSize=(VoxelsPerChunk*VoxelEditDataSize+HeaderSize)/Splits;
          terrainGetFileEditDataToNetSyncBG.voxelsPerSegment=(terrainGetFileEditDataToNetSyncBG.segmentSize-HeaderSize)/VoxelEditDataSize;
         }
        }
        internal void OnDestroyingCore(){
         terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
         if(terrainGetFileEditDataToNetSyncBG.dataToSendToClients!=null){
          foreach(var segmentBufferPair in terrainGetFileEditDataToNetSyncBG.dataToSendToClients){
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients.Clear();
          dataToSendDictionaryPool.Enqueue(terrainGetFileEditDataToNetSyncBG.dataToSendToClients);
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients=null;
         }
        }
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         if(Core.singleton.isClient){
          OnClientSideNetcnkIdxValueChanged(netcnkIdx.Value,netcnkIdx.Value);//  update on spawn
          netcnkIdx.OnValueChanged+=OnClientSideNetcnkIdxValueChanged;
         }
         if(Core.singleton.isServer){
          serverSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
        }
        public override void OnNetworkDespawn(){
         if(this!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
          StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
         }
         if(Core.singleton.isClient){
          netcnkIdx.OnValueChanged-=OnClientSideNetcnkIdxValueChanged;
         }
         base.OnNetworkDespawn();
         if(sendingDataToClients!=null){
          foreach(var segmentBufferPair in sendingDataToClients){
           int segment=segmentBufferPair.Key;
           if(sentSegments.Contains(segment)){
            continue;
           }
           FastBufferWriter writer=segmentBufferPair.Value;
           if(writer.IsInitialized){
            writer.Dispose();
           }
          }
          sendingDataToClients.Clear();
          dataToSendDictionaryPool.Enqueue(sendingDataToClients);
          sendingDataToClients=null;
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
         //Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:cnkIdx:"+id.Value.cnkIdx);
         clientIdsRequestingData.Add(clientId);
         pendingGetFileEditData=true;
        }
     [SerializeField]bool DEBUG_SEND_UNCHANGED_VOXEL_DATA=false;
     bool waitingGetFileEditData;
     bool pendingGetFileEditData;
        internal void ManualUpdate(){
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
     //  TO DO: send interval
     readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
        bool CanGetFileEditData(){
         if(clientIdsRequestingData.Count>0){
          terrainGetFileEditDataToNetSyncBG.DEBUG_SEND_UNCHANGED_VOXEL_DATA=DEBUG_SEND_UNCHANGED_VOXEL_DATA;
          terrainGetFileEditDataToNetSyncBG.cCoord=id.Value.cCoord;
          terrainGetFileEditDataToNetSyncBG.cnkRgn=id.Value.cnkRgn;
          terrainGetFileEditDataToNetSyncBG.cnkIdx=id.Value.cnkIdx;
          VoxelTerrainGetFileEditDataToNetSyncMultithreaded.Schedule(terrainGetFileEditDataToNetSyncBG);
          return true;
         }
         return false;
        }
        bool OnGotFileEditData(){
         if(segmentCount<0&&terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          //Log.DebugMessage("OnGotFileEditData");
          clientIdsToSendData.AddRange(clientIdsRequestingData);
          clientIdsRequestingData.Clear();
          sendingcnkIdx       =terrainGetFileEditDataToNetSyncBG.cnkIdx;
          sendingDataToClients=terrainGetFileEditDataToNetSyncBG.dataToSendToClients;
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients=null;
          segmentCount=sendingDataToClients.Count;
          //Log.DebugMessage("segmentCount:"+segmentCount);
          return true;
         }
         return false;
        }
     Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
      WaitUntil waitUntilGetFileData;
      readonly List<ulong>clientIdsToSendData=new List<ulong>();
      internal static float segmentSizeToTimeInSecondsDelayRatio=.1f/VoxelsPerChunk;//  turns segment Length into seconds to wait
       WaitUntil waitForDelayToSendNewMessages;//  delay with WaitUntil and a cooldown
        internal float minTimeInSecondsToStartDelayToSendNewMessages=.05f;
         internal float delayToSendNewMessages;//  writer.Length*segmentSizeToTimeInSecondsDelayRatio
      internal static int maxMessagesPerFrame=2;//  a value around Splits so it stops sending messages and starts a global cooldown early
       internal static int messagesSent;
        internal static int totalLengthOfDataSent;
         internal static float globalCooldownToSendNewMessages;//  totalLengthOfDataSent*segmentSizeToTimeInSecondsDelayRatio
      internal static double sendingMaxExecutionTime=1.0;
       internal static double sendingExecutionTime;
      int segmentSize;
      int segmentCount=-1;
      int sendingcnkIdx;
      Dictionary<int,FastBufferWriter>sendingDataToClients;
      readonly List<int>sentSegments=new List<int>();
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
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
             //Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             FastBufferWriter writer;
             stopwatch.Restart();
             foreach(var segmentBufferPair in sendingDataToClients){
              int segment=segmentBufferPair.Key;
              //Log.DebugMessage("send segment:"+segment);
              writer=segmentBufferPair.Value;
              if(writer.IsInitialized){
               foreach(ulong clientId in clientIdsToSendData){
                if(NetworkManager.ConnectedClientsIds.Contains(clientId)){
                 while(LimitExecutionTime()){
                  yield return null;
                  stopwatch.Restart();
                 }
                 while(LimitMessagesSentPerFrame()){
                  yield return null;
                 }
                 totalLengthOfDataSent+=writer.Length;
                 delayToSendNewMessages+=writer.Length*segmentSizeToTimeInSecondsDelayRatio;
                 //Log.DebugMessage("sending segment FastBufferWriter writer.Length:"+writer.Length);
                 NetworkManager.CustomMessagingManager.SendUnnamedMessage(clientId,writer,NetworkDelivery.ReliableFragmentedSequenced);
                 if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
                  //Log.DebugMessage("waitForDelayToSendNewMessages:"+delayToSendNewMessages+" seconds");
                  yield return waitForDelayToSendNewMessages;
                 }
                }
               }
               writer.Dispose();
              }
              sentSegments.Add(segment);
             }
             sendingDataToClients.Clear();
             dataToSendDictionaryPool.Enqueue(sendingDataToClients);
             sendingDataToClients=null;
             sentSegments.Clear();
             if(delayToSendNewMessages>0f){
              yield return waitForDelayToSendNewMessages;
             }
             Log.DebugMessage("sent all segments:"+segmentCount);
             clientIdsToSendData.Clear();
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
            }
            goto Loop;
        }
    }
}