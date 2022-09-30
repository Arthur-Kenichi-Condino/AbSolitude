#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AKCondinoO.UI{
    internal class MainMenu:MonoBehaviour{
     internal static bool netManagerInitialized;
     NetworkManager netManager;
     [SerializeField]bool editorNetAsClient;
        void Update(){
         GameObject netManagerGameObject;
         if(netManager==null&&((netManagerGameObject=GameObject.Find("NetworkManager"))==null||
          (netManager=netManagerGameObject.GetComponent<NetworkManager>())==null)
         ){
          Log.Warning("NetworkManager not found");
         }else{
          if(!netManagerInitialized){
           Log.DebugMessage("initialize netManager");
           foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
            GameObject gameObject=(GameObject)o;
            SimObject  simObject=gameObject.GetComponent<SimObject>();
            if(simObject==null)continue;
            if(simObject.GetComponent<NetworkObject>()==null){
             Log.Error("NetworkObject not found for simObject:"+simObject);
             continue;
            }
            netManager.AddNetworkPrefab(simObject.gameObject);
           }
          }
          netManagerInitialized=true;
          if(!netManager.IsServer&&!netManager.IsHost&&!netManager.IsClient){
           if(Application.isEditor){
            if(editorNetAsClient){
             if(NetworkManager.Singleton.StartClient()){
              Log.DebugMessage("NetworkManager StartClient successful");
             }else{
              Log.Error("NetworkManager StartClient failed");
             }
            }else{
             if(NetworkManager.Singleton.StartHost()){
              Log.DebugMessage("NetworkManager StartHost successful");
             }else{
              Log.Error("NetworkManager StartHost failed");
             }
            }
           }else{
            if(NetworkManager.Singleton.StartClient()){
             Log.DebugMessage("NetworkManager StartClient successful");
            }else{
             Log.Error("NetworkManager StartClient failed");
            }
           }
          }
         }
         SceneManager.LoadScene("MainScene",LoadSceneMode.Single);
        }
    }
}