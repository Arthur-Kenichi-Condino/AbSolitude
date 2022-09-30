#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Gameplaying{
    internal class GameplayerManagement:MonoBehaviour,ISingletonInitialization{
     internal static GameplayerManagement singleton{get;set;}
     [SerializeField]Gameplayer _GameplayerPrefab;
     internal string hostId;
     internal readonly Dictionary<string,Gameplayer>all=new();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
         if(Core.singleton.isServer){
          if(String.IsNullOrEmpty(hostId)){
           hostId="0";
          }
          all.Add(hostId,Gameplayer.main=Instantiate(_GameplayerPrefab));
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("GameplayerManagement:OnDestroyingCoreEvent");
        }
    }
}