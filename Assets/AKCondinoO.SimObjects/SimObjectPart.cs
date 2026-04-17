using AKCondinoO.SimActors.SimInteractions;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal partial class SimObjectPart:MonoBehaviour,IInteractable{
     [SerializeField]internal SimObjectPartStateTransition[]states;
     [SerializeField]internal int[]usePartMeshSubMeshesForCollider;
     [SerializeField]internal GameObject simObjectPartRendererComponents;
     [SerializeField]internal GameObject simObjectPartCollisionComponents;
     internal SimObject holder;
     internal MeshRenderer simObjectPartMeshRenderer;
     internal MeshFilter   simObjectPartMeshFilter;
     internal MeshCollider simObjectPartMeshCollider;
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