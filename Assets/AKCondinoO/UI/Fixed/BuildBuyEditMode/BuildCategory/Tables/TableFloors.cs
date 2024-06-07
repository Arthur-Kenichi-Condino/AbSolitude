#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.GameMode;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables{
    internal class TableFloors:BuildCategorySimObjectsTable{
        public void OnPlaceSimFloorButtonPress(string type){
         Log.DebugMessage("TableFloors:OnPlaceSimFloorButtonPress:type:"+type);
         Type simFloorType=Type.GetType(type);
         Log.DebugMessage("simFloorType:"+simFloorType);
        }
    }
}