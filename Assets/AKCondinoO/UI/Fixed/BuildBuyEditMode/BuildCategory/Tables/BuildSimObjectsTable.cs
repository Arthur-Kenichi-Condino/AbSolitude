#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables{
    internal class BuildSimObjectsTable:MonoBehaviour{
     [NonSerialized]internal readonly SortedDictionary<string,SimObject>tableSimObjectPrefabs=new SortedDictionary<string,SimObject>();
      [NonSerialized]internal readonly Dictionary<SimObject,PlaceholderObject>tableSimObjectPlaceholders=new();
     [NonSerialized]internal RectTransform tableScrollViewContent;
        void Awake(){
         TraverseHierarchy(transform);
        }
        void TraverseHierarchy(Transform parent){
         foreach(Transform child in parent){
          //Log.DebugMessage("BuildSimObjectsTable:TraverseHierarchy:child:"+child.name);
          if(child.name=="TableScrollView"){
           TraverseHierarchy(child);
          }else if(child.name=="Viewport"){
           TraverseHierarchy(child);
          }else if(child.name=="Content"){
           tableScrollViewContent=(RectTransform)child;
           //Log.DebugMessage("'tableScrollViewContent set'");
          }
         }
        }
        internal void OnCreateTable(){
         RectTransform simObjectButtonPrefabRectTransform=(RectTransform)FixedUI.singleton.buildBuyEditModeUIContent.tableButtonPrefab.transform;
         TableFloors tableFloors=this as TableFloors;
         int x=0;
         int y=0;
         foreach(var typeNameSimObjectPair in tableSimObjectPrefabs){
          string typeName=typeNameSimObjectPair.Key;
          SimObject simObject=typeNameSimObjectPair.Value;
          //Log.DebugMessage("OnCreateTable:"+typeName+"..."+simObject);
          Vector3 pos=new Vector3(
           simObjectButtonPrefabRectTransform.position.x+x*(simObjectButtonPrefabRectTransform.sizeDelta.x+10),
           simObjectButtonPrefabRectTransform.position.y-y*(simObjectButtonPrefabRectTransform.sizeDelta.y+10),
           0
          );
          GameObject simObjectButton=Instantiate(FixedUI.singleton.buildBuyEditModeUIContent.tableButtonPrefab,tableScrollViewContent.transform);
          //Log.DebugMessage("simObjectButton pos:"+pos);
          simObjectButton.transform.localPosition=pos;
          Button simObjectButtonComponent=simObjectButton.GetComponent<Button>();
          //Log.DebugMessage("simObjectButtonComponent:"+simObjectButtonComponent,simObjectButtonComponent);
          if(tableFloors!=null){
           simObjectButtonComponent.onClick.RemoveAllListeners();
           UnityAction call=delegate{tableFloors.OnPlaceSimFloorButtonPress(typeName);};
           simObjectButtonComponent.onClick.AddListener(call);
           //Log.DebugMessage("added listener:"+call,this);
          }
          x++;
          if(x>1){
           x=0;
           y++;
          }
         }
        }
    }
}