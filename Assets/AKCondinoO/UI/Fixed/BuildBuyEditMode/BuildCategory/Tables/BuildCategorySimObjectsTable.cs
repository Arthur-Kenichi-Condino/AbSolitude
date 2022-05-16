#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables{
    internal class BuildCategorySimObjectsTable:MonoBehaviour{
     internal readonly SortedDictionary<string,SimObject>tableSimObjects=new SortedDictionary<string,SimObject>();
     internal RectTransform tableScrollViewContent;
        void Awake(){
         TraverseHierarchy(transform);
        }
        void TraverseHierarchy(Transform parent){
         foreach(Transform child in parent){
          Log.DebugMessage("BuildCategorySimObjectsTable child:"+child.name);
          if(child.name=="TableScrollView"){
           TraverseHierarchy(child);
          }else if(child.name=="Viewport"){
           TraverseHierarchy(child);
          }
         }
        }
        internal void OnCreateTable(){
         int x=0;
         int y=0;
         foreach(var typeNameSimObjectPair in tableSimObjects){
          string typeName=typeNameSimObjectPair.Key;
          SimObject simObject=typeNameSimObjectPair.Value;
          Log.DebugMessage("OnCreateTable:"+typeName+"..."+simObject);
         }
        }
    }
}