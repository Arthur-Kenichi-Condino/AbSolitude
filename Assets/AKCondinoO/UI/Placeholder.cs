#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UI{
    internal class Placeholder:MonoBehaviour,ISingletonInitialization{
     internal static Placeholder singleton{get;set;}
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("Placeholder:OnDestroyingCoreEvent");
        }
     internal readonly List<Collider>collidersForTesting=new List<Collider>();
        internal void GetPlaceholderFor(Type t){
         SimObject simObjectPrefab=SimObjectSpawner.singleton.simObjectPrefabs[t].GetComponent<SimObject>();
         collidersForTesting.Clear();
         foreach(Collider collider in simObjectPrefab.GetComponentsInChildren<Collider>()){
          if(collider.CompareTag("SimObjectVolume")){
           Log.DebugMessage("adding collider for testing positioning...");
           collidersForTesting.Add(collider);
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