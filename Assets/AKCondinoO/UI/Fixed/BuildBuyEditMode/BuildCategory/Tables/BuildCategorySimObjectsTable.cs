using AKCondinoO.Sims;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI.Fixed.BuildBuyEditMode.BuildCategory.Tables{
    internal class BuildCategorySimObjectsTable:MonoBehaviour{
     internal readonly SortedDictionary<string,SimObject>tableSimObjects=new SortedDictionary<string,SimObject>();
     internal void OnCreateTable(){
     }
    }
}