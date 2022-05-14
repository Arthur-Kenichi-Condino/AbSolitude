using AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode{
    [Serializable]internal class BuildBuyEditModeUIContent{
     [SerializeField]internal GameObject buildCategorySimObjectsTablePrefab;
     [SerializeField]internal RectTransform buildCategoryRectTransform;
      internal BuildCategorySimObjectsTable buildCategoryTableFloors;
    }
}