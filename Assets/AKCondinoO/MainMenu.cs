#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
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
     [SerializeField]NetworkPrefabsList defaultPrefabList;
     internal static bool netManagerInitialized;
     NetworkManager netManager;
     [SerializeField]bool editorNetAsClient;
     [SerializeField]bool devBuildNetAsClient;
     readonly Dictionary<Type,PooledPrefabInstanceHandler>prefabInstanceHandlers=new Dictionary<Type,PooledPrefabInstanceHandler>();
     [SerializeField]GameObject[]manuallyAddedPrefabs;
      readonly List<NetworkPrefab>toRemove=new List<NetworkPrefab>();
      readonly List<NetworkPrefab>duplicateDefaultToRemove=new List<NetworkPrefab>();
        void Update(){
         GameObject netManagerGameObject;
         if(netManager==null&&((netManagerGameObject=GameObject.Find("NetworkManager"))==null||
          (netManager=netManagerGameObject.GetComponent<NetworkManager>())==null)
         ){
          Log.Warning("NetworkManager not found");
          netManagerInitialized=false;
         }else{
          if(!netManager.IsServer&&!netManager.IsHost&&!netManager.IsClient){
           netManagerInitialized=false;
          }
          foreach(var prefabInstanceHandler in prefabInstanceHandlers){
           prefabInstanceHandler.Value.pool.Clear();
          }
          if(!netManagerInitialized){
           //Log.DebugMessage("initialize netManager");
           toRemove.AddRange(netManager.NetworkConfig.Prefabs.Prefabs);
           foreach(NetworkPrefab networkPrefab in toRemove){
            //Log.DebugMessage("'netManager.NetworkConfig.Prefabs.Remove':"+networkPrefab.Prefab+":"+networkPrefab);
            netManager.NetworkConfig.Prefabs.Remove(networkPrefab);
           }
           toRemove.Clear();
           foreach(var gO in manuallyAddedPrefabs){
            if(!netManager.NetworkConfig.Prefabs.Contains(gO)){
             netManager.AddNetworkPrefab(gO);
             Log.DebugMessage("'manuallyAddedPrefabs':'netManager.AddNetworkPrefab':"+gO);
            }else{
             Log.DebugMessage("'manuallyAddedPrefabs':'netManager.NetworkConfig.Prefabs.Contains(gO)':"+gO);
            }
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
            if(!netManager.NetworkConfig.Prefabs.Contains(simObject.gameObject)){
             netManager.AddNetworkPrefab(simObject.gameObject);
             Log.DebugMessage("'netManager.AddNetworkPrefab':"+simObject.gameObject);
            }else{
             Log.DebugMessage("'netManager.NetworkConfig.Prefabs.Contains(simObject.gameObject)':"+simObject.gameObject);
            }
            PooledPrefabInstanceHandler prefabInstanceHandler=new PooledPrefabInstanceHandler(simObject.gameObject);
            netManager.PrefabHandler.AddHandler(simObject.gameObject,prefabInstanceHandler);
            Log.DebugMessage("'prefabInstanceHandlers.Add':"+t);
            prefabInstanceHandlers.Add(t,prefabInstanceHandler);
           }
           duplicateDefaultToRemove.AddRange(defaultPrefabList.PrefabList);
           foreach(NetworkPrefab defaultPrefab in duplicateDefaultToRemove){
            if(netManager.NetworkConfig.Prefabs.Contains(defaultPrefab)){
             Log.DebugMessage("'defaultPrefabList.Remove(defaultPrefab)':"+defaultPrefab.Prefab+":"+defaultPrefab);
             defaultPrefabList.Remove(defaultPrefab);
            }
           }
           duplicateDefaultToRemove.Clear();
          }
          netManagerInitialized=true;
          if(!netManager.IsServer&&!netManager.IsHost&&!netManager.IsClient){
           foreach(NetworkPrefab netPrefab in netManager.NetworkConfig.Prefabs.Prefabs){
            Log.DebugMessage("netPrefab:"+netPrefab.Prefab);
           }
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