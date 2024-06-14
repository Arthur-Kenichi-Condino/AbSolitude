#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables{
    internal class TableFloors:BuildCategorySimObjectsTable{
        public void OnPlaceSimFloorButtonPress(string type){
         Log.DebugMessage("TableFloors:OnPlaceSimFloorButtonPress:type:"+type);
         if(tableSimObjectPrefabs.TryGetValue(type,out SimObject prefab)){
          Type prefabType=prefab.GetType();
          Log.DebugMessage("sim floor prefabType:"+prefabType);
          if(!tableSimObjectPlaceholders.TryGetValue(prefab,out var placeholder)){
           tableSimObjectPlaceholders.Add(prefab,placeholder=Placeholder.singleton.GetPlaceholderFor(prefabType));
          }
          Placeholder.singleton.SetCurrentPlaceholder(placeholder);
         }
        }
    }
}