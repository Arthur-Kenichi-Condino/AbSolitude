using AKCondinoO.World;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObject:MonoBehaviour{
     [SerializeField]internal bool useInstancedRendering=false;
     [SerializeField]internal GameObject meshObject;
     internal Type simObjectType;
     internal int instancedRenderingIndex=-1;
        internal void OnPositionChanged(out bool outOfBounds){
         outOfBounds=!WorldChunkManager.singleton.OnAddSimObjectAt(transform.position,this);
        }
        internal void OnChunkPooled(){
         SimObjectManager.singleton.Despawn(this);
        }
    }
}