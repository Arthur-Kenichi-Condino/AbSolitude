using AKCondinoO.SimActors.SimInteractions;
using AKCondinoO.SimObjects.StateMachines;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal partial class SimObjectPart:MonoBehaviour,IInteractable{
     [SerializeField]internal int[]usePartMeshSubMeshesForCollider;
     [SerializeField]internal GameObject simObjectPartRendererComponents;
     [SerializeField]internal GameObject simObjectPartCollisionComponents;
     internal MeshRenderer simObjectPartMeshRenderer;
     internal MeshFilter   simObjectPartMeshFilter;
     internal MeshCollider simObjectPartMeshCollider;
     internal SimObjectPartStateMachine partStateMachine;
     public StateMachine stateMachine=>partStateMachine;
     internal SimObject holder;
        internal virtual void ManualUpdate(){
        }
    }
}