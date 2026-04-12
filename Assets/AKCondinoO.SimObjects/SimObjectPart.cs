using AKCondinoO.SimActors.SimInteractions;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectPart:MonoBehaviour,IInteractable{
     [SerializeField]internal SimObjectPartStateTransition[]states;
     internal GameObject simObjectRendererComponents;
     internal GameObject simObjectCollisionComponents;
     internal int currentState;
        internal virtual void ManualUpdate(){
        }
    }
    [Serializable]
    internal class SimObjectPartStateTransition{
     public Transform stateStart;
     public Transform stateEnd;
    }
}