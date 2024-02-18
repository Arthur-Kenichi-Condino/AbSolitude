#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AKCondinoO.Sims.SimObjectSpawner;
namespace AKCondinoO.UI{
    internal class MainMenu:MonoBehaviour{
     internal static bool netManagerInitialized;
     NetworkManager netManager;
     [SerializeField]bool editorNetAsClient;
     [SerializeField]bool devBuildNetAsClient;
     readonly Dictionary<Type,PooledPrefabInstanceHandler>prefabInstanceHandlers=new Dictionary<Type,PooledPrefabInstanceHandler>();
     [SerializeField]GameObject[]manuallyAddedPrefabs;
      readonly List<NetworkPrefab>toRemove=new List<NetworkPrefab>();
        void Update(){
         GameObject netManagerGameObject;
         if(netManager==null&&((netManagerGameObject=GameObject.Find("NetworkManager"))==null||
          (netManager=netManagerGameObject.GetComponent<NetworkManager>())==null)
         ){
          Log.Warning("NetworkManager not found");
         }else{
          foreach(var prefabInstanceHandler in prefabInstanceHandlers){
           prefabInstanceHandler.Value.pool.Clear();
          }
          if(!netManagerInitialized){
           Log.DebugMessage("initialize netManager");
           toRemove.AddRange(netManager.NetworkConfig.Prefabs.Prefabs);
           foreach(NetworkPrefab networkPrefab in toRemove){
            netManager.NetworkConfig.Prefabs.Remove(networkPrefab);
           }
           toRemove.Clear();
           foreach(var gO in manuallyAddedPrefabs){
            netManager.AddNetworkPrefab(gO);
           }
           foreach(var o in Resources.LoadAll("AKCondinoO/Prefabs/Network/",typeof(GameObject))){
            GameObject gameObject=(GameObject)o;
            SimObject  simObject=gameObject.GetComponent<SimObject>();
            if(simObject==null)continue;
            if(simObject.GetComponent<NetworkObject>()==null){
             Log.Error("NetworkObject not found for simObject:"+simObject);
             continue;
            }
            Type t=simObject.GetType();
            netManager.AddNetworkPrefab(simObject.gameObject);
            PooledPrefabInstanceHandler prefabInstanceHandler=new PooledPrefabInstanceHandler(simObject.gameObject);
            netManager.PrefabHandler.AddHandler(simObject.gameObject,prefabInstanceHandler);
            prefabInstanceHandlers.Add(t,prefabInstanceHandler);
           }
          }
          netManagerInitialized=true;
          if(!netManager.IsServer&&!netManager.IsHost&&!netManager.IsClient){
           if(Application.isEditor){
            if(editorNetAsClient){
             if(NetworkManager.Singleton.StartClient()){
              Log.DebugMessage("NetworkManager StartClient successful");
              OnStarted();
             }else{
              Log.Error("NetworkManager StartClient failed");
             }
            }else{
             if(NetworkManager.Singleton.StartHost()){
              Log.DebugMessage("NetworkManager StartHost successful");
              OnStarted(true);
             }else{
              Log.Error("NetworkManager StartHost failed");
             }
            }
           }else{
            if(devBuildNetAsClient){
             if(NetworkManager.Singleton.StartClient()){
              Log.DebugMessage("NetworkManager StartClient successful");
              OnStarted();
             }else{
              Log.Error("NetworkManager StartClient failed");
             }
            }else{
             if(NetworkManager.Singleton.StartHost()){
              Log.DebugMessage("NetworkManager StartHost successful");
              OnStarted(true);
             }else{
              Log.Error("NetworkManager StartHost failed");
             }
            }
           }
           void OnStarted(bool server=false){
            foreach(var prefab in netManager.NetworkConfig.Prefabs.Prefabs){
             Log.DebugMessage("prefab:"+prefab.Prefab+" is registered");
            }
           }
          }
         }
         SceneManager.LoadScene("MainScene",LoadSceneMode.Single);
        }
    }
}