using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.ArthurCondino{
    internal class ArthurCondinoAI:SimActor{
        #if UNITY_EDITOR
            protected override void OnDrawGizmos(){
             DrawColliders();
            }
            void DrawColliders(){
             if(interactionsEnabled){
              foreach(Collider collider in volumeColliders){
               if(collider is CapsuleCollider capsule){
                Gizmos.color=Color.gray;
                Gizmos.matrix=Matrix4x4.TRS(transform.position+capsule.center,transform.rotation,transform.localScale);
                Util.DrawWireCapsule(Vector3.up*(capsule.height/2f-capsule.radius),Vector3.down*(capsule.height/2f-capsule.radius),capsule.radius);
               }
              }
              Gizmos.matrix=Matrix4x4.identity;
             }
            }
        #endif
    }
}