#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels;
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
     internal PlaceholderObject currentPlaceholder;
        internal void SetCurrentPlaceholder(PlaceholderObject placeholder){
         currentPlaceholder=placeholder;
        }
        void Update(){
         if(currentPlaceholder!=null){
          if(ScreenInput.singleton.screenPointRaycastResultsCount>0){
           for(int i=0;i<ScreenInput.singleton.screenPointRaycastResultsCount;++i){
            RaycastHit hit=ScreenInput.singleton.screenPointRaycastResults[i];
            if(hit.collider==null){
             continue;
            }
            if(hit.collider.CompareTag("SimObjectVolume")){
             if((SimConstruction.constructionLayer&(1<<hit.collider.gameObject.layer))!=0){
              Log.DebugMessage("currentPlaceholder will touch constructionLayer");
             }
            }
            if((VoxelSystem.voxelTerrainLayer&(1<<hit.collider.gameObject.layer))!=0){
             Log.DebugMessage("currentPlaceholder will touch voxelTerrainLayer");
            }
           }
          }
         }
        }
    }
}