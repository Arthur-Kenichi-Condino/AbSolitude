#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static AKCondinoO.Voxels.Terrain.Networking.VoxelTerrainGetFileEditDataToNetSyncContainer;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Voxels.Terrain.Networking{
    internal partial class VoxelArraySync:NetworkBehaviour{
     [NonSerialized]internal NetworkObject netObj;
      internal readonly NetworkVariable<NetVoxelArrayContainer>voxels=new NetworkVariable<NetVoxelArrayContainer>(
       new NetVoxelArrayContainer(),
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server
      );
     [NonSerialized]internal ServerData asServer;
     [NonSerialized]internal ClientData asClient;
        [Serializable]internal partial class ServerData{
         [NonSerialized]internal VoxelArraySync netVoxelArray;
            internal ServerData(VoxelArraySync netVoxelArray){
             this.netVoxelArray=netVoxelArray;
            }
            internal partial void OnInstantiated();
        }
        [Serializable]internal partial class ClientData{
         [NonSerialized]internal VoxelArraySync netVoxelArray;
            internal ClientData(VoxelArraySync netVoxelArray){
             this.netVoxelArray=netVoxelArray;
            }
        }
        void Awake(){
         netObj=GetComponent<NetworkObject>();
         if(Core.singleton.isServer){
          asServer=new ServerData(this);
          VoxelSystem.singleton.asServer.netVoxelArrays.Add(this);
         }
         if(Core.singleton.isClient){
          asClient=new ClientData(this);
          VoxelSystem.singleton.asClient.netVoxelArrays.Add(this);
         }
         NetworkVariableUpdateTraits updateTraits=new NetworkVariableUpdateTraits();
         int poolSize=
          (VoxelSystem.expropriationDistance.x*2+1)*
          (VoxelSystem.expropriationDistance.y*2+1);
         //updateTraits.MinSecondsBetweenUpdates=voxels.Value.voxelArray.Length*VoxelTerrainChunkArraySync.segmentSizeToTimeInSecondsDelayRatio;
         //updateTraits.MaxSecondsBetweenUpdates=poolSize*updateTraits.MinSecondsBetweenUpdates;
         voxels.SetUpdateTraits(updateTraits);
        }
     internal bool spawnInitialization;
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         spawnInitialization=true;
         Log.DebugMessage("NetworkVariableSerialization<NetVoxelArrayContainer>.AreEqual:"+NetworkVariableSerialization<NetVoxelArrayContainer>.AreEqual);
         if(Core.singleton.isClient){
          asClient.OnClientSideVoxelsValueChanged(null,null);
          voxels.OnValueChanged+=asClient.OnClientSideVoxelsValueChanged;
         }
         spawnInitialization=false;
        }
        public override void OnNetworkDespawn(){
         if(Core.singleton.isClient){
          voxels.OnValueChanged-=asClient.OnClientSideVoxelsValueChanged;
         }
         base.OnNetworkDespawn();
        }
     //[NonSerialized]internal readonly HashSet<ulong>clientIdsRequestingData=new HashSet<ulong>();
     //[NonSerialized]float timeToIgnoreClientIdsRequestingDataToPool=5f;
     //[NonSerialized]float timerToIgnoreClientIdsRequestingDataToPool;
     //   internal void NetServerSideManualUpdate(HashSet<ulong>clientIdsDisconnectedToRemove,out bool toPool){
     //    toPool=false;
     //    if(voxels.IsDirty()){
     //     //Log.DebugMessage("'voxels.IsDirty()':'data wasn't sent yet'",this);
     //    }else{
     //     //Log.DebugMessage("clientIdsRequestingData.Count:"+clientIdsRequestingData.Count);
     //     if(clientIdsDisconnectedToRemove.Count>0){
     //      clientIdsRequestingData.ExceptWith(clientIdsDisconnectedToRemove);
     //     }
     //     if(clientIdsRequestingData.Count<=0){
     //      toPool=true;
     //     }
     //    }
     //    if(!toPool){
     //     if(timerToIgnoreClientIdsRequestingDataToPool<timeToIgnoreClientIdsRequestingDataToPool){
     //      timerToIgnoreClientIdsRequestingDataToPool+=Time.deltaTime;
     //      if(timerToIgnoreClientIdsRequestingDataToPool>=timeToIgnoreClientIdsRequestingDataToPool){
     //       toPool=true;
     //      }
     //     }
     //    }
     //    if(toPool){
     //     timerToIgnoreClientIdsRequestingDataToPool=0f;
     //     clientIdsRequestingData.Clear();
     //     //Log.DebugMessage("'toPool'",this);
     //    }
     //   }
    }
    public class NetVoxelArrayContainer:IEquatable<NetVoxelArrayContainer>,INetworkSerializable{
     public int cnkIdx;
     public int segment;
     public NetVoxel[]voxelArray=new NetVoxel[VoxelsPerChunk/chunkVoxelArraySplits];
        public NetVoxelArrayContainer(){
        }
        public void NetworkSerialize<T>(BufferSerializer<T>serializer)where T:IReaderWriter{
         if(serializer.IsWriter){
          serializer.GetFastBufferWriter().WriteValueSafe(cnkIdx);
          serializer.GetFastBufferWriter().WriteValueSafe(segment);
          for(int i=0;i<voxelArray.Length;++i){
           serializer.GetFastBufferWriter().WriteValueSafe(voxelArray[i].vxlIdx);
           serializer.GetFastBufferWriter().WriteValueSafe(voxelArray[i].density);
           serializer.GetFastBufferWriter().WriteValueSafe((ushort)voxelArray[i].material);
          }
         }else{
          serializer.GetFastBufferReader().ReadValueSafe(out cnkIdx);
          serializer.GetFastBufferReader().ReadValueSafe(out segment);
          for(int i=0;i<voxelArray.Length;++i){
           serializer.GetFastBufferReader().ReadValueSafe(out int vxlIdx);
           serializer.GetFastBufferReader().ReadValueSafe(out double density);
           serializer.GetFastBufferReader().ReadValueSafe(out ushort material);
           voxelArray[i]=new(vxlIdx,density,(MaterialId)material);
          }
         }
        }
        public static bool operator==(NetVoxelArrayContainer a,NetVoxelArrayContainer b){
         if(ReferenceEquals(a,null)){
          if(ReferenceEquals(b,null)){
           return true;
          }
          return false;
         }
         if(ReferenceEquals(b,null)){
          return false;
         }
         if(a.cnkIdx==b.cnkIdx){
          if(a.segment==b.segment){
           if(a.voxelArray.SequenceEqual(b.voxelArray)){
            return true;
           }
          }
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
         return HashCode.Combine(cnkIdx,segment,voxelArray);
        }
        public bool Equals(NetVoxelArrayContainer other){
         return this==other;
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
}