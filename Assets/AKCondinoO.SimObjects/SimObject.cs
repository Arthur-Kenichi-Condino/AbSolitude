using AKCondinoO.World;
using System;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObject:MonoBehaviour{
     internal Type simObjectType=>type??=GetType();private Type type;
     [SerializeField]internal string variant;
     [SerializeField]internal bool useInstancedRendering=false;
     [SerializeField]internal GameObject meshObject;
     internal int instancedRenderingIndex=-1;
        internal void OnPositionChanged(out bool outOfBounds){
         outOfBounds=!WorldChunkManager.singleton.OnAddSimObjectAt(transform.position,this);
         if(outOfBounds){
          SimObjectManager.singleton.Despawn(this);
         }
        }
        internal void OnChunkPooled(){
         SimObjectManager.singleton.Despawn(this);
        }
    }
}