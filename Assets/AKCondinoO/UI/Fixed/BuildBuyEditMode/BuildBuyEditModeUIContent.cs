using AKCondinoO.Sims;
using AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode{
    [Serializable]internal class BuildBuyEditModeUIContent{
     [SerializeField]internal GameObject tablePrefab;
     [SerializeField]internal GameObject tableButtonPrefab;
     [SerializeField]internal RectTransform tablesParent;
      [NonSerialized]internal readonly HashSet<Type>tablesTypes=new(){
       typeof(TableFloors),
      };
      [NonSerialized]internal readonly Dictionary<Type,List<Type>>simsTablesTypes=new(){
       {
        typeof(SimFloor),
        new List<Type>(){
         typeof(TableFloors),
        }
       },
      };
       [NonSerialized]internal readonly Dictionary<Type,BuildSimObjectsTable>tables=new();
      //[NonSerialized]internal BuildSimObjectsTable tableFloors;
    }
}