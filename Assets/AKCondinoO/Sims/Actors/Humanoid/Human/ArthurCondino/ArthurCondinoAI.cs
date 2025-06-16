#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal partial class ArthurCondinoAI:HumanAI{
        protected override void Awake(){
         base.Awake();
        }
        internal override void OnActivated(){
         requiredSkills.Clear();
         requiredSkills.Add(typeof(GenerateHomunculus),new SkillData(){skill=typeof(GenerateHomunculus),level=10,});
         requiredSkills.Add(typeof(CallHomunculus    ),new SkillData(){skill=typeof(CallHomunculus    ),level=10,});
         requiredSkills.Add(typeof(AbSolitude        ),new SkillData(){skill=typeof(AbSolitude        ),level=10,});
         requiredSlaves.Clear();
         requiredSlaves.Add(typeof(ArquimedesAI),
          new List<SlaveData>(){
           new SlaveData(){simObjectType=typeof(ArquimedesAI),},
          }
         );
         base.OnActivated();
        }
        //protected override void OnIDLE_ST_Routine(){
        // base.OnIDLE_ST_Routine();
        //}
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