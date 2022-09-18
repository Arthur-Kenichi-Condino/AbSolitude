#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AKCondinoO.UI{
    internal class MainMenu:MonoBehaviour{
     internal static bool netManagerInitialized;
        void Awake(){
         if(!netManagerInitialized){
          Log.DebugMessage("initialize netManager");
          foreach(var o in Resources.LoadAll("AKCondinoO/Sims/",typeof(GameObject))){
           GameObject gameObject=(GameObject)o;
           SimObject  simObject=gameObject.GetComponent<SimObject>();
           if(simObject==null)continue;
          }
         }
         netManagerInitialized=true;
        }
        void Update(){
         SceneManager.LoadScene("MainScene",LoadSceneMode.Single);
        }
    }
}