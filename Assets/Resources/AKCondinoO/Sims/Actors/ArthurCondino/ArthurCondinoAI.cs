#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.ArthurCondino{
    internal class ArthurCondinoAI:BaseAI{
        protected override void Awake(){
         base.Awake();
			      List<Type>requiredSkills=new List<Type>{
									};
			      foreach(var skill in skills){
				     }
			     }
        #if UNITY_EDITOR
            protected override void OnDrawGizmos(){
             base.OnDrawGizmos();
             DrawColliders();
            }
            void DrawColliders(){
             if(UnityEditor.Selection.activeGameObject!=null&&UnityEditor.Selection.activeGameObject.transform.IsChildOf(this.gameObject.transform)){
              return;
             }
             if(interactionsEnabled){
              foreach(Collider collider in volumeColliders){
               if(collider is CapsuleCollider capsule){
                Gizmos.color=Color.gray;
                Gizmos.matrix=Matrix4x4.TRS(transform.position+capsule.center,transform.rotation,transform.localScale);
                Util.DrawWireCapsule(Vector3.up*(capsule.height/2f-capsule.radius),Vector3.down*(capsule.height/2f-capsule.radius),capsule.radius);
               }else if(collider is CharacterController characterController){
                Gizmos.color=Color.gray;
                Gizmos.matrix=Matrix4x4.TRS(transform.position+characterController.center,transform.rotation,transform.localScale);
                Util.DrawWireCapsule(Vector3.up*(characterController.height/2f-characterController.radius),Vector3.down*(characterController.height/2f-characterController.radius),characterController.radius);
               }
              }
              Gizmos.matrix=Matrix4x4.identity;
             }
            }
        #endif
    }
}