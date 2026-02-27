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
    internal partial class NetworkSimObject:NetworkBehaviour{
     SimObject sim;
     [NonSerialized]internal NetworkObject netObj;
        protected virtual void Awake(){
         netObj=GetComponent<NetworkObject>();
        }
        public override void OnDestroy(){
         base.OnDestroy();
        }
        internal virtual void OnActivated(bool IsOwner=false){
        }
        internal virtual void DoNetSpawn(){
         if(Core.singleton.isServer){
          if(!netObj.IsSpawned){
           //Log.DebugMessage("SimObject:OnActivated:'netObj should be spawned now'");
           try{
            netObj.Spawn(destroyWithScene:false);
           }catch(Exception e){
            Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
           }
           netObj.DontDestroyWithOwner=true;
          }else if(IsOwner){
           //Log.DebugMessage("SimObject:OnActivated:'IsOwner, so set net variables'");
           netPosition.Value=sim.persistentData.position  ;
           netRotation.Value=sim.persistentData.rotation  ;
           netScale   .Value=sim.persistentData.localScale;
          }
         }
        }
        internal virtual void DoNetDespawn(){
         if(Core.singleton.isServer){
          if(netObj.IsSpawned){
           netObj.DontDestroyWithOwner=true;
           netObj.Despawn(destroy:false);
          }
         }
        }
        internal virtual void DoNetOwnershipChange(ulong clientId){
         netObj.ChangeOwnership(clientId);
         netObj.DontDestroyWithOwner=true;
        }
        internal virtual void DoNetTransformUpdate(){
         if(Core.singleton.isServer){
          if(IsOwner){
           if(NetworkManager!=null){
            try{
             netPosition.Value=sim.persistentData.position  ;
             netRotation.Value=sim.persistentData.rotation  ;
             netScale   .Value=sim.persistentData.localScale;
            }catch(Exception e){
             Log.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
            }
           }
          }
         }
        }
    }
}