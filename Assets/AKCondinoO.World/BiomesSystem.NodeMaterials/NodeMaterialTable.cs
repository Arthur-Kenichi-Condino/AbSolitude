using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/MaterialTable")]
    internal class NodeMaterialTable:ScriptableObject{
     public List<NodeMaterial>entries;
    }
}