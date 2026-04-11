using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectPart:MonoBehaviour{
     [SerializeField]internal SimObjectPartStateTransition[]states;
     internal GameObject simObjectRendererComponents;
     internal GameObject simObjectCollisionComponents;
     internal int currentState;
    }
    [Serializable]
    internal class SimObjectPartStateTransition{
     public Transform stateStart;
     public Transform stateEnd;
    }
}