#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditingMultithreaded;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelTerrainChunkUnnamedMessageHandler:NetworkBehaviour{
     internal static readonly ConcurrentQueue<Dictionary<int,FastBufferWriter>>dataToSendDictionaryPool=new ConcurrentQueue<Dictionary<int,FastBufferWriter>>();
     internal const int VoxelEditDataSize=sizeof(int)*3+sizeof(double)+sizeof(ushort);// VoxelsPerChunk*VoxelEditDataSize is all edit data size if whole chunk is edited
     internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     internal NetworkObject netObj;
      private readonly NetworkVariable<int>netcnkIdx=new NetworkVariable<int>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
       private void OnClientSideNetcnkIdxValueChanged(int previous,int current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          Log.DebugMessage("ask server for chunk data");
         }
        }
       }
     internal LinkedListNode<VoxelTerrainChunkUnnamedMessageHandler>expropriated;
     internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         waitUntilGetFileData=new WaitUntil(()=>{return segmentCount>=0;});
        }
        internal void OnInstantiated(){
         if(Core.singleton.isServer){
          terrainGetFileEditDataToNetSyncBG.segmentSize=NetworkManager.GetComponent<UnityTransport>().MaxPayloadSize;
          //add sizeof(int) for the message type
          //add sizeof(int) for the total segments (segment count)
          //add sizeof(int) for the current segment
          //add sizeof(int) for the segment writes count
          terrainGetFileEditDataToNetSyncBG.voxelsPerSegment=(terrainGetFileEditDataToNetSyncBG.segmentSize-sizeof(int)*4)/VoxelEditDataSize;
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
         NetworkManager.CustomMessagingManager.OnUnnamedMessage+=OnReceivedUnnamedMessage;
         if(Core.singleton.isServer){
          serverSideSendVoxelTerrainChunkEditDataFileCoroutine=StartCoroutine(ServerSideSendVoxelTerrainChunkEditDataFileCoroutine());
         }
        }
        public override void OnNetworkDespawn(){
         if(this!=null&&serverSideSendVoxelTerrainChunkEditDataFileCoroutine!=null){
          StopCoroutine(serverSideSendVoxelTerrainChunkEditDataFileCoroutine);
         }
         NetworkManager.CustomMessagingManager.OnUnnamedMessage-=OnReceivedUnnamedMessage;
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
        private void OnReceivedUnnamedMessage(ulong clientId,FastBufferReader reader){
         var messageType=(int)UnnamedMessageTypes.Undefined;
         reader.ReadValueSafe(out messageType);
         if(messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest){
          Log.DebugMessage("messageType==(int)UnnamedMessageTypes.FromClientVoxelTerrainChunkEditDataRequest");
          if(Core.singleton.isServer){
           OnServerSideReceivedVoxelTerrainChunkEditDataRequest(clientId,reader);
          }
         }else if(messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment){
          Log.DebugMessage("messageType==(int)UnnamedMessageTypes.VoxelTerrainChunkEditDataSegment");
          if(Core.singleton.isClient){
           OnClientSideReceivedVoxelTerrainChunkEditDataSegment(clientId,reader);
          }
         }
        }
        void OnServerSideReceivedVoxelTerrainChunkEditDataRequest(ulong clientId,FastBufferReader reader){
         Log.DebugMessage("OnServerSideReceivedVoxelTerrainChunkEditDataRequest");
        }
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          netcnkIdx.Value=id.Value.cnkIdx;
          pendingGetFileEditData=true;
         }
        }
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
     readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
        bool CanGetFileEditData(){
         if(clientIdsRequestingData.Count>0){
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
          Log.DebugMessage("OnGotFileEditData");
          clientIdsToSendData.AddRange(clientIdsRequestingData);
          clientIdsRequestingData.Clear();
          sendingcnkIdx       =terrainGetFileEditDataToNetSyncBG.cnkIdx;
          sendingDataToClients=terrainGetFileEditDataToNetSyncBG.dataToSendToClients;
          terrainGetFileEditDataToNetSyncBG.dataToSendToClients=null;
          segmentCount=sendingDataToClients.Count;
          Log.DebugMessage("segmentCount:"+segmentCount);
          return true;
         }
         return false;
        }
     Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
      WaitUntil waitUntilGetFileData;
      readonly List<ulong>clientIdsToSendData=new List<ulong>();
      internal static double writingMaxExecutionTime=20.0;
       internal static double writingExecutionTime;
      int segmentCount=-1;
      int sendingcnkIdx;
      Dictionary<int,FastBufferWriter>sendingDataToClients;
      readonly List<int>sentSegments=new List<int>();
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
            //Log.DebugMessage("writingMaxExecutionTime:"+writingMaxExecutionTime);
            System.Diagnostics.Stopwatch stopwatch=new System.Diagnostics.Stopwatch();
            bool LimitExecutionTime(){
             writingExecutionTime+=stopwatch.Elapsed.TotalMilliseconds;
             if(writingExecutionTime>=writingMaxExecutionTime){
              return true;
             }
             return false;
            }
            Loop:{
             yield return waitUntilGetFileData;
             Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             FastBufferWriter writer;
             //FastBufferWriter writer=new FastBufferWriter(sizeof(int)*3,Allocator.Persistent);
             //writer.WriteValueSafe((int)UnnamedMessageTypes.VoxelTerrainChunkcnkIdx);//  message type
             //writer.WriteValueSafe(sendingcnkIdx);//  cnkIdx
             //writer.WriteValueSafe(segmentCount);//  total segments (segment count)
             //foreach(ulong clientId in clientIdsToSendData){
             // NetworkManager.CustomMessagingManager.SendUnnamedMessage(clientId,writer,NetworkDelivery.ReliableFragmentedSequenced);
             //}
             //writer.Dispose();
             //yield return null;
             stopwatch.Restart();
             foreach(var segmentBufferPair in sendingDataToClients){
              while(LimitExecutionTime()){
               yield return null;
               stopwatch.Restart();
              }
              int segment=segmentBufferPair.Key;
              //Log.DebugMessage("send segment:"+segment);
              writer=segmentBufferPair.Value;
              if(writer.IsInitialized){
               foreach(ulong clientId in clientIdsToSendData){
                NetworkManager.CustomMessagingManager.SendUnnamedMessage(clientId,writer,NetworkDelivery.ReliableFragmentedSequenced);
               }
               writer.Dispose();
              }
              sentSegments.Add(segment);
             }
             sendingDataToClients.Clear();
             dataToSendDictionaryPool.Enqueue(sendingDataToClients);
             sendingDataToClients=null;
             sentSegments.Clear();
             Log.DebugMessage("sent all segments:"+segmentCount);
             clientIdsToSendData.Clear();
             segmentCount=-1;//  restart loop but don't repeat for the same edit data file
            }
            goto Loop;
        }
    }
}