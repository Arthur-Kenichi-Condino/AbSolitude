using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal class InteractionSlot:MonoBehaviour{
     [SerializeField]internal SlotPurpose purpose;
        internal enum SlotPurpose{
         InteractFront=0,
         SitPosition=1,
        }
    }
}