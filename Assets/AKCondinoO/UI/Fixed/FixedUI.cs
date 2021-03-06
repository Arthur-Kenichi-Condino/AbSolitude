#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.UI.Fixed.BuildBuyEditMode;
using AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed{
    internal class FixedUI:MonoBehaviour{
     internal static FixedUI singleton;
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         (buildBuyEditModeUIContent.buildCategoryTableFloors=Instantiate(buildBuyEditModeUIContent.buildCategorySimObjectsTablePrefab,buildBuyEditModeUIContent.buildCategoryRectTransform,false).AddComponent<TableFloors>()).name="TableFloors";
        }
        internal void Init(){
         Core.singleton.OnDestroyingCoreEvent+=OnDestroyingCoreEvent;
         //  Change UI for game mode:
         GameMode.singleton.OnGameModeChangeEvent+=OnGameModeChangeEvent;
         foreach(var typePrefabPair in SimObjectSpawner.singleton.simObjectPrefabs){
          Type t=typePrefabPair.Key;
          GameObject prefab=typePrefabPair.Value;
          switch(prefab.GetComponent<SimObject>()){
           case SimFloor simFloor:{
            buildBuyEditModeUIContent.buildCategoryTableFloors.tableSimObjects.Add(t.ToString(),simFloor);
            break;
           }
          }
         }
         buildBuyEditModeUIContent.buildCategoryTableFloors.OnCreateTable();
        }
        void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("FixedUI:OnDestroyingCoreEvent");
        }
     [SerializeField]internal RectTransform              buildBuyEditModeUI;
      [SerializeField]internal BuildBuyEditModeUIContent buildBuyEditModeUIContent=new BuildBuyEditModeUIContent();
        public void OnBuildBuyEditModeButtonPress(){
         Log.DebugMessage("FixedUI:OnBuildBuyEditModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.BuildBuyEdit);
        }
     [SerializeField]internal RectTransform interactModeUI;
        public void OnInteractModeButtonPress(){
         Log.DebugMessage("FixedUI:OnInteractModeButtonPress");
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.Interact);
        }
        void OnGameModeChangeEvent(object sender,EventArgs ev){
         OnGameModeChangeEventArgs args=(OnGameModeChangeEventArgs)ev;
         buildBuyEditModeUI.gameObject.SetActive(false);
             interactModeUI.gameObject.SetActive(false);
         switch(GameMode.singleton.current){
          case GameModesEnum.BuildBuyEdit:{
           buildBuyEditModeUI.gameObject.SetActive(true);
           break;
          }
          case GameModesEnum.Interact:{
               interactModeUI.gameObject.SetActive(true);
           break;
          }
         }
        }
     [SerializeField]SimObject DEBUG_SET_PLACEHOLDER=null;
        void Update(){
         if(DEBUG_SET_PLACEHOLDER!=null){
          Log.DebugMessage("DEBUG_SET_PLACEHOLDER:"+DEBUG_SET_PLACEHOLDER);
          Placeholder.singleton.GetPlaceholderFor(DEBUG_SET_PLACEHOLDER.GetType());
            DEBUG_SET_PLACEHOLDER=null;
         }
        }
    }
}