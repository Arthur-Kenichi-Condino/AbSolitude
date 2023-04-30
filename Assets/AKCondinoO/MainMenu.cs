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
     [SerializeField]NetworkPrefabsList manualNetworkPrefabs;
     [SerializeField]NetworkPrefabsList autoNetworkPrefabs;
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
           toRemove.Clear();
           toRemove.AddRange(manualNetworkPrefabs.PrefabList);
           toRemove.AddRange(  autoNetworkPrefabs.PrefabList);
           foreach(NetworkPrefab networkPrefab in toRemove){
            if(autoNetworkPrefabs.Contains(networkPrefab)){
             autoNetworkPrefabs.Remove(networkPrefab);
            }
            netManager.NetworkConfig.Prefabs.Remove(networkPrefab);
           }
           toRemove.Clear();
           foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
            GameObject gameObject=(GameObject)o;
            SimObject  simObject=gameObject.GetComponent<SimObject>();
            if(simObject==null)continue;
            if(simObject.GetComponent<NetworkObject>()==null){
             Log.Error("NetworkObject not found for simObject:"+simObject);
             continue;
            }
            Type t=simObject.GetType();
            //netManager.AddNetworkPrefab(simObject.gameObject);
            NetworkPrefab networkPrefab=new NetworkPrefab{Prefab=simObject.gameObject};
            autoNetworkPrefabs.Add(networkPrefab);
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
            foreach(var prefabList in netManager.NetworkConfig.Prefabs.NetworkPrefabsLists){
             Log.DebugMessage("prefabList:"+prefabList);
             foreach(var prefab in prefabList.PrefabList){
              Log.DebugMessage("prefab:"+prefab.Prefab+" is registered:"+netManager.NetworkConfig.Prefabs.Contains(prefab));
             }
            }
           }
          }
         }
         SceneManager.LoadScene("MainScene",LoadSceneMode.Single);
        }
    }
}