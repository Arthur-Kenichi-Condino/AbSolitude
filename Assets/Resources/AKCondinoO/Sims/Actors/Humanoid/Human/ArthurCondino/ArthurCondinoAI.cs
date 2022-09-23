#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino{
    internal class ArthurCondinoAI:HumanAI{
        protected override void Awake(){
         base.Awake();
        }
        internal override void OnActivated(){
         requiredSkills.Add(typeof(GenerateHomunculus),10);
         base.OnActivated();
        }
        protected override void OnIDLE_ST(){
         if(MySkill==null){
          Skill.GetBest(this,Skill.SkillUseContext.OnCallSlaves,skillsToUse);
          if(skills.TryGetValue(typeof(GenerateHomunculus),out Skill skillToGet)&&skillsToUse.TryGetValue(skillToGet,out Skill skill)){
           GenerateHomunculus generateHomunculusSkill=(GenerateHomunculus)skill;
           if(generateHomunculusSkill.IsAvailable(this,generateHomunculusSkill.level)){
            //if(slaves.Count<=0){//  should Arthur generate his "homunculi friends" now?
             MySkill=generateHomunculusSkill;
             Log.DebugMessage("check skillsToUse.Count:"+skillsToUse.Count+";should use generateHomunculusSkill");
            //}
           }
          }
          if(MySkill==null){
           //  TO DO: get other skills
          }
         }
         base.OnIDLE_ST();
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