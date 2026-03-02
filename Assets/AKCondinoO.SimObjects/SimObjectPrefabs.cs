using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    [CreateAssetMenu(menuName="AKCondinoO/SimObjectPrefabs")]
    internal class SimObjectPrefabs:ScriptableObject{
     public List<SimObjectPrefabData>list;
    }
    [Serializable]
    internal struct SimObjectPrefabData{
     public SimObject simObject;
     public GameObject meshObject;
     public bool useInstancing;
    }
}