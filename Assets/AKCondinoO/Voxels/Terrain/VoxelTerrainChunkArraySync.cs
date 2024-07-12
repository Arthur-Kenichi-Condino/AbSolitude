#if UNITY_EDITOR||DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
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
    internal partial class VoxelTerrainChunkArraySync:NetworkBehaviour{
     [NonSerialized]internal VoxelTerrainGetFileEditDataToNetSyncContainer terrainGetFileEditDataToNetSyncBG=new VoxelTerrainGetFileEditDataToNetSyncContainer();
     [NonSerialized]internal LinkedListNode<VoxelTerrainChunkArraySync>expropriated;
     [NonSerialized]internal(Vector2Int cCoord,Vector2Int cnkRgn,int cnkIdx)?id=null;
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         NetworkVariableUpdateTraits updateTraits=new NetworkVariableUpdateTraits();
         int poolSize=
          (VoxelSystem.expropriationDistance.x*2+1)*
          (VoxelSystem.expropriationDistance.y*2+1);
         updateTraits.MaxSecondsBetweenUpdates=poolSize*.1f;
         updateTraits.MinSecondsBetweenUpdates=(VoxelsPerChunk/poolSize)*.005f;
         voxels.SetUpdateTraits(updateTraits);
        }
        internal void OnInstantiated(){
         if(Core.singleton.isServer){
         }
        }
        internal void OnDestroyingCore(){
         terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning,-1);
        }
     [NonSerialized]internal NetworkObject netObj;
      private readonly NetworkVariable<int>netcnkIdx=new NetworkVariable<int>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
      private readonly AnticipatedNetworkVariable<NetVoxelArrayContainer>voxels=new AnticipatedNetworkVariable<NetVoxelArrayContainer>(
       new NetVoxelArrayContainer(),
       StaleDataHandling.Ignore
      );
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         Log.DebugMessage("NetworkVariableSerialization<NetVoxelArrayContainer>.AreEqual:"+NetworkVariableSerialization<NetVoxelArrayContainer>.AreEqual);
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
        }
        internal void OncCoordChanged(Vector2Int cCoord1,int cnkIdx1,bool firstCall){
         if(firstCall||cCoord1!=id.Value.cCoord){
          id=(cCoord1,cCoordTocnkRgn(cCoord1),cnkIdx1);
          netcnkIdx.Value=id.Value.cnkIdx;
          pendingGetFileEditData=true;
         }
        }
        internal void OnReceivedVoxelTerrainChunkEditDataRequest(ulong clientId){
         Log.DebugMessage("OnReceivedVoxelTerrainChunkEditDataRequest:'cnkIdx':"+id.Value.cnkIdx);
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
     [SerializeField]bool DEBUG_FORCE_SEND_ALL_VOXEL_DATA=false;
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
        bool CanGetFileEditData(){
         if(terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          terrainGetFileEditDataToNetSyncBG.DEBUG_FORCE_SEND_ALL_VOXEL_DATA=DEBUG_FORCE_SEND_ALL_VOXEL_DATA;
          terrainGetFileEditDataToNetSyncBG.cCoord=id.Value.cCoord;
          terrainGetFileEditDataToNetSyncBG.cnkRgn=id.Value.cnkRgn;
          terrainGetFileEditDataToNetSyncBG.cnkIdx=id.Value.cnkIdx;
          VoxelTerrainGetFileEditDataToNetSyncMultithreaded.Schedule(terrainGetFileEditDataToNetSyncBG);
          return true;
         }
         return false;
        }
        bool OnGotFileEditData(){
         if(!sending&&terrainGetFileEditDataToNetSyncBG.IsCompleted(VoxelSystem.singleton.terrainGetFileEditDataToNetSyncBGThreads[0].IsRunning)){
          //Log.DebugMessage("OnGotFileEditData");
          for(int i=0;i<terrainGetFileEditDataToNetSyncBG.changes.Length;++i){
           bool change=terrainGetFileEditDataToNetSyncBG.changes[i];
           sending|=change;
           terrainGetFileEditDataToNetSyncBG.changes[i]=false;
          }
          sendingcnkIdx=terrainGetFileEditDataToNetSyncBG.cnkIdx;
          return true;
         }
         return false;
        }
     [NonSerialized]Coroutine serverSideSendVoxelTerrainChunkEditDataFileCoroutine;
      [NonSerialized]internal float minTimeInSecondsToStartDelayToSendNewMessages=.05f;
       [NonSerialized]internal float delayToSendNewMessages;//  writer.Length * segmentSizeToTimeInSecondsDelayRatio
      [NonSerialized]bool sending;
      [NonSerialized]int sendingcnkIdx;
        internal IEnumerator ServerSideSendVoxelTerrainChunkEditDataFileCoroutine(){
         yield return null;
         WaitUntil waitUntilGetFileData=new WaitUntil(()=>{return sending;});
         WaitUntil waitForDelayToSendNewMessages=new WaitUntil(()=>{if(delayToSendNewMessages>0f){delayToSendNewMessages-=Time.deltaTime;}return delayToSendNewMessages<=0f;});//  delay with WaitUntil and a cooldown
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
             sending=false;
             stopwatch.Restart();
             Log.DebugMessage("ServerSideSendVoxelTerrainChunkEditDataFileCoroutine");
             int destinationIndex=0;
             for(int i=0;i<terrainGetFileEditDataToNetSyncBG.voxels.Length;++i){
              while(LimitExecutionTime()){
               yield return null;
               stopwatch.Restart();
              }
              while(LimitMessagesSentPerFrame()){
               yield return null;
              }
              //Array.Copy(terrainGetFileEditDataToNetSyncBG.voxels[i],0,voxels.Value.voxelArray,destinationIndex,terrainGetFileEditDataToNetSyncBG.voxels[i].Length);
              destinationIndex+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length;
              totalLengthOfDataSent+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length;
              delayToSendNewMessages+=terrainGetFileEditDataToNetSyncBG.voxels[i].Length*segmentSizeToTimeInSecondsDelayRatio;
              if(delayToSendNewMessages>minTimeInSecondsToStartDelayToSendNewMessages){
               Log.DebugMessage("'waitForDelayToSendNewMessages':"+delayToSendNewMessages+" seconds");
               yield return waitForDelayToSendNewMessages;
              }
             }
            }
            goto Loop;
        }
    }
}
public struct NetVoxel:IEquatable<NetVoxel>{
 public int vxlIdx;
 public double density;
 public MaterialId material;
    public NetVoxel(int vxlIdx,double d,MaterialId m){
     this.vxlIdx=vxlIdx;density=d;material=m;
    }
    public static bool operator==(NetVoxel a,NetVoxel b){
     if(
      a.vxlIdx==b.vxlIdx&&
      a.density==b.density&&
      a.material==b.material
     ){
      return true;
     }
     return false;
    }
    public static bool operator!=(NetVoxel a,NetVoxel b){
     return!(a==b);
    }
    public override bool Equals(object obj){
     if(!(obj is NetVoxel netVoxel)){
      return false;
     }
     return this==netVoxel;
    }
    public override int GetHashCode(){
     return HashCode.Combine(vxlIdx,density,material);
    }
    public bool Equals(NetVoxel other){
     return this==other;
    }
}
public class NetVoxelArrayContainer:IEquatable<NetVoxelArrayContainer>,INetworkSerializable{
 public NetVoxel[]voxelArray=new NetVoxel[0];
    public NetVoxelArrayContainer(){
    }
    public void NetworkSerialize<T>(BufferSerializer<T>serializer)where T:IReaderWriter{
     if(serializer.IsWriter){
      for(int i=0;i<voxelArray.Length;++i){
       serializer.GetFastBufferWriter().WriteValueSafe(voxelArray[i].vxlIdx);
       serializer.GetFastBufferWriter().WriteValueSafe(voxelArray[i].density);
       serializer.GetFastBufferWriter().WriteValueSafe((ushort)voxelArray[i].material);
      }
     }else{
      for(int i=0;i<voxelArray.Length;++i){
       serializer.GetFastBufferReader().ReadValueSafe(out int vxlIdx);
       serializer.GetFastBufferReader().ReadValueSafe(out double density);
       serializer.GetFastBufferReader().ReadValueSafe(out ushort material);
       voxelArray[i]=new(vxlIdx,density,(MaterialId)material);
      }
     }
    }
    public static bool operator==(NetVoxelArrayContainer a,NetVoxelArrayContainer b){
     if(ReferenceEquals(a,b)){
      return true;
     }
     if(a.voxelArray.SequenceEqual(b.voxelArray)){
      return true;
     }
     return false;
    }
    public static bool operator!=(NetVoxelArrayContainer a,NetVoxelArrayContainer b){
     return!(a==b);
    }
    public override bool Equals(object obj){
     if(!(obj is NetVoxelArrayContainer container)){
      return false;
     }
     return this==container;
    }
    public override int GetHashCode(){
     return HashCode.Combine(voxelArray);
    }
    public bool Equals(NetVoxelArrayContainer other){
     return this==other;
    }
}