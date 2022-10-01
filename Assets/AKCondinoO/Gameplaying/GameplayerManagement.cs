#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Gameplaying{
    /// <summary>
    ///  [https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/session-management/index.html]
    /// </summary>
    internal class GameplayerManagement:MonoBehaviour,ISingletonInitialization{
     internal static GameplayerManagement singleton{get;set;}
     [SerializeField]Gameplayer _GameplayerPrefab;
     internal ulong?hostId;
     internal readonly Dictionary<ulong,Gameplayer>all=new();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          Core.singleton.netManager.OnClientConnectedCallback +=OnServerSideClientConnected;
          Core.singleton.netManager.OnClientDisconnectCallback+=OnServerSideClientDisconnect;
          hostId=Core.singleton.netManager.LocalClientId;
          Log.DebugMessage("hostId:"+hostId);
          all.Add(hostId.Value,Gameplayer.main=Instantiate(_GameplayerPrefab));
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("GameplayerManagement:OnDestroyingCoreEvent");
         if(Core.singleton.isServer){
          Core.singleton.netManager.OnClientConnectedCallback -=OnServerSideClientConnected;
          Core.singleton.netManager.OnClientDisconnectCallback-=OnServerSideClientDisconnect;
         }
        }
        void OnServerSideClientConnected(ulong clientId){
         Log.DebugMessage("OnServerSideClientConnected:clientId:"+clientId);
         if(clientId==Core.singleton.netManager.LocalClientId){
          Log.DebugMessage("OnServerSideClientConnected:this event was raised by the server connection to itself");
          return;
         }
        }
        void OnServerSideClientDisconnect(ulong clientId){
         Log.DebugMessage("OnServerSideClientDisconnect:clientId:"+clientId);
        }
    }
}