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
          Log.Warning("OnServerSideClientConnected:somehow this event was raised by the server connection to itself");
          return;
         }
         connectedClientsPendingPlayerPrefabInstantiation.Add(clientId);
        }
        void OnServerSideClientDisconnect(ulong clientId){
         Log.DebugMessage("OnServerSideClientDisconnect:clientId:"+clientId);
        }
        readonly HashSet<ulong>connectedClientsPendingPlayerPrefabInstantiation=new HashSet<ulong>();
         readonly HashSet<ulong>connectedClientsPlayerPrefabInstantiated=new HashSet<ulong>();
        void Update(){
         if(Core.singleton.isServer){
          //  to do: remove disconnected from connected pending here (so it cancels the spawning)
          if(connectedClientsPendingPlayerPrefabInstantiation.Count>0){
           foreach(ulong clientId in connectedClientsPendingPlayerPrefabInstantiation){
            Log.DebugMessage("instantiating player prefab for clientId:"+clientId);
            Gameplayer gameplayer=Instantiate(_GameplayerPrefab);
            all.Add(clientId,gameplayer);
            gameplayer.Init(clientId);
            connectedClientsPlayerPrefabInstantiated.Add(clientId);
           }
           foreach(ulong clientId in connectedClientsPlayerPrefabInstantiated){
            connectedClientsPendingPlayerPrefabInstantiation.Remove(clientId);
           }
           connectedClientsPlayerPrefabInstantiated.Clear();
          }
         }
        }
    }
}