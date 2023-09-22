#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal NetworkObject netObj;
        public override void OnNetworkSpawn(){
         base.OnNetworkSpawn();
         if(Core.singleton.isServer){
          //Log.DebugMessage("SimObject:OnNetworkSpawn:isServer");
          if(IsOwner){
           //Log.DebugMessage("init net variables");
           netPosition.Value=persistentData.position  ;
           netRotation.Value=persistentData.rotation  ;
           netScale   .Value=persistentData.localScale;
          }
          OnServerSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnServerSideNetPositionValueChanged;
          OnServerSideNetRotationValueChanged(transform.rotation  ,netRotation.Value);//  update on spawn
          netRotation.OnValueChanged+=OnServerSideNetRotationValueChanged;
          OnServerSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          Log.DebugMessage("SimObject:OnNetworkSpawn:isClient");
          OnClientSideNetPositionValueChanged(transform.position  ,netPosition.Value);//  update on spawn
          netPosition.OnValueChanged+=OnClientSideNetPositionValueChanged;
          OnClientSideNetRotationValueChanged(transform.rotation  ,netRotation.Value);//  update on spawn
          netRotation.OnValueChanged+=OnClientSideNetRotationValueChanged;
          OnClientSideNetScaleValueChanged   (transform.localScale,netScale   .Value);//  update on spawn
          netScale   .OnValueChanged+=OnClientSideNetScaleValueChanged   ;
          if(!IsOwner){
           SimObjectManager.singleton.netActive.Add(this);
           EnableRenderers();
          }
         }
        }
        public override void OnNetworkDespawn(){
         if(Core.singleton.isServer){
          //Log.DebugMessage("SimObject:OnNetworkDespawn:isServer");
          netPosition.OnValueChanged-=OnServerSideNetPositionValueChanged;
          netRotation.OnValueChanged-=OnServerSideNetRotationValueChanged;
          netScale   .OnValueChanged-=OnServerSideNetScaleValueChanged   ;
         }
         if(Core.singleton.isClient){
          //Log.DebugMessage("SimObject:OnNetworkDespawn:isClient");
          netPosition.OnValueChanged-=OnClientSideNetPositionValueChanged;
          netRotation.OnValueChanged-=OnClientSideNetRotationValueChanged;
          netScale   .OnValueChanged-=OnClientSideNetScaleValueChanged   ;
          if(!IsOwner){
           DisableRenderers();
           SimObjectManager.singleton.netActive.Remove(this);
          }
         }
         base.OnNetworkDespawn();
        }
      private readonly NetworkVariable<Vector3>netPosition=new NetworkVariable<Vector3>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetPositionValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.position=current;
         }
        }
       }
       private void OnServerSideNetPositionValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.position=current;
         }
        }
       }
      private readonly NetworkVariable<Quaternion>netRotation=new NetworkVariable<Quaternion>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetRotationValueChanged(Quaternion previous,Quaternion current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.rotation=current;
         }
        }
       }
       private void OnServerSideNetRotationValueChanged(Quaternion previous,Quaternion current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.rotation=current;
         }
        }
       }
      private readonly NetworkVariable<Vector3>netScale=new NetworkVariable<Vector3>(default,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner
      );
       private void OnClientSideNetScaleValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isClient){
         if(!IsOwner){
          transform.localScale=current;
         }
        }
       }
       private void OnServerSideNetScaleValueChanged(Vector3 previous,Vector3 current){
        if(Core.singleton.isServer){
         if(!IsOwner){
          transform.localScale=current;
         }
        }
       }
    }
}