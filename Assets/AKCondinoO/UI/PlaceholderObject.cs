#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI{
    internal class PlaceholderObject:MonoBehaviour{
     internal readonly List<Collider>collidersForTesting=new List<Collider>();
      readonly HashSet<GameObject>gameObjectsCloned=new HashSet<GameObject>();
        internal void BuildFrom(SimObject simObjectPrefab){
         collidersForTesting.Clear();
         gameObjectsCloned.Clear();
         foreach(Collider collider in simObjectPrefab.GetComponentsInChildren<Collider>()){
          if(collider.CompareTag("SimObjectVolume")&&!gameObjectsCloned.Contains(collider.gameObject)){
           Log.DebugMessage("adding collider for testing positioning...");
           collidersForTesting.Add(Instantiate(collider,this.transform,false));
           gameObjectsCloned.Add(collider.gameObject);
          }
         }
        }
        #if UNITY_EDITOR
            protected void OnDrawGizmos(){
             DrawColliders();
            }
            void DrawColliders(){
             foreach(Collider collider in collidersForTesting){
              if(collider is BoxCollider box){
               Gizmos.color=Color.green;
               Gizmos.matrix=Matrix4x4.TRS(transform.position+box.center,transform.rotation,transform.localScale);
               Gizmos.DrawCube(Vector3.zero,box.size);
              }
             }
             Gizmos.matrix=Matrix4x4.identity;
            }
        #endif
    }
}