using UnityEngine;
namespace AKCondinoO.SimActors{
    [CreateAssetMenu(menuName="AKCondinoO/Sims/SimDefinition")]
    internal class SimDefinition:ScriptableObject{
     [SerializeField]internal SimBrain simBrain;
     [SerializeField]internal SimMovement simMovement;
        internal virtual void Tick(SimActor sim){
         if((object)simBrain!=null){
          simBrain.AI(sim);
         }
         if((object)simMovement!=null){
          simMovement.OnTick(sim);
         }
        }
    }
}