#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI{
    internal class Placeholder:MonoBehaviour,ISingletonInitialization{
     internal static Placeholder singleton{get;set;}
     [SerializeField]internal PlaceholderObject placeholderObjectPrefab;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("Placeholder:OnDestroyingCoreEvent");
        }
        internal PlaceholderObject GetPlaceholderFor(Type t){
         SimObject simObjectPrefab=SimObjectSpawner.singleton.simObjectPrefabs[t].GetComponent<SimObject>();
         PlaceholderObject placeholderObject=Instantiate(placeholderObjectPrefab);
         placeholderObject.BuildFrom(simObjectPrefab);
         return placeholderObject;
        }
    }
}