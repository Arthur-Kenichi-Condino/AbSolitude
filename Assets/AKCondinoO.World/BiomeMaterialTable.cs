using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.World.Biomes{
    [CreateAssetMenu(menuName="AKCondinoO/Biomes/MaterialTable")]
    internal class BiomeMaterialTable:ScriptableObject{
     public List<BiomeMaterial>entries;
    }
}