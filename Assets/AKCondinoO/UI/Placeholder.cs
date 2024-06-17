#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
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
         if(currentPlaceholder!=null){//  disable the one before
          activatePlaceholder=false;
         }
         currentPlaceholder=placeholder;
         activatePlaceholder=false;
        }
     internal bool activatePlaceholder{
      get{
       return activatePlaceholder_value;
      }
      set{
       if(value){
        if(currentPlaceholder!=null&&!currentPlaceholder.gameObject.activeSelf){
         currentPlaceholder.gameObject.SetActive(true);
        }
       }else{
        if(currentPlaceholder!=null&&currentPlaceholder.gameObject.activeSelf){
         currentPlaceholder.gameObject.SetActive(false);
        }
       }
       activatePlaceholder_value=value;
      }
     }
     bool activatePlaceholder_value=false;
        void Update(){
         bool placeholderActivated=false;
         if(
          InputHandler.singleton.escape||
          InputHandler.singleton.tab
         ){
          if(currentPlaceholder!=null){
           SetCurrentPlaceholder(null);
          }
         }
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
              activatePlaceholder=true;
              placeholderActivated=true;
              break;
             }
            }
            if((VoxelSystem.voxelTerrainLayer&(1<<hit.collider.gameObject.layer))!=0){
             Log.DebugMessage("currentPlaceholder will touch voxelTerrainLayer");
             activatePlaceholder=true;
             placeholderActivated=true;
             PreviewOnTerrain(hit);
             break;
            }
           }
          }
         }
         if(placeholderActivated){
         }else{
         }
        }
        void PreviewOnTerrain(RaycastHit center){
         currentPlaceholder.transform.position=center.point;
        }
    }
}