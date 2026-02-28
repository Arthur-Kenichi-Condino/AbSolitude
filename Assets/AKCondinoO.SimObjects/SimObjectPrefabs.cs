using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    [CreateAssetMenu(menuName="AKCondinoO/SimObjectPrefabs")]
    internal class SimObjectPrefabs:ScriptableObject{
     public List<SimObject>list;
    }
}