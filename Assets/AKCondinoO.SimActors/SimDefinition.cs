using UnityEngine;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/SimDefinition")]
    internal class SimDefinition:ScriptableObject{
     [SerializeField]internal SimBrain simBrain;
    }
}