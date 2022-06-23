using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimConstruction:SimObject{
     float snapTimer=0f;
        internal override void ManualUpdate(bool doValidationChecks){
         if(transform.hasChanged){
          if(!SimObjectManager.singleton.disableSnappingToSlots){
          }
         }
         base.ManualUpdate(doValidationChecks);
        }
        #if UNITY_EDITOR
            protected override void OnDrawGizmos(){
             base.OnDrawGizmos();
             DrawColliders();
            }
            void DrawColliders(){
             if(interactionsEnabled){
              foreach(Collider collider in volumeColliders){
               if(collider is BoxCollider box){
                Gizmos.color=Color.gray;
                Gizmos.matrix=Matrix4x4.TRS(transform.position+box.center,transform.rotation,transform.localScale);
                Gizmos.DrawCube(Vector3.zero,box.size);
               }
              }
              Gizmos.matrix=Matrix4x4.identity;
             }
            }
        #endif
    }
}