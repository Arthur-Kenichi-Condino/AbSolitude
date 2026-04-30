using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/Interactions/InteractablesRegistry")]
    internal class InteractablesRegistry:ScriptableObject{
     [SerializeField]internal InteractableInteractions[]interactables;
    }
}